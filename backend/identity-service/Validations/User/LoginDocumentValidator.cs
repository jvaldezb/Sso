using System;
using FluentValidation;
using identity_service.Dtos.User;

namespace identity_service.Validations.User;

public class LoginDocumentValidator: AbstractValidator<LoginDocumentDto>
{
    public LoginDocumentValidator()
    {
        // Tipo de documento: 'DNI', 'RUC', 'CARNET DE EXTRANJERÍA', 'PASAPORTE'
        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Tipo de documento es requerido.")
            .Must(type => type == "DNI" || type == "RUC" || type == "CARNET DE EXTRANJERÍA" || type == "PASAPORTE")
            .WithMessage("Tipo de documento inválido.");

        // Validación del número de documento pero sólo DNI
        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Número de documento es requerido.")
            .When(x => x.DocumentType == "DNI")
            .Matches(@"^\d{8}$").WithMessage("Número de DNI inválido. Debe contener exactamente 8 dígitos.");

        // Validación del numero de documento pero sólo RUC
        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Número de documento es requerido.")
            .When(x => x.DocumentType == "RUC")
            .Matches(@"^\d{11}$").WithMessage("Número de RUC inválido. Debe contener exactamente 11 dígitos.");    

        // Validación del numero de documento pero sólo Carnet de extranjería
        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Número de documento es requerido.")
            .When(x => x.DocumentType == "CARNET DE EXTRANJERÍA")
            .MaximumLength(15);

        // Validación del numero de documento pero sólo Pasaporte
        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Número de documento es requerido.")
            .When(x => x.DocumentType == "PASAPORTE")
            .MaximumLength(15);        

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password es requerido.");
    }
}
