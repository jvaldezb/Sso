using System;

namespace identity_service.Dtos.RoleMenu;

public class RoleMenuCreateDto
{
    public required string RoleId { get; set; }
    public Guid MenuId { get; set; }
    public int AccessLevel { get; set; }
}
