
using AddOptimization.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Common;

public class BaseEntityNewCreatedOnlyProps<TId> : BaseEntityCreatedDateOnly<TId>
{
    public int? CreatedByUserId { get; set; }
    [ForeignKey(nameof(CreatedByUserId))]
    public virtual ApplicationUser CreatedByUser { get; set; }
}
