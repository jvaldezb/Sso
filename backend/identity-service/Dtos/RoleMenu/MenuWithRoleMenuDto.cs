using System;
using identity_service.Dtos.Role;

namespace identity_service.Dtos.RoleMenu;

public class MenuWithRoleMenuDto
{
    // Menu fields
    public Guid MenuId { get; set; }
    public string MenuLabel { get; set; } = default!;    
    public int Level { get; set; }
    public Guid? ParentMenuId { get; set; }
    public string? Module { get; set; } = default!;
    public string? ModuleType { get; set; } 
    public string? MenuType { get; set; }
    public string? IconUrl { get; set; }
    public short OrderIndex { get; set; }

    // RoleMenu fields
    public Guid RoleMenuId { get; set; }
    public int AccessLevel { get; set; } = 0;
}
