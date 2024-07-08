using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;

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
    
    public string Address { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public int? ZipCode { get; set; }
    public string? SwiftCode { get; set; }
    public string? State { get; set; }
    public string? TaxNumber { get; set; }
    public Guid? DialCodeId { get; set; }

}
