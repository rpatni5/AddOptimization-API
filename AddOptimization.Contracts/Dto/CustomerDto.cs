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
    }
}
