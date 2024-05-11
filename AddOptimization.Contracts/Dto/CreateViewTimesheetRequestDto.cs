using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class CreateViewTimesheetRequestDto 
    {
        public Guid ClientId { get; set; }
        public int? UserId { get; set; }
        public int ApprovarId { get; set; }
        public DateTime DateMonth { get; set; }
       
    }
}
