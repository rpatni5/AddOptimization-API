﻿using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class Employee : BaseEntityNew<Guid>
    {
        public int UserId { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ExternalCountryId {  get; set; }
        public bool IsExternal { get; set; }
        public bool IsNDASigned { get; set; }
        public decimal? Salary { get; set; }
        public string VATNumber { get; set; }
        public string? ZipCode { get; set; }
        public string State { get; set; }
        public string JobTitle { get; set; }
        public string City { get; set; }
        public string CompanyName { get; set; }
        public string? ExternalZipCode { get; set; }
        public string ExternalCity { get; set; }
        public string ExternalState { get; set; }
        public string Address { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime? NdaSignDate { get; set; }
        public string ExternalAddress { get; set; }

        [MaxLength(200)]
        public string BankName { get; set; }

        [MaxLength(200)]
        public string BankAccountName { get; set; }

        [MaxLength(200)]
        public string BankAccountNumber { get; set; }
        public string? SwiftCode { get; set; }
        [MaxLength(200)]
        public string?BankAddress { get; set; }
        public string? BankPostalCode { get; set; }
        public string? BankCity { get; set; }
        public string? BankState { get; set; }
        public string? BankCountry { get; set; }
        public Guid? IdentityId {  get; set; }

        [MaxLength(500)]
        public string BillingAddress { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey(nameof(CountryId))]
        public virtual Country Country { get; set; }

        [ForeignKey(nameof(ExternalCountryId))]
        public virtual Country ExternalCountry { get; set; }

        [ForeignKey(nameof(IdentityId))]
        public EmployeeIdentity EmployeeIdentity { get; set; }
    }
}
