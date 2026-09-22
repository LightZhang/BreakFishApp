using System;
using System.Drawing;
using System.Windows.Forms;
using BreakFishApp.Models;
using BreakFishApp.Utils;

namespace BreakFishApp.Controls
{
    public sealed class HomePage : UserControl
    {
        private readonly Label _brand;
        private readonly Label _sub;
        private readonly RoundPanel _badge;
        private readonly Label _badgeText;
        private readonly Label _countdown;
        private readonly Label _hint;
        private readonly Label _nowLabel;
        private readonly Label _offLabel;
        private readonly Label _holidayLabel;
        private readonly RoundPanel _card;
        private readonly Label _nextLabel;
        private readonly Label _nextTitle;
        private readonly Label _nextMessage;
        private readonly Button _rest;
        private readonly Button _settings;
        private readonly Label _statsTitle;
        private readonly Label _work;
        private readonly Label _rest2;
        private readonly Label _reminders;
        private readonly Label _skips;

        public event Action RestNow;
        public event Action OpenSettings;

        public HomePage()
        {
            BackColor = UiTheme.Paper;
            Font = UiTheme.UiFont;
            Dock = DockStyle.Fill;
            Padding = new Padding(28, 22, 28, 18);

            var brandRow = new Panel { Dock = DockStyle.Top, Height = 32 };
            _brand = new Label
            {
                Text = "🐟  FishBreak",
                Font = UiTheme.BrandFont,
                ForeColor = UiTheme.Ink,
                AutoSize = true,
                Dock = DockStyle.Left
            };
            _settings = UiTheme.GhostButton("⚙  设置");
            _settings.Dock = DockStyle.Right;
            _settings.Width = 84;
            _settings.Height = 30;
            _settings.Click += delegate
            {
                if (OpenSettings != null)
                {
                    OpenSettings();
                }
            };
            brandRow.Controls.Add(_brand);
            brandRow.Controls.Add(_settings);

            _sub = new Label
            {
                Text = "记得照顾好自己，别太累啦",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Mute,
                AutoSize = true,
                Dock = DockStyle.Top,
                Height = 20
            };

            _badge = new RoundPanel
            {
                Radius = 14,
                BackColor = UiTheme.AccentSoft,
                Size = new Size(96, 28),
                Dock = DockStyle.Top,
                Height = 36,
                Margin = new Padding(0, 14, 0, 0)
            };
            _badgeText = new Label
            {
                Text = "正在工作",
                Font = UiTheme.UiFont,
                ForeColor = UiTheme.Accent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _badge.Controls.Add(_badgeText);

            _countdown = new Label
            {
                Text = "--:--",
                Font = UiTheme.ClockFont,
                ForeColor = UiTheme.Ink,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 76,
                Margin = new Padding(0, 6, 0, 0)
            };

            _hint = new Label
            {
                Text = "距离下一次提醒",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Mute,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 22
            };

            var clockBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 4, 0, 0)
            };
            _nowLabel = new Label
            {
                Text = "现在 --:--",
                Font = UiTheme.UiFont,
                ForeColor = UiTheme.Ink,
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _offLabel = new Label
            {
                Text = "距下班 --",
                Font = UiTheme.UiFont,
                ForeColor = UiTheme.Mute,
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            clockBar.Controls.Add(_nowLabel);
            clockBar.Controls.Add(_offLabel);

            _holidayLabel = new Label
            {
                Text = "距下一个节假日 --",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Accent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 22
            };

            _card = new RoundPanel
            {
                Radius = UiTheme.Radius,
                BackColor = UiTheme.Panel,
                BorderColor = UiTheme.Line,
                Dock = DockStyle.Top,
                Height = 130,
                Padding = new Padding(18, 16, 18, 16),
                Margin = new Padding(0, 16, 0, 0)
            };

            _nextLabel = new Label
            {
                Text = "下一步",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Mute,
                Dock = DockStyle.Top,
                Height = 22
            };
            _nextTitle = new Label
            {
                Text = "🚶  起来走走",
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.Ink,
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 4, 0, 0)
            };
            _nextMessage = new Label
            {
                Text = "站起来活动 3～5 分钟",
                Font = UiTheme.UiFont,
                ForeColor = UiTheme.Mute,
                Dock = DockStyle.Top,
                Height = 48,
                Margin = new Padding(0, 2, 0, 0)
            };
            _card.Controls.Add(_nextMessage);
            _card.Controls.Add(_nextTitle);
            _card.Controls.Add(_nextLabel);

            _rest = UiTheme.PrimaryButton("立即休息一下");
            _rest.Dock = DockStyle.Top;
            _rest.Height = 42;
            _rest.Margin = new Padding(0, 16, 0, 0);
            _rest.Click += delegate
            {
                if (RestNow != null)
                {
                    RestNow();
                }
            };

            _statsTitle = new Label
            {
                Text = "今日统计",
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.Ink,
                Dock = DockStyle.Top,
                Height = 28,
                Margin = new Padding(0, 18, 0, 6)
            };

            var statsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 86,
                ColumnCount = 4,
                RowCount = 1
            };
            for (var i = 0; i < 4; i++)
            {
                statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }
            _work = AddStat(statsGrid, 0, "工作", "0m", UiTheme.Accent);
            _rest2 = AddStat(statsGrid, 1, "休息", "0m", UiTheme.Break);
            _reminders = AddStat(statsGrid, 2, "提醒", "0", UiTheme.Lunch);
            _skips = AddStat(statsGrid, 3, "跳过", "0", UiTheme.Mute);

            Controls.Add(statsGrid);
            Controls.Add(_statsTitle);
            Controls.Add(_rest);
            Controls.Add(_card);
            Controls.Add(_holidayLabel);
            Controls.Add(clockBar);
            Controls.Add(_hint);
            Controls.Add(_countdown);
            Controls.Add(_badge);
            Controls.Add(_sub);
            Controls.Add(brandRow);
        }

