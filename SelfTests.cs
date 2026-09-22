using System;
using System.Collections.Generic;
using BreakFishApp.Models;
using BreakFishApp.Services;

namespace BreakFishApp
{
    public static class SelfTests
    {
        public static int Run()
        {
            try
            {
                DefaultPlan_MatchesSpecTimes();
                ReminderSelector_DoesNotRepeatType();
                BeforeWork_IsIdle();
                DuringLunch_IsLunch();
                AfterEnd_IsFinished();
                SleepRecovery_DoesNotReplayMissed();
                Snooze_MovesFiveMinutes();
                TodayEnd_RebuildsPlan();
                Pause_BlocksWorking();
                Holiday_OnWorkday_IsIdle();
                Makeup_OnWeekend_IsWorking();
                HolidayDisabled_FallsBackToWorkDays();
                BuiltinCalendar_NextHoliday_FromSep2026();
                HolidayYearParser_MapJson_OffAndMakeup();
                HolidayYearParser_DaysArrayJson();
                Console.WriteLine("self-test ok");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static void DefaultPlan_MatchesSpecTimes()
        {
            var day = new DateTime(2026, 9, 21);
            var plan = ScheduleBuilder.Build(AppSettings.CreateDefault(), day, null);
            AssertEqual("09:00", Find(plan, "开始工作", 0), "first start");
            AssertEqual("09:50", FindKind(plan, ScheduleKinds.Reminder, 0), "r1");
            AssertEqual("10:40", FindKind(plan, ScheduleKinds.Reminder, 1), "r2");
            AssertEqual("11:30", FindKind(plan, ScheduleKinds.Reminder, 2), "r3");
            AssertEqual("12:00", FindTitle(plan, "午休"), "lunch");
            AssertEqual("13:30", Find(plan, "开始工作", 1), "afternoon");
            AssertEqual("14:20", FindKind(plan, ScheduleKinds.Reminder, 3), "r4");
            AssertEqual("17:40", FindKind(plan, ScheduleKinds.Reminder, 7), "last rest");
            AssertEqual("18:00", FindTitle(plan, "下班"), "end");
        }

        private static void ReminderSelector_DoesNotRepeatType()
        {
            var selector = new ReminderSelector(new ReminderCatalog(), null);
            string last = null;
            for (var i = 0; i < 8; i++)
            {
                var item = selector.Next(last, 50, 5, false);
                if (item.Type == last)
                {
                    throw new Exception("type repeated: " + item.Type);
                }

                last = item.Type;
            }
        }

        private static void BeforeWork_IsIdle()
        {
            var clock = new FakeClock(new DateTime(2026, 9, 21, 8, 30, 0));
            var scheduler = Create(clock);
            var state = scheduler.GetCurrentState();
            Assert(state.Status == WorkStatus.Idle, "expected idle");
        }

        private static void DuringLunch_IsLunch()
        {
            var clock = new FakeClock(new DateTime(2026, 9, 21, 12, 10, 0));
            var scheduler = Create(clock);
            scheduler.Tick();
            Assert(scheduler.GetCurrentState().Status == WorkStatus.Lunch, "expected lunch");
        }

        private static void AfterEnd_IsFinished()
        {
            var clock = new FakeClock(new DateTime(2026, 9, 21, 18, 1, 0));
            var scheduler = Create(clock);
            scheduler.Tick();
            Assert(scheduler.GetCurrentState().Status == WorkStatus.Finished, "expected finished");
        }

        private static void SleepRecovery_DoesNotReplayMissed()
        {
            var clock = new FakeClock(new DateTime(2026, 9, 21, 10, 0, 0));
            var scheduler = Create(clock);
            scheduler.Tick();
            clock.Now = new DateTime(2026, 9, 21, 11, 0, 0);
            scheduler.Tick();
            var state = scheduler.GetCurrentState();
            Assert(state.Status == WorkStatus.Working, "working after wake");
            Assert(!state.ReminderPending, "should not pending missed");
            Assert(state.NextAt.HasValue && state.NextAt.Value.Hour == 11 && state.NextAt.Value.Minute == 30, "next should be 11:30");
        }

        private static void Snooze_MovesFiveMinutes()
        {
            var clock = new FakeClock(new DateTime(2026, 9, 21, 9, 50, 0));
            var scheduler = Create(clock);
            scheduler.Tick();
            scheduler.Snooze(TimeSpan.FromMinutes(5));
            var state = scheduler.GetCurrentState();
            Assert(state.NextAt.HasValue && state.NextAt.Value == clock.Now.AddMinutes(5), "snooze 5");
        }

        private static void TodayEnd_RebuildsPlan()
        {
            var clock = new FakeClock(new DateTime(2026, 9, 21, 9, 0, 0));
            var scheduler = Create(clock);
            scheduler.SetTodayEnd("17:00", true);
            var last = scheduler.GetCurrentState().TodayPlan;
            var end = last[last.Count - 1];
            AssertEqual("17:00", end.At.ToString("HH:mm"), "today end");
        }

        private static void Pause_BlocksWorking()
        {
            var clock = new FakeClock(new DateTime(2026, 9, 21, 10, 0, 0));
            var scheduler = Create(clock);
            scheduler.Pause(TimeSpan.FromMinutes(15));
            scheduler.Tick();
            Assert(scheduler.GetCurrentState().Status == WorkStatus.Paused, "paused");
        }

        private static WorkScheduler Create(FakeClock clock)
        {
            return new WorkScheduler(AppSettings.CreateDefault(), new DailyState { Date = clock.Now.ToString("yyyy-MM-dd") }, clock, delegate { }, new FakeHolidayProvider());
        }

        private static WorkScheduler Create(FakeClock clock, AppSettings settings)
        {
            return new WorkScheduler(settings, new DailyState { Date = clock.Now.ToString("yyyy-MM-dd") }, clock, delegate { }, new FakeHolidayProvider());
        }

        private static void Holiday_OnWorkday_IsIdle()
        {
            // 2026-10-01 国庆，周四（本是工作日），启用识别应为休息
            var clock = new FakeClock(new DateTime(2026, 10, 1, 10, 0, 0));
            var scheduler = Create(clock);
            scheduler.Tick();
            Assert(scheduler.GetCurrentState().Status == WorkStatus.Idle, "holiday should be idle");
        }

        private static void Makeup_OnWeekend_IsWorking()
        {
            // 2026-10-10 周六补班，启用识别应为工作
            var clock = new FakeClock(new DateTime(2026, 10, 10, 10, 0, 0));
            var scheduler = Create(clock);
            scheduler.Tick();
            Assert(scheduler.GetCurrentState().Status == WorkStatus.Working, "makeup workday should be working");
        }

        private static void HolidayDisabled_FallsBackToWorkDays()
        {
            // 2026-10-01 国庆周四，关闭识别后按工作日勾选应为工作
            var clock = new FakeClock(new DateTime(2026, 10, 1, 10, 0, 0));
            var settings = AppSettings.CreateDefault();
            settings.HolidayAware = false;
            var scheduler = Create(clock, settings);
            scheduler.Tick();
            Assert(scheduler.GetCurrentState().Status == WorkStatus.Working, "holiday disabled should fall back to workdays");
        }

        private static void BuiltinCalendar_NextHoliday_FromSep2026()
        {
            // 今天是 9/22，下一个法定节假日应是中秋 9/25（不是国庆）
            string name;
            var next = HolidayCalendar.GetNextHoliday(new DateTime(2026, 9, 22), out name);
            Assert(next.HasValue, "builtin next holiday missing");
            AssertEqual("2026-09-25", next.Value.ToString("yyyy-MM-dd"), "next holiday date");
            AssertEqual("中秋节", name, "next holiday name");
        }

        private static void HolidayYearParser_MapJson_OffAndMakeup()
        {
            var json = "{\"2026-09-20\":{\"date\":\"2026-09-20\",\"name\":\"国庆节\",\"isOffDay\":false}," +
                       "\"2026-09-25\":{\"date\":\"2026-09-25\",\"name\":\"中秋节\",\"isOffDay\":true}," +
                       "\"2026-10-10\":{\"date\":\"2026-10-10\",\"name\":\"国庆节\",\"isOffDay\":false}}";
            var map = HolidayYearParser.Parse(json);
            Assert(map["2026-09-25"].Kind == 1, "mid-autumn should be off");
            AssertEqual("中秋节", map["2026-09-25"].Name, "mid-autumn name");
            Assert(map["2026-10-10"].Kind == 2, "national makeup should be work");
            Assert(map["2026-09-20"].Kind == 2, "sep20 makeup should be work");
            string nextName;
            var next = HolidayYearParser.GetNextHoliday(map, new DateTime(2026, 9, 22), out nextName);
            AssertEqual("2026-09-25", next.Value.ToString("yyyy-MM-dd"), "parser next date");
            AssertEqual("中秋节", nextName, "parser next name");
        }

        private static void HolidayYearParser_DaysArrayJson()
        {
            var json = "{\"year\":2026,\"days\":[{\"name\":\"元旦\",\"date\":\"2026-01-01\",\"isOffDay\":true}," +
                       "{\"name\":\"元旦\",\"date\":\"2026-01-04\",\"isOffDay\":false}]}";
            var map = HolidayYearParser.Parse(json);
            Assert(map["2026-01-01"].Kind == 1, "new year off");
            Assert(map["2026-01-04"].Kind == 2, "new year makeup");
        }

        private static string FindKind(List<ScheduleEvent> plan, string kind, int index)
        {
            var n = 0;
            foreach (var item in plan)
            {
                if (item.Kind == kind)
                {
                    if (n == index)
                    {
                        return item.At.ToString("HH:mm");
                    }

                    n++;
                }
            }

            throw new Exception("missing kind " + kind + " #" + index);
        }

        private static string Find(List<ScheduleEvent> plan, string title, int index)
        {
            var n = 0;
            foreach (var item in plan)
            {
                if (item.Title == title)
                {
                    if (n == index)
                    {
                        return item.At.ToString("HH:mm");
                    }

                    n++;
                }
            }

            throw new Exception("missing title " + title);
        }

        private static string FindTitle(List<ScheduleEvent> plan, string title)
        {
            return Find(plan, title, 0);
        }

        private static void AssertEqual(string expected, string actual, string name)
        {
            if (expected != actual)
            {
                throw new Exception(name + ": expected " + expected + " got " + actual);
            }
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
            {
                throw new Exception(name);
            }
        }

        private sealed class FakeHolidayProvider : IHolidayProvider
        {
            public HolidayInfo GetInfo(DateTime date)
            {
                var d = date.Date;
                if (d >= new DateTime(2026, 10, 1) && d <= new DateTime(2026, 10, 8))
                {
                    return new HolidayInfo { Kind = 1, Name = "国庆节" };
                }
                if (d == new DateTime(2026, 10, 10))
                {
                    return new HolidayInfo { Kind = 2, Name = null };
                }
                return new HolidayInfo { Kind = 0, Name = null };
            }

            public DateTime? GetNextHoliday(DateTime from, out string name)
            {
                name = "国庆节";
                return new DateTime(2026, 10, 1);
            }
        }
    }
}
