using SurveyBasket.Api.Abstractions;

namespace SurveyBasket.Api.Errors;

public static class UserErrors
{
    public static readonly Error InvalidCredentials = new ("User.InvalidCredentials", "Invalid Email or Password");
    public static readonly Error InvalidToken = new ("User.InvalidToken", "Invalid Token");
}
