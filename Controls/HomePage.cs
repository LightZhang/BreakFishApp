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

            _countdown.Text = TimeHelper.FormatCountdown(state.Countdown);
            _hint.Text = HintFor(state.Status);

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
