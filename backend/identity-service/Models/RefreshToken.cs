using System;

namespace identity_service.Models;

public class RefreshToken: EntityBase
{    
    public string UserId { get; set; } = null!;
    public Guid? SystemId { get; set; }
    public Guid? SessionId { get; set; }  
    public string Token { get; set; } = null!;
    public DateTime CreatedAt { get; set; }          
    public DateTime ExpiresAt { get; set; }          
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }         

    public string? ReplacedByToken { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public SystemRegistry? System { get; set; }
    public UserSession? Session { get; set; }
}
