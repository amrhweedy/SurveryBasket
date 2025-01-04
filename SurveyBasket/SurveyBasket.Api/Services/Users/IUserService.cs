using SurveyBasket.Api.Contracts.Users;

namespace SurveyBasket.Api.Services.Users;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetProfileAsync(string userId);

    // Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request);

    Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);


    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
