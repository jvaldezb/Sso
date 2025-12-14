using System;

namespace identity_service.Dtos.ExchangeToken;

public class ExchangeResponseDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public string TokenType { get; set; } = "Bearer";
}
