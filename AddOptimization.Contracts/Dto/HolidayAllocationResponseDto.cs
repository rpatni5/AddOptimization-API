using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class HolidayAllocationResponseDto : BaseDto<Guid>
    {
        public string UserName { get; set; }
        public int Holidays { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UserId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

    }
}
