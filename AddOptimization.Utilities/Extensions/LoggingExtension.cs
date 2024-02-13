using Microsoft.Extensions.Logging;
using System;

namespace AddOptimization.Utilities.Extensions
{
    public static class LoggingExtension
    {
        public static void LogException(this ILogger logger,Exception ex)
        {
            logger.LogError($"{ex.Message} {ex.StackTrace}");
        }
    }
}
