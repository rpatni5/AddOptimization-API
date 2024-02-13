using AddOptimization.Utilities.Enums;
using System.Collections.Generic;
using System.Text.Json;

namespace AddOptimization.Utilities.Models;

public class Predicate
{
    public bool IsComplex { get; set; } = true;
    public string Field { get; set; }
    public string Condition { get; set; }= FilterConditions.and.ToString();
    public JsonElement Value { get; set; } 
    public string Operator { get; set; } = OperatorType.equal.ToString();
    public List<Predicate> Predicates { get; set; }=new List<Predicate>();
}
