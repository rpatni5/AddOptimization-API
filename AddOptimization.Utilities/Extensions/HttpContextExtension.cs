using Microsoft.AspNetCore.Http;
using AddOptimization.Utilities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AddOptimization.Utilities.Extensions;

public static class HttpContextExtension
{
    public static int? GetCurrentUserId(this HttpContext httpContext)
    {
        if(httpContext.User == null)
        {
            return null;
        }
        var claimVal= httpContext.User.FindFirst("id")?.Value;
        if(claimVal == null)
        {
            return null;
        }
        if(int.TryParse(claimVal, out var userId))
        {
            return userId;
        }
        return null;
    }
    public static string GetCurrentUserFullName(this HttpContext httpContext)
    {
        return httpContext.User?.FindFirst(ClaimTypes.Name)?.Value;
    }
    public static Guid? GetBranchId(this HttpContext httpContext)
    {
        return httpContext.Request.Headers.ContainsKey(RequestHeaders.Branch) ? Guid.Parse(httpContext.Request.Headers[RequestHeaders.Branch]) : null;
    }
    public static List<string> GetCurrentUserRoles(this HttpContext httpContext)
    {
        return httpContext.User?.FindAll(ClaimTypes.Role)?.Select(c=> c.Value)?.ToList() ?? new List<string>();
    }
    public static List<Guid> GetCurrentUserDepartments(this HttpContext httpContext)
    {
        return httpContext.User?.FindAll(CustomClaimType.Department)?.Select(c => Guid.Parse(c.Value))?.ToList() ?? new List<Guid>();
    }
}
