using System.Collections.Generic;

namespace BreakFishApp.Models
{
    public class AppSettings
    {
        public List<int> WorkDays { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public bool LunchEnabled { get; set; }

        public string LunchStart { get; set; }

        public string LunchEnd { get; set; }

        public int WorkMinutes { get; set; }

        public int BreakMinutes { get; set; }

        public bool AutoStart { get; set; }

        public bool NotificationEnabled { get; set; }

        public bool SoundEnabled { get; set; }

        public bool HolidayAware { get; set; }

        public static AppSettings CreateDefault()
        {
            return new AppSettings
            {
                WorkDays = new List<int> { 1, 2, 3, 4, 5 },
                StartTime = "09:00",
                EndTime = "18:00",
                LunchEnabled = true,
                LunchStart = "12:00",
                LunchEnd = "13:30",
                WorkMinutes = 50,
                BreakMinutes = 5,
                AutoStart = false,
                NotificationEnabled = true,
                SoundEnabled = true,
                HolidayAware = true
            };
        }
    }
}