        public void Bind(ScheduleState state)
        {
            if (state == null)
            {
                return;
            }

            _badgeText.Text = state.StatusText ?? "正在工作";
            var badgeColor = StatusColor(state.Status);
            _badge.BackColor = Tint(badgeColor);
            _badgeText.ForeColor = badgeColor;

            _sub.Text = SubtitleFor(state);

            _countdown.Text = TimeHelper.FormatCountdown(state.Countdown);
            _hint.Text = HintFor(state.Status);

            _nowLabel.Text = "现在 " + state.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _offLabel.Text = FormatOff(state);
            _holidayLabel.Text = FormatHoliday(state);

            if (state.NextReminder != null)
            {
                _nextTitle.Text = (state.NextReminder.Emoji ?? "") + "  " + state.NextReminder.Title;
                _nextMessage.Text = state.NextReminder.Message;
            }
            else if (state.Status == WorkStatus.Finished)
            {
                _nextTitle.Text = "🏠  下班啦";
                _nextMessage.Text = "今天的安排已经结束，好好休息吧。";
            }
            else
            {
                _nextTitle.Text = "暂无提醒";
                _nextMessage.Text = "当前不在工作提醒时段，放松一下。";
            }

            _work.Text = TimeHelper.FormatDuration(state.WorkedSeconds);
            _rest2.Text = TimeHelper.FormatDuration(state.BreakSeconds);
            _reminders.Text = state.ReminderCount.ToString();
            _skips.Text = state.SkipCount.ToString();
            _rest.Enabled = state.Status == WorkStatus.Working;
        }

        private static Label AddStat(TableLayoutPanel grid, int col, string caption, string value, Color accent)
        {
            var panel = new RoundPanel
            {
                Radius = 10,
                BackColor = UiTheme.Panel,
                BorderColor = UiTheme.Line,
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
                Padding = new Padding(8, 8, 8, 8)
            };

            var cap = new Label
            {
                Text = caption,
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Mute,
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var val = new Label
            {
                Text = value,
                Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold),
                ForeColor = accent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(val);
            panel.Controls.Add(cap);
            grid.Controls.Add(panel, col, 0);
            return val;
        }

        private static string SubtitleFor(ScheduleState state)
        {
            if (state == null)
            {
                return "记得照顾好自己，别太累啦";
            }

            switch (state.Status)
            {
                case WorkStatus.Working:
                    return PickByTime(state.Now, "上午好，记得喝水活动一下", "中午好，别太赶啦", "下午好，再坚持一会儿", "还在加班吗，别太累啦");
                case WorkStatus.Breaking:
                    return "趁现在起来活动一下吧";
                case WorkStatus.Lunch:
                    return "先去吃饭，休息一会儿";
                case WorkStatus.Paused:
                    return "暂停中，忙完再继续";
                case WorkStatus.Finished:
                    return "今天辛苦啦，好好休息";
                default:
                    if (!IsWorkDay(state))
                    {
                        return "今天休息，放松一下";
                    }
                    return "还没到上班时间，慢慢来";
            }
        }

        private static bool IsWorkDay(ScheduleState state)
        {
            return state.EndAt.HasValue;
        }

        private static string PickByTime(DateTime now, string morning, string noon, string afternoon, string night)
        {
            var h = now.Hour;
            if (h < 11) return morning;
            if (h < 13) return noon;
            if (h < 18) return afternoon;
            return night;
        }

        private static string FormatOff(ScheduleState state)
        {
            if (!state.EndAt.HasValue || state.EndAt.Value <= state.Now)
            {
                return "已下班";
            }

            var left = state.EndAt.Value - state.Now;
            var hours = (int)left.TotalHours;
            var minutes = left.Minutes;
            if (hours > 0)
            {
                return "距下班 " + hours + "h" + minutes.ToString("00") + "m";
            }
            return "距下班 " + minutes + "m";
        }

        private static string FormatHoliday(ScheduleState state)
        {
            if (!state.NextHolidayDate.HasValue || string.IsNullOrEmpty(state.NextHolidayName))
            {
                return "距下一个节假日 --";
            }

            var days = (int)(state.NextHolidayDate.Value.Date - state.Now.Date).TotalDays;
            if (days <= 0)
            {
                return "今天是 " + state.NextHolidayName;
            }
            if (days == 1)
            {
                return "明天是 " + state.NextHolidayName;
            }
            return "距" + state.NextHolidayName + "还有 " + days + " 天";
        }

        private static string HintFor(WorkStatus status)
        {
            switch (status)
            {
                case WorkStatus.Breaking:
                    return "距离休息结束";
                case WorkStatus.Lunch:
                    return "距离午休结束";
                case WorkStatus.Idle:
                    return "距离上班";
                case WorkStatus.Finished:
                    return "今天已经结束，辛苦啦";
                case WorkStatus.Paused:
                    return "已暂停，距离恢复提醒";
                default:
                    return "距离下一次提醒";
            }
        }

        private static Color StatusColor(WorkStatus status)
        {
            switch (status)
            {
                case WorkStatus.Breaking:
                    return UiTheme.Break;
                case WorkStatus.Lunch:
                    return UiTheme.Lunch;
                case WorkStatus.Finished:
                    return UiTheme.Off;
                case WorkStatus.Paused:
                    return UiTheme.Mute;
                default:
                    return UiTheme.Work;
            }
        }

        private static Color Tint(Color c)
        {
            return Color.FromArgb((c.R + 255 * 3) / 4, (c.G + 255 * 3) / 4, (c.B + 255 * 3) / 4);
        }
    }
}
