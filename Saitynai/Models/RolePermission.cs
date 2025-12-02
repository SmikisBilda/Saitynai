namespace Saitynai.Models;

public class RolePermission
{
    public int RoleId { get; set; }
    public virtual Role Role { get; set; }

    public int PermissionId { get; set; }
    public virtual Permission Permission { get; set; }

    public int ResourceTypeId { get; set; }
    public virtual ResourceType ResourceType { get; set; } 
    
    public int ResourceId { get; set; }

    public bool Allow { get; set; }
    public bool Cascade { get; set; }
}
