using System;
using System.Threading;
using System.Windows.Forms;
using BreakFishApp.Forms;

namespace BreakFishApp
{
    static class Program
    {
        private const string MutexName = "FishBreak.SingleInstance";
        private const string ShowEventName = "FishBreak.ShowWindow";

        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && string.Equals(args[0], "--self-test", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(SelfTests.Run());
                return;
            }

            bool created;
            using (var mutex = new Mutex(true, MutexName, out created))
            {
                if (!created)
                {
                    using (var ev = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName))
                    {
                        ev.Set();
                    }

                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName))
                {
                    Application.Run(new MainForm(showEvent));
                }
            }
        }
    }
}
