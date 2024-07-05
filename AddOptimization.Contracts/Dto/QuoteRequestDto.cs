using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class QuoteRequestDto
    {
        public long Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerAddress { get; set; }
        public long QuoteNo { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyBankAddress { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime QuoteDate { get; set; }
        public List<QuoteSummaryDto> QuoteSummaries { get; set; }

    }
}
