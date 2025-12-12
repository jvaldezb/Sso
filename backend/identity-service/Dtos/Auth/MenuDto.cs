using System;

namespace identity_service.Dtos.Auth;

public class MenuDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? ModuleLabel { get; set; }
    public string? Module { get; set; }
    public string? ModuleType { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? Url { get; set; }
    public int Level { get; set; }
    public int OrderIndex { get; set; }
}
