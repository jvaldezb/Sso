using System;

namespace identity_service.Dtos.ExchangeToken;

public class ExchangeCodeRequestDto
{
    public string ExchangeCode { get; set; } = default!;
    public Guid SystemId { get; set; }
    public string ClientSecret { get; set; } = default!;
}
