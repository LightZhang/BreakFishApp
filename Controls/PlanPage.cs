using System;
using System.Drawing;
using System.Windows.Forms;
using BreakFishApp.Models;

namespace BreakFishApp.Controls
{
    public sealed class PlanPage : UserControl
    {
        private readonly FlowLayoutPanel _list;

        public PlanPage()
        {
            BackColor = UiTheme.Paper;
            Dock = DockStyle.Fill;
            Padding = new Padding(20);

            var title = new Label
            {
                Text = "今日计划",
                Font = UiTheme.CaptionFont,
                ForeColor = UiTheme.Ink,
                Dock = DockStyle.Top,
                Height = 32
            };

            _list = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 8, 0, 8)
            };

            Controls.Add(_list);
            Controls.Add(title);
        }

        public void Bind(ScheduleState state)
        {
            _list.SuspendLayout();
            _list.Controls.Clear();
            if (state == null || state.TodayPlan == null)
            {
                _list.ResumeLayout();
                return;
            }

            var now = DateTime.Now;
            foreach (var item in state.TodayPlan)
            {
                var passed = item.At <= now;
                var row = new Panel
                {
                    Width = _list.ClientSize.Width - 28,
                    Height = 46,
                    Margin = new Padding(0, 0, 0, 8),
                    BackColor = passed ? Color.FromArgb(243, 241, 236) : UiTheme.Panel
                };

                var time = new Label
                {
                    Text = item.At.ToString("HH:mm"),
                    Font = UiTheme.CaptionFont,
                    ForeColor = passed ? UiTheme.Mute : UiTheme.Ink,
                    Location = new Point(12, 12),
                    AutoSize = true
                };

                var text = new Label
                {
                    Text = (item.Emoji ?? "") + "  " + item.Title,
                    Font = UiTheme.UiFont,
                    ForeColor = passed ? UiTheme.Mute : UiTheme.Ink,
                    Location = new Point(78, 13),
                    AutoSize = true
                };

                row.Controls.Add(time);
                row.Controls.Add(text);
                _list.Controls.Add(row);
            }

            _list.ResumeLayout();
        }
    }
}
