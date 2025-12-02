using System;
using identity_service.Models;

namespace identity_service.Models;

public class EmailVerificationToken : EntityBase
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }

    // Navigation property
    public ApplicationUser? User { get; set; }
}
