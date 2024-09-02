using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface IExternalInvoiceService
    {
        Task<ApiResult<List<ExternalInvoiceResponseDto>>> GenerateExternalInvoice(Guid customerId, MonthDateRange month, List<CustomerEmployeeAssociationDto> associatedEmployees);
        Task<ApiResult<ExternalInvoiceResponseDto>> Create(ExternalInvoiceRequestDto model);
        Task<PagedApiResult<ExternalInvoiceResponseDto>> Search(PageQueryFiterBase filters);
        Task<ApiResult<ExternalInvoiceResponseDto>> FetchExternalInvoiceDetails(long id, bool getRoleBasedData = true);
        Task<ApiResult<ExternalInvoiceResponseDto>> Update(long id, ExternalInvoiceRequestDto model);
        Task<bool> SendInvoiceApprovalEmailToAccountAdmin(int Id);
        Task<ApiResult<bool>> DeclineRequest(ExternalInvoiceActionRequestDto model);
        Task<bool> SendInvoiceApprovalEmailToCustomer(int id);
        Task<ApiResult<List<ExternalInvoiceHistoryDto>>> GetExternalInvoiceHistoryById(int id);


    }
}
