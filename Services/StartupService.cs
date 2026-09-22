using System;
using Microsoft.Win32;

namespace BreakFishApp.Services
{
    public static class StartupService
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "FishBreak";

        public static void Apply(bool enabled, string executablePath)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                if (key == null)
                {
                    return;
                }

                if (enabled)
                {
                    key.SetValue(ValueName, "\"" + executablePath + "\"");
                }
                else if (key.GetValue(ValueName) != null)
                {
                    key.DeleteValue(ValueName, false);
                }
            }
        }
    }
}
