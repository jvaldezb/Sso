using System;

namespace identity_service.Dtos.Role;

public class MenuRoleRwxDto
{
    public Guid Id { get; set; }
    public string? Module { get; set; }        
    public int? BitPosition { get; set; }  
    public int? RwxValue { get; set; }
}
