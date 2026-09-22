using System;
using System.Globalization;

namespace BreakFishApp.Utils
{
    public static class TimeHelper
    {
        public static bool TryParseHm(string value, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            DateTime parsed;
            if (DateTime.TryParseExact(value.Trim(), new[] { "HH:mm", "H:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                time = parsed.TimeOfDay;
                return true;
            }

            return false;
        }

        public static TimeSpan ParseHm(string value, TimeSpan fallback)
        {
            TimeSpan time;
            return TryParseHm(value, out time) ? time : fallback;
        }

        public static DateTime OnDate(DateTime date, TimeSpan timeOfDay)
        {
            var day = date.Date;
            return day.Add(timeOfDay);
        }

        public static DateTime OnDate(DateTime date, string hm, TimeSpan fallback)
        {
            return OnDate(date, ParseHm(hm, fallback));
        }

        public static string FormatHm(TimeSpan time)
        {
            return string.Format("{0:00}:{1:00}", (int)time.TotalHours, time.Minutes);
        }

        public static string FormatDuration(int totalSeconds)
        {
            if (totalSeconds < 0)
            {
                totalSeconds = 0;
            }

            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            if (hours > 0)
            {
                return hours + "h" + minutes.ToString("00") + "m";
            }

            return minutes + "m";
        }

        public static string FormatCountdown(TimeSpan span)
        {
            if (span.TotalSeconds < 0)
            {
                span = TimeSpan.Zero;
            }

            var totalMinutes = (int)span.TotalMinutes;
            var seconds = span.Seconds;
            return string.Format("{0:00}:{1:00}", totalMinutes, seconds);
        }

        public static int ToIsoDay(DayOfWeek day)
        {
            return ((int)day + 6) % 7 + 1;
        }
    }
}
