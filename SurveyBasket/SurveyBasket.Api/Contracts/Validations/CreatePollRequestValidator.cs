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

        RuleFor(x => x.Summary)
            .NotEmpty()
            .WithMessage("{PropertyName} is required")
            .Length(3, 1000)
            .WithMessage("{PropertyName} length must be between {MinLength} and {MaxLength}, you entered {TotalLength}");

        RuleFor(x => x.StartsAt)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));

        RuleFor(x => x.EndsAt)
            .NotEmpty();
        //    .GreaterThan(x => x.StartsAt); => this is correct but i make it using must 

        RuleFor(x => x)
             .Must(HasValidDate)
             .WithName(nameof(CreatePollRequest.EndsAt))
             .WithMessage("{PropertyName} must be greater than start date");
    }

    private bool HasValidDate(CreatePollRequest request)
    {
        return request.EndsAt > request.StartsAt;
    }
}
