using AddOptimization.Data.Common;
namespace AddOptimization.Data.Entities
{
    public class Setting : BaseEntityNew<Guid>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeleted { get; set; }
    }
}

