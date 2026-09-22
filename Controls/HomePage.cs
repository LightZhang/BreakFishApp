using System;
using System.Drawing;
using System.Windows.Forms;
using BreakFishApp.Models;
using BreakFishApp.Utils;

namespace BreakFishApp.Controls
{
    public sealed class HomePage : UserControl
    {
        private readonly Label _status;
        private readonly Label _countdown;
        private readonly Label _hint;
        private readonly Label _nextTitle;
        private readonly Label _nextMessage;
        private readonly Label _today;
        private readonly Button _rest;

        public event Action RestNow;

        public HomePage()
        {
            BackColor = UiTheme.Paper;
            Font = UiTheme.UiFont;
            Dock = DockStyle.Fill;
            Padding = new Padding(28, 20, 28, 20);

            var brand = new Label
            {
                Text = "🐟  FishBreak",
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.Ink,
                AutoSize = true,
                Location = new Point(28, 18)
            };

            _status = new Label
            {
                Text = "正在工作",
                Font = UiTheme.StatusFont,
                ForeColor = UiTheme.Accent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(0, 36, 0, 0)
            };

            _countdown = new Label
            {
                Text = "--:--",
                Font = UiTheme.ClockFont,
                ForeColor = UiTheme.Ink,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 72
            };

            _hint = new Label
            {
                Text = "距离下一次提醒",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Mute,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 24
            };

            var card = new Panel
            {
                BackColor = UiTheme.Panel,
                Dock = DockStyle.Top,
                Height = 168,
                Padding = new Padding(18),
                Margin = new Padding(0, 16, 0, 0)
            };

            var nextLabel = new Label
            {
                Text = "下一步",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Mute,
                Dock = DockStyle.Top,
                Height = 22
            };

            _nextTitle = new Label
            {
                Text = "起来走走",
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.Ink,
                Dock = DockStyle.Top,
                Height = 28
            };

            _nextMessage = new Label
            {
                Text = "站起来活动 3～5 分钟",
                ForeColor = UiTheme.Mute,
                Dock = DockStyle.Top,
                Height = 48
            };

            _rest = UiTheme.PrimaryButton("立即休息");
            _rest.Dock = DockStyle.Bottom;
            _rest.Click += delegate
            {
                if (RestNow != null)
                {
                    RestNow();
                }
            };

            card.Controls.Add(_rest);
            card.Controls.Add(_nextMessage);
            card.Controls.Add(_nextTitle);
            card.Controls.Add(nextLabel);

            _today = new Label
            {
                Text = "今日：工作 0m · 休息 0m",
                ForeColor = UiTheme.Mute,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 28
            };

            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 8) };
            body.Controls.Add(card);
            body.Controls.Add(_hint);
            body.Controls.Add(_countdown);
            body.Controls.Add(_status);

            Controls.Add(_today);
            Controls.Add(body);
            Controls.Add(brand);
        }

        public void Bind(ScheduleState state)
        {
            if (state == null)
            {
                return;
            }

            _status.Text = state.StatusText;
            _countdown.Text = TimeHelper.FormatCountdown(state.Countdown);

            if (state.Status == WorkStatus.Breaking)
            {
                _hint.Text = "距离休息结束";
            }
            else if (state.Status == WorkStatus.Lunch)
            {
                _hint.Text = "距离午休结束";
            }
            else if (state.Status == WorkStatus.Idle)
            {
                _hint.Text = "距离上班";
            }
            else if (state.Status == WorkStatus.Finished)
            {
                _hint.Text = "今天已经结束";
                _countdown.Text = "--:--";
            }
            else if (state.Status == WorkStatus.Paused)
            {
                _hint.Text = "距离恢复提醒";
            }
            else
            {
                _hint.Text = "距离下一次提醒";
            }

            if (state.NextReminder != null)
            {
                _nextTitle.Text = (state.NextReminder.Emoji ?? "") + "  " + state.NextReminder.Title;
                _nextMessage.Text = state.NextReminder.Message;
            }
            else if (state.Status == WorkStatus.Finished)
            {
                _nextTitle.Text = "🏠  下班啦";
                _nextMessage.Text = "今天的安排已经结束。";
            }
            else
            {
                _nextTitle.Text = "暂无提醒";
                _nextMessage.Text = "当前不在工作提醒时段。";
            }

            _today.Text = "今日：工作 " + TimeHelper.FormatDuration(state.WorkedSeconds) +
                          " · 休息 " + TimeHelper.FormatDuration(state.BreakSeconds);
            _rest.Enabled = state.Status == WorkStatus.Working;
        }
    }
}
