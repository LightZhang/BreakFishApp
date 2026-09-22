using System.Drawing;
using System.Windows.Forms;

namespace BreakFishApp
{
    public static class UiTheme
    {
        public static readonly Color Paper = Color.FromArgb(247, 244, 239);
        public static readonly Color Panel = Color.FromArgb(255, 252, 247);
        public static readonly Color Accent = Color.FromArgb(47, 111, 94);
        public static readonly Color AccentSoft = Color.FromArgb(232, 240, 236);
        public static readonly Color Ink = Color.FromArgb(36, 40, 38);
        public static readonly Color Mute = Color.FromArgb(110, 116, 112);
        public static readonly Color Line = Color.FromArgb(226, 220, 210);
        public static readonly Color Nav = Color.FromArgb(239, 234, 226);

        public static readonly Font TitleFont = new Font("Microsoft YaHei UI", 18F, FontStyle.Bold);
        public static readonly Font StatusFont = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
        public static readonly Font ClockFont = new Font("Consolas", 36F, FontStyle.Bold);
        public static readonly Font UiFont = new Font("Microsoft YaHei UI", 9.5F);
        public static readonly Font SmallFont = new Font("Microsoft YaHei UI", 9F);
        public static readonly Font CaptionFont = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);

        public static Button PrimaryButton(string text)
        {
            var button = new Button
            {
                Text = text,
                BackColor = Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = CaptionFont,
                Height = 40,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        public static Button GhostButton(string text)
        {
            var button = new Button
            {
                Text = text,
                BackColor = AccentSoft,
                ForeColor = Accent,
                FlatStyle = FlatStyle.Flat,
                Font = UiFont,
                Height = 32,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}
