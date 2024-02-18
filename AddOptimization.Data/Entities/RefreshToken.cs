using AddOptimization.Data.Common;
using AddOptimization.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities
{
    public class RefreshToken: BaseEntityCreatedDateOnly<int>
    {
        public int ApplicationUserId { get; set; }
        public Guid Token { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? ExpiredAt { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
