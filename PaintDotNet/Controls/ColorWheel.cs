namespace PaintDotNet.Controls
{
    using Microsoft.Win32;
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class ColorWheel : UserControl
    {
        private const int colorTesselation = 60;
        private PaintDotNet.HsvColor hsvColor;
        private Point lastMouseXY;
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;
        private Bitmap renderBitmap;
        private bool tracking;

        public event EventHandler ColorChanged;

        public ColorWheel()
        {
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            this.InitializeComponent();
            this.hsvColor = new PaintDotNet.HsvColor(0, 0, 0);
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.SystemEvents_UserPreferenceChanged);
        }

        private static float ComputeDiameter(Size size) => 
            Math.Min((float) size.Width, (float) size.Height);

        private static float ComputeRadius(Size size) => 
            Math.Min((float) (((float) size.Width) / 2f), (float) (((float) size.Height) / 2f));

        protected override void Dispose(bool disposing)
        {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.SystemEvents_UserPreferenceChanged);
            base.Dispose(disposing);
        }

        private void DrawWheel(Graphics g, int width, int height)
        {
            SmoothingMode smoothingMode = g.SmoothingMode;
            float x = ComputeRadius(new Size(width, height));
            using (PathGradientBrush brush = new PathGradientBrush(GetCirclePoints(Math.Max((float) 1f, (float) (x - 1f)), new PointF(x, x))))
            {
                brush.CenterColor = new PaintDotNet.HsvColor(0, 0, 100).ToColor();
                brush.CenterPoint = new PointF(x, x);
                brush.SurroundColors = this.GetColors();
                g.SmoothingMode = SmoothingMode.None;
                g.FillEllipse(brush, (float) 0f, (float) 0f, (float) (x * 2f), (float) (x * 2f));
            }
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawEllipse(this.penBrushCache.GetPen(this.BackColor), (float) 0.75f, (float) 0.75f, (float) ((x * 2f) - 1.5f), (float) ((x * 2f) - 1.5f));
            g.SmoothingMode = smoothingMode;
        }

        private static PointF[] GetCirclePoints(float r, PointF center)
        {
            PointF[] tfArray = new PointF[60];
            for (int i = 0; i < 60; i++)
            {
                float theta = ((((float) i) / 60f) * 2f) * 3.141593f;
                tfArray[i] = SphericalToCartesian(r, theta);
                tfArray[i].X += center.X;
                tfArray[i].Y += center.Y;
            }
            return tfArray;
        }

        private System.Drawing.Color[] GetColors()
        {
            System.Drawing.Color[] colorArray = new System.Drawing.Color[60];
            for (int i = 0; i < 60; i++)
            {
                int hue = (i * 360) / 60;
                colorArray[i] = new PaintDotNet.HsvColor(hue, 100, 100).ToColor();
            }
            return colorArray;
        }

        private void GrabColor(Point mouseXY)
        {
            int num = mouseXY.X - (base.Width / 2);
            int num2 = mouseXY.Y - (base.Height / 2);
            double num3 = Math.Atan2((double) num2, (double) num);
            if (num3 < 0.0)
            {
                num3 += 6.2831853071795862;
            }
            double num4 = Math.Sqrt((double) ((num * num) + (num2 * num2)));
            int hue = (int) ((num3 / 6.2831853071795862) * 360.0);
            int saturation = (int) Math.Min((double) 100.0, (double) ((num4 / ((double) (base.Width / 2))) * 100.0));
            int num7 = 100;
            this.hsvColor = new PaintDotNet.HsvColor(hue, saturation, num7);
            this.OnColorChanged();
            base.Invalidate(true);
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            base.Name = "ColorWheel";
            base.ResumeLayout(false);
        }

        private void InitRendering()
        {
            if (this.renderBitmap == null)
            {
                this.InitRenderSurface();
            }
        }

        private void InitRenderSurface()
        {
            if (this.renderBitmap != null)
            {
                this.renderBitmap.Dispose();
            }
            int num = (int) ComputeDiameter(base.Size);
            this.renderBitmap = new Bitmap(Math.Max(1, num), Math.Max(1, num), PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(this.renderBitmap))
            {
                graphics.Clear(this.BackColor);
                this.DrawWheel(graphics, this.renderBitmap.Width, this.renderBitmap.Height);
            }
        }

        private void OnColorChanged()
        {
            if (this.ColorChanged != null)
            {
                this.ColorChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            this.InitRendering();
            base.OnLoad(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                this.tracking = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.lastMouseXY = new Point(e.X, e.Y);
            if (this.tracking)
            {
                this.GrabColor(new Point(e.X, e.Y));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (this.tracking)
            {
                this.GrabColor(new Point(e.X, e.Y));
            }
            this.tracking = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.InitRendering();
            int width = Math.Min(base.Width, base.Height);
            PixelOffsetMode pixelOffsetMode = e.Graphics.PixelOffsetMode;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
            SmoothingMode smoothingMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.DrawImage(this.renderBitmap, new Rectangle(0, 0, width, width), new Rectangle(0, 0, this.renderBitmap.Width, this.renderBitmap.Height), GraphicsUnit.Pixel);
            float num2 = ComputeRadius(base.Size);
            float num3 = ((((float) this.HsvColor.Hue) / 360f) * 2f) * 3.141593f;
            float num4 = ((float) this.HsvColor.Saturation) / 100f;
            float num5 = ((num4 * (num2 - 1f)) * ((float) Math.Cos((double) num3))) + num2;
            float num6 = ((num4 * (num2 - 1f)) * ((float) Math.Sin((double) num3))) + num2;
            int x = (int) num5;
            int y = (int) num6;
            e.Graphics.DrawRectangle(this.penBrushCache.GetPen(System.Drawing.Color.Black), x - 1, y - 1, 3, 3);
            e.Graphics.DrawRectangle(this.penBrushCache.GetPen(System.Drawing.Color.White), x, y, 1, 1);
            e.Graphics.SmoothingMode = smoothingMode;
            e.Graphics.PixelOffsetMode = pixelOffsetMode;
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if ((this.renderBitmap != null) && (ComputeRadius(base.Size) != ComputeRadius(this.renderBitmap.Size)))
            {
                this.renderBitmap.Dispose();
                this.renderBitmap = null;
            }
            base.Invalidate();
        }

        private static PointF SphericalToCartesian(float r, float theta)
        {
            float x = r * ((float) Math.Cos((double) theta));
            return new PointF(x, r * ((float) Math.Sin((double) theta)));
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (this.renderBitmap != null)
            {
                this.renderBitmap.Dispose();
                this.renderBitmap = null;
                base.Invalidate(true);
            }
        }

        public PaintDotNet.HsvColor HsvColor
        {
            get => 
                this.hsvColor;
            set
            {
                if (this.hsvColor != value)
                {
                    this.hsvColor = value;
                    this.OnColorChanged();
                    this.Refresh();
                }
            }
        }
    }
}

