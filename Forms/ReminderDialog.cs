using System;
using System.Drawing;
using System.Windows.Forms;
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

        public ReminderAction ResultAction { get; private set; }

        public ReminderDialog(ReminderItem item)
        {
            Text = "FishBreak";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            ClientSize = new Size(380, 300);
            BackColor = UiTheme.Paper;
            Font = UiTheme.UiFont;

            var emoji = new Label
            {
                Text = item != null ? (item.Emoji ?? "🐟") : "🐟",
                Font = new Font("Microsoft YaHei UI", 28F),
                ForeColor = UiTheme.Ink,
                Location = new Point(24, 22),
                AutoSize = true
            };

            var title = new Label
            {
                Text = item != null
                    ? (item.Title ?? "该休息一下了")
                    : "该休息一下了",
                Font = UiTheme.StatusFont,
                ForeColor = UiTheme.Ink,
                Location = new Point(76, 30),
                AutoSize = true
            };

            var message = new Label
            {
                Text = item != null
                    ? (item.Message ?? "站起来活动几分钟，再回来继续工作。")
                    : "站起来活动几分钟，再回来继续工作。",
                Font = UiTheme.UiFont,
                ForeColor = UiTheme.Mute,
                Location = new Point(24, 84),
                Size = new Size(332, 60)
            };

            var start = UiTheme.PrimaryButton("好，起来活动一下");
            start.SetBounds(24, 160, 332, 44);
            start.Click += delegate
            {
                ResultAction = ReminderAction.StartBreak;
                DialogResult = DialogResult.OK;
                Close();
            };

            var snooze = UiTheme.GhostButton("5 分钟后再说");
            snooze.SetBounds(24, 214, 162, 36);
            snooze.Click += delegate
            {
                ResultAction = ReminderAction.Snooze;
                DialogResult = DialogResult.OK;
                Close();
            };

            var skip = UiTheme.GhostButton("这次先跳过");
            skip.SetBounds(194, 214, 162, 36);
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
                Location = new Point(24, 262),
                AutoSize = true
            };

            Controls.Add(emoji);
            Controls.Add(title);
            Controls.Add(message);
            Controls.Add(start);
            Controls.Add(snooze);
            Controls.Add(skip);
            Controls.Add(tip);
        }
    }
}
