using SurveyBasket.Api.Contracts.Authentication;

namespace SurveyBasket.Api.Services.Authentication;

public interface IAuthService
{
    Task<Result<AuthResponse>> GetTokenAsync(string Email, string Password, CancellationToken cancellationToken = default);

    //Task<OneOf<AuthResponse, Error>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default); // we can use OneOf package instead of Result class

    // Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);  // create jwt token and refresh token after the user register
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> GetRefreshTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default);

    Task<Result> RevokeTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default);

    Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);

    Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request);

}
