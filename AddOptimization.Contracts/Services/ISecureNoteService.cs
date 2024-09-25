using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ISecureNoteService
    {
        Task<ApiResult<bool>> SaveSecureNote(TemplateEntryDto model);
        Task<ApiResult<List<TemplateEntryDto>>> Search();
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<TemplateEntryDto>> GetSecureNoteById(Guid id);
        Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model);
    }
}

