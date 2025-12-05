using System.ComponentModel.DataAnnotations;
using Saitynai.Models;

namespace Saitynai.DTOs;

public class CreateRoleDto
{
    [Required]
    public string Name { get; set; }
}

public class CreatePermissionDto
{
    [Required]
    public string Name { get; set; }
}

public class AssignRoleDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int RoleId { get; set; }
}

public class AssignPermissionDto
{
    [Required]
    public int RoleId { get; set; }

    [Required]
    public int PermissionId { get; set; }

    [Required]
    public string ResourceType { get; set; }

    [Required]
    public int ResourceId { get; set; }

    public bool Allow { get; set; } = true;
    public bool Cascade { get; set; } = true;
}

public class UserPermissionDto
{
    public string PermissionName { get; set; }
    public string ResourceType { get; set; }
    public int ResourceId { get; set; }
    public bool Allow { get; set; }
    public bool Cascade { get; set; }
}

public class UserRoleResponseDto
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public string Username { get; set; }
    public string RoleName { get; set; }
}

public class RolePermissionResponseDto
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public int ResourceTypeId { get; set; }
    public int ResourceId { get; set; }
    public bool Allow { get; set; }
    public bool Cascade { get; set; }
    public string RoleName { get; set; }
    public string PermissionName { get; set; }
    public string ResourceTypeName { get; set; }
}

public class PermissionScopeDto
{
    public string PermissionName { get; set; }
    public string ResourceType { get; set; }
    public List<int> AllowIds { get; set; } = new();
    // Map of ancestor resource type -> list of IDs where permission cascades from
    public Dictionary<string, List<int>> CascadeFrom { get; set; } = new();
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class UserPermissionsResponseDto
{
    public List<RoleDto> Roles { get; set; } = new();
    public List<UserPermissionDto> Permissions { get; set; } = new();
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}
