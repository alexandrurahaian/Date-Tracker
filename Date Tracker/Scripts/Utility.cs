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
            
            int years = 0, months = 0, days = 0;

            while (targetDate.Year < now.Year)
            {
                targetDate = targetDate.AddYears(1);
                years++;
                Debug.WriteLine($"TARGET YEAR: {targetDate.Year}\nCURRENT YEAR: {now.Year}\nCOUNTED YEARS: {years}");
            }
            while (targetDate.Month < now.Month)
            {
                targetDate = targetDate.AddMonths(1);
                months++;
                Debug.WriteLine($"TARGET MONTH: {targetDate.Month}\nCURRENT MONTH: {now.Month}\nCOUNTED MONTHS: {months}");
            }

            days = (now - targetDate).Days;
            return (compact ? $"{years}y {months}m {days}d since" : $"{years} years {months} months {days} days since");
        }
    }
}
