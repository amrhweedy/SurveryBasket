namespace SurveyBasket.Api.Services.Roles;

public class RoleService(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context) : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<RoleResponse>> GetAllAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return await _roleManager.Roles
            .Where(r => !r.IsDefault && (!r.IsDeleted || includeDeleted))
            .Select(r => new RoleResponse
            (
                 r.Id,
                 r.Name!,
                 r.IsDeleted
            )).ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<Result<RoleDetailResponse>> GetAsync(string id)
    {
        if (await _roleManager.FindByIdAsync(id) is not { } role)
            return Result.Failure<RoleDetailResponse>(RoleErrors.RoleNotFound);

        var permissions = await _roleManager.GetClaimsAsync(role);


        var response = new RoleDetailResponse(
            role.Id,
            role.Name!,
            role.IsDeleted,
            permissions.Select(p => p.Value)
            );

        return Result.Success(response);

    }


    public async Task<Result<RoleDetailResponse>> AddAsync(RoleRequest request)
    {
        var roleIsExist = await _roleManager.RoleExistsAsync(request.Name);
        if (roleIsExist)
            return Result.Failure<RoleDetailResponse>(RoleErrors.DuplicatedRole);

        var allowedPermissions = Permissions.GetAllPermissions();

        if (request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailResponse>(RoleErrors.InvalidPermissions);

        var role = new ApplicationRole
        {
            Name = request.Name,
            NormalizedName = request.Name.ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),

        };

        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            var permissions = request.Permissions
                .Select(p => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = p,
                    RoleId = role.Id
                });


            await _context.RoleClaims.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();

            return Result.Success(new RoleDetailResponse(role.Id, role.Name, role.IsDeleted, request.Permissions));
        }

        var error = result.Errors.First();

        return Result.Failure<RoleDetailResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateAsync(string id, RoleRequest request)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
            return Result.Failure(RoleErrors.RoleNotFound);

        var roleIsExist = await _roleManager.Roles.AnyAsync(r => r.Name == request.Name & r.Id != id);
        if (roleIsExist)
            return Result.Failure<RoleDetailResponse>(RoleErrors.DuplicatedRole);


        var allowedPermissions = Permissions.GetAllPermissions();

        if (request.Permissions.Except(allowedPermissions).Any())
            return Result.Failure<RoleDetailResponse>(RoleErrors.InvalidPermissions);

        role.Name = request.Name;

        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            // permissions for this role in the database
            var currentPermissions = await _context.RoleClaims
                .Where(rc => rc.RoleId == id && rc.ClaimType == Permissions.Type)
                .Select(rc => rc.ClaimValue)
                .ToListAsync();


            // if there are permissions in the request not exist in the permissions which in the database we will add them 
            var newPermissions = request.Permissions.Except(currentPermissions)
                .Select(p => new IdentityRoleClaim<string>
                {
                    ClaimType = Permissions.Type,
                    ClaimValue = p,
                    RoleId = id
                });

            // if there are permissions in the database not exist in the permissions which in the request we will remove them
            var removedPermissions = currentPermissions.Except(request.Permissions);

            await _context.RoleClaims
                .Where(rc => rc.RoleId == id && removedPermissions.Contains(rc.ClaimValue))
                .ExecuteDeleteAsync();


            await _context.RoleClaims.AddRangeAsync(newPermissions);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure<RoleDetailResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ToggleDeleteStatusAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
            return Result.Failure(RoleErrors.RoleNotFound);

        role.IsDeleted = !role.IsDeleted;

        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }
}
