using Microsoft.AspNetCore.Mvc;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;

namespace AddOptimization.API.Common
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CustomApiControllerBase : ControllerBase
    {
        protected readonly ILogger<CustomApiControllerBase> Logger;
        public CustomApiControllerBase(ILogger<CustomApiControllerBase> logger)
        {
            Logger = logger;
        }
        protected IActionResult HandleResponse<T>(T data, int statusCode = 200)
        {
            if (data == null)
            {
                return NoContent();
            }

            return StatusCode(statusCode, data);
        }

        protected IActionResult HandleException(Exception ex)
        {
            // Log the exception
            Logger.LogError(ex, ex.Message);
            var restult = ApiResult<string>.Failure("InternalServerError");
            // Return an error response to the client
            return StatusCode(500, restult);
        }


        protected IActionResult HandleCustomException(string errorType, string message)
        {
            // Log the exception
            Logger.LogError(message);
            var restult = ApiResult<string>.Failure(errorType, message);
            // Return an error response to the client
            return StatusCode(500, restult);
        }

    }
}
