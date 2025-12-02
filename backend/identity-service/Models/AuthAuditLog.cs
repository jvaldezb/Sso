using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace identity_service.Models;

public class AuthAuditLog : EntityBase
{        
    public string? UserId { get; set; }
    public string? ProviderName { get; set; }
    public string? EventType { get; set; }    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public JsonDocument? Details { get; set; }    

    // Navigation property    
    public virtual ApplicationUser? User { get; set; }
}