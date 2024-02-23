using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities;

public class RolePermission : BaseEntityNewCreatedOnlyProps<Guid>
{
    public Guid? RoleId { get; set; }
    public Guid ScreenId { get; set; }
    public Guid? FieldId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; }

    [ForeignKey(nameof(ScreenId))]
    public virtual Screen Screen { get; set; }

    [ForeignKey(nameof(FieldId))]
    public virtual Field Field { get; set; }
}
