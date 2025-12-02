namespace identity_service.Models;

public class UserPasswordHistory : EntityBase
{    
    public string UserId { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public virtual ApplicationUser User { get; set; } = null!;
}