using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class InvoiceStatusDto : BaseDto<Guid>
    {
        public string StatusKey { get; set; }
      
    }
}
