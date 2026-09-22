using System;
using System.Drawing;
using System.Windows.Forms;
using BreakFishApp.Models;

namespace BreakFishApp.Managers
{
    public sealed class TrayManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _menu;
        private readonly ToolStripMenuItem _statusItem;
        private readonly ToolStripMenuItem _nextItem;
        private readonly Func<ScheduleState> _state;
        private bool _disposed;

        public TrayManager(
            Icon icon,
            Func<ScheduleState> state,
            Action showMain,
            Action showSettings,
            Action restNow,
            Action pause15,
            Action pause30,
            Action pause60,
            Action pauseToday,
            Action exit)
        {
            _state = state;
            _menu = new ContextMenuStrip();
            _menu.Font = UiTheme.UiFont;

            _statusItem = new ToolStripMenuItem("FishBreak") { Enabled = false };
            _nextItem = new ToolStripMenuItem("下一次：--") { Enabled = false };

            var pause = new ToolStripMenuItem("暂停提醒");
            pause.DropDownItems.Add("15分钟", null, delegate { pause15(); });
            pause.DropDownItems.Add("30分钟", null, delegate { pause30(); });
            pause.DropDownItems.Add("1小时", null, delegate { pause60(); });
            pause.DropDownItems.Add("今天", null, delegate { pauseToday(); });

            _menu.Items.Add(_statusItem);
            _menu.Items.Add(_nextItem);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add("立即休息", null, delegate { restNow(); });
            _menu.Items.Add(pause);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add("打开主窗口", null, delegate { showMain(); });
            _menu.Items.Add("设置", null, delegate { showSettings(); });
            _menu.Items.Add("联系作者", null, delegate { ShowAuthor(); });
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add("退出", null, delegate { exit(); });
            _menu.Opening += OnOpening;

            _notifyIcon = new NotifyIcon
            {
                Icon = icon,
                Text = "FishBreak 摸鱼一下",
                Visible = true,
                ContextMenuStrip = _menu
            };
            _notifyIcon.DoubleClick += delegate { showMain(); };
        }

        public NotifyIcon NotifyIcon
        {
            get { return _notifyIcon; }
        }

        public void Refresh()
        {
            var state = _state();
            if (state == null)
            {
                return;
            }

            _notifyIcon.Text = Truncate("FishBreak · " + state.StatusText);
        }

        private void OnOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var state = _state();
            if (state == null)
            {
                return;
            }

            _statusItem.Text = StatusEmoji(state.Status) + " " + state.StatusText;
            if (state.NextReminder != null && state.NextAt.HasValue)
            {
                _nextItem.Text = "下一次：" + state.NextAt.Value.ToString("HH:mm") + " " + state.NextReminder.Title;
            }
            else if (state.NextAt.HasValue)
            {
                _nextItem.Text = "下一次：" + state.NextAt.Value.ToString("HH:mm");
            }
            else
            {
                _nextItem.Text = "下一次：暂无";
            }
        }

        private static void ShowAuthor()
        {
            const string wechat = "z5678701";
            try
            {
                Clipboard.SetText(wechat);
                MessageBox.Show("微信号：" + wechat + "\n已复制到剪贴板，打开微信即可添加。", "联系作者", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("微信号：" + wechat, "联系作者", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string StatusEmoji(WorkStatus status)
        {
            switch (status)
            {
                case WorkStatus.Working:
                    return "🟢";
                case WorkStatus.Breaking:
                    return "🐟";
                case WorkStatus.Lunch:
                    return "🍚";
                case WorkStatus.Paused:
                    return "⏸";
                case WorkStatus.Finished:
                    return "🏠";
                default:
                    return "🐟";
            }
        }

        private static string Truncate(string text)
        {
            if (text == null)
            {
                return "FishBreak";
            }

            return text.Length <= 63 ? text : text.Substring(0, 63);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _menu.Dispose();
        }
    }

    public static class AppIconFactory
    {
        public static Icon Create()
        {
            return FromResource(32);
        }

        public static Icon CreateTray()
        {
            int size = SystemInformation.SmallIconSize.Width;
            if (size < 16)
            {
                size = 16;
            }
            else if (size > 64)
            {
                size = 64;
            }

            return FromResource(size);
        }

        private static Icon FromResource(int size)
        {
            var stream = typeof(AppIconFactory).Assembly.GetManifestResourceStream("BreakFishApp.Assets.app.ico");
            if (stream == null)
            {
                throw new InvalidOperationException("找不到应用图标资源 Assets/app.ico。");
            }

            using (stream)
            {
                return new Icon(stream, size, size);
            }
        }
    }
}
