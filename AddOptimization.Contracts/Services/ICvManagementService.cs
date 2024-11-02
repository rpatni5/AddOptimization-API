using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;

namespace AddOptimization.Contracts.Services
{
    public interface ICvManagementService
    {
        Task<ApiResult<bool>> Save(CvEntryDto model);
        Task<PagedApiResult<CvEntryDto>> Search(PageQueryFiterBase filters);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<CvEntryDto>> GetById(Guid id);
        Task<ApiResult<CvEntryDto>> Update(Guid id, CvEntryDto model);
    }
}

