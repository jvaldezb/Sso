using System;

namespace identity_service.Dtos.SystemRegistry;

public class SystemRegistryResponseDto
{
    public Guid Id { get; set; }
    public string SystemCode { get; set; } = default!;
    public string SystemName { get; set; } = default!;
    public string? Description { get; set; }
    public string BaseUrl { get; set; } = default!;
    public string? IconUrl { get; set; }    
    public string? Category { get; set; }
    public string? ApiKey { get; set; }
    public string? ContactEmail { get; set; }    
}
