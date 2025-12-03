using System;
using FluentValidation;
using identity_service.Dtos.AuthAuditLog;

namespace identity_service.Validations.AuthAuditLog;

public class AuthAuditLogForCreateValidator : AbstractValidator<AuthAuditLogForCreateDto>
{
    public AuthAuditLogForCreateValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.ProviderName)
            .NotEmpty().WithMessage("ProviderName is required.");

        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("EventType is required.");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IpAddress is required.");
    }
}
