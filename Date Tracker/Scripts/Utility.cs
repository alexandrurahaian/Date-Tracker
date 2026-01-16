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

        public static string GetTimeLeftStr(DateTime targetDate, bool compact = false, bool countUp = false)
        {
            DateTime now = DateTime.Today;

            if (targetDate <= now) return "Date already passed!";
            DateTime tempDate = now;

            int years = 0;
            int months = 0;
            int days = (targetDate - tempDate).Days;

            if (!countUp)
            {
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
            }
            else
            {
                while (targetDate.Year < tempDate.Year)
                {
                    targetDate = targetDate.AddYears(1);
                    years++;
                }
                while (targetDate.Month < tempDate.Month)
                {
                    targetDate = targetDate.AddMonths(1);
                    months++;
                }
            }

            return (compact ? $"{years}y {months}m {days}d" : $"{years} years {months} months {days} days left");
        }
    }
}
