using System;

namespace identity_service.Dtos.User;

public class AssignRolesDto
{
    public List<Guid> RoleIds { get; set; } = new();
}
