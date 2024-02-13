using AddOptimization.Utilities.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AddOptimization.Utilities.Extensions;

public static class PageQueryFiterExtension
{
    public static void AddFilter(this PageQueryFiterBase filters,string fieldName,string filterOperator, object value)
    {
        filters.Where ??= new List<Predicate>
        {
            new Predicate()
        };
        filters.RemoveFilter(fieldName);
        var json = JsonSerializer.Serialize(value);
        var jsonElement = JsonDocument.Parse(json).RootElement;
        var predicate = new Predicate
        {
            Field = fieldName,
            Operator = filterOperator,
            Value = jsonElement
        };
        filters.Where.FirstOrDefault().Predicates.Add(predicate);
    }
    public static void RemoveFilter(this PageQueryFiterBase filters,string fieldName=null,bool isComplex=false)
    {
        if (fieldName == null)
        {
            filters.Where = new List<Predicate>();
        }
        if(!(filters.Where.FirstOrDefault()?.Predicates?.Any()??false))
        {
            return;
        }
        filters.Where.FirstOrDefault().Predicates=filters.Where.FirstOrDefault().Predicates.Where(p=> isComplex? p.Predicates.All(pp=> pp.Field != fieldName): p.Field!= fieldName).ToList();
    }
}
