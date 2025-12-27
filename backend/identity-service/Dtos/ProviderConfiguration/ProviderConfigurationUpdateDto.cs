using System.ComponentModel.DataAnnotations;

namespace identity_service.Dtos.ProviderConfiguration;

public class ProviderConfigurationUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string ProviderName { get; set; } = null!;
    
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    
    [Required]
    public string EndpointUrl { get; set; } = null!;
    
    public string? Scopes { get; set; }
    public bool Enabled { get; set; } = true;
}
