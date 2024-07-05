using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class QuoteResponseDto : BaseDto<long>
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public Guid QuoteStatusId { get; set; }
        public string QuoteStatusesName { get; set; }
        public string CustomerAddress { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyBankAddress { get; set; }
        public long QuoteNo { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime QuoteDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UserId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public List<QuoteSummaryDto> QuoteSummaries { get; set; }
    }
}
