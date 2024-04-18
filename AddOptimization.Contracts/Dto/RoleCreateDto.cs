namespace AddOptimization.Contracts.Dto;

public class RoleCreateDto : BaseDto<Guid>
{
    public bool IsDeleted { get; set; }
}
