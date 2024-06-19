using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class ExternalInvoiceRequestDto
    {
        public int Id { get; set; }
        public Guid? CompanyId { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyName { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? PaymentClearanceDays { get; set; }
        public List<ExternalInvoiceDetailDto> ExternalInvoiceDetails { get; set; }
    }
}
