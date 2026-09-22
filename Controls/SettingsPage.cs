using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BreakFishApp.Models;
using BreakFishApp.Services;
using BreakFishApp.Utils;

namespace BreakFishApp.Controls
{
    public sealed class SettingsPage : UserControl
    {
        private readonly CheckBox[] _days = new CheckBox[7];
        private readonly DateTimePicker _start;
        private readonly DateTimePicker _end;
        private readonly CheckBox _lunch;
        private readonly DateTimePicker _lunchStart;
        private readonly DateTimePicker _lunchEnd;
        private readonly NumericUpDown _workMinutes;
        private readonly NumericUpDown _breakMinutes;
        private readonly CheckBox _autoStart;
        private readonly CheckBox _notify;
        private readonly CheckBox _sound;
        private readonly DateTimePicker _todayEnd;
        private readonly CheckBox _todayOnly;
        private readonly Label _error;

        public event Action<AppSettings, string, bool> Saved;
        public event Action ResetRequested;

        public SettingsPage()
        {
            BackColor = UiTheme.Paper;
            Dock = DockStyle.Fill;
            AutoScroll = true;
            Padding = new Padding(20);

            var title = new Label
            {
                Text = "设置",
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.Ink,
                Location = new Point(20, 16),
                AutoSize = true
            };
            Controls.Add(title);

            var y = 52;
            Controls.Add(LabelAt("工作日", y));
            y += 24;
            var names = new[] { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };
            for (var i = 0; i < 7; i++)
            {
                _days[i] = new CheckBox
                {
                    Text = names[i],
                    Location = new Point(20 + (i % 4) * 90, y + (i / 4) * 28),
                    AutoSize = true
                };
                Controls.Add(_days[i]);
            }

            y += 72;
            Controls.Add(LabelAt("上班时间", y));
            _start = TimePicker(120, y - 2);
            Controls.Add(_start);

            y += 36;
            Controls.Add(LabelAt("下班时间", y));
            _end = TimePicker(120, y - 2);
            Controls.Add(_end);

            y += 36;
            _lunch = new CheckBox { Text = "启用午休", Location = new Point(20, y), AutoSize = true, Checked = true };
            Controls.Add(_lunch);

            y += 32;
            Controls.Add(LabelAt("午休开始", y));
            _lunchStart = TimePicker(120, y - 2);
            Controls.Add(_lunchStart);

            y += 36;
            Controls.Add(LabelAt("午休结束", y));
            _lunchEnd = TimePicker(120, y - 2);
            Controls.Add(_lunchEnd);

            y += 40;
            Controls.Add(LabelAt("工作分钟", y));
            _workMinutes = Number(120, y - 2, 15, 180, 50);
            Controls.Add(_workMinutes);

            y += 36;
            Controls.Add(LabelAt("休息分钟", y));
            _breakMinutes = Number(120, y - 2, 1, 60, 5);
            Controls.Add(_breakMinutes);

            y += 40;
            Controls.Add(LabelAt("预设", y));
            y += 24;
            AddPreset("轻量 60/3", 20, y, 60, 3);
            AddPreset("正常 50/5", 110, y, 50, 5);
            y += 36;
            AddPreset("摸鱼 40/5", 20, y, 40, 5);
            AddPreset("佛系 30/5", 110, y, 30, 5);

            y += 44;
            _autoStart = new CheckBox { Text = "Windows 开机自动启动", Location = new Point(20, y), AutoSize = true };
            Controls.Add(_autoStart);

            y += 28;
            _notify = new CheckBox { Text = "启用通知", Location = new Point(20, y), AutoSize = true };
            Controls.Add(_notify);

            y += 28;
            _sound = new CheckBox { Text = "提醒时播放提示音", Location = new Point(20, y), AutoSize = true };
            Controls.Add(_sound);

            y += 36;
            Controls.Add(LabelAt("今天下班", y));
            _todayEnd = TimePicker(120, y - 2);
            Controls.Add(_todayEnd);

            y += 32;
            _todayOnly = new CheckBox { Text = "仅今天（不改默认下班时间）", Location = new Point(20, y), AutoSize = true };
            Controls.Add(_todayOnly);

            y += 40;
            var save = UiTheme.PrimaryButton("保存");
            save.Location = new Point(20, y);
            save.Width = 120;
            save.Click += OnSave;
            Controls.Add(save);

            var reset = UiTheme.GhostButton("恢复默认");
            reset.Location = new Point(150, y);
            reset.Width = 100;
            reset.Click += delegate
            {
                if (ResetRequested != null)
                {
                    ResetRequested();
                }
            };
            Controls.Add(reset);

            _error = new Label
            {
                ForeColor = Color.FromArgb(176, 62, 48),
                Location = new Point(20, y + 48),
                AutoSize = true
            };
            Controls.Add(_error);
        }

