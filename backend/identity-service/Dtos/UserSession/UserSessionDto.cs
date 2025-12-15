using System;

namespace identity_service.Dtos.UserSession;

public class UserSessionDto
{
    public string? UserId { get; set; }
    public string? JwtId { get; set; }
    public string? TokenType { get; set; }
    public string? SystemName { get; set; }
    public string? Device { get; set; } 
    public string? IpAddress { get; set; }

    // Datos del token
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    // Información opcional para auditoría
    public string? Audience { get; set; }
    public string? Scope { get; set; }
}
