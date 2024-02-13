namespace AddOptimization.Data.Common
{
    public class BaseEntity<TId>: BaseEntityCreatedDateOnly<TId>
    {
        public DateTime? UpdatedAt { get; set; }
    }
}
