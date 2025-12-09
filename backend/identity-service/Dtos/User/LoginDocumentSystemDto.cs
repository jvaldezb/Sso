using System;

namespace identity_service.Dtos.User;

public class LoginDocumentSystemDto
{
    public required string DocumentType { get; set; }
    public required string DocumentNumber { get; set; }
    public required string Password { get; set; }
    public required string SystemCode { get; set; }
    public required string ApiKey { get; set; }
}
