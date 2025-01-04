using SurveyBasket.Api.Contracts.Users;

namespace SurveyBasket.Api.Services.Users;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;


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
