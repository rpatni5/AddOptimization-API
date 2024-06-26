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

        Task<ApiResult<List<ExternalInvoiceResponseDto>>> Search(PageQueryFiterBase filters);

        Task<ApiResult<ExternalInvoiceResponseDto>> FetchInvoiceDetails(long id);

        Task<ApiResult<ExternalInvoiceResponseDto>> Update(long id, ExternalInvoiceRequestDto model);

        Task<bool> SendInvoiceApprovalEmailToAccountAdmin(long id);



    }
}
