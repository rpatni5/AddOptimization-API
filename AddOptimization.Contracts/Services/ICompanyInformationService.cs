using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
   public interface ICompanyInformationService
    {

        Task<ApiResult<bool>> SaveCompanyInformationDetails(TemplateEntryDto model);
        Task<ApiResult<TemplateEntryDto>> Update(Guid id, TemplateEntryDto model);
        Task<ApiResult<TemplateEntryDto>> GetCompanyDetailsById(Guid id);
        Task<ApiResult<List<TemplateEntryDto>>> Search();
        Task<ApiResult<bool>> Delete(Guid id);
    }
}
