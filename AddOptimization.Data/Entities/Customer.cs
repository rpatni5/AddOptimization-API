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
    //public virtual ICollection<Order> Orders { get; set; }
    public virtual ICollection<Address> Addresses { get; set; }

}
