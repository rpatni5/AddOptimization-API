﻿using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto
{
    public class CompanyDto : BaseDto<Guid>
    {
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAddress { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public Guid? CountryId { get; set; }

        public Guid? DialCodeId { get; set; }
        public string? CountryName { get; set; }
        public string? ZipCode { get; set; }
        public string? SwiftCode { get; set; }
        public string? State { get; set; }
        public string? TaxNumber { get; set; }
        public string? AccountingName {  get; set; }
        public string? AccountingEmail { get; set; }
        public string? SalesContactName{ get; set; }
        public string? SalesContactEmail { get; set; }
        public string? TechnicalContactName { get; set; }
        public string? TechnicalContactEmail { get; set; }
        public string? AdministrationContactName { get; set; }
        public string? AdministrationContactEmail { get; set; }
    }
}
