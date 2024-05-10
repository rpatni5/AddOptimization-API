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
    public interface IAbsenceRequestService
    {
        Task<ApiResult<AbsenceRequestResponseDto>> Create(AbsenceRequestRequestDto model);
        Task<ApiResult<AbsenceRequestResponseDto>> Get(Guid id);
        Task<ApiResult<List<AbsenceRequestResponseDto>>> Search(PageQueryFiterBase filters);

        Task<ApiResult<AbsenceRequestResponseDto>> Update(Guid id, AbsenceRequestRequestDto model);


        Task<ApiResult<bool>> Delete(Guid id);


    }
}
