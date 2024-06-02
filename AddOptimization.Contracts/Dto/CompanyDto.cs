using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto
{
    public class CompanyDto : BaseDto<Guid>
    {
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string Website { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BillingAddress { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int? ZipCode { get; set; }


    }
}
