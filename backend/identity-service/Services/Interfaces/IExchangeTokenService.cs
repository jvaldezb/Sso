using System;
using identity_service.Dtos.ExchangeToken;

namespace identity_service.Services.Interfaces;

public interface IExchangeTokenService
{
    string GenerateExchangeCode(
            Guid userId,
            Guid systemId,
            Guid sessionId,
            string? ipAddress,
            string? userAgent
        );

    Task<ExchangeResponseDto> ExchangeCode(
            string exchangeCode,
            Guid systemId,
            string clientSecret
        );
}
