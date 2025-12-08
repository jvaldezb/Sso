using System;

namespace identity_service.Dtos.SystemRegistry;

public class CreateSystemRegistryDto
{
    public required string SystemCode { get; set; }
    public required string SystemName { get; set; }
    public string? Description { get; set; }
    public required string BaseUrl { get; set; }
    public string? IconUrl { get; set; }
    public string? Category { get; set; }
    public string? ContactEmail { get; set; }    
    public string? ApiKey { get; set; }
}
