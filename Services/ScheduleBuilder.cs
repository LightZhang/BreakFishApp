using System;
using System.Collections.Generic;
using BreakFishApp.Models;
using BreakFishApp.Utils;

namespace BreakFishApp.Services
{
    public static class ScheduleBuilder
    {
        public static List<ScheduleEvent> Build(AppSettings settings, DateTime day, TimeSpan? todayEnd)
        {
            var events = new List<ScheduleEvent>();
            if (settings == null)
            {
                return events;
            }

            var date = day.Date;
            var start = TimeHelper.OnDate(date, settings.StartTime, new TimeSpan(9, 0, 0));
            var endTime = todayEnd.HasValue
                ? todayEnd.Value
                : TimeHelper.ParseHm(settings.EndTime, new TimeSpan(18, 0, 0));
            var end = TimeHelper.OnDate(date, endTime);
            if (end <= start)
            {
                return events;
            }

            var selector = new ReminderSelector();
            string lastType = null;

            events.Add(Make(start, ScheduleKinds.WorkStart, "🟢", "开始工作", "今天的工作开始了。", null));

            DateTime workUntil = end;
            if (settings.LunchEnabled)
            {
                var lunchStart = TimeHelper.OnDate(date, settings.LunchStart, new TimeSpan(12, 0, 0));
                var lunchEnd = TimeHelper.OnDate(date, settings.LunchEnd, new TimeSpan(13, 30, 0));
                if (lunchStart > start && lunchStart < end && lunchEnd > lunchStart)
                {
                    workUntil = lunchStart;
                    AddReminders(events, start, lunchStart, settings, selector, ref lastType);
                    events.Add(Make(lunchStart, ScheduleKinds.Lunch, "🍚", "午休", "先去吃饭，休息一会儿。", null));
                    if (lunchEnd < end)
                    {
                        events.Add(Make(lunchEnd, ScheduleKinds.LunchEnd, "🟢", "开始工作", "下午继续工作。", null));
                        AddReminders(events, lunchEnd, end, settings, selector, ref lastType);
                    }
                }
                else
                {
                    AddReminders(events, start, end, settings, selector, ref lastType);
                }
            }
            else
            {
                AddReminders(events, start, workUntil, settings, selector, ref lastType);
            }

            events.Add(Make(end, ScheduleKinds.WorkEnd, "🏠", "下班", "今天可以下班啦。", null));
            events.Sort((a, b) => a.At.CompareTo(b.At));
            return events;
        }

        private static void AddReminders(
            List<ScheduleEvent> events,
            DateTime from,
            DateTime until,
            AppSettings settings,
            ReminderSelector selector,
            ref string lastType)
        {
            var workMinutes = settings.WorkMinutes > 0 ? settings.WorkMinutes : 50;
            var cursor = from.AddMinutes(workMinutes);
            while (cursor < until)
            {
                var remaining = until - cursor;
                var useBreakType = remaining.TotalMinutes < workMinutes;
                var item = selector.Next(lastType, workMinutes, settings.BreakMinutes, useBreakType);
                lastType = item.Type;
                events.Add(Make(cursor, ScheduleKinds.Reminder, item.Emoji, item.Title, item.Message, item.Type));
                cursor = cursor.AddMinutes(workMinutes);
            }
        }

        private static ScheduleEvent Make(DateTime at, string kind, string emoji, string title, string message, string type)
        {
            return new ScheduleEvent
            {
                At = at,
                Kind = kind,
                Emoji = emoji,
                Title = title,
                Message = message,
                ReminderType = type
            };
        }
    }
}
