using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface IGroupService
    {

        Task<ApiResult<List<GroupDto>>> GetAllGroups();
        Task<ApiResult<bool>> Create(CombineGroupModelDto model);
        Task<ApiResult<CombineGroupModelDto>> GetGroupAndMembersByGroupId(Guid groupId);
        Task<ApiResult<bool>> Delete(Guid id);
    }
}

