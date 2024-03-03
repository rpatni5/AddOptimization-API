using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Data.Entities;

public partial class Address: BaseEntityNew<Guid>
{
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(50)]
    public string Phone { get; set; }

    [MaxLength(200)]
    public string Address1 { get; set; }

    [MaxLength(200)]
    public string Address2 { get; set; }

    [MaxLength(100)]
    public string City { get; set; }

    [MaxLength(20)]
    public string Zip { get; set; }

    [MaxLength(100)]
    public string Province { get; set; }

    [MaxLength(10)]
    public string ProvinceCode { get; set; }

    [MaxLength(100)]
    public string Country { get; set; }

    [MaxLength(5)]
    public string CountryCode { get; set; }
    public Guid? CustomerId { get; set; }
    public int? ExternalId { get; set; }
    public bool IsDeleted { get; set; }

    [MaxLength(100)]
    public string GPSCoordinates { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; }
}
