using CarameloBet.Application.DTOs.Auth;
using FluentValidation;

namespace CarameloBet.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(input => input.Name).NotEmpty().WithMessage("Name is required")
        .MaximumLength(50).WithMessage("Name must not exceed 50 characters")
        .MinimumLength(3).WithMessage("Name must be at least 3 characters");

        RuleFor(input => input.Email).NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");

        RuleFor(input => input.Password)
            .ApplyPasswordPolicy();

    }
}
