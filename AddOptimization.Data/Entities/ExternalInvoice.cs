using AddOptimization.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Data.Entities
{

    public class ExternalInvoice : BaseEntityNew<long>

    {
        public long InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Guid? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public Guid PaymentStatusId { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public string CustomerAddress { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyBankDetails { get; set; }
        public decimal VatValue { get; set; }
        public decimal TotalPriceIncludingVat { get; set; }
        public decimal TotalPriceExcludingVat { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? PaymentClearanceDays { get; set; }
        public int? EmployeeId { get; set; }
        public decimal DueAmount { get; set; }

        [ForeignKey(nameof(PaymentStatusId))]
        public virtual PaymentStatus PaymentStatus { get; set; }

        [ForeignKey(nameof(InvoiceStatusId))]
        public virtual InvoiceStatus InvoiceStatus { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<ExternalInvoiceDetail> InvoiceDetails { get; set; }

    }
}