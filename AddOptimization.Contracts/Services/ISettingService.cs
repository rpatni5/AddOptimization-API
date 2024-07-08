using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface ISettingService
    {
        Task<ApiResult<SettingDto>> GetSettingByCode(string settingCode);
    }
}
