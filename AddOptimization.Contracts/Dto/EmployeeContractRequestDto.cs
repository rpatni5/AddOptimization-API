﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddOptimization.Contracts.Dto
{
    public class EmployeeContractRequestDto
    {
        public Guid? Id { get; set; }
        public Guid? InvoicingPaymentModeId { get; set; }
        public Guid? CustomerId { get; set; }
        public int? Hours { get; set; }
        public decimal? ProjectFees { get; set; }
        public decimal? ExpensePaymentFees { get; set; }
        public int? NoticePeriod { get; set; }
        public string? JobTitle { get; set; }
        public string? Address { get; set; }
        public Guid? EmployeeAssociationId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        public Guid? ProjectFeePaymentModeId { get; set; }
        public string? WorkMode { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public bool IsExternal { get; set; }
        public decimal? Salary { get; set; }
        public int? PublicHoliday { get; set; }        
        public Guid? IdentityId {  get; set; }
        public string IdentityNumber {  get; set; } 
    }
}
