using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Services.Constants
{
    public enum SchedulerStatusesEnum
    {
        PENDING_ACCOUNT_ADMIN_APPROVAL,
        DRAFT,
        PENDING_INVOICING,
        CUSTOMER_PAID,
        PENDING_CUSTOMER_APPROVAL,
        DECLINED,
        CUSTOMER_DECLINED,
        CUSTOMER_APPROVED,
        ADMIN_APPROVED,
    }
}
