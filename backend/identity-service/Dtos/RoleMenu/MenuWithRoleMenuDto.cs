using System;
using identity_service.Dtos.Role;

namespace identity_service.Dtos.RoleMenu;

public class MenuWithRoleMenuDto
{
    // Menu fields
    public Guid Id { get; set; }
    public string MenuLabel { get; set; } = default!;
    public Guid SystemId { get; set; }
    public int Level { get; set; }
    public Guid? ParentId { get; set; }
    public short OrderIndex { get; set; }

    // RoleMenu fields
    public Guid RoleMenuId { get; set; }
    public int AccessLevel { get; set; }
}
