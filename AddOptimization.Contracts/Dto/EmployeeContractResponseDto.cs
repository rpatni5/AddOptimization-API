using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class EmployeeContractResponseDto
    {
        public Guid Id { get; set; }
        public Guid InvoicingPaymentModeId { get; set; }
        public int Hours { get; set; }
        public decimal ProjectFees { get; set; }
        public decimal? ExpensePaymentFees { get; set; }
        public int NoticePeriod { get; set; }
        public string? JobTitle { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public string? Address { get; set; }
        public bool? IsSigned { get; set; }
        public DateTime? SignedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UserId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public Guid EmployeeAssociationId { get; set; }
        public Guid CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public virtual CustomerDto Customer { get; set; }
        public virtual ApplicationUserDto ApplicationUser { get; set; }

    }
}
