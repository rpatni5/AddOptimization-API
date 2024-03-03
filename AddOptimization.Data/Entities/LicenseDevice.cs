using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public partial class LicenseDevice : BaseEntityNew<Guid>
    {
        public Guid CustomerId { get; set; }
        public string MotherBoardId { get; set; }
        public string MachineName { get; set; }
        public Guid LicenseId { get; set; }

        [ForeignKey(nameof(LicenseId))]
        public virtual License Licenses { get; set; }
    }
}
