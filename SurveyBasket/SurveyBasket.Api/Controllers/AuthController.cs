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
    IOptionsMonitor<JwtOptions> optionsMonitor) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IOptions<JwtOptions> _options = Options;
    private readonly IOptionsSnapshot<JwtOptions> _optionsSnapshot = optionsSnapshot;
    private readonly IOptionsMonitor<JwtOptions> _optionsMonitor = optionsMonitor;

    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {

        var authResult = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return authResult.IsSuccess ? Ok(authResult.Value) : authResult.ToProblem();
    }



    [HttpPut("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
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



