using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class CustomerCreateDto:BaseDto<Guid?>
{
    [Required]
    public string Email { get; set; }
    public string Company { get; set; }
    public string Phone { get; set; }
    public string Notes { get; set; }
    public DateTime Birthday { get; set; }
    public string ContactInfo { get; set; }
    public Guid? BillingAddressId { get; set; }
    public bool IsDeleted { get; set; }

    public Guid? CustomerStatusId { get; set; }
    public List<AddressCreateDto> Addresses { get; set; }

    public string CountryCode { get; set; }
    public string ManagerName { get; set; }
    public int? PaymentClearanceDays { get; set; }
    public decimal VAT { get; set; }
    public Guid CountryId { get; set; }
    public bool IsApprovalRequired { get; set; }
    public string PartnerName { get; set; }
    public string PartnerBankName { get; set; }
    public string PartnerBankAccountName { get; set; }
    public string PartnerBankAccountNumber { get; set; }
    public string PartnerAddress { get; set; }
    public string PartnerDescriptions { get; set; }
    public Guid? PartnerCountryId { get; set; }
    public int? PartnerPostalCode { get; set; }
}
