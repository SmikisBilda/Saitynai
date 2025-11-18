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
