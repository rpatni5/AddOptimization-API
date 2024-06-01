using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities;

public partial class Customer : BaseEntityNew<Guid>
{
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(200)]
    public string Email { get; set; }

    [MaxLength(200)]
    public string Phone { get; set; }

    [MaxLength(200)]
    public string Birthday { get; set; }

    [MaxLength(500)]
    public string ContactInfo { get; set; }

    [MaxLength(2000)]
    public string Organizations { get; set; }
    public Guid? BillingAddressId { get; set; }
    public Guid CustomerStatusId { get; set; }
    public string Notes { get; set; }
    public int? ExternalId { get; set; }

    [ForeignKey(nameof(CustomerStatusId))]
    public virtual CustomerStatus CustomerStatus { get; set; }

    [ForeignKey(nameof(BillingAddressId))]
    public virtual Address BillingAddress { get; set; }
    public virtual ICollection<License> Licenses { get; set; }
    public virtual ICollection<Address> Addresses { get; set; }
    public string CountryCode { get; set; }
    public Guid? CountryId { get; set; }
    public bool IsApprovalRequired { get; set; }
    public int? PaymentClearanceDays { get; set; }
    public decimal? VAT { get; set; }

    [MaxLength(200)]
    public string ManagerName { get; set; }

    [MaxLength(200)]
    public string PartnerName { get; set; }

    [MaxLength(200)]
    public string PartnerBankName { get; set; }

    [MaxLength(200)]
    public string PartnerBankAccountName { get; set; }

    [MaxLength(200)]
    public string PartnerBankAccountNumber { get; set; }

    [MaxLength(200)]
    public string PartnerAddress { get; set; }

    [MaxLength(200)]
    public string PartnerDescriptions { get; set; }

    public Guid? PartnerCountryId { get; set; }
    public int? PartnerPostalCode { get; set; }

    public string Street { get; set; }
    public string City { get; set; }
    public int ZipCode { get; set; }
    public string PartnerStreet { get; set; }
    public string PartnerCity { get; set; }
    public int PartnerZipCode { get; set; }


    [ForeignKey(nameof(CountryId))]
    public virtual Country Country { get; set; }

    [ForeignKey(nameof(PartnerCountryId))]
    public virtual Country PartnerCountry { get; set; }



}
