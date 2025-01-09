namespace SurveyBasket.Api.Contracts.Roles;

public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .Length(3, 200);

        RuleFor(r => r.Permissions)
            .NotEmpty()
            .Must(p => p.Distinct().Count() == p.Count)
            .WithMessage("you can't add duplicated permissions for the same role")
            .When(r => r.Permissions != null);
    }
}
