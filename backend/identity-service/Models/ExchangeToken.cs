using System;

namespace identity_service.Models;

public class ExchangeToken
{
    public Guid Jti { get; set; }
    public Guid SystemId { get; set; }
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public bool IsExpired()
        => DateTimeOffset.UtcNow > ExpiresAt;
    public bool IsUsed()
        => UsedAt.HasValue;
}
