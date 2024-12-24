using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Contracts.Authentication;
using SurveyBasket.Api.Errors;
using System.Security.Cryptography;

namespace SurveyBasket.Api.Services.Authentication;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;

    private readonly int _RefreshTokenExpirationInDays = 14;

    public async Task<Result<AuthResponse>> GetTokenAsync(string Email, string Password, CancellationToken cancellationToken = default)
    {
        // get user
        var user = await _userManager.FindByEmailAsync(Email);
        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        // check password

        var isValidPassword = await _userManager.CheckPasswordAsync(user, Password);
        if (!isValidPassword)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        // generate jwt token

        (string token, int expiresIn) = _jwtProvider.GenerateToken(user);

        // generate refresh token

        var refreshToken = GenerateRefreshToke();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_RefreshTokenExpirationInDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresOn = refreshTokenExpiration
        });

        await _userManager.UpdateAsync(user);

        // return AuthResponse

        var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn * 60, refreshToken, refreshTokenExpiration);

        return Result.Success<AuthResponse>(response);

    }


    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(Token);

        if (userId is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == RefreshToken && x.IsActive);
        if (userRefreshToken is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);


        userRefreshToken.RevokedOn = DateTime.UtcNow; // Revoke the old refresh token because the refresh token is used once time only 


        // generate a new token and refresh token

        (string newToken, int expiresIn) = _jwtProvider.GenerateToken(user);

        // generate refresh token

        var newRefreshToken = GenerateRefreshToke();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_RefreshTokenExpirationInDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = refreshTokenExpiration
        });

        await _userManager.UpdateAsync(user);

        // return AuthResponse

        var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, newToken, expiresIn * 60, newRefreshToken, refreshTokenExpiration);

        return Result.Success<AuthResponse>(response);
    }

    public async Task<Result> RevokeTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(Token);

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == RefreshToken && x.IsActive);
        if (userRefreshToken is null)
            return Result.Failure(UserErrors.InvalidRefreshToken);


        userRefreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }


    public async Task<Result> AddUserAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
            return Result.Success();

        return Result.Failure(new Error(result.Errors.FirstOrDefault().Code , result.Errors.FirstOrDefault().Description));
     }


    private string GenerateRefreshToke()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    
}
