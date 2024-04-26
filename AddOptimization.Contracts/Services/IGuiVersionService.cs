using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface IGuiVersionService
    {
        Task<ApiResult<GuiVersionResponseDto>> Create(GuiVersionCreateDto model);
        Task<ApiResult<List<GuiVersionResponseDto>>> Search();
        Task<ApiResult<bool>> Delete(Guid id);
        Task<ApiResult<bool>> UpdateStatus(Guid id);
        Task<ApiResult<List<GuiVersionResponseDto>>>GetLatestversion();
    }
}
