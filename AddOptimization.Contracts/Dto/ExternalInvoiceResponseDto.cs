using AddOptimization.Contracts.Dto;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class ExternalInvoiceResponseDto:BaseDto<long>
    {
        public long InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Guid? CompanyId {  get; set; }
        public Guid PaymentStatusId { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public string InvoiceStatusName { get; set; }
        public string PaymentStatusName { get; set; }
        public string CustomerAddress { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyBankDetails { get; set; }
        public decimal VatValue { get; set; }
        public decimal TotalPriceIncludingVat { get; set; }
        public decimal TotalPriceExcludingVat { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CompanyName { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? PaymentClearanceDays { get; set; }
        public virtual CompanyDto Company { get; set; }
        public List<ExternalInvoiceDetailDto> ExternalInvoiceDetails { get; set; }
    }
}
