using AddOptimization.Data.Common;

namespace AddOptimization.Data.Entities
{
    public class Country : BaseEntityNew<Guid>
    {
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public bool IsDeleted { get; set; }
    }
}
