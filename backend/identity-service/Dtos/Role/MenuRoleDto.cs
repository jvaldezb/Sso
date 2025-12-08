using System;

namespace identity_service.Dtos.Role;

public class MenuRoleDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public Guid SystemId { get; set; }
    public string MenuLabel { get; set; } = null!; 
    public string? IconUrl { get; set; }        
    public short Level { get; set; } = 1;
    public string? Module { get; set; }    
    public int? BitPosition { get; set; }  
}
