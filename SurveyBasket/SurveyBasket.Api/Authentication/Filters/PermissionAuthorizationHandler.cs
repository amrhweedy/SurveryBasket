namespace SurveyBasket.Api.Authentication.Filters;

public class PermissionAuthorizationHandler() : AuthorizationHandler<PermissionRequirement>
{

    // context => contain the information about the user and its claims which in the token which is sent with the request
    // requirement => contain the permission which the HasPermission attribute has 
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User.Identity;
        if (user is null || !user.IsAuthenticated)
            return;

        // x.value => is the value of the claims in the token like the value of sub(user id), email , given_name , family_name , roles ,and permissions
        // x.type => is the name of the claim email , sub,  given_name , family_name , roles ,and permissions
        var hasPermission = context.User.Claims.Any(x => x.Value == requirement.Permission && x.Type == Permissions.Type);

        if (!hasPermission)
            return;

        context.Succeed(requirement);
        return;
    }
}
