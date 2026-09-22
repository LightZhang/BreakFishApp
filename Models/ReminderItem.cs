namespace BreakFishApp.Models
{
    public class ReminderItem
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public int DurationMinutes { get; set; }

        public bool Enabled { get; set; }

        public string Emoji { get; set; }
    }
}
