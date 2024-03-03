using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public partial class License : BaseEntityNew<Guid>
    {

        [MaxLength(255)]
        public string LicenseKey { get; set; }
        public int LicenseDuration { get; set; }
        public int NoOfDevices { get; set; }
        public DateTime ExpirationDate { get; set; }
        public Guid CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }
        public virtual ICollection<LicenseDevice> LicenseDevices { get; set; }
    }
}
