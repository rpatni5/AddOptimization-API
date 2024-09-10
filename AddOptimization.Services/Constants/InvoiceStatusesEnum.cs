using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Services.Constants
{
    public enum InvoiceStatusesEnum
    {
        CLOSED,
        SEND_TO_CUSTOMER,
        DRAFT,
        DECLINED,
        READY_TO_SEND,
        PARTIALLY_PAID,
        CLOSED_WITH_CREDIT_NOTE
    }
}
