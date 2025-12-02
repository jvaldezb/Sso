namespace identity_service.Models;

public class UserAuthenticationProvider: EntityBase
{    
    public string? UserId { get; set; }
    public string ProviderType { get; set; } = null!;
    public string? ProviderName { get; set; }
    public string? ExternalUserId { get; set; }
    public DateTime? LastSync { get; set; }
    public bool IsActive { get; set; } = true; 

    public virtual ApplicationUser? User { get; set; }
}