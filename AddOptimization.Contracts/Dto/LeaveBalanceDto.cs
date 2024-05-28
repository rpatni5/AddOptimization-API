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
        public  int LeaveTaken  { get; set; }
        public int leavesLeft { get; set; }

    }
}
