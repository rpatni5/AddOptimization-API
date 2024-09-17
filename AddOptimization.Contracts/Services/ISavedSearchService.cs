using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface ISavedSearchService
    {
        Task<ApiResult<SavedSearchDto>> Create(SavedSearchDto model);
        Task<ApiResult<SavedSearchDto>> Update(Guid id, SavedSearchDto model);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<List<SavedSearchDto>>> GetCurrentUserSearches(string searchScreen);
    }
}
