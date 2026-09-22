using System;
using System.Collections.Generic;

namespace BreakFishApp.Services
{
    /// <summary>
    /// 内置中国法定节假日与调休补班（离线兜底）。
    /// Kind: 0=普通日（交给工作日勾选），1=放假，2=调休补班。
    /// </summary>
    public static class HolidayCalendar
    {
        private static readonly Dictionary<string, HolidayInfo> Days = Build();

        public static HolidayInfo GetInfo(DateTime date)
        {
            HolidayInfo info;
            if (Days.TryGetValue(date.Date.ToString("yyyy-MM-dd"), out info))
            {
                return info;
            }

            return new HolidayInfo { Kind = 0, Name = null };
        }

        public static DateTime? GetNextHoliday(DateTime from, out string name)
        {
            name = null;
            var today = from.Date;
            var current = GetInfo(today);
            var currentName = current.Kind == 1 ? current.Name : null;
            var cursor = today.AddDays(1);
            var limit = today.AddYears(1);
            while (cursor <= limit)
            {
                var info = GetInfo(cursor);
                if (info.Kind == 1 && !string.IsNullOrEmpty(info.Name) && info.Name != currentName)
                {
                    name = info.Name;
                    return cursor;
                }

                cursor = cursor.AddDays(1);
            }

            return null;
        }

        private static Dictionary<string, HolidayInfo> Build()
        {
            var map = new Dictionary<string, HolidayInfo>();

            // 2025（国务院办公厅通知）
            AddRange(map, "2025-01-01", "2025-01-01", "元旦");
            AddRange(map, "2025-01-28", "2025-02-04", "春节");
            AddMakeup(map, "2025-01-26");
            AddMakeup(map, "2025-02-08");
            AddRange(map, "2025-04-04", "2025-04-06", "清明节");
            AddRange(map, "2025-05-01", "2025-05-05", "劳动节");
            AddMakeup(map, "2025-04-27");
            AddRange(map, "2025-05-31", "2025-06-02", "端午节");
            AddRange(map, "2025-10-01", "2025-10-08", "国庆节");
            AddMakeup(map, "2025-09-28");
            AddMakeup(map, "2025-10-11");

            // 2026（国务院办公厅通知 国办发明电〔2025〕7号）
            AddRange(map, "2026-01-01", "2026-01-03", "元旦");
            AddMakeup(map, "2026-01-04");
            AddRange(map, "2026-02-15", "2026-02-23", "春节");
            AddMakeup(map, "2026-02-14");
            AddMakeup(map, "2026-02-28");
            AddRange(map, "2026-04-04", "2026-04-06", "清明节");
            AddRange(map, "2026-05-01", "2026-05-05", "劳动节");
            AddMakeup(map, "2026-05-09");
            AddRange(map, "2026-06-19", "2026-06-21", "端午节");
            AddRange(map, "2026-09-25", "2026-09-27", "中秋节");
            AddRange(map, "2026-10-01", "2026-10-07", "国庆节");
            AddMakeup(map, "2026-09-20");
            AddMakeup(map, "2026-10-10");

            return map;
        }

        private static void AddRange(Dictionary<string, HolidayInfo> map, string start, string end, string name)
        {
            var from = DateTime.Parse(start);
            var to = DateTime.Parse(end);
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                map[d.ToString("yyyy-MM-dd")] = new HolidayInfo { Kind = 1, Name = name };
            }
        }

        private static void AddMakeup(Dictionary<string, HolidayInfo> map, string day)
        {
            map[day] = new HolidayInfo { Kind = 2, Name = null };
        }
    }
}
