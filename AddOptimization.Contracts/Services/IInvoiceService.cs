using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Helpers;

namespace AddOptimization.Contracts.Services
{
    public interface IInvoiceService
    {
        Task<ApiResult<bool>> GenerateInvoice(Guid customerId, MonthDateRange month, List<CustomerEmployeeAssociationDto> associatedEmployees);

        Task<ApiResult<InvoiceResponseDto>> Create(InvoiceRequestDto model);
    }
}
