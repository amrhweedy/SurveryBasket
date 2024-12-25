namespace SurveyBasket.Api.Contracts.Questions;

public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
{
    public QuestionRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Question {PropertyName} is required")
            .Length(3, 1000)
            .WithMessage("Question {PropertyName} should be between {MinLength} and {MaxLength} characters");

        RuleFor(x => x.Answers)
            .NotNull();

        RuleFor(x => x.Answers)
            .Must(answers => answers.Count > 1)
            .WithMessage("Question should has at least 2 answers")
            .When(x => x.Answers is not null);


        // Distinct =>  returns a sequence that contains only distinct elements, it removes the duplicates
        // if the user sends the answers  [a,b,b] this answers.Distinct().Count() will return 2 because it will remove the duplicates => answers.Distinct() will return [a,b]
        // but the answers.Count will return 3
        // so it will give me this error
        RuleFor(x => x.Answers)
          .Must(answers => answers.Distinct().Count() == answers.Count)
          .WithMessage("you can not add duplicated answers for the same question")
          .When(x => x.Answers is not null);

    }
}
