using System;

namespace identity_service.Dtos.Menu;

public class UpdateMenuBatchDto
{
    public Guid? Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? MenuLabel { get; set; }
    public string? Description { get; set; }
    public short? Level { get; set; }
    public string? Module { get; set; }
    public string? ModuleType { get; set; }
    public string? MenuType { get; set; }
    public string? IconUrl { get; set; }
    public short? OrderIndex { get; set; }    
    public string? Url { get; set; }
    public bool? IsEnabled { get; set; }
}
