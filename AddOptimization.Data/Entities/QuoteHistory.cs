using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{
    public class QuoteHistory : BaseEntityNew<Guid>
    {
        public long QuoteId { get; set; }
        public Guid QuoteStatusId { get; set; }
        public string Comment { get; set; }

        [ForeignKey(nameof(QuoteId))]
        public virtual Quote Quote { get; set; }

        [ForeignKey(nameof(QuoteStatusId))]
        public virtual QuoteStatuses QuoteStatuses { get; set; }

    }
}
