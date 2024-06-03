using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto
{
    public class CustomerDto: CustomerSummaryDto
    {
        public string Email { get; set; }
        
        public string BirthDay { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string BillingAddressString { get; set; }
        public string Notes { get; set; }
        public Guid CustomerStatusId { get; set; }
        public string CustomerStatusName { get; set; }

        public string ContactInfo { get; set; }
        public CustomerStatusDto CustomerStatus { get; set; }

        public List<LicenseDetailsDto>  Licenses{  get; set; }  
        
        public string CountryCode {  get; set; }
        public string ManagerName { get; set; }
        public int? PaymentClearanceDays { get; set; }
        public decimal? VAT { get; set; }
        public Guid? CountryId { get; set; }
        public bool IsApprovalRequired { get; set; }
        public CountryDto Country { get; set; }
        public string PartnerName { get; set; }
        public string PartnerBankName { get; set; }
        public string PartnerBankAccountName { get; set; }
        public string PartnerBankAccountNumber { get; set; }
        public string PartnerAddress { get; set; }
        public string PartnerDescriptions { get; set; }
        public Guid? PartnerCountryId { get; set; }
        public int? PartnerPostalCode { get; set; }

        public string Street { get; set; }
        public string City { get; set; }
        public int? ZipCode { get; set; }
        public string PartnerStreet { get; set; }
        public string PartnerCity { get; set; }
        public int? PartnerZipCode { get; set; }


    }
}
