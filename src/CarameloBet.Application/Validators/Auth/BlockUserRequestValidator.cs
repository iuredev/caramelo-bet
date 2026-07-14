using CarameloBet.Application.DTOs.Auth;
using FluentValidation;

namespace CarameloBet.Application.Validators.Auth;

public class BlockUserRequestValidator : AbstractValidator<BlockUserRequest>
{
    public BlockUserRequestValidator()
    {
        RuleFor(input => input.Reason)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(input => input.ExpiresAt)
            .Must(expiresAt => !expiresAt.HasValue || expiresAt.Value > DateTimeOffset.UtcNow)
            .WithMessage("Block expiration must be in the future");
    }
}
