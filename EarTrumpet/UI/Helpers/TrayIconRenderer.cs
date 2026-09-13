using EarTrumpet.Extensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace EarTrumpet.UI.Helpers
{
    /// <summary>
    /// Draws the notification area icons instead of borrowing them from
    /// SndVolSSO.dll, so they are ours to change rather than a copy of the
    /// shell's own speaker.
    ///
    /// The vocabulary is deliberately rectangles and triangles. A tray icon is
    /// 16px at 100% scaling, and at that size hinted geometric shapes stay
    /// crisp where a drawn speaker cone turns to mush. Level bars also carry
    /// more than the shell's icon does -- four steps rather than three -- and
    /// the silhouette never changes, so only the fill moves with volume.
    /// </summary>
    internal static class TrayIconRenderer
    {
        // Everything below is laid out on a 16x16 grid and scaled to whatever
        // size the taskbar asks for at the current DPI.
        private const float Grid = 16f;

        private const int BarCount = 4;
        private const float BarWidth = 2.2f;
        private const float BarGap = 1.2f;
        private const float BarTop = 2.2f;
        private const float BarBottom = 13.8f;

        // Fixed silhouette as a fraction of the drawable height. Uneven on
        // purpose: it should read as a level meter, not a bar chart.
        private static readonly float[] BarHeights = { 0.48f, 0.86f, 0.66f, 1.0f };

        private const float DimAlpha = 0.30f;

        public static Icon RenderLevel(int size, Color color, int volume, bool isMuted, bool hasDevice)
        {
            var silent = isMuted || !hasDevice;

            return Render(size, (g, f) =>
            {
                var lit = 0;
                if (!silent)
                {
                    lit = (int)Math.Ceiling(volume / (100f / BarCount));

                    // Any audible volume should light at least one bar.
                    if (volume > 0 && lit == 0)
                    {
                        lit = 1;
                    }
                }

                DrawBars(g, f, color, lit);

                // Every bar is dim when silent, so a full-strength slash reads
                // against them without needing to punch a gap.
                if (silent)
                {
                    DrawSlash(g, f, size, color);
                }
            });
        }

        /// <summary>
        /// Two opposed arrows. The second icon is a button rather than a
        /// readout, and a swap symbol says so at a glance where a device glyph
        /// would just look like a duplicate of the first icon.
        /// </summary>
        public static Icon RenderSwap(int size, Color color)
        {
            return Render(size, (g, f) =>
            {
                using (var brush = new SolidBrush(color))
                using (var pen = new Pen(brush, Stroke(1.5f, size)) { StartCap = LineCap.Round, EndCap = LineCap.Flat })
                {
                    // Upper arrow, pointing right.
                    g.DrawLine(pen, 3.2f * f, 5.6f * f, 11.2f * f, 5.6f * f);
                    FillTriangle(g, brush, f, 14.0f, 5.6f, 10.8f, 3.2f, 10.8f, 8.0f);

                    // Lower arrow, pointing left.
                    g.DrawLine(pen, 12.8f * f, 10.4f * f, 4.8f * f, 10.4f * f);
                    FillTriangle(g, brush, f, 2.0f, 10.4f, 5.2f, 8.0f, 5.2f, 12.8f);
                }
            });
        }

        private static void DrawBars(Graphics g, float f, Color color, int litCount)
        {
            var totalWidth = (BarCount * BarWidth) + ((BarCount - 1) * BarGap);
            var startX = (Grid - totalWidth) / 2f;
            var available = BarBottom - BarTop;
            var radius = (BarWidth / 2f) * f;

            using (var lit = new SolidBrush(color))
            using (var dim = new SolidBrush(Color.FromArgb((int)(color.A * DimAlpha), color)))
            {
                for (int i = 0; i < BarCount; i++)
                {
                    var height = available * BarHeights[i];
                    var x = startX + (i * (BarWidth + BarGap));
                    var rect = new RectangleF(x * f, (BarBottom - height) * f, BarWidth * f, height * f);

                    FillRounded(g, i < litCount ? lit : dim, rect, radius);
                }
            }
        }

        private static void DrawSlash(Graphics g, float f, int size, Color color)
        {
            using (var pen = new Pen(color, Stroke(1.6f, size)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, 2.8f * f, 13.2f * f, 13.2f * f, 2.8f * f);
            }
        }

        private static void FillRounded(Graphics g, Brush brush, RectangleF rect, float radius)
        {
            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
            if (radius <= 0.5f)
            {
                g.FillRectangle(brush, rect);
                return;
            }

            var d = radius * 2f;
            using (var path = new GraphicsPath())
            {
                path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }

        private static void FillTriangle(Graphics g, Brush brush, float f, float x1, float y1, float x2, float y2, float x3, float y3)
        {
            g.FillPolygon(brush, new[]
            {
                new PointF(x1 * f, y1 * f),
                new PointF(x2 * f, y2 * f),
                new PointF(x3 * f, y3 * f),
            });
        }

        private static float Stroke(float value, int size) => Math.Max(1f, value * (size / Grid));

        private static Icon Render(int size, Action<Graphics, float> draw)
        {
            using (var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.Clear(Color.Transparent);
                    draw(g, size / Grid);
                }

                // GetHicon hands back a copy, so the bitmap can go immediately.
                // AsDisposableIcon gives the Icon ownership of the handle --
                // without it a long-running tray app leaks one per redraw.
                return Icon.FromHandle(bitmap.GetHicon()).AsDisposableIcon();
            }
        }
    }
}
