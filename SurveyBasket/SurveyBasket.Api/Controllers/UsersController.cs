namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.GetUsers)]
    public async Task<IActionResult> GetUser([FromRoute] string id)
    {
        var result = await _userService.GetUserAsync(id);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost]
    [HasPermission(Permissions.AddUsers)]
    public async Task<IActionResult> AddUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.AddUserAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetUser), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }


    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateUsers)]
    public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateUserAsync(id, request, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{id}/toggle_isDisabled")]
    [HasPermission(Permissions.UpdateUsers)]
    public async Task<IActionResult> ToggleIsDisabled([FromRoute] string id)
    {
        var result = await _userService.ToggleIsDisabledStatusAsync(id);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{id}/unlock")]
    [HasPermission(Permissions.UpdateUsers)]
    public async Task<IActionResult> UnlockUser([FromRoute] string id)
    {
        var result = await _userService.UnLockUserAsync(id);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }



}
