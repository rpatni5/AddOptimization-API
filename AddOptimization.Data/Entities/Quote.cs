using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class Quote : BaseEntityNew<long>
    {
        public Guid CustomerId { get; set; }
        public Guid QuoteStatusId { get; set; }
        public string CustomerAddress { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyBankAddress { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime QuoteDate { get; set; }
        public long QuoteNo { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public decimal? TotalPriceExcVat { get; set; }
        public decimal TotalPriceIncVat { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        [ForeignKey(nameof(QuoteStatusId))]
        public virtual QuoteStatuses QuoteStatuses { get; set; }
        public ICollection<QuoteSummary> QuoteSummaries { get; set; }
    }

}
