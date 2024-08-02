namespace AddOptimization.Contracts.Dto
{
    public class InvoiceResponseDto : BaseDto<long>
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
        public string CompanyCity { get; set; }
        public string CompanyCountry { get; set; }
        public string CompanyState { get; set; }
        public int? CompanyZipCode { get; set; }
        public string CompanyBankName { get; set; }
        public string CompanyBankAccountName { get; set; }
        public string CompanyBankAccontNumber { get; set; }
        public string CompanyBankAddress { get; set; }

        public string CompanyBankDetails { get; set; }
        public decimal VatValue { get; set; }
        public decimal TotalPriceIncludingVat { get; set; }
        public decimal TotalPriceExcludingVat { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? PaymentClearanceDays { get; set; }
        public decimal DueAmount { get; set; }
        public long? CreditNoteNumber { get; set; }
        public bool HasCreditNotes { get; set; }

        public string? SwiftCode { get; set; }

        public virtual PaymentStatusDto PaymentStatus { get; set; }
        public virtual InvoiceStatusDto InvoiceStatus { get; set; }
        public virtual CustomerDto Customer { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<InvoiceDetailDto> InvoiceDetails { get; set; }
    }
}
