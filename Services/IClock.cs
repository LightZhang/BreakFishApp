using System;

namespace BreakFishApp.Services
{
    public interface IClock
    {
        DateTime Now { get; }
    }

    public sealed class SystemClock : IClock
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
    }

    public sealed class FakeClock : IClock
    {
        public FakeClock(DateTime now)
        {
            Now = now;
        }

        public DateTime Now { get; set; }
    }
}
