using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Dto
{
    public class InvoiceResponseDto : BaseDto<int>
    {
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid PaymentStatusId { get; set; }
        public Guid InvoiceStatusId { get; set; }
        public bool IsDeleted { get; set; }

        //public virtual PaymentStatus PaymentStatus { get; set; }

        //public virtual InvoiceStatus InvoiceStatus { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
