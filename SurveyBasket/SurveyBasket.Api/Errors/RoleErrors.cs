
namespace SurveyBasket.Api.Errors;

public static class RoleErrors
{
    public static readonly Error RoleNotFound =
        new("Role.RoleNotFound", "Role is not found", StatusCodes.Status404NotFound);

    public static readonly Error InvalidPermissions =
        new("Role.InvalidPermissions", "Invalid Permissions", StatusCodes.Status400BadRequest);

    //public static readonly Error InvalidRefreshToken =
    //    new("User.InvalidRefreshToken", "Invalid refresh token", StatusCodes.Status401Unauthorized);

    public static readonly Error DuplicatedRole =
        new("Role.DuplicatedRole", "Another role with the same name is already exists", StatusCodes.Status409Conflict);

    //public static readonly Error EmailNotConfirmed =
    //    new("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Status401Unauthorized);


    //public static readonly Error InvalidCode =
    //    new("User.InvalidCode", "Invalid code", StatusCodes.Status401Unauthorized);

    //public static readonly Error DuplicatedConfirmation =
    //    new("User.DuplicatedConfirmation", "Email is already confirmed", StatusCodes.Status400BadRequest);

}
