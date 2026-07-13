using CarameloBet.Application.DTOs.Auth;
using FluentValidation;

namespace CarameloBet.Application.Validators.Auth;

public class ChangeCurrentUserPasswordRequestValidator : AbstractValidator<ChangeCurrentUserPasswordRequest>
{
    public ChangeCurrentUserPasswordRequestValidator()
    {
        RuleFor(input => input.CurrentPassword)
            .NotEmpty();

        RuleFor(input => input.NewPassword)
            .ApplyPasswordPolicy();
    }
}
