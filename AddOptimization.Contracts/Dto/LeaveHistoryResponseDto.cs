using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class LeaveHistoryResponseDto
    {
        public int TotalAllocatedHoliday { get; set; }
        public decimal LeaveTaken { get; set; }
        public decimal LeavesLeft { get; set; }
        public List<AbsenceRequestResponseDto> AbsenceRequests { get; set; }
    }
}
