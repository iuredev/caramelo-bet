using CarameloBet.Application.DTOs.Auth;
using FluentValidation;

namespace CarameloBet.Application.Validators.Auth;

public class UpdateAdminUserRequestValidator : AbstractValidator<UpdateAdminUserRequest>
{
    public UpdateAdminUserRequestValidator()
    {
        RuleFor(input => input.Name)
            .NotEmpty()
            .MaximumLength(50)
            .MinimumLength(3);

        RuleFor(input => input.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
