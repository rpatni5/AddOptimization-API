using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class ClientEmployeeAssociationDto : BaseDto<Guid>
    {
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int ApproverId { get; set; }
        public string ApproverName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

    }
}
