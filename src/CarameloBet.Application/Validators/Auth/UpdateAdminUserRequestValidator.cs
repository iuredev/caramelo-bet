using CarameloBet.Application.DTOs.Auth;
using FluentValidation;

namespace CarameloBet.Application.Validators.Auth;

public class UpdateAdminUserRequestValidator : AbstractValidator<UpdateAdminUserRequest>
{
    private static readonly string[] AllowedStatuses = ["active", "blocked", "deleted"];

    public UpdateAdminUserRequestValidator()
    {
        RuleFor(input => input.Name)
            .NotEmpty()
            .MaximumLength(50)
            .MinimumLength(3);

        RuleFor(input => input.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(input => input.Status)
            .NotEmpty()
            .Must(status => AllowedStatuses.Contains(status))
            .WithMessage("Status must be active, blocked, or deleted");
    }
}
