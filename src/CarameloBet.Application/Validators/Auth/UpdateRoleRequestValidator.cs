using CarameloBet.Application.DTOs.Auth;
using FluentValidation;

namespace CarameloBet.Application.Validators.Auth;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(input => input.Name)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[a-z_]+$")
            .WithMessage("Role name must use lowercase letters and underscores only");

        RuleFor(input => input.Description)
            .MaximumLength(255);
    }
}
