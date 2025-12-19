using System;

namespace identity_service.Dtos.RoleMenu;

public class RoleMenuResponseDto
{
    public Guid Id { get; set; }
    public required string RoleId { get; set; }
    public Guid MenuId { get; set; }
    public int AccessLevel { get; set; }
    public string? UserCreate { get; set; }
    public DateTimeOffset? DateCreate { get; set; }
    public string? UserUpdate { get; set; }
    public DateTimeOffset? DateUpdate { get; set; }
}
