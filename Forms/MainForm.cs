using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BreakFishApp.Controls;
using BreakFishApp.Managers;
using BreakFishApp.Models;
using BreakFishApp.Services;

namespace BreakFishApp.Forms
{
    public sealed class MainForm : Form
    {
        private readonly SettingsService _settingsService = new SettingsService();
        private readonly DailyStateService _dailyService = new DailyStateService();
        private readonly WorkScheduler _scheduler;
        private readonly HomePage _home = new HomePage();
        private readonly PlanPage _plan = new PlanPage();
        private readonly StatsPage _stats = new StatsPage();
        private readonly SettingsPage _settingsPage = new SettingsPage();
        private readonly Panel _content = new Panel();
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private readonly EventWaitHandle _showEvent;
        private TrayManager _tray;
        private NotificationService _notifications;
        private AppSettings _settings;
        private bool _allowExit;
        private bool _reminderOpen;
        private Button _navHome;
        private Button _navPlan;
        private Button _navStats;
        private Button _navSettings;

        public MainForm(EventWaitHandle showEvent)
        {
            _showEvent = showEvent;
            _settings = _settingsService.Load();
            var daily = _dailyService.Load(DateTime.Today);
            _scheduler = new WorkScheduler(_settings, daily, new SystemClock(), state => _dailyService.Save(state));
            _scheduler.ReminderDue += OnReminderDue;
            _scheduler.StatusNotice += OnStatusNotice;

            Text = "FishBreak 摸鱼一下";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(430, 680);
            MinimumSize = new Size(400, 620);
            BackColor = UiTheme.Paper;
            Font = UiTheme.UiFont;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            BuildUi();

            _home.RestNow += () => _scheduler.RestNow();
            _settingsPage.Saved += OnSettingsSaved;
            _settingsPage.ResetRequested += OnReset;
            _settingsPage.Bind(_settings, daily.TodayEndTime);

            _timer.Interval = 1000;
            _timer.Tick += OnTick;
            _timer.Start();

            var icon = AppIconFactory.Create();
            Icon = icon;
            _tray = new TrayManager(
                icon,
                () => _scheduler.GetCurrentState(),
                ShowMain,
                ShowSettings,
                () => _scheduler.RestNow(),
                () => _scheduler.Pause(TimeSpan.FromMinutes(15)),
                () => _scheduler.Pause(TimeSpan.FromMinutes(30)),
                () => _scheduler.Pause(TimeSpan.FromHours(1)),
                () => _scheduler.PauseToday(),
                ExitApp);
            _notifications = new NotificationService(_tray.NotifyIcon, () => _settings);

            ShowPage(_home, _navHome);
            RefreshUi();

            if (_showEvent != null)
            {
                var thread = new Thread(ListenShowSignal) { IsBackground = true };
                thread.Start();
            }
        }

        private void BuildUi()
        {
            var nav = new Panel
            {
                Dock = DockStyle.Left,
                Width = 88,
                BackColor = UiTheme.Nav,
                Padding = new Padding(8)
            };

            _navHome = NavButton("首页", 12);
            _navPlan = NavButton("计划", 52);
            _navStats = NavButton("统计", 92);
            _navSettings = NavButton("设置", 132);
            _navHome.Click += delegate { ShowPage(_home, _navHome); };
            _navPlan.Click += delegate
            {
                _plan.Bind(_scheduler.GetCurrentState());
                ShowPage(_plan, _navPlan);
            };
            _navStats.Click += delegate { ShowPage(_stats, _navStats); };
            _navSettings.Click += delegate { ShowPage(_settingsPage, _navSettings); };

            nav.Controls.Add(_navHome);
            nav.Controls.Add(_navPlan);
            nav.Controls.Add(_navStats);
            nav.Controls.Add(_navSettings);

            _content.Dock = DockStyle.Fill;
            _content.BackColor = UiTheme.Paper;
            _home.Dock = DockStyle.Fill;
            _plan.Dock = DockStyle.Fill;
            _stats.Dock = DockStyle.Fill;
            _settingsPage.Dock = DockStyle.Fill;

            Controls.Add(_content);
            Controls.Add(nav);
        }

        private Button NavButton(string text, int y)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(8, y),
                Size = new Size(72, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = UiTheme.Ink,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void ShowPage(Control page, Button nav)
        {
            _content.Controls.Clear();
            _content.Controls.Add(page);
            Highlight(nav);
        }

        private void Highlight(Button active)
        {
            foreach (var button in new[] { _navHome, _navPlan, _navStats, _navSettings })
            {
                button.BackColor = button == active ? UiTheme.AccentSoft : Color.Transparent;
                button.ForeColor = button == active ? UiTheme.Accent : UiTheme.Ink;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            _scheduler.Tick();
            RefreshUi();
        }

        private void RefreshUi()
        {
            var state = _scheduler.GetCurrentState();
            _home.Bind(state);
            _stats.Bind(state);
            if (_tray != null)
            {
                _tray.Refresh();
            }
        }

        private void OnReminderDue(ReminderItem item)
        {
            if (IsDisposed)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                if (_notifications != null)
                {
                    _notifications.ShowReminder(item);
                }

                if (_reminderOpen)
                {
                    return;
                }

                _reminderOpen = true;
                using (var dialog = new ReminderDialog(item))
                {
                    var result = dialog.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        if (dialog.ResultAction == ReminderDialog.ReminderAction.StartBreak)
                        {
                            _scheduler.StartBreak();
                        }
                        else if (dialog.ResultAction == ReminderDialog.ReminderAction.Snooze)
                        {
                            _scheduler.Snooze(TimeSpan.FromMinutes(5));
                        }
                        else
                        {
                            _scheduler.Skip();
                        }
                    }
                    else
                    {
                        _scheduler.Skip();
                    }
                }

                _reminderOpen = false;
                RefreshUi();
            }));
        }

        private void OnStatusNotice(string emoji, string title, string message)
        {
            if (IsDisposed || _notifications == null)
            {
                return;
            }

            BeginInvoke(new Action(() => _notifications.Show(emoji + " " + title, message)));
        }

        private void OnSettingsSaved(AppSettings settings, string todayEnd, bool onlyToday)
        {
            _settings = settings;
            _settingsService.Save(settings);
            StartupService.Apply(settings.AutoStart, Application.ExecutablePath);
            _scheduler.ReplaceSettings(settings);
            _scheduler.SetTodayEnd(todayEnd, onlyToday);
            RefreshUi();
        }

        private void OnReset()
        {
            _settings = _settingsService.Reset();
            _settingsPage.Bind(_settings, null);
            StartupService.Apply(false, Application.ExecutablePath);
            _scheduler.ReplaceSettings(_settings);
            _scheduler.SetTodayEnd(null, false);
            RefreshUi();
        }

        public void ShowMain()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            Activate();
        }

        public void ShowSettings()
        {
            ShowMain();
            ShowPage(_settingsPage, _navSettings);
        }

        private void ExitApp()
        {
            _allowExit = true;
            Close();
        }

        private void ListenShowSignal()
        {
            while (!_allowExit)
            {
                if (_showEvent.WaitOne(500))
                {
                    BeginInvoke(new Action(ShowMain));
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (File.Exists(Path.Combine(AppPaths.Root, "settings.json")))
            {
                BeginInvoke(new Action(() =>
                {
                    Hide();
                    ShowInTaskbar = false;
                }));
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowExit && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;
                return;
            }

            _dailyService.Save(_scheduler.Daily);
            _timer.Stop();
            if (_tray != null)
            {
                _tray.Dispose();
            }

            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                ShowInTaskbar = false;
            }
        }
    }
}
