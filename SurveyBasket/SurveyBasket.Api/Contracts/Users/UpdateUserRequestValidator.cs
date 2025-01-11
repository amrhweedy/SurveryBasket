namespace SurveyBasket.Api.Contracts.Users;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(3, 100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(3, 100);


        RuleFor(x => x.Roles)
            .NotEmpty()
            .Must(roles => roles.Distinct().Count() == roles.Count())
            .WithMessage("you can't add duplicated roles for the same user")
            .When(x => x.Roles != null);
    }
}
