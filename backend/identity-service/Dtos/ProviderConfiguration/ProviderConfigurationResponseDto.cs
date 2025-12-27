namespace identity_service.Dtos.ProviderConfiguration;

public class ProviderConfigurationResponseDto
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = null!;
    public string? ProviderType { get; set; }
    public string? ClientId { get; set; }
    public string? EndpointUrl { get; set; }
    public string? Scopes { get; set; }
    public bool Enabled { get; set; }
    public DateTime LastModified { get; set; }
    public DateTimeOffset? DateCreate { get; set; }
    public string? UserCreate { get; set; }
}
