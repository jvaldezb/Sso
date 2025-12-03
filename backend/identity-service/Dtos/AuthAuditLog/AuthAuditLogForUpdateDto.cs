using System;

namespace identity_service.Dtos.AuthAuditLog;

public class AuthAuditLogForUpdateDto
{
    public string? UserId { get; set; }
    public string? ProviderName { get; set; }
    public string? EventType { get; set; }    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; } 
}
