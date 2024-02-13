using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace AddOptimization.Utilities.Common
{
    public static class ValidationMessages
    {
        public static string GetValidationMessage(string code)
        {
            if(code == null || code.Length>100)
            {
                return code;
            }
            var workingDir= Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var filePath = $"{workingDir}\\Assets\\Translations\\validationmessages.en.json";
            if (File.Exists(filePath))
            {
                var jsonStr=File.ReadAllText(filePath);
                var obj= JsonSerializer.Deserialize<Dictionary<string, string>>(jsonStr);
                return obj.ContainsKey(code) ? obj[code] : null;
            }
            return code;
        }
       
       
    }
}
