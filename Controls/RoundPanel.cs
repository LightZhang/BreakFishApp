using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BreakFishApp.Controls
{
    /// <summary>
    /// 圆角面板：自带圆角区域与细边，随尺寸自适应。
    /// </summary>
    public sealed class RoundPanel : Panel
    {
        private int _radius = UiTheme.Radius;
        private Color _border = UiTheme.Line;

        public RoundPanel()
        {
            DoubleBuffered = true;
            BackColor = UiTheme.Panel;
        }

        public int Radius
        {
            get { return _radius; }
            set { _radius = value; RefreshRegion(); }
        }

        public Color BorderColor
        {
            get { return _border; }
            set { _border = value; Invalidate(); }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RefreshRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RefreshRegion();
        }

        private void RefreshRegion()
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            using (var path = new GraphicsPath())
            {
                var r = _radius;
                var w = Width;
                var h = Height;
                path.AddArc(0, 0, r, r, 180, 90);
                path.AddArc(w - r, 0, r, r, 270, 90);
                path.AddArc(w - r, h - r, r, r, 0, 90);
                path.AddArc(0, h - r, r, r, 90, 90);
                path.CloseFigure();
                Region = new Region(path);
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = ClientRectangle;
            using (var path = RoundPath(rect, _radius))
            {
                // 顶部高光
                using (var topBrush = new SolidBrush(Color.FromArgb(8, 255, 255, 255)))
                {
                    g.FillPath(topBrush, path);
                }
                // 边框
                using (var pen = new Pen(_border, 1))
                {
                    g.DrawPath(pen, path);
                }
            }
            base.OnPaint(e);
        }

        private static GraphicsPath RoundPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var r = radius;
            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
