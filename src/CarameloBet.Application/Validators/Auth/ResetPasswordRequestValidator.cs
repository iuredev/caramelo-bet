using CarameloBet.Application.DTOs.Auth;
using FluentValidation;

namespace CarameloBet.Application.Validators.Auth;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .ApplyPasswordPolicy();
    }
}
