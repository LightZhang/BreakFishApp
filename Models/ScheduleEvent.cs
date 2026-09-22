using System;

namespace BreakFishApp.Models
{
    public class ScheduleEvent
    {
        public DateTime At { get; set; }

        public string Kind { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Emoji { get; set; }

        public string ReminderType { get; set; }
    }

    public static class ScheduleKinds
    {
        public const string WorkStart = "work-start";
        public const string Reminder = "reminder";
        public const string Lunch = "lunch";
        public const string LunchEnd = "lunch-end";
        public const string WorkEnd = "work-end";
    }
}
