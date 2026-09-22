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
            ClientSize = new Size(360, 260);
            BackColor = UiTheme.Paper;
            Font = UiTheme.UiFont;

            var title = new Label
            {
                Text = (item != null ? item.Emoji + "  " + item.Title : "该休息一下了"),
                Font = UiTheme.StatusFont,
                ForeColor = UiTheme.Ink,
                Location = new Point(24, 24),
                AutoSize = true
            };

            var message = new Label
            {
                Text = item != null ? item.Message : string.Empty,
                ForeColor = UiTheme.Mute,
                Location = new Point(24, 72),
                Size = new Size(312, 72)
            };

            var start = UiTheme.PrimaryButton("开始休息");
            start.SetBounds(24, 160, 312, 40);
            start.Click += delegate
            {
                ResultAction = ReminderAction.StartBreak;
                DialogResult = DialogResult.OK;
                Close();
            };

            var snooze = UiTheme.GhostButton("5分钟后提醒");
            snooze.SetBounds(24, 208, 150, 32);
            snooze.Click += delegate
            {
                ResultAction = ReminderAction.Snooze;
                DialogResult = DialogResult.OK;
                Close();
            };

            var skip = UiTheme.GhostButton("跳过");
            skip.SetBounds(186, 208, 150, 32);
            skip.Click += delegate
            {
                ResultAction = ReminderAction.Skip;
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(title);
            Controls.Add(message);
            Controls.Add(start);
            Controls.Add(snooze);
            Controls.Add(skip);
        }
    }
}
