using System;

namespace identity_service.Dtos.Role;

public class MenuDto
{
    public Guid Id { get; set; }
    public string MenuLabel { get; set; } = default!;
    public string? RequiredClaimType { get; set; }
    public int RequiredClaimMinValue { get; set; }
    public Guid SystemId { get; set; }
    public int Level { get; set; }
    public Guid? ParentId { get; set; }
    public short OrderIndex { get; set; }
}
