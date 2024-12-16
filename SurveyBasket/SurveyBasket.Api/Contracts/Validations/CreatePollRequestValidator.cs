namespace SurveyBasket.Api.Contracts.Validations;

public class CreatePollRequestValidator : AbstractValidator<CreatePollRequest>
{
    public CreatePollRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("{PropertyName} is required")
            .Length(3, 100)
            .WithMessage("{PropertyName} length must be between {MinLength} and {MaxLength}, you entered {TotalLength}");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("{PropertyName} is required")
            .Length(3, 1000)
            .WithMessage("{PropertyName} length must be between {MinLength} and {MaxLength}, you entered {TotalLength}");
    }
}