        public void Bind(AppSettings settings, string todayEnd)
        {
            if (settings == null)
            {
                return;
            }

            for (var i = 0; i < 7; i++)
            {
                var iso = i + 1;
                _days[i].Checked = settings.WorkDays != null && settings.WorkDays.Contains(iso);
            }

            _start.Value = Today(settings.StartTime, 9, 0);
            _end.Value = Today(settings.EndTime, 18, 0);
            _lunch.Checked = settings.LunchEnabled;
            _lunchStart.Value = Today(settings.LunchStart, 12, 0);
            _lunchEnd.Value = Today(settings.LunchEnd, 13, 30);
            _workMinutes.Value = Clamp(settings.WorkMinutes, 15, 180);
            _breakMinutes.Value = Clamp(settings.BreakMinutes, 1, 60);
            _autoStart.Checked = settings.AutoStart;
            _notify.Checked = settings.NotificationEnabled;
            _sound.Checked = settings.SoundEnabled;
            _todayEnd.Value = string.IsNullOrEmpty(todayEnd) ? _end.Value : Today(todayEnd, 18, 0);
            _todayOnly.Checked = !string.IsNullOrEmpty(todayEnd);
            _error.Text = string.Empty;
        }

        private void OnSave(object sender, EventArgs e)
        {
            var settings = ReadSettings();
            var message = SettingsService.Validate(settings);
            if (message != null)
            {
                _error.Text = message;
                return;
            }

            _error.Text = string.Empty;
            if (Saved != null)
            {
                Saved(settings, TimeHelper.FormatHm(_todayEnd.Value.TimeOfDay), _todayOnly.Checked);
            }
        }

        private AppSettings ReadSettings()
        {
            var days = new List<int>();
            for (var i = 0; i < 7; i++)
            {
                if (_days[i].Checked)
                {
                    days.Add(i + 1);
                }
            }

            return new AppSettings
            {
                WorkDays = days,
                StartTime = TimeHelper.FormatHm(_start.Value.TimeOfDay),
                EndTime = TimeHelper.FormatHm(_end.Value.TimeOfDay),
                LunchEnabled = _lunch.Checked,
                LunchStart = TimeHelper.FormatHm(_lunchStart.Value.TimeOfDay),
                LunchEnd = TimeHelper.FormatHm(_lunchEnd.Value.TimeOfDay),
                WorkMinutes = (int)_workMinutes.Value,
                BreakMinutes = (int)_breakMinutes.Value,
                AutoStart = _autoStart.Checked,
                NotificationEnabled = _notify.Checked,
                SoundEnabled = _sound.Checked
            };
        }

        private void AddPreset(string text, int x, int y, int work, int rest)
        {
            var button = UiTheme.GhostButton(text);
            button.Location = new Point(x, y);
            button.Width = 84;
            button.Click += delegate
            {
                _workMinutes.Value = work;
                _breakMinutes.Value = rest;
            };
            Controls.Add(button);
        }

        private static Label LabelAt(string text, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(20, y),
                AutoSize = true,
                ForeColor = UiTheme.Ink
            };
        }

        private static DateTimePicker TimePicker(int x, int y)
        {
            return new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Location = new Point(x, y),
                Width = 90
            };
        }

        private static NumericUpDown Number(int x, int y, int min, int max, int value)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Width = 90,
                Minimum = min,
                Maximum = max,
                Value = value
            };
        }

        private static DateTime Today(string hm, int hour, int minute)
        {
            var time = TimeHelper.ParseHm(hm, new TimeSpan(hour, minute, 0));
            return DateTime.Today.Add(time);
        }

        private static decimal Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
