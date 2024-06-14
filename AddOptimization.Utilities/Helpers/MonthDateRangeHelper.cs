using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddOptimization.Utilities.Helpers
{
    public static class MonthDateRangeHelper
    {
        public static List<MonthDateRange> GetMonthDateRanges(bool isLastDayOfMonth = false)
        {
            var date = DateTime.Today.AddMonths(isLastDayOfMonth ? -2 : -3);
            var startDate = new DateTime(date.Year, date.Month, 1); // First Day of previous 3 Month
            DateTime today = DateTime.Today;
            DateTime endDate = new DateTime(today.Year, isLastDayOfMonth ? today.Month: (today.Month-1), DateTime.DaysInMonth(today.Year, isLastDayOfMonth ? today.Month : (today.Month - 1)));
            List<MonthDateRange> monthDateRanges = new List<MonthDateRange>();
            while (startDate <= endDate)
            {
                var firstDayOfMonth = new DateTime(startDate.Year, startDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                monthDateRanges.Add(new MonthDateRange { StartDate = firstDayOfMonth, EndDate = lastDayOfMonth });

                startDate = lastDayOfMonth.AddDays(1);
            }

            return monthDateRanges;
        }
        public static List<DateTime> FilterWeekdays(List<DateTime> dates) { return dates.Where(date => IsWeekday(date)).ToList(); }
        public static bool IsWeekday(DateTime date) { return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday; }
        public static bool IsSaturday(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday;
        }

        public static bool IsSunday(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Sunday;
        }
    }


    public class MonthDateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}