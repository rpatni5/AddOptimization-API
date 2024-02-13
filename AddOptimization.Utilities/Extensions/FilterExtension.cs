using AddOptimization.Utilities.Enums;
using AddOptimization.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AddOptimization.Utilities.Extensions;

public static class FilterExtension
{
    public static void GetValue<T>(this PageQueryFiterBase filters, string filterName, Action<T> afterGet, OperatorType operatorType = OperatorType.equal, bool? isComplex = false)
    {
        try
        {
            filterName = filterName.ToLower();
            if ((!filters.Where?.Any()) ?? true)
            {
                return;
            }
            var predicatesToSearch = GetAllPredicates(filters.Where);
            if (predicatesToSearch == null) return;
            if (isComplex ?? false)
            {
                var matches = predicatesToSearch.Where(p => p.Field?.ToLower() == filterName && p.Operator == operatorType.ToString()).ToList();
                matches.ForEach(p =>
                {
                    var val = p.Value.Deserialize<T>();
                    afterGet(val);
                });
            }
            else
            {
                var filter = predicatesToSearch.FirstOrDefault(f => f.Field?.ToLower() == filterName && f.Operator == operatorType.ToString());
                if (filter == null)
                {
                    return;
                }

                var val = filter.Value.Deserialize<T>();
                afterGet(val);
            }
           
        }
        catch (Exception)
        {
            return;
        }
    }

    public static void GetList<T>(this PageQueryFiterBase filters, string filterName, Action<List<T>> afterGet, OperatorType operatorType = OperatorType.equal, bool? isComplex = false)
    {
        try
        {
            filterName=filterName.ToLower();
            if ((!filters.Where?.Any()) ?? true)
            {
                return;
            }
            var retVal=Enumerable.Empty<T>().ToList();
            var predicatesToSearch = GetAllPredicates(filters.Where);
            var matches = predicatesToSearch.Where(f => f.Field?.ToLower() == filterName && f.Operator == operatorType.ToString()).ToList();
            matches.ForEach(p =>
            {
                var val = p.Value.Deserialize<T>();
                retVal.Add(val);
            });
            if (retVal.Any())
            {
                afterGet(retVal);
            }

        }
        catch (Exception)
        {
            return;
        }
    }

    private static List<Predicate> GetAllPredicates(List<Predicate> predicates)
    {
        List<Predicate> allPredicates = new List<Predicate>();

        if (predicates != null)
        {
            foreach (var child in predicates)
            {
                if(child.Predicates == null || !child.Predicates.Any())
                allPredicates.Add(child);
                else
                allPredicates.AddRange(GetAllPredicates(child.Predicates));
            }
        }

        return allPredicates;
    }

}
