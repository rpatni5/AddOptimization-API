using AddOptimization.Data.Common;

namespace AddOptimization.Data.Entities
{

    public class LeaveStatuses : BaseEntityNew<Guid>
    {

        public int Id { get; set; }
        public string Name { get; set; }
    }
}
