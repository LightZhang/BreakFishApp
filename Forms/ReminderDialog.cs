using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BreakFishApp.Managers;
using BreakFishApp.Models;

namespace BreakFishApp.Forms
{
    public sealed class ReminderDialog : Form
    {
        public enum ReminderAction
        {
            StartBreak,
            Snooze,
            Skip
        }

        private readonly Label _emoji;
        private readonly Label _title;
        private readonly Label _message;
        private readonly Func<ReminderItem> _replacer;

        public ReminderAction ResultAction { get; private set; }

        public ReminderDialog(ReminderItem item)
            : this(item, null)
        {
        }

        public ReminderDialog(ReminderItem item, Func<ReminderItem> replacer)
        {
            _replacer = replacer;

            Text = "FishBreak";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            ClientSize = new Size(380, 340);
            BackColor = UiTheme.Paper;
            Font = UiTheme.UiFont;
            Icon = AppIconFactory.Create();

            _emoji = new Label
            {
                Text = item != null ? (item.Emoji ?? "🐟") : "🐟",
                Font = new Font("Microsoft YaHei UI", 28F),
                ForeColor = UiTheme.Ink,
                Location = new Point(24, 22),
                AutoSize = true
            };

            _title = new Label
            {
                Text = item != null ? (item.Title ?? "该休息一下了") : "该休息一下了",
                Font = UiTheme.StatusFont,
                ForeColor = UiTheme.Ink,
                Location = new Point(76, 30),
                AutoSize = true
            };

            _message = new Label
            {
                Text = item != null
                    ? (item.Message ?? "站起来活动几分钟，再回来继续工作。")
                    : "站起来活动几分钟，再回来继续工作。",
                Font = UiTheme.UiFont,
                ForeColor = UiTheme.Mute,
                Location = new Point(24, 84),
                Size = new Size(332, 60)
            };

            var change = UiTheme.GhostButton("🔄  换一个");
            change.SetBounds(24, 150, 110, 30);
            change.Click += OnChange;

            var start = UiTheme.PrimaryButton("好，起来活动一下");
            start.SetBounds(24, 192, 332, 44);
            start.Click += delegate
            {
                ResultAction = ReminderAction.StartBreak;
                DialogResult = DialogResult.OK;
                Close();
            };

            var snooze = UiTheme.GhostButton("5 分钟后再说");
            snooze.SetBounds(24, 246, 162, 36);
            snooze.Click += delegate
            {
                ResultAction = ReminderAction.Snooze;
                DialogResult = DialogResult.OK;
                Close();
            };

            var skip = UiTheme.GhostButton("这次先跳过");
            skip.SetBounds(194, 246, 162, 36);
            skip.Click += delegate
            {
                ResultAction = ReminderAction.Skip;
                DialogResult = DialogResult.OK;
                Close();
            };

            var tip = new Label
            {
                Text = "身体是自己的，歇一下不耽误事",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Hint,
                Location = new Point(24, 294),
                AutoSize = true
            };

            Controls.Add(_emoji);
            Controls.Add(_title);
            Controls.Add(_message);
            Controls.Add(change);
            Controls.Add(start);
            Controls.Add(snooze);
            Controls.Add(skip);
            Controls.Add(tip);

            Opacity = 0;
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 淡入动画，不占用 UI Timer
            for (double o = 0.2; o <= 1.0; o += 0.15)
            {
                Opacity = o;
                await Task.Delay(25);
            }
            Opacity = 1.0;
        }

        private void OnChange(object sender, EventArgs e)
        {
            if (_replacer == null)
            {
                return;
            }

            var neu = _replacer();
            if (neu == null)
            {
                return;
            }

            _emoji.Text = neu.Emoji ?? "🐟";
            _title.Text = neu.Title ?? "该休息一下了";
            _message.Text = neu.Message ?? string.Empty;
        }
    }
}
