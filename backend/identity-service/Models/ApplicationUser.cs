using System;
using Microsoft.AspNetCore.Identity;

namespace identity_service.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? ManagementCode { get; set; }
    public string? SecretQuestion { get; set; }
    public string? SecretAnswer { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? EntityType { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? SourceSystem { get; set; }
    public string? LastLoginProvider { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? UserUpdate { get; set; }
    public string? UserCreate { get; set; }
    public DateTime? DateUpdate { get; set; }
    public DateTime? DateCreate { get; set; }

    public ICollection<UserAuthenticationProvider>? AuthenticationProviders { get; set; }
}
