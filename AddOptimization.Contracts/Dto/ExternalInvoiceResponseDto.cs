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
        public int? EmployeeId { get; set; }
        public Guid? CompanyId {  get; set; }
        public Guid PaymentStatusId { get; set; }
        public decimal DueAmount { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public string EmployeeName { get; set; }
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
        public string ExternalCompanyName { get; set; }

        public string ExternalEmployeeAddress { get; set; }
        public string ExternalCompanyAddress { get; set; }
        public string ExternalCompanyCity { get; set; }
        public string? ExternalCompanyZipCode { get; set; }
        public string ExternalCompanyState { get; set; }

        public string ExternalCity { get; set; }
        public string ExternalState { get; set; }

        public string? ExternalZipCode { get; set; }

        public string BankName { get; set; }

        public string BankAddress { get; set; }

        public string BankCity { get; set; }

        public string BankState { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }

        public string? BankZipCode { get; set; }
        public string? VATNumber { get; set; }

        public List<ExternalInvoiceDetailDto> ExternalInvoiceDetails { get; set; }
    }
}
