using System;
using System.Collections.Generic;
using System.Globalization;
using BreakFishApp.Models;
using BreakFishApp.Utils;

namespace BreakFishApp.Services
{
    public class WorkScheduler
    {
        public event Action<ReminderItem> ReminderDue;
        public event Action<string, string, string> StatusNotice;

        private readonly IClock _clock;
        private readonly Action<DailyState> _saveDaily;
        private readonly ReminderSelector _selector;

        private AppSettings _settings;
        private DailyState _daily;
        private List<ScheduleEvent> _plan = new List<ScheduleEvent>();
        private DateTime? _nextReminderAt;
        private DateTime? _breakUntil;
        private WorkStatus _status = WorkStatus.Idle;
        private ReminderItem _nextReminder;
        private bool _reminderPending;
        private bool _lunchNotified;
        private bool _endNotified;
        private DateTime _lastTick;
        private DateTime _currentDay;

        public WorkScheduler(AppSettings settings, DailyState daily, IClock clock, Action<DailyState> saveDaily)
        {
            _settings = settings ?? AppSettings.CreateDefault();
            _daily = daily ?? new DailyState();
            _clock = clock ?? new SystemClock();
            _saveDaily = saveDaily;
            _selector = new ReminderSelector();
            _lastTick = _clock.Now;
            _currentDay = _clock.Now.Date;
            RebuildPlan();
            InitializeNext();
        }

        public void ReplaceSettings(AppSettings settings)
        {
            _settings = settings ?? AppSettings.CreateDefault();
            RebuildPlan();
            if (!_reminderPending && _status != WorkStatus.Breaking)
            {
                InitializeNext();
            }
        }

        public void ReplaceDaily(DailyState daily)
        {
            _daily = daily ?? new DailyState();
        }

        public DailyState Daily
        {
            get { return _daily; }
        }

        public void Tick()
        {
            var now = _clock.Now;
            EnsureDay(now);
            Accumulate(now);
            UpdateStatus(now);

            if (_status == WorkStatus.Working && !_reminderPending && _nextReminderAt.HasValue && now >= _nextReminderAt.Value)
            {
                FireReminder();
            }

            _lastTick = now;
        }

        public ScheduleState GetCurrentState()
        {
            var now = _clock.Now;
            EnsureDay(now);
            UpdateStatus(now);

            var nextAt = GetDisplayNextAt(now);
            var countdown = nextAt.HasValue && nextAt.Value > now ? nextAt.Value - now : TimeSpan.Zero;

            return new ScheduleState
            {
                Status = _status,
                StatusText = ToStatusText(_status, now),
                NextAt = nextAt,
                NextReminder = _nextReminder,
                Countdown = countdown,
                TodayPlan = new List<ScheduleEvent>(_plan),
                WorkedSeconds = _daily.WorkedSeconds,
                BreakSeconds = _daily.BreakSeconds,
                ReminderCount = _daily.ReminderCount,
                SkipCount = _daily.SkipCount,
                ReminderPending = _reminderPending,
                TodayEndTime = _daily.TodayEndTime
            };
        }

        public ReminderItem GetNextReminder()
        {
            return _nextReminder;
        }

        public void StartBreak()
        {
            var now = _clock.Now;
            var minutes = _settings.BreakMinutes > 0 ? _settings.BreakMinutes : 5;
            _breakUntil = now.AddMinutes(minutes);
            _reminderPending = false;
            _status = WorkStatus.Breaking;
            Persist();
        }

        public void Skip()
        {
            var now = _clock.Now;
            _daily.SkipCount++;
            _reminderPending = false;
            _nextReminderAt = ClampNext(now.AddMinutes(WorkMinutes()));
            PrepareNextItem();
            Persist();
        }

        public void Snooze(TimeSpan delay)
        {
            var now = _clock.Now;
            _reminderPending = false;
            _nextReminderAt = ClampNext(now.Add(delay));
            Persist();
        }

        public void Pause(TimeSpan duration)
        {
            var until = _clock.Now.Add(duration);
            _daily.PauseUntil = until.ToString("s");
            _reminderPending = false;
            Persist();
        }

        public void PauseToday()
        {
            var until = _clock.Now.Date.AddDays(1);
            _daily.PauseUntil = until.ToString("s");
            _reminderPending = false;
            Persist();
        }

        public void Resume()
        {
            _daily.PauseUntil = null;
            var now = _clock.Now;
            if (IsWorkingWindow(now))
            {
                _nextReminderAt = ClampNext(now.AddMinutes(WorkMinutes()));
                PrepareNextItem();
            }
            Persist();
        }

        public void RestNow()
        {
            StartBreak();
        }

