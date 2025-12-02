using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;

namespace Saitynai.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly PostgresContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionHandler(PostgresContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {


        var userIdString = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var userId))
        {
            context.Fail(new AuthorizationFailureReason(this, "User ID not found in token."));
            return;
        }

        var roleNames = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (!roleNames.Any())
        {
            context.Fail(new AuthorizationFailureReason(this, "User has no roles."));
            return;
        }

        var roleIds = await _context.Role
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync();

        var resourceName = requirement.Resource;

         if (requirement.Permission.Equals("create", StringComparison.OrdinalIgnoreCase))
        {

        

        var hasDirectCreatePermission = await _context.RolePermission
            .AsNoTracking()
            .AnyAsync(p => roleIds.Contains(p.RoleId) &&
                           p.Permission.Name.ToLower() == "create" &&
                           p.ResourceType.Name.ToLower() == requirement.Resource.ToLower() && 
                           p.Allow == true);

        if (hasDirectCreatePermission)
        {
            context.Succeed(requirement);
            return;
        }
       
        string parentType = null;
        switch (requirement.Resource.ToLower())
        {
            case "floor":       parentType = "building";    break;
            case "point":       parentType = "floor";       break;
            case "scan":        parentType = "point";       break;
            case "accesspoint": parentType = "scan";        break;
            
        }
        
        if (!string.IsNullOrEmpty(parentType))
        {
            
            var parentId = await GetParentIdFromRequestBody(parentType + "Id");
            
            if (!parentId.HasValue)
            {
                context.Fail(new AuthorizationFailureReason(this, $"Parent ID for '{parentType}' not found in request body."));
                return;
            }

            // Now check the parent for a cascading create permission
            var parentPermission = await CheckPermissionForHierarchy(roleIds, "create", parentType, parentId.Value);

            if (parentPermission.HasValue && parentPermission.Value)
            {
                context.Succeed(requirement);
                return;
            }
        }
        
        context.Fail(new AuthorizationFailureReason(this, "Missing 'create' permission."));
        return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var resourceIdString = httpContext.GetRouteValue("id")?.ToString();
        
        if (!int.TryParse(resourceIdString, out var resourceId))
        {
            resourceIdString = httpContext.GetRouteValue($"{requirement.Resource.ToLower()}Id")?.ToString();
            if (!int.TryParse(resourceIdString, out resourceId))
            {
                context.Fail(new AuthorizationFailureReason(this, "Could not determine the resource ID from the request URL."));
                return;
            }
        }

        var permissionResult = await CheckPermissionForHierarchy(roleIds, requirement.Permission, resourceName, resourceId);

        if (permissionResult.HasValue && permissionResult.Value)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, $"Permission '{requirement.Permission}' denied for resource '{resourceName}' with ID '{resourceId}'."));
        }
    }

    private async Task<bool?> CheckPermissionForHierarchy(List<int> roleIds, string permissionName, string resourceName, int resourceId)
    {
        string currentResourceName = resourceName;
        int currentResourceId = resourceId;

        // Pre-convert the parameters to lower case once, outside the loop.
        string permissionNameLower = permissionName.ToLower();
        string resourceNameLower = resourceName.ToLower();

        while (true)
        {
            string currentResourceNameLower = currentResourceName.ToLower();
            
            var directRule = await _context.RolePermission
                .AsNoTracking()
                .Include(p => p.ResourceType)
                .Include(p => p.Permission)
                .FirstOrDefaultAsync(p =>
                    roleIds.Contains(p.RoleId) &&
                    p.Permission.Name.ToLower() == permissionNameLower && // CORRECTED
                    p.ResourceType.Name.ToLower() == currentResourceNameLower && // CORRECTED
                    p.ResourceId == currentResourceId);

            if (directRule != null)
            {
                if (currentResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase) || directRule.Cascade)
                {
                    return directRule.Allow;
                }
                else
                {
                    return null;
                }
            }

            (string parentName, int parentId)? parentInfo = await GetParentInfo(currentResourceName, currentResourceId);

            if (parentInfo.HasValue)
            {
                currentResourceName = parentInfo.Value.parentName;
                currentResourceId = parentInfo.Value.parentId;
            }
            else
            {
                return null;
            }
        }
    }



    private async Task<(string parentName, int parentId)?> GetParentInfo(string resourceName, int resourceId)
    {
        switch (resourceName.ToLower())
        {
            case "floor":
                var floor = await _context.Floor.AsNoTracking().FirstOrDefaultAsync(f => f.Id == resourceId);
                return floor != null ? ("Building", floor.BuildingId) : null;
            
            case "point":
                var point = await _context.Point.AsNoTracking().FirstOrDefaultAsync(f => f.Id == resourceId);
                return point != null ? ("Floor", point.FloorId) : null;
            
            case "scan":
                 var scan = await _context.Scan.AsNoTracking().FirstOrDefaultAsync(f => f.Id == resourceId);
                 return scan != null ? ("Point", scan.PointId) : null;

            case "accesspoint":
             var ap = await _context.AccessPoint.AsNoTracking().FirstOrDefaultAsync(f => f.Id == resourceId);
             return ap != null ? ("Scan", ap.ScanId) : null;
                 
            case "building":
            default:
                return null;
        }
    }

    private async Task<int?> GetParentIdFromRequestBody(string parentIdKey)
    {
    var httpContext = _httpContextAccessor.HttpContext;
    httpContext.Request.Body.Position = 0; 

    using (var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true))
    {
        var body = await reader.ReadToEndAsync();
        httpContext.Request.Body.Position = 0;

        try
        {
            var json = System.Text.Json.JsonDocument.Parse(body);
             string prettyJson = System.Text.Json.JsonSerializer.Serialize(
                json.RootElement, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );

            if (json.RootElement.TryGetProperty(parentIdKey, out var property) && property.TryGetInt32(out var id))
            {
                            Console.WriteLine("--- DEBUG: Parsed JSON Body ---");
                            Console.WriteLine(prettyJson);
                            Console.WriteLine("-----------------------------");
                return id;
            }
        }
        catch {} // Ignore parsing errors
    }
    return null;
}

}
