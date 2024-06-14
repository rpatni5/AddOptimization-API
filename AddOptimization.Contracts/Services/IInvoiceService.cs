using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface IInvoiceService
    {
        Task<ApiResult<List<InvoiceResponseDto>>> GenerateInvoice(Guid customerId, MonthDateRange month, List<CustomerEmployeeAssociationDto> associatedEmployees);

        Task<ApiResult<InvoiceResponseDto>> Create(InvoiceRequestDto model);
    }
}
