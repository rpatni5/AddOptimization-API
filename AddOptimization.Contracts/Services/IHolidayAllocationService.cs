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
    public interface IHolidayAllocationService
    {

        Task<PagedApiResult<HolidayAllocationResponseDto>> Search(PageQueryFiterBase filter);
        Task<ApiResult<HolidayAllocationResponseDto>> Create(HolidayAllocationRequestDto model);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<HolidayAllocationResponseDto>> GetAllocatedHolidays(int employeeId);
        Task<ApiResult<LeaveBalanceDto>> GetEmployeeLeaveBalance(int employeeId);

    }
}
