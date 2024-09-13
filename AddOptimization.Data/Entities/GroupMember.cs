using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class GroupMember : BaseEntityNew<Guid>
    {
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedDate { get; set; }
        public bool IsDeleted { get; set; }

    }

}