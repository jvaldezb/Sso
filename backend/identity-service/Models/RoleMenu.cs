using System;

namespace identity_service.Models;

public class RoleMenu : EntityBase
{
    public Guid RoleId { get; set; }
    public Guid MenuId { get; set; }
    public int AccessLevel { get; set; }

    // Navigation properties
    public ApplicationRole? Role { get; set; }
    public Menu? Menu { get; set; }
}
