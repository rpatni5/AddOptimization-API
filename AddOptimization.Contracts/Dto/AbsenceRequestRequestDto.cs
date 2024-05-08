using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class AbsenceRequestRequestDto : BaseDto<Guid>
    {
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        public  int UserId { get; set; }

        public int LeaveStatusId { get; set; }

    }
}
