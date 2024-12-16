
namespace SurveyBasket.Api.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MinAgeAttribute(int minAge) : ValidationAttribute
{
    private readonly int _minAge = minAge;
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not null)
        {
            var date = (DateTime)value;
            if (DateTime.Today < date.AddYears(_minAge))
            {
                return new ValidationResult($"invalid {validationContext.DisplayName}, age must be at least {_minAge} years old");
            }

        }

        return ValidationResult.Success;
    }
}
