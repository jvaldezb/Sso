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
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Información opcional para auditoría
    public string? Audience { get; set; }
    public string? Scope { get; set; }
}
