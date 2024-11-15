﻿using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities;

public partial class Customer : BaseEntityNew<Guid>
{

    [MaxLength(200)]
    public string Phone { get; set; }

    [MaxLength(2000)]
    public string Organizations { get; set; }
    public Guid? BillingAddressId { get; set; }
    public Guid CustomerStatusId { get; set; }
    public string Notes { get; set; }

    [ForeignKey(nameof(CustomerStatusId))]
    public virtual CustomerStatus CustomerStatus { get; set; }

    [ForeignKey(nameof(BillingAddressId))]
    public virtual Address BillingAddress { get; set; }
    public virtual ICollection<License> Licenses { get; set; }
    public virtual ICollection<Address> Addresses { get; set; }
    public Guid? CountryCodeId { get; set; }
    public Guid? CountryId { get; set; }
    public bool IsApprovalRequired { get; set; }
    public int? PaymentClearanceDays { get; set; }
    public decimal? VAT { get; set; }
    public decimal? PartnerVAT { get; set; }

    public string VATNumber { get; set; }
    [MaxLength(200)]
    public string ManagerName { get; set; }
    public string ManagerPhone { get; set; }
    public string ManagerEmail { get; set; }

    [MaxLength(200)]
    public string PartnerName { get; set; }
    public string PartnerPhone { get; set; }
    public string PartnerEmail { get; set; }
    [MaxLength(200)]
    public string PartnerBankName { get; set; }

    [MaxLength(200)]
    public string PartnerBankAccountName { get; set; }

    [MaxLength(200)]
    public string PartnerBankAccountNumber { get; set; }

    [MaxLength(200)]
    public string PartnerAddress { get; set; }
    public string PartnerAddress2 { get; set; }

    [MaxLength(200)]
    public string PartnerDescriptions { get; set; }
    public string PartnerVATNumber { get; set; }

    public Guid? PartnerCountryId { get; set; }
    public string State { get; set; }
    public string PartnerState { get; set; }
    public string Address { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string? ZipCode { get; set; }
    public string PartnerCity { get; set; }
    public string? PartnerZipCode { get; set; }
    public string PartnerCompany { get; set; }
    public string PartnerBankAddress { get; set; }

    public string AccountContactName { get; set; }
    public string AccountContactEmail { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string AdministrationContactName { get; set; }
    public string AdministrationContactEmail { get; set; }
    public string TechnicalContactName { get; set; }
    public string TechnicalContactEmail { get; set; }
    public bool? IsAccountSAM { get; set; }
    public bool? IsAdministrationSAM { get; set; }
    public bool? IsTechnicalSAM { get; set; }



    [ForeignKey(nameof(CountryId))]
    public virtual Country Country { get; set; }

    [ForeignKey(nameof(CountryCodeId))]
    public virtual Country CountryCodes { get; set; }

    [ForeignKey(nameof(PartnerCountryId))]
    public virtual Country PartnerCountry { get; set; }


}
