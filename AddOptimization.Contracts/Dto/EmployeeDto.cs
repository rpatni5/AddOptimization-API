using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public bool IsExternal { get; set; }
        public decimal? Salary { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string? SwiftCode { get; set; }
        public string? BankAddress { get; set; }
        public string? BankPostalCode { get; set; }
        public string? BankCity { get; set; }
        public string? BankState { get; set; }
        public string? BankCountry { get; set; }
        public string BillingAddress { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isActive { get; set; }
        public string VATNumber { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string JobTitle { get; set; }
        public string City { get; set; }
        public string CompanyName { get; set; }
        public Guid? CountryId { get; set; }
        public string Address { get; set; }
        public string ExternalZipCode { get; set; }
        public string ExternalCity { get; set; }
        public string ExternalState { get; set; }
        public string ExternalAddress { get; set; }
        public Guid? ExternalCountryId { get; set; }
        public DateTime? NdaSignDate { get; set; }
        public string CountryName { get; set; }
        public virtual CountryDto Country { get; set; }
        public bool? HasContract { get; set; }
        public bool IsNDASigned { get; set; }
        public string IdentityNumber {  get; set; } 
        public Guid? IdentityId { get; set; }
        public string IdentityName { get; set; }


    }
}
