namespace SurveyBasket.Api.Contracts.Validations;

public class StudentValidator : AbstractValidator<Student>
{
    public StudentValidator()
    {
        RuleFor(x => x.DateOfBirth)
            .Must(x => DateTime.Today > x!.Value.AddYears(18))
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("invalid {PropertyName} , age must be greater than 18");

        // this validation will be applied only if DateOfBirth is not null, because if the when does not exist and the date of birth is null it will throw an exception


    }
}
