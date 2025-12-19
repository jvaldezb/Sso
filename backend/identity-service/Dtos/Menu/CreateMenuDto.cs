using System;

namespace identity_service.Dtos.Menu;

public class CreateMenuDto
{
    public Guid? ParentId { get; set; }
    public required Guid SystemId { get; set; }
    public required string MenuLabel { get; set; }
    public string? Description { get; set; }
    public short Level { get; set; } = 1;
    public string? Module { get; set; }
    public string? ModuleType { get; set; }
    public string? MenuType { get; set; }
    public string? IconUrl { get; set; }
    public string? AccessScope { get; set; }
    public short OrderIndex { get; set; } = 1;    
    public string? Url { get; set; }
    public bool IsEnabled { get; set; } = true;
}
