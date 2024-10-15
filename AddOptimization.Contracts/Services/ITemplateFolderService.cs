using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ITemplateFolderService
    {
        Task<ApiResult<List<TemplateFolderDto>>> GetAllTemplateFolders();
        Task<ApiResult<bool>> Create(TemplateFolderDto model);
        Task<ApiResult<TemplateFolderDto>> Update(Guid id, TemplateFolderDto model);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<List<TemplateEntryDto>>> GetTemplates(Guid folderId);
    }
}

