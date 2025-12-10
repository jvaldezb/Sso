using System;

namespace identity_service.Dtos.RefreshToken;

public class RefreshTokenDto
{
    public Guid Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
}
