using System;

namespace identity_service.Dtos.User;

public class SystemTokenRequestDto
{
    public required string SystemName { get; set; }
    public string? Scope { get; set; } = "read";
}
