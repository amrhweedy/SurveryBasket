using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Contracts.Authentication;
using SurveyBasket.Api.Services.Authentication;

namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService,
    IOptions<JwtOptions> Options,
    IOptionsSnapshot<JwtOptions> optionsSnapshot,
    IOptionsMonitor<JwtOptions> optionsMonitor,
    ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IOptions<JwtOptions> _options = Options;
    private readonly IOptionsSnapshot<JwtOptions> _optionsSnapshot = optionsSnapshot;
    private readonly IOptionsMonitor<JwtOptions> _optionsMonitor = optionsMonitor;
    private readonly ILogger<AuthController> _logger = logger;



    /// <summary>
    /// allow user to login
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> return jwt token</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]   // return the response with the shape of the type in case of success or failure
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [Produces("application/json")] // the response content by defualt is text/plain so we change it to json

    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        // placeholder
        _logger.LogInformation("logging with email : {email} and password : {password}", request.Email, request.Password);

        // string interpolation
        // _logger.LogInformation($"logging with email : {request.Email} and password : {request.Password}");
        var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
    }


    #region normal register , create token and refresh token after the user registers

    //[HttpPost("register")]
    //public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    //{
    //    var result = await _authService.RegisterAsync(request, cancellationToken);

    //    return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    //}
    #endregion


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {

        var authResult = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
    }


    [HttpPut("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.RevokeTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return authResult.IsSuccess ? Ok() : authResult.ToProblem();
    }


    // after the user cliked on the link in the email to confirm the email, we will call this endpoint to confirm the email
    // to enable the user to make login

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var result = await _authService.ConfirmEmailAsync(request);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }



    // if the email is not send to the user because of any problem we will use this endpoint to resend the email to the user again

    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request)
    {
        var result = await _authService.ResendConfirmationEmailAsync(request);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }


    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
    {
        var result = await _authService.SendResetPasswordCodeAsync(request.Email);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }



    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }



    [HttpGet("TestOptionsPattern")]
    public async Task<IActionResult> Test()
    {
        var values = new
        {
            ioptionsValue = _options.Value.ExpiryMinutes,
            ioptionsSnapshotValue = _optionsSnapshot.Value.ExpiryMinutes,
            ioptionsMonitorValue = _optionsMonitor.CurrentValue.ExpiryMinutes
        };

        await Task.Delay(8000);

        var value2 = new
        {
            ioptionsValue = _options.Value.ExpiryMinutes,
            ioptionsSnapshotValue = _optionsSnapshot.Value.ExpiryMinutes,
            ioptionsMonitorValue = _optionsMonitor.CurrentValue.ExpiryMinutes
        };


        return Ok(new
        {
            values,
            value2
        });

    }


    [HttpGet("test-concurrency-rate-limiting")]
    [EnableRateLimiting("concurrency")]
    public IActionResult TestConncurrencyRateLimiting()
    {
        // we make sleep for 10 seconds because i need to send 4 requests at the same time and every request will sleep for 10 seconds before the reponse will be returned
        // so the first and second request will sleep for 10 seconds but will be executed but the third will be put in the queue unitl the first or second will be executed and then the third will be executed
        // and the fourth will be rejected and give a 429 status code (too many requests) becuuse the queue is full becuase the queue size is 1 and the rate limit is 2
        Thread.Sleep(10000);
        return Ok();
    }


    [HttpGet("test-token-bucket-limiter")]
    [EnableRateLimiting("tokenBucketLimiter")]
    public IActionResult TestTokenBucketLimiter()
    {
        // we make sleep for 10 seconds because i need to send 4 requests at the same time and every request will sleep for 10 seconds before the reponse will be returned
        // so the first and second request will sleep for 10 seconds and take the 2 tokens from the bucket and will be executed but the third will be put in the queue unitl the bucket will be full with 1 token at least and then the third will be executed
        // and the fourth will be rejected and give a 429 status code (too many requests) becuuse the queue is full becuase the queue size is 1 and the rate limit is 2
        Thread.Sleep(10000);
        return Ok();
    }


    [HttpGet("test-fixed-window-limiter")]
    [EnableRateLimiting("fixedWindowLimiter")]
    public IActionResult TestFixedWindowLimiter()
    {
        // we make sleep for 10 seconds because i need to send 4 requests at the same time and every request will sleep for 10 seconds before the reponse will be returned
        // so the first and second request will sleep for 10 seconds and executed after the 10 seconds  but the third will be put in the queue unitl the window time will be over 20 seconds and then the third will be executed
        // and the fourth will be rejected and give a 429 status code (too many requests) becuuse the queue is full becuase the queue size is 1 and the rate limit is 2
        Thread.Sleep(10000);
        return Ok();
    }


    [HttpGet("test-ip-address-limiter")]
    [EnableRateLimiting("ipLimit")]
    public IActionResult TestIpAddressLimiter()
    {
        return Ok();
    }

    [HttpGet("test-user-limiter")]
    [EnableRateLimiting("userLimit")]
    public IActionResult TestUserLimiter()
    {
        return Ok();
    }
}



