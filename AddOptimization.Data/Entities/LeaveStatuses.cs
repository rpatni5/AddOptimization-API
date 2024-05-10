using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{
   
    public class LeaveStatuses : BaseEntityNew<Guid>
    {

        public int Id { get; set; }
        public string Name { get; set; }
    }
}
