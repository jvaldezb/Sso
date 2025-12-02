using System;
using Microsoft.AspNetCore.Identity;

namespace identity_service.Models;

public class ApplicationRole : IdentityRole
{
    public Guid? SystemId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? UserUpdate { get; set; }
    public string? UserCreate { get; set; }
    public DateTime? DateUpdate { get; set; }
    public DateTime? DateCreate { get; set; }
}
