using System;

namespace identity_service.Dtos.RoleMenu;

public class RoleMenuUpdateDto
{
    public Guid RoleId { get; set; }
    public Guid MenuId { get; set; }
    public int AccessLevel { get; set; }
}
