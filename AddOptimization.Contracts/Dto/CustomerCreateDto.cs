using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class CustomerCreateDto:BaseDto<Guid?>
{
    public string Company { get; set; }
    public string Phone { get; set; }
    public string Notes { get; set; }
    public Guid? BillingAddressId { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? CustomerStatusId { get; set; }
    public List<AddressCreateDto> Addresses { get; set; }
    public string CountryCode { get; set; }
    public string ManagerName { get; set; }
    public string ManagerPhone { get; set; }
    public string ManagerEmail { get; set; }
    public int? PaymentClearanceDays { get; set; }
    public decimal VAT { get; set; }
    public decimal PartnerVAT { get; set; }
    public string VATNumber { get; set; }
    public string PartnerVATNumber { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? CountryCodeId { get; set; }
    public bool IsApprovalRequired { get; set; }
    public string PartnerName { get; set; }
    public string PartnerPhone { get; set; }
    public string PartnerEmail { get; set; }
    public string PartnerCompany { get; set; }
    public string PartnerBankName { get; set; }
    public string PartnerBankAccountName { get; set; }
    public string PartnerBankAccountNumber { get; set; }
    public string PartnerAddress { get; set; }
    public string PartnerAddress2 { get; set; }
    public string PartnerState { get; set; }
    public string PartnerDescriptions { get; set; }
    public Guid? PartnerCountryId { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string? ZipCode { get; set; }
    public string PartnerCity { get; set; }
    public string? PartnerZipCode { get; set; }
    public CountryDto Country { get; set; }
    public string CustomerStatusName { get; set; }

    public CustomerStatusDto CustomerStatus { get; set; }

    public List<LicenseDetailsDto> Licenses { get; set; }

    public string PartnerBankAddress { get; set; }
}
