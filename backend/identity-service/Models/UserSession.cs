using System;

namespace identity_service.Models;

public class UserSession : EntityBase
{     
    public string UserId { get; set; } = default!;
    public string JwtId { get; set; } = default!;   
    public string TokenType { get; set; } = "access";  // session o access
    public string SystemName { get; set; } = default!; // SIGA, SIAF, RRHH...    
    public string Device { get; set; } = "Unknown";    
    public string? IpAddress { get; set; }

    // Datos del token
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }

    // Información opcional para auditoría
    public string? Audience { get; set; }   // coincide con SystemName normalmente
    public string? Scope { get; set; }      // ej: read, write, admin    
    public virtual ApplicationUser? User { get; set; }
}

