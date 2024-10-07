using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ITemplateEntryService
    {
        Task<ApiResult<bool>> Save(TemplateEntryDto model);
        Task<ApiResult<List<TemplateEntryDto>>> Search(Guid? templateId, string textSearch = null);
        Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<TemplateEntryDto>> Get(Guid id);
    }
}

