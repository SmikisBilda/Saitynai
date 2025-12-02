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


    [HttpGet("my-permissions")]
    [Authorize]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        if (userId == 0)
        {
            return Unauthorized("User ID not found in token.");
        }

        var userRoles = await _context.UserRole
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!userRoles.Any())
        {
            return Ok(new List<UserPermissionDto>());
        }

        var rolePermissions = await _context.RolePermission
            .Include(rp => rp.Permission)
            .Include(rp => rp.ResourceType)
            .Where(rp => userRoles.Contains(rp.RoleId))
            .Select(rp => new UserPermissionDto
            {
                PermissionName = rp.Permission.Name,
                ResourceType = rp.ResourceType.Name,
                ResourceId = rp.ResourceId,
                Allow = rp.Allow,
                Cascade = rp.Cascade
            })
            .ToListAsync();

        return Ok(rolePermissions);
    }

        // === VIEW METHODS ===

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Role.ToListAsync();
            return Ok(roles);
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _context.Permission.ToListAsync();
            return Ok(permissions);
        }

        [HttpGet("user-roles")]
        public async Task<IActionResult> GetUserRoles()
        {
            var userRoles = await _context.UserRole.ToListAsync();
            return Ok(userRoles);
        }

        [HttpGet("role-permissions")]
        public async Task<IActionResult> GetRolePermissions()
        {
            var rolePermissions = await _context.RolePermission.ToListAsync();
            return Ok(rolePermissions);
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

        // === DELETE METHODS ===

        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Role.FindAsync(id);
            if (role == null) return NotFound("Role not found.");
            _context.Role.Remove(role);
            await _context.SaveChangesAsync();
            return Ok("Role deleted successfully.");
        }

        [HttpDelete("permissions/{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permission.FindAsync(id);
            if (permission == null) return NotFound("Permission not found.");
            _context.Permission.Remove(permission);
            await _context.SaveChangesAsync();
            return Ok("Permission deleted successfully.");
        }

        [HttpDelete("user-roles/{userId}/{roleId}")]
        public async Task<IActionResult> DeleteUserRole(int userId, int roleId)
        {
            var userRole = await _context.UserRole.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole == null) return NotFound("UserRole not found.");
            _context.UserRole.Remove(userRole);
            await _context.SaveChangesAsync();
            return Ok("UserRole deleted successfully.");
        }

        [HttpDelete("role-permissions/{roleId}/{permissionId}/{resourceType}/{resourceId}")]
        public async Task<IActionResult> DeleteRolePermission(int roleId, int permissionId, string resourceType, int resourceId)
        {
            var resourceTypeEntity = await _context.ResourceType.FirstOrDefaultAsync(rt => rt.Name.ToLower() == resourceType.ToLower());
            if (resourceTypeEntity == null) return BadRequest($"Invalid resource type: '{resourceType}'.");
            var resourceTypeId = resourceTypeEntity.Id;
            var rolePermission = await _context.RolePermission.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.ResourceTypeId == resourceTypeId && rp.ResourceId == resourceId);
            if (rolePermission == null) return NotFound("RolePermission not found.");
            _context.RolePermission.Remove(rolePermission);
            await _context.SaveChangesAsync();
            return Ok("RolePermission deleted successfully.");
        }

            // === USER VIEW & DELETE ===

            [HttpGet("users")]
            public async Task<IActionResult> GetUsers()
            {
                var users = await _context.User.ToListAsync();
                return Ok(users);
            }

            [HttpDelete("users/{id}")]
            public async Task<IActionResult> DeleteUser(int id)
            {
                var user = await _context.User.FindAsync(id);
                if (user == null) return NotFound("User not found.");
                _context.User.Remove(user);
                await _context.SaveChangesAsync();
                return Ok("User deleted successfully.");
            }
}
