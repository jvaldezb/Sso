using System.DirectoryServices.Protocols;
using System.Net;
using identity_service.Dtos;
using identity_service.Services.Interfaces;

namespace identity_service.Services;

public class LdapAuthenticationService : ILdapAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LdapAuthenticationService> _logger;

    public LdapAuthenticationService(
        IConfiguration configuration,
        ILogger<LdapAuthenticationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<LdapUserInfo>> AuthenticateAsync(string username, string password)
    {
        try
        {
            var ldapServer = _configuration["LdapSettings:Server"];
            var ldapPort = int.Parse(_configuration["LdapSettings:Port"] ?? "389");
            var baseDn = _configuration["LdapSettings:BaseDn"];
            var userSearchBase = _configuration["LdapSettings:UserSearchBase"];
            var useSsl = bool.Parse(_configuration["LdapSettings:UseSsl"] ?? "false");
            
            _logger.LogInformation("LDAP Configuration - Server: {Server}, Port: {Port}, BaseDn: {BaseDn}, UserSearchBase: {UserSearchBase}", 
                ldapServer, ldapPort, baseDn, userSearchBase);
            
            if (string.IsNullOrWhiteSpace(ldapServer) || string.IsNullOrWhiteSpace(baseDn))
            {
                return Result<LdapUserInfo>.Failure("LDAP configuration is incomplete");
            }

            // Crear identificador del servidor LDAP
            var ldapIdentifier = new LdapDirectoryIdentifier(ldapServer, ldapPort);
            
            // PASO 1: Autenticar al usuario
            using (var userConnection = new LdapConnection(ldapIdentifier))
            {
                userConnection.Timeout = TimeSpan.FromSeconds(30);
                userConnection.SessionOptions.ProtocolVersion = 3;
                
                if (useSsl)
                {
                    userConnection.SessionOptions.SecureSocketLayer = true;
                    userConnection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
                }
                
                userConnection.AuthType = AuthType.Basic;

                var userDn = $"uid={username},{userSearchBase},{baseDn}";
                
                _logger.LogInformation("Attempting LDAP bind with DN: {UserDn}", userDn);
                
                try
                {
                    userConnection.Bind(new NetworkCredential(userDn, password));
                    _logger.LogInformation("LDAP bind successful for user {Username}", username);
                }
                catch (LdapException ex) when (ex.ErrorCode == 49)
                {
                    _logger.LogWarning("LDAP authentication failed for user {Username} - Invalid credentials", username);
                    return Result<LdapUserInfo>.Failure("Invalid LDAP credentials");
                }
                catch (LdapException ex)
                {
                    _logger.LogError(ex, "LDAP exception during bind for user {Username}. ErrorCode: {ErrorCode}, ServerErrorMessage: {ServerErrorMessage}", 
                        username, ex.ErrorCode, ex.ServerErrorMessage);
                    return Result<LdapUserInfo>.Failure($"LDAP connection error: {ex.Message}");
                }
            }

            // PASO 2: Buscar información del usuario con una conexión admin
            using (var adminConnection = new LdapConnection(ldapIdentifier))
            {
                adminConnection.Timeout = TimeSpan.FromSeconds(30);
                adminConnection.SessionOptions.ProtocolVersion = 3;
                
                if (useSsl)
                {
                    adminConnection.SessionOptions.SecureSocketLayer = true;
                    adminConnection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
                }
                
                adminConnection.AuthType = AuthType.Basic;

                // Bind con admin para buscar información del usuario
                var adminDn = $"cn=admin,{baseDn}";
                var adminPassword = "admin"; // En producción, esto debe venir de configuración
                
                try
                {
                    adminConnection.Bind(new NetworkCredential(adminDn, adminPassword));
                    _logger.LogInformation("Admin bind successful for user info search");
                }
                catch (LdapException ex)
                {
                    _logger.LogError(ex, "Failed to bind as admin for user info search");
                    // Si falla el admin bind, retornamos info básica del usuario
                    return Result<LdapUserInfo>.Success(new LdapUserInfo
                    {
                        Username = username,
                        Email = $"{username}@mycompany.com",
                        DisplayName = username,
                        GivenName = string.Empty,
                        Surname = string.Empty,
                        Groups = new List<string>()
                    });
                }

                // Si llegamos aquí, la autenticación fue exitosa
                var userInfo = await SearchUserInfoAsync(adminConnection, username, userSearchBase ?? "", baseDn);
                
                if (userInfo == null)
                {
                    // Si no se puede buscar la info, retornamos info básica
                    return Result<LdapUserInfo>.Success(new LdapUserInfo
                    {
                        Username = username,
                        Email = $"{username}@mycompany.com",
                        DisplayName = username,
                        GivenName = string.Empty,
                        Surname = string.Empty,
                        Groups = new List<string>()
                    });
                }

                _logger.LogInformation("LDAP authentication successful for user {Username}", username);
                return Result<LdapUserInfo>.Success(userInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during LDAP authentication for user {Username}", username);
            return Result<LdapUserInfo>.Failure($"LDAP authentication error: {ex.Message}");
        }
    }

    private async Task<LdapUserInfo?> SearchUserInfoAsync(
        LdapConnection connection, 
        string username,
        string userSearchBase,
        string baseDn)
    {
        try
        {
            var searchBase = string.IsNullOrWhiteSpace(userSearchBase) 
                ? baseDn 
                : $"{userSearchBase},{baseDn}";
            var searchFilter = $"(uid={username})";
            
            _logger.LogInformation("Searching LDAP user - SearchBase: {SearchBase}, Filter: {Filter}", searchBase, searchFilter);
            
            var searchRequest = new SearchRequest(
                searchBase,
                searchFilter,
                SearchScope.Subtree,
                new[] { "cn", "mail", "givenName", "sn", "memberOf", "displayName" }
            );

            var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

            _logger.LogInformation("LDAP search returned {Count} entries", searchResponse.Entries.Count);

            if (searchResponse.Entries.Count == 0)
            {
                _logger.LogWarning("No LDAP entries found for user {Username}", username);
                return null;
            }

            var entry = searchResponse.Entries[0];
            
            var userInfo = new LdapUserInfo
            {
                Username = username,
                Email = GetAttribute(entry, "mail") ?? $"{username}@example.com",
                DisplayName = GetAttribute(entry, "displayName") ?? GetAttribute(entry, "cn") ?? username,
                GivenName = GetAttribute(entry, "givenName") ?? string.Empty,
                Surname = GetAttribute(entry, "sn") ?? string.Empty,
                Groups = GetMultiValueAttribute(entry, "memberOf")
            };

            _logger.LogInformation("Successfully retrieved user info for {Username}: Email={Email}, DisplayName={DisplayName}", 
                username, userInfo.Email, userInfo.DisplayName);

            return await Task.FromResult(userInfo);
        }
        catch (DirectoryOperationException ex)
        {
            _logger.LogError(ex, "Directory operation error searching user info for {Username}. Extended error: {ExtendedError}", 
                username, ex.Response?.ErrorMessage);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching user info for {Username}", username);
            return null;
        }
    }

    private string? GetAttribute(SearchResultEntry entry, string attributeName)
    {
        if (entry.Attributes.Contains(attributeName))
        {
            var attr = entry.Attributes[attributeName];
            if (attr.Count > 0)
            {
                return attr[0]?.ToString();
            }
        }
        return null;
    }

    private List<string> GetMultiValueAttribute(SearchResultEntry entry, string attributeName)
    {
        var values = new List<string>();
        
        if (entry.Attributes.Contains(attributeName))
        {
            var attr = entry.Attributes[attributeName];
            for (int i = 0; i < attr.Count; i++)
            {
                var value = attr[i]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }
        }
        
        return values;
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var ldapServer = _configuration["LdapSettings:Server"];
            var ldapPort = int.Parse(_configuration["LdapSettings:Port"] ?? "389");
            
            if (string.IsNullOrWhiteSpace(ldapServer))
            {
                return false;
            }

            var ldapIdentifier = new LdapDirectoryIdentifier(ldapServer, ldapPort);
            using var connection = new LdapConnection(ldapIdentifier)
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            
            // Intentar conexión simple
            connection.Bind();
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LDAP server is not available");
            return false;
        }
    }
}
