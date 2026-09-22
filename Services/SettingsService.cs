using System;
using System.IO;
using BreakFishApp.Models;
using BreakFishApp.Utils;

namespace BreakFishApp.Services
{
    public class SettingsService
    {
        private readonly string _path;

        public SettingsService()
        {
            _path = Path.Combine(AppPaths.Root, "settings.json");
        }

        public AppSettings Load()
        {
            var settings = JsonFile.Load<AppSettings>(_path);
            return Normalize(settings);
        }

        public void Save(AppSettings settings)
        {
            JsonFile.Save(_path, Normalize(settings ?? AppSettings.CreateDefault()));
        }

        public AppSettings Reset()
        {
            var settings = AppSettings.CreateDefault();
            Save(settings);
            return settings;
        }

        public static string Validate(AppSettings settings)
        {
            if (settings == null)
            {
                return "设置不能为空。";
            }

            TimeSpan start;
            TimeSpan end;
            if (!TimeHelper.TryParseHm(settings.StartTime, out start))
            {
                return "上班时间格式不正确。";
            }

            if (!TimeHelper.TryParseHm(settings.EndTime, out end))
            {
                return "下班时间格式不正确。";
            }

            if (end <= start)
            {
                return "下班时间必须晚于上班时间。";
            }

            if (settings.WorkMinutes < 15 || settings.WorkMinutes > 180)
            {
                return "工作时长需要在 15～180 分钟之间。";
            }

            if (settings.BreakMinutes < 1 || settings.BreakMinutes > 60)
            {
                return "休息时长需要在 1～60 分钟之间。";
            }

            if (settings.LunchEnabled)
            {
                TimeSpan lunchStart;
                TimeSpan lunchEnd;
                if (!TimeHelper.TryParseHm(settings.LunchStart, out lunchStart) || !TimeHelper.TryParseHm(settings.LunchEnd, out lunchEnd))
                {
                    return "午休时间格式不正确。";
                }

                if (!(start < lunchStart && lunchStart < lunchEnd && lunchEnd < end))
                {
                    return "午休需要落在上班和下班之间，且结束晚于开始。";
                }
            }

            if (settings.WorkDays == null || settings.WorkDays.Count == 0)
            {
                return "请至少选择一个工作日。";
            }

            return null;
        }

        private static AppSettings Normalize(AppSettings settings)
        {
            var defaults = AppSettings.CreateDefault();
            if (settings == null)
            {
                return defaults;
            }

            if (settings.WorkDays == null || settings.WorkDays.Count == 0)
            {
                settings.WorkDays = defaults.WorkDays;
            }

            if (string.IsNullOrWhiteSpace(settings.StartTime))
            {
                settings.StartTime = defaults.StartTime;
            }

            if (string.IsNullOrWhiteSpace(settings.EndTime))
            {
                settings.EndTime = defaults.EndTime;
            }

            if (string.IsNullOrWhiteSpace(settings.LunchStart))
            {
                settings.LunchStart = defaults.LunchStart;
            }

            if (string.IsNullOrWhiteSpace(settings.LunchEnd))
            {
                settings.LunchEnd = defaults.LunchEnd;
            }

            if (settings.WorkMinutes <= 0)
            {
                settings.WorkMinutes = defaults.WorkMinutes;
            }

            if (settings.BreakMinutes <= 0)
            {
                settings.BreakMinutes = defaults.BreakMinutes;
            }

            return settings;
        }
    }

    public class DailyStateService
    {
        public DailyState Load(DateTime day)
        {
            var path = PathFor(day);
            var state = JsonFile.Load<DailyState>(path);
            var key = day.ToString("yyyy-MM-dd");
            if (state.Date != key)
            {
                state = new DailyState { Date = key };
            }

            return state;
        }

        public void Save(DailyState state)
        {
            if (state == null || string.IsNullOrEmpty(state.Date))
            {
                return;
            }

            DateTime day;
            if (!DateTime.TryParse(state.Date, out day))
            {
                day = DateTime.Today;
            }

            JsonFile.Save(PathFor(day), state);
        }

        private static string PathFor(DateTime day)
        {
            return Path.Combine(AppPaths.Root, "daily-" + day.ToString("yyyy-MM-dd") + ".json");
        }
    }

    public static class AppPaths
    {
        public static string Root
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FishBreak");
            }
        }
    }
}
