namespace identity_service.Models;

public class ProviderConfiguration: EntityBase
{    
    public string ProviderName { get; set; } = null!;
    public string? ProviderType { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? EndpointUrl { get; set; }
    public string? Scopes { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime LastModified { get; set; } = DateTime.UtcNow; 
}