using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class QuoteRequestDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public string BillingAddress { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime QuoteDate { get; set; }
        public List<QuoteSummaryDto> QuoteSummaries { get; set; }

    }
}
