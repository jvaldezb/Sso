using System;

namespace identity_service.Dtos.ExchangeToken;

public class GenerateExchangeCodeDto
{
    public Guid SystemId { get; set; }
    public Guid SessionId { get; set; }
}
