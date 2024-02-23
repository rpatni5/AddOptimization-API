using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities;

public class UserRole : BaseEntityNewCreatedOnlyProps<Guid>
{
    public int UserId { get; set; }
    public Guid RoleId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; }

    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; }
}
