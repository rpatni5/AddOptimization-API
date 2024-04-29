
using System;

namespace AddOptimization.Utilities.Common;

public class ApiResult<TModel> 
{
    public ApiResult(string code,string message=null)
    {
        Message = message??(code == null ? null : ValidationMessages.GetValidationMessage(code));
        Code = code;
    }

    public ApiResult()
    {
    }

    public string Code { get; set; }

    public TModel Result { get; set; }

    public string Message { get; set; }

    public bool IsSuccess { get; set; }


    public static ApiResult<TModel> Failure(string code = null)
    {
        return new ApiResult<TModel>(code) { IsSuccess=false};
    }
    public static ApiResult<TModel> Failure(string code = null,string message=null)
    {
        return new ApiResult<TModel>(code,message) { IsSuccess = false };
    }
    public static ApiResult<TModel> NotFound(string entityName = null)
    {
        return new ApiResult<TModel>("NotFound", $"{entityName??"Entity"} not found") { IsSuccess = false };
    }

    public static ApiResult<TModel> EntityAlreadyExists(string entityName = null,string with=null)
    {
        return new ApiResult<TModel>("AlreadyExists", $"{entityName ?? "Entity"} {(with != null ? " with selected "+with : string.Empty)} already exists in the system.") { IsSuccess = false };
    }

    public static ApiResult<TModel> Success(string code=null)
    {
        return new ApiResult<TModel>(code) { IsSuccess = true };
    }

    public static ApiResult<TModel> Success(TModel data)
    {
        var result = new ApiResult<TModel>(null, null) { Result = data,IsSuccess=true };
        return result;
    }

    public static ApiResult<TModel> Success(TModel data,string code)
    {
        var result = new ApiResult<TModel>(code) { Result = data,IsSuccess=true };
        return result;
    }

   }