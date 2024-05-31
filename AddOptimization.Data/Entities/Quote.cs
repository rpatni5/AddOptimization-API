using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class Quote : BaseEntityNew<Guid>
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public Guid QuoteStatusId { get; set; }
        public string BillingAddress { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime QuoteDate { get; set; }
      
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }

        [ForeignKey(nameof(QuoteStatusId))]
        public virtual QuoteStatuses QuoteStatuses { get; set; }
        public ICollection<QuoteSummary> QuoteSummaries { get; set; }
    }

}
