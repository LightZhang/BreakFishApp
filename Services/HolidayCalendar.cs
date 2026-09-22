using System;
using System.Collections.Generic;

namespace BreakFishApp.Services
{
    /// <summary>
    /// 中国法定节假日与调休补班日历。
    /// 数据以国务院公布的《关于部分节假日安排的通知》为准，按年维护。
    /// 表里没有的年份/日期，会回退到按用户勾选的工作日判断，不会误判。
    /// </summary>
    public static class HolidayCalendar
    {
        private static readonly HashSet<DateTime> Holidays = new HashSet<DateTime>();
        private static readonly HashSet<DateTime> MakeupWorkdays = new HashSet<DateTime>();

        static HolidayCalendar()
        {
            // ===== 2026 年 =====
            // 元旦
            AddHoliday(2026, 1, 1);
            // 春节：2/16(除夕)~2/22 放假调休共7天；2/14、2/15 上班
            AddHoliday(2026, 2, 16);
            AddHoliday(2026, 2, 17);
            AddHoliday(2026, 2, 18);
            AddHoliday(2026, 2, 19);
            AddHoliday(2026, 2, 20);
            AddHoliday(2026, 2, 21);
            AddHoliday(2026, 2, 22);
            AddMakeup(2026, 2, 14);
            AddMakeup(2026, 2, 15);
            // 清明：4/4~4/6
            AddHoliday(2026, 4, 4);
            AddHoliday(2026, 4, 5);
            AddHoliday(2026, 4, 6);
            // 劳动节：5/1~5/5；4/26 上班
            AddHoliday(2026, 5, 1);
            AddHoliday(2026, 5, 2);
            AddHoliday(2026, 5, 3);
            AddHoliday(2026, 5, 4);
            AddHoliday(2026, 5, 5);
            AddMakeup(2026, 4, 26);
            // 端午：6/19~6/21
            AddHoliday(2026, 6, 19);
            AddHoliday(2026, 6, 20);
            AddHoliday(2026, 6, 21);
            // 中秋、国庆：10/1~10/8；9/27、10/10 上班
            AddHoliday(2026, 10, 1);
            AddHoliday(2026, 10, 2);
            AddHoliday(2026, 10, 3);
            AddHoliday(2026, 10, 4);
            AddHoliday(2026, 10, 5);
            AddHoliday(2026, 10, 6);
            AddHoliday(2026, 10, 7);
            AddHoliday(2026, 10, 8);
            AddMakeup(2026, 9, 27);
            AddMakeup(2026, 10, 10);

            // ===== 2027 年及以后 =====
            // 暂未内置。每年通知公布后在此追加 AddHoliday/AddMakeup 即可。
            // 未覆盖的年份会回退到按用户勾选的工作日判断。
        }

        /// <summary>该日是法定节假日（应休息）。</summary>
        public static bool IsHoliday(DateTime date)
        {
            return Holidays.Contains(date.Date);
        }

        /// <summary>该日是调休补班日（周末但应上班）。</summary>
        public static bool IsMakeupWorkday(DateTime date)
        {
            return MakeupWorkdays.Contains(date.Date);
        }

        /// <summary>该日是否已被节假日日历覆盖（用于决定是否回退到工作日勾选）。</summary>
        public static bool IsCovered(DateTime date)
        {
            return Holidays.Contains(date.Date) || MakeupWorkdays.Contains(date.Date);
        }

        private static void AddHoliday(int year, int month, int day)
        {
            Holidays.Add(new DateTime(year, month, day));
        }

        private static void AddMakeup(int year, int month, int day)
        {
            MakeupWorkdays.Add(new DateTime(year, month, day));
        }
    }
}
