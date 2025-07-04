﻿namespace SurveyBasket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    [HttpGet]
    [HasPermission(Permissions.GetRoles)]
    public async Task<IActionResult> GetAllRoles([FromQuery] bool includeDeleted, CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetAllAsync(includeDeleted, cancellationToken);
        return Ok(roles);
    }


    [HttpGet("{id}")]
    [HasPermission(Permissions.GetRoles)]
    public async Task<IActionResult> GetRoleById([FromRoute] string id)
    {
        var result = await _roleService.GetAsync(id);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPost]
    [HasPermission(Permissions.AddRoles)]
    public async Task<IActionResult> AddRole([FromBody] RoleRequest request)
    {
        var result = await _roleService.AddAsync(request);
        return result.IsSuccess ? CreatedAtAction(nameof(GetRoleById), new { id = result.Value.Id }, result.Value) : result.ToProblem();
    }



    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateRoles)]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleRequest request)
    {
        var result = await _roleService.UpdateAsync(id, request);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }



    [HttpPut("{id}/toggle-delete-status")]
    [HasPermission(Permissions.UpdateRoles)]
    public async Task<IActionResult> ToggleDeleteStatus([FromRoute] string id)
    {
        var result = await _roleService.ToggleDeleteStatusAsync(id);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
