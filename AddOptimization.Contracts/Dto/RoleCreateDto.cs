namespace AddOptimization.Contracts.Dto;

public class RoleCreateDto : BaseDto<Guid>
{
    public Guid? DepartmentId { get; set; }
    public bool IsDeleted { get; set; }
}
