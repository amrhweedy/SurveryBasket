using Microsoft.AspNetCore.WebUtilities;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Contracts.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace SurveyBasket.Api.Services.Authentication;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtProvider jwtProvider,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly ILogger<AuthService> _logger = logger;

    private readonly int _RefreshTokenExpirationInDays = 14;

    #region the normal login without verification code and Email Confirmation
    //public async Task<Result<AuthResponse>> GetTokenAsync(string Email, string Password, CancellationToken cancellationToken = default)
    //{
    //    // get user
    //    var user = await _userManager.FindByEmailAsync(Email);
    //    if (user is null)
    //        return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

    //    // check password

    //    var isValidPassword = await _userManager.CheckPasswordAsync(user, Password);
    //    if (!isValidPassword)
    //        return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

    //    // generate jwt token

    //    (string token, int expiresIn) = _jwtProvider.GenerateToken(user);

    //    // generate refresh token

    //    var refreshToken = GenerateRefreshToke();
    //    var refreshTokenExpiration = DateTime.UtcNow.AddDays(_RefreshTokenExpirationInDays);

    //    user.RefreshTokens.Add(new RefreshToken
    //    {
    //        Token = refreshToken,
    //        ExpiresOn = refreshTokenExpiration
    //    });

    //    await _userManager.UpdateAsync(user);

    //    // return AuthResponse

    //    var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expiresIn * 60, refreshToken, refreshTokenExpiration);

    //    return Result.Success<AuthResponse>(response);

    //}

    #endregion


    // login with Verification Code and Email Confirmation 
    // we here check if email is confirmed or not using PasswordSignInAsync method 
    public async Task<Result<AuthResponse>> GetTokenAsync(string Email, string Password, CancellationToken cancellationToken = default)
    {
        // get user
        var user = await _userManager.FindByEmailAsync(Email);

        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        // check password

        var result = await _signInManager.PasswordSignInAsync(user, Password, false,false);

        if (result.Succeeded)
        {
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

        
        return Result.Failure<AuthResponse>(result.IsNotAllowed? UserErrors.EmailNotConfirmed : UserErrors.InvalidCredentials);
    }


    #region the normal register , creating a jwt token and refresh token after the user make the registration
    //public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    //{
    //    //1- check for existing email
    //    var isExistingEmail = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

    //    if (isExistingEmail)
    //        return Result.Failure<AuthResponse>(UserErrors.DuplicatedEmail);

    //    //2- create user

    //    var user = new ApplicationUser
    //    {
    //        Email = request.Email,
    //        UserName = request.Email,
    //        FirstName = request.FirstName,
    //        LastName = request.LastName,
    //    };

    //    var result = await _userManager.CreateAsync(user, request.Password);

    //    if (result.Succeeded)
    //    {
    //        // generate a new token and refresh token

    //        (string newToken, int expiresIn) = _jwtProvider.GenerateToken(user);

    //        // generate refresh token

    //        var newRefreshToken = GenerateRefreshToke();
    //        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_RefreshTokenExpirationInDays);

    //        user.RefreshTokens.Add(new RefreshToken
    //        {
    //            Token = newRefreshToken,
    //            ExpiresOn = refreshTokenExpiration
    //        });

    //        await _userManager.UpdateAsync(user);

    //        // return AuthResponse

    //        var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, newToken, expiresIn * 60, newRefreshToken, refreshTokenExpiration);

    //        return Result.Success<AuthResponse>(response);

    //    }

    //    var error = result.Errors.First();

    //    return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    //}

    #endregion

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        //1- check for existing email
        var isExistingEmail = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (isExistingEmail)
            return Result.Failure<AuthResponse>(UserErrors.DuplicatedEmail);

        //2- create user

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            // generate a verification code
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("confirmation code :{code}" , code);

            // TODO => send email
            return Result.Success();

        }

        var error = result.Errors.First();

        return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
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


    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
         var user = await _userManager.FindByIdAsync(request.UserId);

         if (user is null)
             return Result.Failure(UserErrors.InvalidCode);

         if(user.EmailConfirmed)
             return Result.Failure(UserErrors.DuplicatedConfirmation);

        var code = request.Code;

        try
        {
          // decode Decodes the Base64 URL-encoded string to its raw byte array then Converts the byte array back into a readable string
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
        }
        catch(FormatException)
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        var result = await _userManager.ConfirmEmailAsync(user, code); // this method update the emailConfirmed field to true in the database for this user to enable the user make login

        if(result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        
     }


    private string GenerateRefreshToke()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

   
}
