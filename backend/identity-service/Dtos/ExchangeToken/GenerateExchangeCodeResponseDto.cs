using System;

namespace identity_service.Dtos.ExchangeToken;

public class GenerateExchangeCodeResponseDto
{
    public string ExchangeCode { get; set; } = default!;
    public DateTimeOffset ExpiresAt { get; set; }
}
