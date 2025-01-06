using Microsoft.Extensions.Options;

namespace SurveyBasket.Api.Authentication.Filters;

public class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    private readonly AuthorizationOptions _authorizationOptions = options.Value;
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)  // policyName comes from the HasPermission attribute the policyname is the permission
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
            return policy;


        // create a new policy
        var permissionPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        _authorizationOptions.AddPolicy(policyName, permissionPolicy);

        return permissionPolicy;
    }

}
