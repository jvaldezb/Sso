using System;

namespace identity_service.Dtos.User;

public class UserRolesResponseDto
{
    public string UserId { get; set; } = default!;
    public List<UserRoleDto> Roles { get; set; } = new();
}

public class UserRoleDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = default!;
    public Guid? SystemId { get; set; }
    public string? SystemCode { get; set; }
    public string? SystemName { get; set; }
}
