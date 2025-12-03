using System;
using System.Text.Json;

namespace identity_service.Dtos.AuthAuditLog;

public class AuthAuditLogDto
{    
    public string? UserId { get; set; }
    public string? ProviderName { get; set; }
    public string? EventType { get; set; }    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }    
    public JsonDocument? Details { get; set; }        
}
