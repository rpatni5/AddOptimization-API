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
    public interface IAbsenceApprovalService
    {
        Task<PagedApiResult<AbsenceRequestResponseDto>> Search(PageQueryFiterBase filters);
        Task<ApiResult<bool>> AbsenceAction(AdminApprovalRequestActionDto model);
        Task<ApiResult<List<AbsenceRequestResponseDto>>> GetAllAbsenseApproval(int employeeId);
    }
}
