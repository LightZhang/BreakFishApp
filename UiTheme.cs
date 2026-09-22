using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BreakFishApp
{
    public static class UiTheme
    {
        // 底色：暖米白系，柔和不刺眼
        public static readonly Color Paper = Color.FromArgb(248, 245, 240);
        public static readonly Color Panel = Color.FromArgb(255, 253, 249);
        public static readonly Color PanelDeep = Color.FromArgb(243, 239, 231);
        public static readonly Color Nav = Color.FromArgb(241, 236, 228);

        // 强调：温润的青绿，不饱和
        public static readonly Color Accent = Color.FromArgb(58, 120, 100);
        public static readonly Color AccentSoft = Color.FromArgb(229, 239, 234);
        public static readonly Color AccentDeep = Color.FromArgb(42, 92, 78);

        // 状态色
        public static readonly Color Work = Color.FromArgb(58, 120, 100);
        public static readonly Color Break = Color.FromArgb(214, 138, 89);
        public static readonly Color Lunch = Color.FromArgb(168, 130, 196);
        public static readonly Color Off = Color.FromArgb(150, 150, 145);

        // 文字
        public static readonly Color Ink = Color.FromArgb(40, 44, 41);
        public static readonly Color Mute = Color.FromArgb(126, 130, 124);
        public static readonly Color Hint = Color.FromArgb(168, 170, 164);
        public static readonly Color Line = Color.FromArgb(228, 222, 212);
        public static readonly Color Danger = Color.FromArgb(190, 92, 78);

        // 字体
        public static readonly Font BrandFont = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold);
        public static readonly Font TitleFont = new Font("Microsoft YaHei UI", 18F, FontStyle.Bold);
        public static readonly Font StatusFont = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold);
        public static readonly Font ClockFont = new Font("Consolas", 40F, FontStyle.Bold);
        public static readonly Font UiFont = new Font("Microsoft YaHei UI", 9.5F);
        public static readonly Font SmallFont = new Font("Microsoft YaHei UI", 9F);
        public static readonly Font CaptionFont = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
        public static readonly Font ValueFont = new Font("Microsoft YaHei UI", 22F, FontStyle.Bold);

        // 间距
        public const int Pad = 24;
        public const int Radius = 12;

        public static Button PrimaryButton(string text)
        {
            var button = new Button
            {
                Text = text,
                BackColor = Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = CaptionFont,
                Height = 42,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = AccentDeep;
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(36, 80, 68);
            button.Resize += delegate { ApplyRoundRegion(button, 8); };
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
                Height = 34,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(216, 230, 222);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 218, 210);
            button.Resize += delegate { ApplyRoundRegion(button, 8); };
            return button;
        }

        /// <summary>给控件套一个圆角区域，固定尺寸时调用一次即可。</summary>
        public static void ApplyRoundRegion(Control target, int radius)
        {
            if (target == null || target.Width <= 0 || target.Height <= 0)
            {
                return;
            }

            using (var path = new GraphicsPath())
            {
                var r = radius;
                var w = target.Width;
                var h = target.Height;
                path.AddArc(0, 0, r, r, 180, 90);
                path.AddArc(w - r, 0, r, r, 270, 90);
                path.AddArc(w - r, h - r, r, r, 0, 90);
                path.AddArc(0, h - r, r, r, 90, 90);
                path.CloseFigure();
                target.Region = new Region(path);
            }
        }
    }
}
