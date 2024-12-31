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

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        // placeholder
        _logger.LogInformation("logging with email : {email} and password : {password}", request.Email, request.Password);

        // string interpolation
        // _logger.LogInformation($"logging with email : {request.Email} and password : {request.Password}");
        var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
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
}



