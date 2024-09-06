using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class LeaveBalanceDto 
    {
        public int EmployeeId { get; set; }
        public int TotalAllocatedHoliday {  get; set; }
        public  decimal LeaveTaken  { get; set; }
        public decimal leavesLeft { get; set; }

    }
}
