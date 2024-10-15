using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ITemplatesService
    {
        Task<ApiResult<List<TemplateDto>>> GetAllTemplate();
        Task<ApiResult<TemplateDto>> GetTemplateById(Guid id);
    }
}

