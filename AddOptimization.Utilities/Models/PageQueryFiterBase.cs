using System.Collections.Generic;

namespace AddOptimization.Utilities.Models
{
    public class PageQueryFiterBase
    {
        public int Take { get; set; } = 10;

        public int Skip { get; set; } = 0;

        public bool SkipPaging { get; set; }

        public List<Predicate> Where { get; set; }
        public List<SortModel> Sorted { get; set; }
        public PageQueryFiterBase()
        {
            Where = new List<Predicate>{ new Predicate()};
        }
    }
}
