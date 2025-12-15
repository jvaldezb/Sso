using System;

namespace identity_service.Dtos.ExchangeToken;

public class ExchangeResponseDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTimeOffset ExpiresAt { get; set; }
    public string AuthorizationScheme { get; set; } = "Bearer";
}
