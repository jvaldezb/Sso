namespace identity_service.Dtos.ProviderConfiguration;

/// <summary>
/// DTO para configuraciones específicas de LDAP almacenadas en el campo Scopes de ProviderConfiguration
/// </summary>
public class LdapSettingsDto
{
    public string BaseDn { get; set; } = null!;
    public string UserSearchBase { get; set; } = null!;
    public bool UseSsl { get; set; }
    public bool UseStartTls { get; set; }
    public int Timeout { get; set; } = 30;
    public string? AdminDn { get; set; }
    public string? SearchFilter { get; set; } = "(uid={0})";
}
