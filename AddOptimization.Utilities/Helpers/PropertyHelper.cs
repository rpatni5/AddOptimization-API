using System;
using System.Globalization;

namespace AddOptimization.Utilities.Helpers
{
    public static class PropertyHelper
    {
        public static decimal? TruncateDecimal(decimal? value, int decimalPlaces)
        {
            if (value == null || value == 0)
                return null;
            var integralValue = Math.Truncate(value ?? 0);

            var fraction = (value ?? 0) - integralValue;

            var factor = (decimal)Math.Pow(10, decimalPlaces);

            var truncatedFraction = Math.Truncate(fraction * factor) / factor;

            var result = integralValue + truncatedFraction;

            return result;
        }

        public static string FormatCurrency(this decimal? value)
        {
            var cultureInfo = new CultureInfo("en-US");
            var numberFormatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
            numberFormatInfo.CurrencySymbol = "$";
            return (value??0).ToString("C", numberFormatInfo);
        }

        public static string ToPascalCase(this string text)
        {
            if (text == null)
                return null;
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(text);
        }
    }
}