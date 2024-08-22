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
    }
}
