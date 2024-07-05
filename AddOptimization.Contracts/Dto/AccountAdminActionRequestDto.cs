using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class AccountAdminActionRequestDto : SchedulerEventResponseDto
    {
        public string Comment { get; set; }
    }
}
