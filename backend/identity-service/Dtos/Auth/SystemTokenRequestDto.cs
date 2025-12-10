using System;

namespace identity_service.Dtos.Auth;

public class SystemTokenRequestDto
{
    public required string SystemName { get; set; }
    public string? Scope { get; set; } = "read";
}
