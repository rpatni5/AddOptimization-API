using System.Text.RegularExpressions;

namespace AddOptimization.Utilities.Helpers
{
    public class StringHelper
    {
        public static bool IsValidPhoneNumber(string inpString)
        {
            try
            {
                if (string.IsNullOrEmpty(inpString))
                {
                    return false;
                }
                return Regex.IsMatch(inpString, @"^([0]|\+91)?[789]\d{9}$");
            }
            catch 
            {
                return false;
            }
        }
    }
}
