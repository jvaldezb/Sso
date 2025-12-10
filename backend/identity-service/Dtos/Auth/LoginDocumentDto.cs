using System;

namespace identity_service.Dtos.Auth;

public class LoginDocumentDto
{
    public required string DocumentType { get; set; }
    public required string DocumentNumber { get; set; }
    public required string Password { get; set; }

}
