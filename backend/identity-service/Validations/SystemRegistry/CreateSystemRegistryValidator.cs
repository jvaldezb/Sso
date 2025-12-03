using System;
using FluentValidation;
using identity_service.Dtos.SystemRegistry;

namespace identity_service.Validations.SystemRegistry;

public class CreateSystemRegistryValidator : AbstractValidator<CreateSystemRegistryDto>
{
    public CreateSystemRegistryValidator()
    {
        RuleFor(x => x.SystemCode)
            .NotEmpty().WithMessage("SystemCode is required.")
            .Length(2, 50).WithMessage("SystemCode must be between 2 and 50 characters.");

        RuleFor(x => x.SystemName)
            .NotEmpty().WithMessage("SystemName is required.")
            .Length(2, 100).WithMessage("SystemName must be between 2 and 100 characters.");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("BaseUrl is required.")
            .Must(BeValidUrl).WithMessage("BaseUrl must be a valid URL.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Category must not exceed 50 characters.");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail))            
            .WithMessage("ContactEmail must be a valid email address.");
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
