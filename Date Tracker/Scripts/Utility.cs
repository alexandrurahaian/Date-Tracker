using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Date_Tracker.Scripts
{
    public static class Utility
    {
        private static Random random = new Random();

        public static int GenerateUID()
        {
            return random.Next(999999999);
        }

        public static string GetTimeLeftStr(DateTime targetDate, bool compact = false)
        {
            DateTime now = DateTime.Today;

            if (targetDate <= now) return "Date already passed!";
            DateTime tempDate = now;

            int years = 0;
            int months = 0;
            int days = (targetDate - tempDate).Days;

            while (tempDate.Year < targetDate.Year)
            {
                tempDate = tempDate.AddYears(1);
                years++;
            }
            while (tempDate.Month < targetDate.Month)
            {
                tempDate = tempDate.AddMonths(1);
                months++;
            }

            return (compact ? $"{years}y {months}m {days}d left" : $"{years} years {months} months {days} days left");
        }

        public static string GetTimeSinceStr(DateTime targetDate, bool compact = false)
        {
            DateTime now = DateTime.Today;
            if (targetDate > now) return "Date is in the future!";

            int years = now.Year - targetDate.Year;
            int months = now.Month - targetDate.Month;
            int days = now.Day - targetDate.Day;

            if (days < 0)
            {
                months--;
                var prevMonth = now.AddMonths(-1);
                days += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            return compact
                ? $"{years}y {months}m {days}d since"
                : $"{years} years {months} months {days} days since";
        }

    }
}
