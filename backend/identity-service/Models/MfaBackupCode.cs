using System;
using identity_service.Models;

namespace identity_service.Models;

public class MfaBackupCode : EntityBase
{
    public required string UserId { get; set; }
    public required string Code { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }

    // Navigation property
    public ApplicationUser? User { get; set; }
}
