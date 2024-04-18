using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Data.Entities;

public class Role :BaseEntityNew<Guid>
{
    public bool IsDeleted { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; }
    public virtual ICollection<RolePermission> RolePermissions { get; set; }
}
