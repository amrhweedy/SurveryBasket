namespace SurveyBasket.Api.Contracts.Users;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    IEnumerable<string> Roles
    );

