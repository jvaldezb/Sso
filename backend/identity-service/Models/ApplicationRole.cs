using System;
using Microsoft.AspNetCore.Identity;

namespace identity_service.Models;

public class ApplicationRole : IdentityRole
{
    // Guid principal key to allow FK relationships from entities using Guid (e.g. RoleMenu.RoleId)
    public Guid RoleGuid { get; set; } = Guid.NewGuid();

    public Guid? SystemId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? UserUpdate { get; set; }
    public string? UserCreate { get; set; }
    public DateTime? DateUpdate { get; set; }
    public DateTime? DateCreate { get; set; }
}
