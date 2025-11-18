using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saitynai.Models;
using Saitynai.DTOs;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Authorize]

[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PermissionController : ControllerBase
{
    private readonly PostgresContext _context;

    public PermissionController(PostgresContext context)
    {
        _context = context;
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole(CreateRoleDto createRoleDto)
    {
        if (await _context.Role.AnyAsync(r => r.Name.ToLower() == createRoleDto.Name.ToLower()))
        {
            return Conflict("Role with that name already exists.");
        }
        var role = new Role { Name = createRoleDto.Name };
        _context.Role.Add(role);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(CreateRole), new { id = role.Id }, role);
    }

    [HttpPost("permissions")]
    public async Task<IActionResult> CreatePermission(CreatePermissionDto createPermissionDto)
    {
        if (await _context.Permission.AnyAsync(p => p.Name.ToLower() == createPermissionDto.Name.ToLower()))
        {
            return Conflict("Permission with that name already exists.");
        }
        var permission = new Permission { Name = createPermissionDto.Name };
        _context.Permission.Add(permission);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(CreatePermission), new { id = permission.Id }, permission);
    }

    [HttpPost("user-roles")]
    public async Task<IActionResult> AssignRoleToUser(AssignRoleDto assignRoleDto)
    {
        var assignmentExists = await _context.UserRole
            .AnyAsync(ur => ur.UserId == assignRoleDto.UserId && ur.RoleId == assignRoleDto.RoleId);

        if (assignmentExists) return Ok("User already has this role.");
        
        var userRole = new UserRole { UserId = assignRoleDto.UserId, RoleId = assignRoleDto.RoleId };
        _context.UserRole.Add(userRole);
        await _context.SaveChangesAsync();

        return Ok("Role assigned successfully.");
    }

    [HttpDelete("user-roles")]
    public async Task<IActionResult> RevokeRoleFromUser([FromQuery] AssignRoleDto assignRoleDto)
    {
        var userRole = await _context.UserRole
            .FirstOrDefaultAsync(ur => ur.UserId == assignRoleDto.UserId && ur.RoleId == assignRoleDto.RoleId);

        if (userRole == null) return NotFound("Role assignment not found.");

        _context.UserRole.Remove(userRole);
        await _context.SaveChangesAsync();
        return Ok("Role revoked successfully.");
    }

    // === Assign/Revoke Permission for Role ===

    [HttpPost("role-permissions")]
    public async Task<IActionResult> AssignOrUpdatePermissionToRole(AssignPermissionDto dto)
    {
        var resourceType = await _context.ResourceType
            .FirstOrDefaultAsync(rt => rt.Name.ToLower() == dto.ResourceType.ToLower());

        if (resourceType == null)
        {
            return BadRequest($"Invalid resource type: '{dto.ResourceType}'.");
        }
        var resourceTypeId = resourceType.Id;

        var rolePermission = await _context.RolePermission
            .FirstOrDefaultAsync(rp => rp.RoleId == dto.RoleId &&
                                      rp.PermissionId == dto.PermissionId &&
                                      rp.ResourceTypeId == resourceTypeId &&
                                      rp.ResourceId == dto.ResourceId);
        if (rolePermission == null)
        {
            rolePermission = new RolePermission
            {
                RoleId = dto.RoleId,
                PermissionId = dto.PermissionId,
                ResourceTypeId = resourceTypeId,
                ResourceId = dto.ResourceId,
                Allow = dto.Allow,
                Cascade = dto.Cascade
            };
            _context.RolePermission.Add(rolePermission);
        }
        else
        {
            rolePermission.Allow = dto.Allow;
            rolePermission.Cascade = dto.Cascade;
            _context.RolePermission.Update(rolePermission);
        }

        await _context.SaveChangesAsync();
        return Ok("Permission assigned or updated successfully.");
    }

    [HttpDelete("role-permissions")]
    public async Task<IActionResult> RevokePermissionFromRole([FromQuery] AssignPermissionDto dto)
    {
        var resourceType = await _context.ResourceType
            .FirstOrDefaultAsync(rt => rt.Name.ToLower() == dto.ResourceType.ToLower());

        if (resourceType == null)
        {
            return BadRequest($"Invalid resource type: '{dto.ResourceType}'.");
        }
        var resourceTypeId = resourceType.Id;


        var rolePermission = await _context.RolePermission
            .FirstOrDefaultAsync(rp => rp.RoleId == dto.RoleId &&
                                  rp.PermissionId == dto.PermissionId &&
                                  rp.ResourceTypeId == resourceTypeId && 
                                  rp.ResourceId == dto.ResourceId);
        

        if (rolePermission == null)
        {
            return NotFound("Permission assignment not found.");
        }

        _context.RolePermission.Remove(rolePermission);
        await _context.SaveChangesAsync();
        return Ok("Permission revoked successfully.");
    }
}
