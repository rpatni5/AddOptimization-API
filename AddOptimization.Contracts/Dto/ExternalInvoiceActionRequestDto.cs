using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class ExternalInvoiceActionRequestDto : ExternalInvoiceResponseDto
    {
        public string Comment { get; set; }
    }
}
