using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface IPersonalInformationService
    {
        Task<ApiResult<bool>> SavePersonalInformationDetails(TemplateEntryDto model);
        Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model);
        Task<ApiResult<TemplateEntryDto>> GetInfoDetailsById(Guid id);
        Task<ApiResult<List<TemplateEntryDto>>> Search();
        Task<ApiResult<bool>> Delete(Guid id);
    }
}
