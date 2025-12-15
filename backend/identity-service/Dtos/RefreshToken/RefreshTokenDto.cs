using System;

namespace identity_service.Dtos.RefreshToken;

public class RefreshTokenDto
{
    public Guid Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
