using System;

namespace identity_service.Dtos.Auth;

public class SystemAccessTokenDto
{
    public string AccessToken { get; set; } = null!;
    public DateTimeOffset Expires { get; set; }
    public string System { get; set; } = null!;
}
