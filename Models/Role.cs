using System.Collections.Generic;

namespace Saitynai.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
