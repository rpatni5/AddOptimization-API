using AddOptimization.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AddOptimization.Data.Entities
{
    public class EmployeeContract : BaseEntityNew<Guid>
    {
        public Guid InvoicingPaymentModeId { get; set; }
        public Guid EmployeeAssociationId { get; set; }
        public Guid CustomerId { get; set; }
        public int EmployeeId { get; set; }
        public  int Hours { get; set; }
        public  decimal ProjectFees { get; set; }
        public  decimal? ExpensePaymentFees { get; set; }
        public  int NoticePeriod { get; set; }
        public string? JobTitle { get; set; }
        public string? Address { get; set; }
        public bool IsContractSigned { get;set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        public Guid ProjectFeePaymentModeId { get; set; }
        public string? WorkMode { get; set; }
        public string ContractName {  get; set; }

        [ForeignKey(nameof(ProjectFeePaymentModeId))]
        public virtual InvoicingPaymentMode ProjectFeePaymentMode { get; set; }

        [ForeignKey(nameof(InvoicingPaymentModeId))]
        public virtual InvoicingPaymentMode InvoicingPaymentMode { get; set; }

        [ForeignKey(nameof(EmployeeAssociationId))]
        public virtual CustomerEmployeeAssociation CustomerEmployeeAssociation { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

    }

}
