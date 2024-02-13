using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Constants;

namespace AddOptimization.API.Common;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class HasPermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly List<string> _requiredScreens;
    private readonly List<string> _requiredFields;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requiredScreen">Semicolon seprated list of screen keys AddOptimization.Contracts.Constants.ScreenKeys;</param>
    /// <param name="requiredFields">Semicolon seprated list of fields</param>
    public HasPermissionAttribute(string requiredScreen, string requiredFields = null)
    {
        _requiredScreens = requiredScreen.Split(',').ToList();
        _requiredFields = (requiredFields?? GlobalFields.Read).Split(',').ToList(); ;
    }

    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Unauthorized);
            return;
        }
        //var permissionService = context.HttpContext.RequestServices.GetService(typeof(IPermissionService)) as IPermissionService;
        //var hasPermission =await permissionService.CheckPermissionForUser(_requiredScreens,_requiredFields);
        var hasPermission = true;
        if (!hasPermission)
        {
            context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            return;
        }
    }
}
