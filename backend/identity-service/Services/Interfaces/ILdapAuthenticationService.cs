using identity_service.Dtos;

namespace identity_service.Services.Interfaces;

public interface ILdapAuthenticationService
{
    /// <summary>
    /// Autentica un usuario contra LDAP
    /// </summary>
    /// <param name="username">Usuario LDAP</param>
    /// <param name="password">Contraseña</param>
    /// <returns>Resultado con información del usuario LDAP si es exitoso</returns>
    Task<Result<LdapUserInfo>> AuthenticateAsync(string username, string password);
    
    /// <summary>
    /// Verifica si el servicio LDAP está disponible
    /// </summary>
    Task<bool> IsAvailableAsync();
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
