using System;
using FluentValidation;
using identity_service.Dtos.User;

namespace identity_service.Validations.User;

public class ChangePasswordDtoValidator: AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("La contraseña actual es obligatoria.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
            .MinimumLength(6).WithMessage("La nueva contraseña debe tener al menos 6 caracteres.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Por favor confirme la nueva contraseña.")
            .Equal(x => x.NewPassword).WithMessage("La nueva contraseña y la confirmación no coinciden.");
    }
}
