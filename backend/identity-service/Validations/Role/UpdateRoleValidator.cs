using System;
using FluentValidation;
using identity_service.Dtos.Role;

namespace identity_service.Validations.Role;

public class UpdateRoleValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters.");

        RuleFor(x => x.SystemId)
            .NotEmpty().WithMessage("SystemId is required.");        
    }
}
