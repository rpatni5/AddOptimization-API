using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ICreditCardService
    {
        Task<ApiResult<bool>> SaveCreditCardDetails(TemplateEntryDto model);
        Task<ApiResult<TemplateEntryDto>> GetCardDetailsById(Guid id);
        Task<ApiResult<List<TemplateEntryDto>>> Search();
    }
}

