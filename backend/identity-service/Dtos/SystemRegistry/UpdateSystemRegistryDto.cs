using System;

namespace identity_service.Dtos.SystemRegistry;

public record UpdateSystemRegistryDto(
    string SystemCode,
    string SystemName,
    string? Description,
    string BaseUrl,
    string? IconUrl,    
    string? Category,
    string? ContactEmail,
    string? ApiKey
);
