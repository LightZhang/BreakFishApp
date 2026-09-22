using System.Drawing;
using System.Windows.Forms;
using BreakFishApp.Models;
using BreakFishApp.Utils;

namespace BreakFishApp.Controls
{
    public sealed class StatsPage : UserControl
    {
        private readonly Label _work;
        private readonly Label _rest;
        private readonly Label _reminders;
        private readonly Label _skips;

        public StatsPage()
        {
            BackColor = UiTheme.Paper;
            Dock = DockStyle.Fill;
            Padding = new Padding(20);

            var title = new Label
            {
                Text = "统计",
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.Ink,
                Dock = DockStyle.Top,
                Height = 32
            };

            var hint = new Label
            {
                Text = "看看今天身体被照顾得怎么样",
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Mute,
                Dock = DockStyle.Top,
                Height = 22
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 240,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0, 12, 0, 0)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            _work = AddCard(grid, "今日工作", "0m", "专注了一阵子", UiTheme.Accent, 0, 0);
            _rest = AddCard(grid, "今日休息", "0m", "有在照顾自己", UiTheme.Break, 1, 0);
            _reminders = AddCard(grid, "提醒次数", "0", "被关心了几次", UiTheme.Lunch, 0, 1);
            _skips = AddCard(grid, "跳过次数", "0", "偶尔偷懒也没关系", UiTheme.Mute, 1, 1);

            Controls.Add(grid);
            Controls.Add(hint);
            Controls.Add(title);
        }

        public void Bind(ScheduleState state)
        {
            if (state == null)
            {
                return;
            }

            _work.Text = TimeHelper.FormatDuration(state.WorkedSeconds);
            _rest.Text = TimeHelper.FormatDuration(state.BreakSeconds);
            _reminders.Text = state.ReminderCount.ToString();
            _skips.Text = state.SkipCount.ToString();
        }

        private static Label AddCard(TableLayoutPanel grid, string caption, string value, string sub, Color accent, int col, int row)
        {
            var panel = new RoundPanel
            {
                Radius = UiTheme.Radius,
                BackColor = UiTheme.Panel,
                BorderColor = UiTheme.Line,
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                Padding = new Padding(16, 14, 16, 14)
            };

            var cap = new Label
            {
                Text = caption,
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Mute,
                Dock = DockStyle.Top,
                Height = 22
            };

            var accentBar = new Panel
            {
                BackColor = accent,
                Size = new Size(24, 3),
                Dock = DockStyle.Top,
                Height = 3,
                Margin = new Padding(0, 2, 0, 6)
            };

            var val = new Label
            {
                Text = value,
                Font = UiTheme.ValueFont,
                ForeColor = UiTheme.Ink,
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var subLabel = new Label
            {
                Text = sub,
                Font = UiTheme.SmallFont,
                ForeColor = UiTheme.Hint,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };

            panel.Controls.Add(subLabel);
            panel.Controls.Add(val);
            panel.Controls.Add(accentBar);
            panel.Controls.Add(cap);
            grid.Controls.Add(panel, col, row);
            return val;
        }
    }
}
