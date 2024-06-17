using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class InvoiceResponseDto : BaseDto<int>
    {


        public long InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid PaymentStatusId { get; set; }
        public string PaymentStatusName { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public string InvoiceStatusName { get; set; }
        public string CustomerAddress { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyBankDetails { get; set; }
        public decimal VatValue { get; set; }
        public decimal TotalPriceIncludingVat { get; set; }
        public decimal TotalPriceExcludingVat { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? PaymentClearanceDays { get; set; }

        //public virtual PaymentStatus PaymentStatus { get; set; }

        //public virtual InvoiceStatus InvoiceStatus { get; set; }

        //public virtual Customer Customer { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<InvoiceDetailDto> InvoiceDetails { get; set; }
    }
}
