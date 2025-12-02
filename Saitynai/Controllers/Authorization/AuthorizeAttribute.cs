using Microsoft.AspNetCore.Authorization;
using Saitynai.Models;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
namespace Saitynai.Authorization;

public class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    const string POLICY_PREFIX = "PERMISSION_";


    public PermissionAuthorizeAttribute(string permission, string resource) =>
        Policy = $"{POLICY_PREFIX}{permission}:{resource}";


    public static (string permission, string resource)? ParsePolicy(string policyName)
    {
        if (policyName.StartsWith(POLICY_PREFIX))
        {
            var parts = policyName.Substring(POLICY_PREFIX.Length).Split(':');
            if (parts.Length == 2)
            {
                return (parts[0], parts[1]);
            }
        }
        return null;
    }
}


public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public string Resource { get; }

    public PermissionRequirement(string permission, string resource)
    {
        Permission = permission;
        Resource = resource;
    }
}

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackProvider.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _fallbackProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var parsedPolicy = PermissionAuthorizeAttribute.ParsePolicy(policyName);
        if (parsedPolicy.HasValue)
        {
            var (permission, resource) = parsedPolicy.Value;
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(permission, resource));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return _fallbackProvider.GetPolicyAsync(policyName);
    }
}