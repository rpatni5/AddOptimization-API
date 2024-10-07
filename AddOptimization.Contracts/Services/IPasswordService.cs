using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface IPasswordService
    {
        Task<ApiResult<bool>> SavePasswordDetails(TemplateEntryDto model);
        Task<ApiResult<List<TemplateEntryDto>>> Search();
        Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model);
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<TemplateEntryDto>> GetPasswordById(Guid id);
    }
}
