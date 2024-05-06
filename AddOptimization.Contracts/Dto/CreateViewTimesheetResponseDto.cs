using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class CreateViewTimesheetResponseDto :BaseDto<Guid>
    {
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public int ApprovarId { get; set; }
        public string ApprovarName { get; set; }
        public Guid UserStatusId { get; set; }
        public string UserStatusName { get; set; }
        public Guid AdminStatusId { get; set; }
        public string AdminStatusName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsDraft { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
