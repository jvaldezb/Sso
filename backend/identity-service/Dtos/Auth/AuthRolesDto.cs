using System;

namespace identity_service.Dtos.Auth;

public class AuthRoleDto
{
    public string RoleId { get; set; } = null!;
    public string RoleName { get; set; } = null!;
}