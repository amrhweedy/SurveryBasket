using System.Collections.Generic;
using Serilog.Filters;
using SurveyBasket.Api.Contracts.Users;
using SurveyBasket.Api.Services.Roles;
namespace SurveyBasket.Api.Services.Users;

public class UserService(UserManager<ApplicationUser> userManager, IRoleService roleService, ApplicationDbContext context) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IRoleService _roleService = roleService;
    private readonly ApplicationDbContext _context = context;


    public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // get users with their roles
        #region join using fluent method using GroupBy object 


        //var x = await (from user in _context.Users
        //               join userRoles in _context.UserRoles
        //               on user.Id equals userRoles.UserId
        //               join role in _context.Roles
        //               on userRoles.RoleId equals role.Id into roles // role is IEnumerable<ApplicationRole> When you use join ... into roles, you are telling LINQ:“For each user, give me all the matching roles as a group(list).”
        //               where roles.Any(x => x.Name != DefaultRoles.Member)
        //               select new
        //               {
        //                   user.Id,
        //                   user.FirstName,
        //                   user.LastName,
        //                   user.Email,
        //                   user.IsDisabled,
        //                   roles = roles.Select(x => x.Name).ToList()
        //               })
        //               .GroupBy(u => new {u.Id, u.FirstName , u.LastName,u.Email,u.IsDisabled})
        //               .Select(g=> new UserResponse (
        //                   g.Key.Id,
        //                   g.Key.FirstName,
        //                   g.Key.LastName,
        //                   g.Key.Email,
        //                   g.Key.IsDisabled,
        //                   g.SelectMany(x=>x.roles).Distinct().ToList()
                                                   
        //                   )).ToListAsync(cancellationToken);
    
                   
         


        return await _context.Users.Join(
           _context.UserRoles,
           u => u.Id,
           ur => ur.UserId,
           (user, userRole) => new { user, userRole }
           ).Join(
           _context.Roles,
           x => x.userRole.RoleId,
           r => r.Id,
           (x, role) => new { x.user, role }
           )
           .Where(x => x.role.Name != DefaultRoles.Member)
           .GroupBy(x => new { x.user.Id, x.user.FirstName, x.user.LastName, x.user.Email, x.user.IsDisabled })
           .Select(g => new UserResponse(
                  g.Key.Id,
                  g.Key.FirstName,
                  g.Key.LastName,
                  g.Key.Email!,
                  g.Key.IsDisabled,
                  g.Select(x => x.role.Name!).ToList()
               )).ToListAsync(cancellationToken);

        #endregion


        #region join using fluent method using GroupBy userId 


        //return await _context.Users.Join(
        //   _context.UserRoles,
        //   u => u.Id,
        //   ur => ur.UserId,
        //   (user, userRole) => new { user, userRole }
        //   ).Join(
        //   _context.Roles,
        //   x => x.userRole.RoleId,
        //   r => r.Id,
        //   (x, role) => new { x.user, role }
        //   )
        //   .Where(x => x.role.Name != DefaultRoles.Member)
        //   .GroupBy(x => x.user.Id)
        //   .Select(g => new UserResponse(
        //          g.Key,
        //          g.Select(x => x.user.FirstName).FirstOrDefault()!,
        //          g.Select(x => x.user.LastName).FirstOrDefault()!,
        //          g.Select(x => x.user.Email).FirstOrDefault()!,
        //          g.Select(x => x.user.IsDisabled).FirstOrDefault()!,
        //          g.Select(x => x.role.Name!)
        //       ))
        //   .ToListAsync(cancellationToken);

        #endregion

        #region join using query syntax using GroupBy object

        //return await (from u in _context.Users
        //              join ur in _context.UserRoles
        //              on u.Id equals ur.UserId
        //              join r in _context.Roles
        //              on ur.RoleId equals r.Id
        //              where r.Name != DefaultRoles.Member
        //              select new { u.Id, u.FirstName, u.LastName, u.Email, u.IsDisabled, r.Name }
        //                )
        //                .GroupBy(x => new { x.Id, x.FirstName, x.LastName, x.Email, x.IsDisabled })
        //                .Select(g => new UserResponse(
        //                    g.Key.Id,
        //                    g.Key.FirstName,
        //                    g.Key.LastName,
        //                    g.Key.Email,
        //                    g.Key.IsDisabled,
        //                    g.Select(x => x.Name)
        //                    )).ToListAsync(cancellationToken);

        #endregion

        #region join using query syntax using GroupBy  userId
        //return await (from u in _context.Users
        //                    join ur in _context.UserRoles
        //                    on u.Id equals ur.UserId
        //                    join r in _context.Roles
        //                    on ur.RoleId equals r.Id
        //                    where r.Name != DefaultRoles.Member
        //                    select new { u.Id, u.FirstName, u.LastName, u.Email, u.IsDisabled, r.Name }
        //                   )
        //                   .GroupBy(x => x.Id)
        //                   .Select(g => new UserResponse(
        //                       g.Key,
        //                       g.Select(x => x.FirstName).FirstOrDefault()!,
        //                       g.Select(x => x.LastName).FirstOrDefault()!,
        //                       g.Select(x => x.Email).FirstOrDefault()!,
        //                       g.Select(x => x.IsDisabled).FirstOrDefault()!,
        //                       g.Select(x => x.Name)
        //                       )).ToListAsync(cancellationToken);


        #endregion

    }

    public async Task<Result<UserResponse>> GetUserAsync(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var roles = await _userManager.GetRolesAsync(user);  // return the roles name

        //var response = new UserResponse(
        //    user.Id,
        //    user.FirstName,
        //    user.LastName,
        //    user.Email!,
        //    user.IsDisabled,
        //    roles
        //    );

        // or 

        var response = (user, roles).Adapt<UserResponse>();  // use mapster but we need to make configuration, the source is more than one objects  

        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> AddUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var isEmailExist = await _userManager.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (isEmailExist)
            return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);

        var allowedRoles = await _roleService.GetAllAsync(cancellationToken: cancellationToken);

        if (request.Roles.Except(allowedRoles.Select(r => r.Name)).Any())
            return Result.Failure<UserResponse>(UserErrors.InvalidRoles);

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            EmailConfirmed = true,
        };

        // createAsync => it fills the columns NormalizedEmail and NormalizedUserName automatically
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRolesAsync(user, request.Roles);
            return Result.Success(new UserResponse(user.Id, user.FirstName, user.LastName, user.Email, user.IsDisabled, request.Roles));
        }
        var error = result.Errors.First();
        return Result.Failure<UserResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }


    public async Task<Result> UpdateUserAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var isEmailExist = await _userManager.Users.AnyAsync(u => u.Email == request.Email && u.Id != id, cancellationToken);

        if (isEmailExist)
            return Result.Failure(UserErrors.DuplicatedEmail);

        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);


        var allowedRoles = await _roleService.GetAllAsync(cancellationToken: cancellationToken);

        if (request.Roles.Except(allowedRoles.Select(r => r.Name)).Any())
            return Result.Failure(UserErrors.InvalidRoles);


        // update the user properties with the value of the propertes which in the request like firstname,LastName , email 
        // we make configuration to update the UserName and make it like the Email in MappingConfiguration File
        user = request.Adapt(user);

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            // we will remove all roles for this user and reassing the request roles to the user
            // we use the ExecuteDeleteAsync to make one query to the database and remove the roles for this user without select them first
            await _context.UserRoles
                 .Where(ur => ur.UserId == id)
                 .ExecuteDeleteAsync(cancellationToken);

            await _userManager.AddToRolesAsync(user, request.Roles);

            return Result.Success();
        }

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ToggleIsDisabledStatusAsync(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        user.IsDisabled = !user.IsDisabled;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UnLockUserAsync(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }



    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
    {

        // I dont make validation foe the user if he is not exist or not 
        // because the User is not responsible for sending the Id
        // the user makes login so if the user needs to get his profile, it will call the endpoint which is responsible for getting the profile
        // and the user must be authenticated so the valid token must be send with the request to get the profile
        // so i will get the userId from the token 

        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync();

        return Result.Success(user);
    }

    #region Update profile but with bad performance

    //public async Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    //{
    //    var user = await _userManager.FindByIdAsync(userId);

    //    user = request.Adapt(user); // update the firstName and lastName of the user
    //    //or 
    //    //user.FirstName = request.FirstName;
    //    //user.LastName = request.LastName;

    //    await _userManager.UpdateAsync(user!);

    //    return Result.Success(user.Adapt<UserProfileResponse>());

    //    // user = request.Adapt<ApplicationUser>() => this makes projection not update
    //    // it means that it will create object from the ApplicationUser and set the FirstName and LastName wit the values from the request
    //    // so when we return the UserProfileResponse the email and UserName will be null
    //}
    #endregion

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {

        // this code does not select the user from the database and then update the FirstName and LastName but it will execute an updated SQL statement in the database directly on the users table
        // and it updates only the FirstName and LastName of the user not all the properties of the user
        await _userManager.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters =>
                        setters
                               .SetProperty(u => u.FirstName, request.FirstName)
                               .SetProperty(setters => setters.LastName, request.LastName)
            );

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        // if the request.currentPassword is not the same as the password in the database this method will return error
        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));

    }


}