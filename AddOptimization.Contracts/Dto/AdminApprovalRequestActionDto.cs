using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
   
    public class AdminApprovalRequestActionDto: BaseDto<Guid>
    {
       
        public string Comment { get; set; }
        public bool IsApproved { get; set; }
        public string LeaveStatusName { get; set; }
    }
}
