using AddOptimization.Data.Common;
using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{
    public class PublicHoliday : BaseEntityNew<Guid>
    {
        [MaxLength(200)]
        public string Title { get; set; }
        [MaxLength(200)]
        public string Info { get; set; }
        public DateTime? Date { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CountryId { get; set; }
        [ForeignKey(nameof(CountryId))]
        public virtual Country Country { get; set; }
    }

}



