namespace BreakFishApp.Models
{
    public class DailyState
    {
        public string Date { get; set; }

        public string PauseUntil { get; set; }

        public string TodayEndTime { get; set; }

        public string LastReminderType { get; set; }

        public int ReminderCount { get; set; }

        public int SkipCount { get; set; }

        public int WorkedSeconds { get; set; }

        public int BreakSeconds { get; set; }
    }
}
