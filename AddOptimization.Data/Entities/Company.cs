﻿using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities;

public class Company : BaseEntityNew<Guid>
{

    [MaxLength(50)]
    public string Email { get; set; }

    [MaxLength(20)]
    public string MobileNumber { get; set; }

    [MaxLength(200)]
    public string CompanyName { get; set; }

    [MaxLength(200)]
    public string Website { get; set; }

    [MaxLength(100)]
    public string BankName { get; set; }

    [MaxLength(200)]
    public string BankAccountName { get; set; }

    [MaxLength(50)]
    public string BankAccountNumber { get; set; }

    [MaxLength(300)]
    public string BankAddress { get; set; }
    public Guid? DialCodeId { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public Guid? CountryId { get; set; }

    [ForeignKey(nameof(CountryId))]
    public virtual Country CountryName { get; set; }
    public string? ZipCode { get; set; }
    public string? SwiftCode { get; set; }
    public string? State { get; set; }
    public string? TaxNumber { get; set; }
    public string? AccountingName { get; set; }
    public string? AccountingEmail { get; set; }
    public string? SalesContactName { get; set; }
    public string? SalesContactEmail { get; set; }
    public string? TechnicalContactName { get; set; }
    public string? TechnicalContactEmail { get; set; }
    public string? AdministrationContactName { get; set; }
    public string? AdministrationContactEmail { get; set; }

}
