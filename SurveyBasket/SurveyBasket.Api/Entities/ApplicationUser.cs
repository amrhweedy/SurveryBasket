
namespace SurveyBasket.Api.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;   // this means this prop in not-nullable if the firstName does not have a value it will take a default value ""
    public string LastName { get; set; } = string.Empty;

    public List<RefreshToken> RefreshTokens { get; set; } = [];

}

//  public string? FirstName { get; set; }    => this means the firstName is nullable , if the firstName does not have a value it will take a default value null


