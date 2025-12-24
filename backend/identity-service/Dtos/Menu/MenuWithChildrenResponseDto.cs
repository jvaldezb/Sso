using System;

namespace identity_service.Dtos.Menu;

public class MenuWithChildrenResponseDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public Guid SystemId { get; set; }
    public string MenuLabel { get; set; } = default!;
    public string? Description { get; set; }
    public short Level { get; set; }
    public string? Module { get; set; }
    public string? ModuleType { get; set; }
    public string? MenuType { get; set; }
    public string? IconUrl { get; set; }    
    public short OrderIndex { get; set; }    
    public string? Url { get; set; }
    public bool IsEnabled { get; set; }
    public string? UserCreate { get; set; }
    public string? UserUpdate { get; set; }
    public DateTimeOffset? DateCreate { get; set; }
    public DateTimeOffset? DateUpdate { get; set; }
    public List<MenuWithChildrenResponseDto>? ChildMenus { get; set; }
}
