using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface  IInvoiceCreditNoteService
       {
        Task<ApiResult<InvoiceCreditPaymentDto>> Create(InvoiceCreditPaymentDto model);
        Task<ApiResult<List<InvoiceCreditNoteDto>>> GetCreditInfoById(int id);
    }
}
