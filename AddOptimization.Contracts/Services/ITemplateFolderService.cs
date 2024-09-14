using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ITemplateFolderService
    {
        Task<ApiResult<List<TemplateFolderDto>>> GetAllTemplateFolders();
    }
}

