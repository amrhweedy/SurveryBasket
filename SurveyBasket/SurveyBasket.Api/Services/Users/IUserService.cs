using SurveyBasket.Api.Contracts.Users;

namespace SurveyBasket.Api.Services.Users;

public interface IUserService
{

    Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> GetUserAsync(string id);
    Task<Result<UserResponse>> AddUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> ToggleIsDisabledStatusAsync(string id);
    Task<Result> UnLockUserAsync(string id);
    Task<Result<UserProfileResponse>> GetProfileAsync(string userId);

    // Task<Result<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request);

    Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
