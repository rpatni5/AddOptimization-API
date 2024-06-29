using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface IInvoicePaymentHistoryService
    {
        Task<ApiResult<InvoiceAmountPaymentDto>> Create(InvoiceAmountPaymentDto model);
        Task<ApiResult<List<InvoicePaymentHistoryDto>>> Search(PageQueryFiterBase filters);
        Task<ApiResult<List<InvoicePaymentHistoryDto>>> GetPaymentById(int id);

    }
}
