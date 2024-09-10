using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface IInvoiceService
    {
        Task<ApiResult<bool>> GenerateInvoice(Guid customerId, MonthDateRange month, List<CustomerEmployeeAssociationDto> associatedEmployees);

        Task<ApiResult<InvoiceResponseDto>> Create(InvoiceRequestDto model);
        Task<PagedApiResult<InvoiceResponseDto>> Search(PageQueryFiterBase filters);
        Task<ApiResult<InvoiceResponseDto>> FetchInvoiceDetails(int id, bool getRoleBasedData = true);
        Task<ApiResult<InvoiceResponseDto>> Update(int id, InvoiceRequestDto model);
        Task<ApiResult<bool>> SendInvoiceToCustomer(int invoiceId, bool onlyEmail = false);
        Task<ApiResult<bool>> DeclineRequest(InvoiceActionRequestDto model);
        Task<ApiResult<List<InvoiceResponseDto>>> GetUnpaidInvoicesForEmailReminder();
        Task<ApiResult<List<InvoiceHistoryDto>>> GetInvoiceHistoryById(int id);
        Task<ApiResult<InvoiceResponseDto>> GetInvoiceById(int invoiceId);
    }
}
