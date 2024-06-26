using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities
{
    public class Invoice : BaseEntityNew<long>
    {
        public long InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid PaymentStatusId { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public string CustomerAddress { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyBankDetails { get; set; }
        public decimal VatValue { get; set; }
        public decimal TotalPriceIncludingVat { get; set; }
        public decimal TotalPriceExcludingVat { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? PaymentClearanceDays { get; set; }
        public decimal DueAmount { get; set; }


        [ForeignKey(nameof(PaymentStatusId))]
        public virtual PaymentStatus PaymentStatus { get; set; }

        [ForeignKey(nameof(InvoiceStatusId))]
        public virtual InvoiceStatus InvoiceStatus { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }

    }
}
