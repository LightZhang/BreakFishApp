using System;
using System.Collections.Generic;

namespace BreakFishApp.Models
{
    public class ScheduleState
    {
        public WorkStatus Status { get; set; }

        public string StatusText { get; set; }

        public DateTime? NextAt { get; set; }

        public ReminderItem NextReminder { get; set; }

        public TimeSpan Countdown { get; set; }

        public List<ScheduleEvent> TodayPlan { get; set; }

        public int WorkedSeconds { get; set; }

        public int BreakSeconds { get; set; }

        public int ReminderCount { get; set; }

        public int SkipCount { get; set; }

        public bool ReminderPending { get; set; }

        public string TodayEndTime { get; set; }

        public DateTime Now { get; set; }

        public DateTime? EndAt { get; set; }

        public DateTime? NextHolidayDate { get; set; }

        public string NextHolidayName { get; set; }
    }
}
