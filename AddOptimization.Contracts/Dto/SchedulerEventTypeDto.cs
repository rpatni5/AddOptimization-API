using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
   
    public class SchedulerEventTypeDto : BaseDto<Guid?>
    {
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
