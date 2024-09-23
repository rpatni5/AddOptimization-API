using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;

namespace AddOptimization.Contracts.Services
{
    public interface IGroupService
    {

        Task<ApiResult<List<GroupDto>>> GetAllGroups();
        Task<ApiResult<bool>> Create(CombineGroupModelRequestDto model);
        Task<ApiResult<List<GroupMemberDto>>> GetGroupMembersByGroupId(Guid groupId);
        Task<ApiResult<bool>> Delete(Guid id);
    }
}

