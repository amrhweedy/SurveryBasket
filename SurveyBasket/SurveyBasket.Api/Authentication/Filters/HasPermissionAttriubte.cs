namespace SurveyBasket.Api.Authentication.Filters;

public class HasPermissionAttribute(string permission) : AuthorizeAttribute(policy: permission)
{
}

// we pass the permission in the constructor to the base constructor of AuthorizeAttribute as a policy
// this means that our permissions are treated as a policy in the authorization middleware
// and we need to specify how this policy should be handled    