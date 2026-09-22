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

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 220,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0, 12, 0, 0)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            _work = AddCard(grid, "今日工作", "0m", 0, 0);
            _rest = AddCard(grid, "今日休息", "0m", 1, 0);
            _reminders = AddCard(grid, "提醒次数", "0", 0, 1);
            _skips = AddCard(grid, "跳过次数", "0", 1, 1);

            Controls.Add(grid);
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

        private static Label AddCard(TableLayoutPanel grid, string caption, string value, int col, int row)
        {
            var panel = new Panel
            {
                BackColor = UiTheme.Panel,
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                Padding = new Padding(14)
            };

            var cap = new Label
            {
                Text = caption,
                ForeColor = UiTheme.Mute,
                Dock = DockStyle.Top,
                Height = 22
            };

            var val = new Label
            {
                Text = value,
                Font = UiTheme.StatusFont,
                ForeColor = UiTheme.Ink,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.Add(val);
            panel.Controls.Add(cap);
            grid.Controls.Add(panel, col, row);
            return val;
        }
    }
}
