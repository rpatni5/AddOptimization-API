using AddOptimization.Utilities.Exceptions;
using AddOptimization.Utilities.Models;
using System.Collections.Generic;

namespace AddOptimization.Utilities.Common
{
    public class PagedApiResult<TModel> : ApiResult<TModel>
    {
        public static PagedApiResult<TModel> Success(Page<TModel> model)
        {
            return new PagedApiResult<TModel>()
            {
                Result = model.Result,
                Count = model.Count,
                Stats = model.Stats
            };
        }
        public PagedApiResult()
        {

        }
        public int Count { get; set; }
        public IDictionary<string, object> Stats { get; set; }
        public new IList<TModel> Result { get; set; }
    }
}