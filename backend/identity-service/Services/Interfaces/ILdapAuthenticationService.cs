using identity_service.Dtos;
using identity_service.Dtos.ProviderConfiguration;

namespace identity_service.Services.Interfaces;

public interface ILdapAuthenticationService
{
    /// <summary>
    /// Autentica un usuario contra LDAP
    /// </summary>
    /// <param name="username">Usuario LDAP</param>
    /// <param name="password">Contraseña</param>
    /// <param name="server">Servidor LDAP</param>
    /// <param name="port">Puerto LDAP</param>
    /// <param name="adminDn">DN del admin para búsqueda</param>
    /// <param name="adminPassword">Contraseña del admin</param>
    /// <param name="settings">Configuración específica de LDAP</param>
    /// <returns>Resultado con información del usuario LDAP si es exitoso</returns>
    Task<Result<LdapUserInfo>> AuthenticateAsync(
        string username, 
        string password,
        string server,
        int port,
        string? adminDn,
        string? adminPassword,
        LdapSettingsDto settings);
}

public class LdapUserInfo
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = new();
}
