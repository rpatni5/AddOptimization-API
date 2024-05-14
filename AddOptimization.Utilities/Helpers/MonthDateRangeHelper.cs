using System;
using System.Collections.Generic;

namespace AddOptimization.Utilities.Helpers
{
    public static class MonthDateRangeHelper
    {
        public static List<MonthDateRange> GetMonthDateRanges()
        {
            int year = DateTime.Now.Year;
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            List<MonthDateRange> monthDateRanges = new List<MonthDateRange>();

            while (startDate <= endDate)
            {
                var firstDayOfMonth = new DateTime(startDate.Year, startDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                monthDateRanges.Add(new MonthDateRange { StartDate = firstDayOfMonth, EndDate = lastDayOfMonth });

                startDate = lastDayOfMonth.AddDays(1);
            }

            return monthDateRanges;
            //foreach (var monthDateRange in monthDateRanges)
            //{
            //    Console.WriteLine($"Start Date: {monthDateRange.StartDate.ToShortDateString()}, End Date: {monthDateRange.EndDate.ToShortDateString()}");
            //}
        }
    }

    public class MonthDateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}