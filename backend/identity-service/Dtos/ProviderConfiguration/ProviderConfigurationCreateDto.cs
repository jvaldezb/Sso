using System.ComponentModel.DataAnnotations;

namespace identity_service.Dtos.ProviderConfiguration;

public class ProviderConfigurationCreateDto
{
    [Required]
    [MaxLength(100)]
    public string ProviderName { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string ProviderType { get; set; } = null!; // "LDAP", "OAuth2", "SAML"
    
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    
    [Required]
    public string EndpointUrl { get; set; } = null!; // ldap://localhost:389
    
    public string? Scopes { get; set; } // JSON con configuraciones específicas
    public bool Enabled { get; set; } = true;
}
