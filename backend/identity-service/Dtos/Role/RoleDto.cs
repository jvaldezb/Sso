using System;

namespace identity_service.Dtos.Role;

public class RoleDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public Guid SystemId { get; set; }
    public string? SystemCode { get; set; }
    public string? SystemName { get; set; }
    public string? UserCreate { get; set; }
    public string? UserUpdate { get; set; }
    public string? UserCreateName { get; set; }
    public string? UserUpdateName { get; set; }
    public DateTimeOffset? DateCreate { get; set; }
    public DateTimeOffset? DateUpdate { get; set; }
    public bool IsEnabled { get; set; }
}
