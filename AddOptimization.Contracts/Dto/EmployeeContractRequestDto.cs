using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class EmployeeContractRequestDto
    {
        public Guid Id { get; set; }
        public Guid InvoicingPaymentModeId { get; set; }
        public Guid CustomerId { get; set; }
        public int Hours { get; set; }
        public decimal ProjectFees { get; set; }
        public decimal? ExpensePaymentFees { get; set; }
        public int NoticePeriod { get; set; }
        public string? JobTitle { get; set; }
        public string? Address { get; set; }
        public Guid EmployeeAssociationId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }

    }
}
