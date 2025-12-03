using System;
using FluentValidation;
using identity_service.Dtos.Role;

namespace identity_service.Validations.Role;

public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters.");

        RuleFor(x => x.SystemId)
            .GreaterThanOrEqualTo(0).When(x => x.SystemId.HasValue)
            .WithMessage("SystemId, if provided, must be greater or equal to 0.");
    }
}
