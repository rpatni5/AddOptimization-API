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
    public interface IQuoteService
    {
        Task<ApiResult<QuoteResponseDto>> Create(QuoteRequestDto model);
        Task<ApiResult<QuoteResponseDto>> Update(long id, QuoteRequestDto model);
        Task<ApiResult<List<QuoteResponseDto>>> Search(PageQueryFiterBase filters);
        Task<ApiResult<QuoteResponseDto>> FetchQuoteDetails(long id);
        Task<bool> SendQuoteEmailToCustomer(long quoteId);
        Task<ApiResult<InvoiceResponseDto>> ConvertInvoice(long quoteId);
    }
}
