using System;

namespace identity_service.Dtos.Role;

public class RoleClaimDto
{
    public string Type { get; set; } = default!;
    public string Value { get; set; } = default!;
}
