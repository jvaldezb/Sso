using System;

namespace identity_service.Dtos.SystemRegistry;

public class SystemRegistryWithMenusDto
{
    public Guid Id { get; set; }
    public string SystemCode { get; set; } = default!;
    public string SystemName { get; set; } = default!;
    public string? Description { get; set; }
    public string BaseUrl { get; set; } = default!;
    public string? IconUrl { get; set; }
    public bool IsEnabled { get; set; }
    public string? Category { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsCentralAdmin { get; set; }
    public string? ApiKey { get; set; }
    public DateTimeOffset? LastSync { get; set; }
    public List<MenuDetailsDto> Menus { get; set; } = new();
}

public class MenuDetailsDto
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
    public string? AccessScope { get; set; }
    public short OrderIndex { get; set; }
    public string? Url { get; set; }
}
