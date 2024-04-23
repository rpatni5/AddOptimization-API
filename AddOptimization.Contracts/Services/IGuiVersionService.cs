using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
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
    }
}
