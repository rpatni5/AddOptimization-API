using AddOptimization.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Common;

public class BaseEntityNew<TId> : BaseEntityNewCreatedOnlyProps<TId>
{
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }

    [ForeignKey(nameof(UpdatedByUserId))]
    public virtual ApplicationUser UpdatedByUser { get; set; }
}
