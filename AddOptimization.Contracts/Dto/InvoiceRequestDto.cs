using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class InvoiceRequestDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerAddress { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? PaymentClearanceDays { get; set; }
        public List<InvoiceDetailDto> InvoiceDetails { get; set; }

    }
}
