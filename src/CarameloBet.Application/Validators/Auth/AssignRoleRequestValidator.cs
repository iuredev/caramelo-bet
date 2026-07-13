using CarameloBet.Application.DTOs.Auth;
using FluentValidation;

namespace CarameloBet.Application.Validators.Auth;

public class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
{
    public AssignRoleRequestValidator()
    {
        RuleFor(input => input.RoleId)
            .NotEmpty();
    }
}
