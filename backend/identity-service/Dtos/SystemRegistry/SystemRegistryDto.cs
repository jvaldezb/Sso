using System;

namespace identity_service.Dtos.SystemRegistry;

public class SystemRegistryDto
{
    public Guid Id { get; set; }
    public string SystemCode { get; set; } = default!;
    public string SystemName { get; set; } = default!;
    public string? Description { get; set; }
    public string BaseUrl { get; set; } = default!;
    public string? IconUrl { get; set; }
    public bool IsEnabled { get; set; }
    public string? Category { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsCentralAdmin { get; set; }
    public DateTimeOffset? LastSync { get; set; }
    public string? UserCreate { get; set; }
    public string? UserUpdate { get; set; }
    public DateTime DateCreate { get; set; }
    public DateTime DateUpdate { get; set; }
}
