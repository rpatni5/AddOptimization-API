using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class GroupMember : BaseEntityNew<Guid>
    {
        public Guid GroupId { get; set; }
        public int UserId { get; set; }
        public DateTime? JoinedDate { get; set; }
        public bool IsDeleted { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual Group Group { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }



    }

}