        public void SetTodayEnd(string hm, bool onlyToday)
        {
            if (!onlyToday)
            {
                _daily.TodayEndTime = null;
            }
            else
            {
                _daily.TodayEndTime = hm;
            }

            RebuildPlan();
            InitializeNext();
            Persist();
        }

        public void AcknowledgeMissed()
        {
            _reminderPending = false;
            var now = _clock.Now;
            _nextReminderAt = NextPlanReminderAfter(now) ?? ClampNext(now.AddMinutes(WorkMinutes()));
            PrepareNextItem();
        }

        private void FireReminder()
        {
            _reminderPending = true;
            _daily.ReminderCount++;
            if (_nextReminder != null)
            {
                _daily.LastReminderType = _nextReminder.Type;
            }

            Persist();
            var handler = ReminderDue;
            if (handler != null)
            {
                handler(_nextReminder);
            }
        }

        private void EnsureDay(DateTime now)
        {
            if (now.Date == _currentDay)
            {
                return;
            }

            _currentDay = now.Date;
            _lunchNotified = false;
            _endNotified = false;
            _reminderPending = false;
            _breakUntil = null;
            _daily = new DailyState { Date = now.Date.ToString("yyyy-MM-dd") };
            RebuildPlan();
            InitializeNext();
            Persist();
        }

        private void Accumulate(DateTime now)
        {
            var delta = now - _lastTick;
            if (delta.TotalSeconds < 0 || delta.TotalMinutes >= 5)
            {
                if (delta.TotalMinutes >= 5)
                {
                    RecoverFromSleep(now);
                }

                return;
            }

            var seconds = (int)Math.Round(delta.TotalSeconds);
            if (seconds <= 0)
            {
                return;
            }

            if (_status == WorkStatus.Working)
            {
                _daily.WorkedSeconds += seconds;
            }
            else if (_status == WorkStatus.Breaking)
            {
                _daily.BreakSeconds += seconds;
            }
        }

        private void RecoverFromSleep(DateTime now)
        {
            _reminderPending = false;
            if (_breakUntil.HasValue && now >= _breakUntil.Value)
            {
                _breakUntil = null;
            }

            if (IsWorkingWindow(now))
            {
                _nextReminderAt = NextPlanReminderAfter(now) ?? ClampNext(now.AddMinutes(WorkMinutes()));
                PrepareNextItem();
            }
        }

        private void UpdateStatus(DateTime now)
        {
            var pauseUntil = ParsePause();
            if (pauseUntil.HasValue && now < pauseUntil.Value)
            {
                _status = WorkStatus.Paused;
                return;
            }

            if (!IsWorkDay(now))
            {
                _status = WorkStatus.Idle;
                _nextReminderAt = null;
                return;
            }

            var start = StartAt(now);
            var end = EndAt(now);
            if (now < start)
            {
                _status = WorkStatus.Idle;
                return;
            }

            if (now >= end)
            {
                if (!_endNotified)
                {
                    _endNotified = true;
                    RaiseNotice("🏠", "下班啦", "今天可以下班啦。");
                }

                _status = WorkStatus.Finished;
                _nextReminderAt = null;
                return;
            }

            if (InLunch(now))
            {
                if (!_lunchNotified)
                {
                    _lunchNotified = true;
                    RaiseNotice("🍚", "午休", "先去吃饭，休息一会儿。");
                }

                _status = WorkStatus.Lunch;
                _reminderPending = false;
                return;
            }

            if (_breakUntil.HasValue)
            {
                if (now < _breakUntil.Value)
                {
                    _status = WorkStatus.Breaking;
                    return;
                }

                _breakUntil = null;
                _nextReminderAt = ClampNext(now.AddMinutes(WorkMinutes()));
                PrepareNextItem();
            }

            _status = WorkStatus.Working;
            if (!_nextReminderAt.HasValue)
            {
                InitializeNext();
            }
        }

        private DateTime? GetDisplayNextAt(DateTime now)
        {
            if (_status == WorkStatus.Breaking && _breakUntil.HasValue)
            {
                return _breakUntil;
            }

            if (_status == WorkStatus.Idle && IsWorkDay(now) && now < StartAt(now))
            {
                return StartAt(now);
            }

            if (_status == WorkStatus.Lunch)
            {
                return LunchEndAt(now);
            }

            if (_status == WorkStatus.Paused)
            {
                return ParsePause();
            }

            return _nextReminderAt;
        }

        private string ToStatusText(WorkStatus status, DateTime now)
        {
            switch (status)
            {
                case WorkStatus.Working:
                    return "正在工作";
                case WorkStatus.Breaking:
                    return "休息中";
                case WorkStatus.Lunch:
                    return "午休中";
                case WorkStatus.Paused:
                    return "已暂停";
                case WorkStatus.Finished:
                    return "已经下班";
                default:
                    if (!IsWorkDay(now))
                    {
                        return "今天休息";
                    }

                    return "还未上班";
            }
        }

