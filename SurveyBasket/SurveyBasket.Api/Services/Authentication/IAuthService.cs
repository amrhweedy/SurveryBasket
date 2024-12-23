using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Contracts.Authentication;

namespace SurveyBasket.Api.Services.Authentication;

public interface IAuthService
{
    Task<Result<AuthResponse>> GetTokenAsync(string Email, string Password, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> GetRefreshTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default);

    Task<bool> RevokeTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default);
}
