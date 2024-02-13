using System.Collections.Generic;

namespace AddOptimization.Utilities.Models
{
    public class Page<TModel>
    {
        public int Count { get; set; }

        public List<TModel> Result { get; set; }

        public IDictionary<string, object> Stats { get; set; }
    }
}
