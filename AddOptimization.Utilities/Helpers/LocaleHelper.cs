using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Utilities.Helpers
{
    public class LocaleHelper
    {
        private static readonly CultureInfo FrenchCulture= new CultureInfo("fr-FR");
        public static string FormatCurrency(decimal amount)
        {
            return amount.ToString("C",FrenchCulture);
        }
        public static string FormatDate(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }
        public static string FormatNumber(decimal number)
        {
            CultureInfo culture = new CultureInfo("de-DE");
            if (number % 1 == 0)
            {
                return number.ToString("N0", culture);
            }
            else
            {
                return number.ToString("N2", culture);
            }
        }


    }
}
