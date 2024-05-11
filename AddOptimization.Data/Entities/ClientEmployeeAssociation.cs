using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities
{
    public class ClientEmployeeAssociation : BaseEntityNew<Guid>
    { 
        public Guid ClientId { get; set; }
        public int EmployeeId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public int ApproverId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey(nameof(ClientId))]
        public virtual Client Client { get; set; }

        [ForeignKey(nameof(ApproverId))]
        public virtual ApplicationUser Approver { get; set; }
    }
}
