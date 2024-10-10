using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ICvManagementService
    {
        Task<ApiResult<bool>> Save(CvEntryDto model);
    }
}

