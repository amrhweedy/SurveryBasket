using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Contracts.Authentication;
using SurveyBasket.Api.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace SurveyBasket.Api.Services.Authentication;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtProvider jwtProvider,
    ILogger<AuthService> logger,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor
    ) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
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

        var result = await _signInManager.PasswordSignInAsync(user, Password, false, false);

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


        return Result.Failure<AuthResponse>(result.IsNotAllowed ? UserErrors.EmailNotConfirmed : UserErrors.InvalidCredentials);
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
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);  //  this method creates a unique code
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));  //  encode the code to add the code in th url as a query string , the code will be a part of the url or link which will be send to the user

            _logger.LogInformation("confirmation code :{code}", code);

            // TODO => send email


            await SendConfirmationEmail(user, code);

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

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        var code = request.Code;

        try
        {
            // decode Decodes the Base64 URL-encoded string to its raw byte array then Converts the byte array back into a readable string
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
        }
        catch (FormatException)
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        // ConfirmEmailAsync => validate the code with the user then if the code is valid
        // it will update the emailConfirmed field to true in the database for this user to enable the user make login
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }

    // if the email is not send to the user because of any problem  the user will use this method 
    // we will send the email again
    public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Result.Success();     // if there is no user we dont send the email but we return success 


        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("confirmation code :{code}", code);

        await SendConfirmationEmail(user, code);

        return Result.Success();

    }


    public async Task<Result> SendResetPasswordCodeAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result.Success();
            // this is misleading for the users, because we dont need to know the user if this email exists or not because maybe the user is hacker and try to get the password of another user
            // so if this hacker send invalid email and I respond with error this email not exist then the hacker will try to send another email and so on 
            // so if the email does not exist i will respond with success and i will not send email to the user because i dont want to expose the fact that this email does not exist
        }

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailNotConfirmed);

        // generate a verification code 


        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Reset Password Code :{code}", code);

        await SendResetPasswordEmail(user, code);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCode);

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));

            // ResetPasswordAsync =>validates the code (reset password token). If the token is invalid (e.g., expired, tampered, or mismatched), the method will return an IdentityResult with an error indicating that the token is invalid.
            // it validated the code (correctly formatted, tied to the user, and not expired ) 
            // and The code must match the one generated and sent during the password reset process.

            result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch (FormatException)
        {
            // it enters here when the code is not corrected format
            // but if the code is correctly formatted but it is not valid (e.g., expired, tampered, or mismatched), it will not enter inside the catch but  the method will return an IdentityResult with an error indicating that the token is invalid.
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
    }


    private string GenerateRefreshToke()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private async Task SendConfirmationEmail(ApplicationUser user, string code)
    {
        // origin => the url of the client (frontend) like http://localhost:4200
        // the clint must tell me the route of the confirmation page (auth/emailconfirmatinon), when the user will click on the link it will redirect to this url $"{origin}/auth/emailConfirmatin?userId={user.Id}&code={code}"

        var origin = _httpContextAccessor.HttpContext!.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
            new Dictionary<string, string>
            {
                {"{{name}}" , user.FirstName },
                { "{{action_url}}" , $"{origin}/auth/emailConfirmatin?userId={user.Id}&code={code}"}
            }
      );

        // Enqueue => This method is used to schedule a background job that should run immediately or as soon as resources allow.
        // jobs enqueued with Enqueue run only once and do not require additional triggers. the job will be executed as soon as it is added to the queue and will not be executed again automatically
        //This job will be added to the Hangfire queue and executed asynchronously, leaving the main thread free to continue handling other requests.
        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "survey basket : email confirmation", emailBody));

        await Task.CompletedTask;
    }


    private async Task SendResetPasswordEmail(ApplicationUser user, string code)
    {
        // origin => the url of the client (frontend) like http://localhost:4200
        // the clint must tell me the route of the confirmation page (auth/forgetPassword), when the user will click on the link it will redirect to this url $"{origin}/auth/forgetPassword?emial={user.Email}&code={code}"

        var origin = _httpContextAccessor.HttpContext!.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
            new Dictionary<string, string>
            {
                {"{{name}}" , user.FirstName },
                {"{{action_url}}" , $"{origin}/auth/forgetPassword?email={user.Email}&code={code}"}
            }
      );

        // Enqueue => This method is used to schedule a background job that should run immediately or as soon as resources allow.
        // jobs enqueued with Enqueue run only once and do not require additional triggers. the job will be executed as soon as it is added to the queue and will not be executed again automatically
        // This job will be added to the Hangfire queue and executed asynchronously, leaving the main thread free to continue handling other requests.
        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✔ survey basket : change password", emailBody));

        await Task.CompletedTask;
    }


}
