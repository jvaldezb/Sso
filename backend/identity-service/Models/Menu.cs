namespace identity_service.Models;

public class Menu : EntityBase
{    
    
    public Guid? ParentId { get; set; }
    public Guid SystemId { get; set; }
    public string MenuLabel { get; set; } = null!;
    public string? Description { get; set; }
    public short Level { get; set; } = 1;
    public string? Module { get; set; }
    public string? ModuleType { get; set; }
    public string? RequiredClaimType { get; set; }
    public int RequiredClaimMinValue { get; set; } = 4;
    public string? IconUrl { get; set; }
    public string? AccessScope { get; set; }
    public short OrderIndex { get; set; } = 1; 
    public int? BitPosition { get; set; }
    public string? Url { get; set; }

    public virtual Menu? ParentMenu { get; set; }
    public virtual ICollection<Menu>? ChildMenus { get; set; }
    public virtual SystemRegistry? System { get; set; }
}