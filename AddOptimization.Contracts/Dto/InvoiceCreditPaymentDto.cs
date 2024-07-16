using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class InvoiceCreditPaymentDto
    {

        public long InvoiceId { get; set; }
        public List<InvoiceCreditNoteDto> InvoiceCreditNotes { get; set; }
    }
}
