using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities;

public partial class CompanyInformation : BaseEntityNew<Guid>
{

    [MaxLength(200)]
    public string Email { get; set; }

    [MaxLength(200)]
    public string MobileNumber { get; set; }

    [MaxLength(200)]
    public string AccountName { get; set; }
    [MaxLength(200)]
    public string AccountNumber { get; set; }

    [MaxLength(200)]
    public string Website { get; set; }

    [MaxLength(200)]
    public string BankName { get; set; }

    [MaxLength(200)]
    public string BankAccountName { get; set; }

    [MaxLength(200)]
    public string BankAccountNumber { get; set; }

    [MaxLength(200)]
    public string BillingAddress { get; set; }


}