        private void RebuildPlan()
        {
            TimeSpan todayEnd;
            TimeSpan? overrideEnd = null;
            if (TimeHelper.TryParseHm(_daily.TodayEndTime, out todayEnd))
            {
                overrideEnd = todayEnd;
            }

            _plan = ScheduleBuilder.Build(_settings, _clock.Now.Date, overrideEnd);
        }

        private void InitializeNext()
        {
            var now = _clock.Now;
            _nextReminderAt = NextPlanReminderAfter(now);
            PrepareNextItem();
        }

        private DateTime? NextPlanReminderAfter(DateTime now)
        {
            foreach (var item in _plan)
            {
                if (item.Kind == ScheduleKinds.Reminder && item.At >= now)
                {
                    return item.At;
                }
            }

            return null;
        }

        private DateTime? ClampNext(DateTime candidate)
        {
            var day = _clock.Now;
            var start = StartAt(day);
            var end = EndAt(day);
            if (candidate < start)
            {
                candidate = start.AddMinutes(WorkMinutes());
            }

            if (_settings.LunchEnabled)
            {
                var lunchStart = LunchStartAt(day);
                var lunchEnd = LunchEndAt(day);
                if (candidate >= lunchStart && candidate < lunchEnd)
                {
                    candidate = lunchEnd.AddMinutes(WorkMinutes());
                }
            }

            if (candidate >= end)
            {
                return null;
            }

            return candidate;
        }

        private void PrepareNextItem()
        {
            if (!_nextReminderAt.HasValue)
            {
                _nextReminder = null;
                return;
            }

            foreach (var item in _plan)
            {
                if (item.Kind == ScheduleKinds.Reminder && item.At == _nextReminderAt.Value)
                {
                    _nextReminder = new ReminderItem
                    {
                        Id = item.Kind,
                        Type = item.ReminderType,
                        Title = item.Title,
                        Message = item.Message,
                        DurationMinutes = _settings.BreakMinutes,
                        Enabled = true,
                        Emoji = item.Emoji
                    };
                    return;
                }
            }

            var lastSlot = false;
            var end = EndAt(_clock.Now);
            if (_nextReminderAt.HasValue && (end - _nextReminderAt.Value).TotalMinutes < WorkMinutes())
            {
                lastSlot = true;
            }

            _nextReminder = _selector.Next(_daily.LastReminderType, WorkMinutes(), _settings.BreakMinutes, lastSlot);
        }

        private bool IsWorkDay(DateTime now)
        {
            if (_settings.WorkDays == null || _settings.WorkDays.Count == 0)
            {
                return false;
            }

            return _settings.WorkDays.Contains(TimeHelper.ToIsoDay(now.DayOfWeek));
        }

        private bool IsWorkingWindow(DateTime now)
        {
            return IsWorkDay(now) && now >= StartAt(now) && now < EndAt(now) && !InLunch(now);
        }

        private bool InLunch(DateTime now)
        {
            if (!_settings.LunchEnabled)
            {
                return false;
            }

            return now >= LunchStartAt(now) && now < LunchEndAt(now);
        }

        private DateTime StartAt(DateTime now)
        {
            return TimeHelper.OnDate(now, _settings.StartTime, new TimeSpan(9, 0, 0));
        }

        private DateTime EndAt(DateTime now)
        {
            TimeSpan todayEnd;
            if (TimeHelper.TryParseHm(_daily.TodayEndTime, out todayEnd))
            {
                return TimeHelper.OnDate(now, todayEnd);
            }

            return TimeHelper.OnDate(now, _settings.EndTime, new TimeSpan(18, 0, 0));
        }

        private DateTime LunchStartAt(DateTime now)
        {
            return TimeHelper.OnDate(now, _settings.LunchStart, new TimeSpan(12, 0, 0));
        }

        private DateTime LunchEndAt(DateTime now)
        {
            return TimeHelper.OnDate(now, _settings.LunchEnd, new TimeSpan(13, 30, 0));
        }

        private int WorkMinutes()
        {
            return _settings.WorkMinutes > 0 ? _settings.WorkMinutes : 50;
        }

        private DateTime? ParsePause()
        {
            if (string.IsNullOrWhiteSpace(_daily.PauseUntil))
            {
                return null;
            }

            DateTime parsed;
            if (DateTime.TryParse(_daily.PauseUntil, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(_daily.PauseUntil, out parsed))
            {
                return parsed;
            }

            return null;
        }

        private void RaiseNotice(string emoji, string title, string message)
        {
            var handler = StatusNotice;
            if (handler != null)
            {
                handler(emoji, title, message);
            }
        }

        private void Persist()
        {
            if (_saveDaily != null)
            {
                _saveDaily(_daily);
            }
        }
    }
}
