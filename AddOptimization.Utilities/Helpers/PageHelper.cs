using AddOptimization.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddOptimization.Utilities.Helpers
{
    public class PageHelper<TEntity,TResult> : Page<TResult>
    {
        public static Page<TResult> ApplyPaging(IQueryable<TEntity> data, PageQueryFiterBase model, Func<IQueryable<TEntity>, List<TResult>> mapper, IDictionary<string, object> stats = null)
        {
            if (model.SkipPaging)
            {
                return new Page<TResult> { Result = mapper(data) };
            }
            if (model.Take == 0)
                model.Take = 10;
            if(model.Take > 1000)
            {
                model.Take = 1000;
            }
            var count = data.Count();
            var pagedData=  data.Skip(model.Skip).Take(model.Take);
            return new Page<TResult> { Result= mapper(pagedData),Count= count, Stats=stats };
        }
       
    }
}