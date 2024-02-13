using System;
using System.Globalization;

namespace AddOptimization.Utilities.Helpers
{
    public static class DateTimeHelper
    {
        public static TimeSpan UtcEstOffset = new(18, 30, 0);
        public static TimeSpan ServerEstOffset = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time").GetUtcOffset(DateTime.Now).Subtract(UtcEstOffset);

        public enum DateStringType
        {
            TimeOnly,
            DateOnly,
            DateTime,
            Month
        };

       

        public static DateTime BOD
        {
            get { return DateTime.UtcNow.ToEST().Date.ToUtc(); }
        }

        public static DateTime OneMonthBeforeToday
        {
            get { return BOD.AddMonths(-1); }
        }

        public static DateTime BodUtc
        {
            get { return DateTime.UtcNow.Date; }
        }

        public static DateTime EodUtc
        {
            get { return BodUtc.Date.AddHours(23).AddMinutes(59).AddSeconds(59); }
        }

        public static string DecimalHoursToHHmm(decimal hours)
        {
            var timeSpan = TimeSpan.FromHours(Convert.ToDouble(hours));
            var hh = timeSpan.Hours + TimeSpan.FromDays(timeSpan.Days).TotalHours;
            var mm = timeSpan.Minutes;
            var shh = hh < 10 ? $"0{hh}" : Convert.ToString(hh);
            var smm = mm < 10 ? $"0{mm}" : Convert.ToString(mm);
            return $"{shh}:{smm}";
        }

        public static DateTime EOD
        {
            get { return BOD.AddDays(1).AddSeconds(-1); }
        }

        public static CultureInfo Culture
        {
            get { return new CultureInfo("en-US"); }
        }

        public static DateTime FirstDayOfCurrentMonth
        {
            get { return GetFirstDayOfMonth(DateTime.Today.Month, DateTime.Today.Year); }
        }

        public static DateTime FirstDayOfCurrentWeek => DateTime.Today.StartOfWeek(DayOfWeek.Monday);

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }

        public static int DayDiff(this DateTime endDate,DateTime startDate,bool roundOff=true)
        {
            TimeSpan ts = endDate - startDate;
            if(!roundOff)
            {
                return ts.Days;
            }
            return (int)Math.Round(ts.TotalDays);
        }

        public static double DayDiffDouble(this DateTime endDate, DateTime startDate,int digits=2)
        {
            TimeSpan ts = endDate - startDate;
            return double.Round(ts.TotalDays, digits);
        }

        public static DateTime FirstDayOfYear
        {
            get { return GetFirstDayOfMonth(1, DateTime.Today.Year); }
        }

        public static DateTime LastDayOfCurrentMonth => GetLastDayOfMonth(DateTime.Today.Month, DateTime.Today.Year);

        public static DateTime LastDayOfCurrentWeek => FirstDayOfCurrentWeek.AddDays(7).AddSeconds(-1);

        public static DateTime LastDayOfYear
        {
            get { return GetLastDayOfMonth(12, DateTime.Today.Year); }
        }

        public static DateTime Yesterday
        {
            get { return BOD.AddDays(-1); }
        }

        public static DateTime GetBOD(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        }

        public static DateTime GetESTDate(this DateTime dateTime)
        {
            var date = dateTime.Date;

            return date.Add(date.Kind == DateTimeKind.Utc ?
                UtcEstOffset :
                ServerEstOffset);
        }

        public static DateTime GetBOW(DateTime date)
        {
            // first day Monday
            int offsetDays = date.DayOfWeek == DayOfWeek.Sunday ? 6 : date.DayOfWeek - DayOfWeek.Monday;
            return date.GetESTDate().AddDays(-offsetDays);
        }

        public static DateTime GetEOW(DateTime date)
        {
            // last day Sunday
            int offsetDays = date.DayOfWeek == DayOfWeek.Sunday ? -1 : DayOfWeek.Saturday - date.DayOfWeek;
            return date.GetESTDate().AddDays(offsetDays + 2);
        }

        public static DateTime GetEOD(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999);
        }

        public static DateTime GetFirstDayOfMonth(DateTime date)
        {
            return GetFirstDayOfMonth(date.Month, date.Year);
        }
        public static DateTime GetWeekStartDate(int year, int week)
        {
            DateTime jan1 = new(year, 1, 1);
            DateTime startOfWeek = jan1.AddDays((week - 1) * 7);
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }
            return startOfWeek;
        }

        public static DateTime GetWeekEndDate(int year, int week)
        {
            DateTime startOfWeek = GetWeekStartDate(year, week);
            return startOfWeek.AddDays(6);
        }

        public static DateTime GetFirstDayOfMonth(int month, int year)
        {
            return new DateTime(year, month, 1);
        }

        public static DateTime GetLastDayOfMonth(DateTime date)
        {
            var totalDaysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            return new DateTime(date.Year, date.Month, totalDaysInMonth);
        }

        public static DateTime GetLastDayOfMonth(int month, int year)
        {
            // get first day of next month
            // move one day back
            return GetLastDayOfMonth(new DateTime(year, month, 1));
        }

       

        public static DateTime ToUtc(this DateTime dateTime)
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            try
            {
                return TimeZoneInfo.ConvertTimeToUtc(dateTime, localTimeZone);
            }
            catch (Exception)
            {
                dateTime = TimeZoneInfo.ConvertTime(dateTime, localTimeZone);
                return TimeZoneInfo.ConvertTimeToUtc(dateTime, localTimeZone);
            }
        }

        public static DateTime ToUtc(this DateTime dateTime, string sourceTimeZoneId)
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(sourceTimeZoneId);
            try
            {
                return TimeZoneInfo.ConvertTimeToUtc(dateTime, localTimeZone);
            }
            catch (Exception)
            {
                dateTime = TimeZoneInfo.ConvertTime(dateTime, localTimeZone);
                return TimeZoneInfo.ConvertTimeToUtc(dateTime, localTimeZone);
            }
        }
            public static DateTime ToEST(this DateTime utcDateTime)
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, localTimeZone);
            }
            catch (Exception)
            {
                utcDateTime = TimeZoneInfo.ConvertTime(utcDateTime, localTimeZone);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, localTimeZone);
            }
        }

        public static DateTime ToZone(this DateTime dateTime, string timeZoneId)
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, localTimeZone);
            }
            catch (Exception)
            {
                dateTime = TimeZoneInfo.ConvertTime(dateTime, localTimeZone);
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, localTimeZone);
            }
        }

        public static string ToString(DateTime? dateTime, DateStringType type = DateStringType.DateTime)
        {
            if (!dateTime.HasValue) return "";
            return ToString(dateTime.Value, type);
        }

        public static string ToString(DateTime dateTime, DateStringType type = DateStringType.DateTime)
        {
            switch (type)
            {
                case DateStringType.DateOnly:
                    return $"{dateTime:MMM d, yyyy}";

                case DateStringType.TimeOnly:
                    return dateTime.ToString(Culture.DateTimeFormat.ShortTimePattern);

                case DateStringType.Month:
                    return dateTime.ToString("MMM yyyy");

                default:
                    return dateTime.ToUniversalTime().ToString(Culture.DateTimeFormat.UniversalSortableDateTimePattern);
            }
        }
    }
}