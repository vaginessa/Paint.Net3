namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class ToleranceSliderControl : Control
    {
        private bool hovering;
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;
        private string percentageFormat;
        private float tolerance;
        public EventHandler ToleranceChanged;
        private string toleranceText;
        private bool tracking;

        public ToleranceSliderControl()
        {
            base.Name = "ToleranceSliderControl";
            this.tolerance = 0.5f;
            this.toleranceText = PdnResources.GetString2("ToleranceSliderControl.Tolerance");
            this.percentageFormat = PdnResources.GetString2("ToleranceSliderControl.Percentage.Format");
            base.ResizeRedraw = true;
            this.DoubleBuffered = true;
        }

        private static Point[] GetOutline1px(Rectangle rect) => 
            new Point[] { new Point(rect.Left + 1, rect.Top), new Point(rect.Right - 2, rect.Top), new Point(rect.Right - 1, rect.Top + 1), new Point(rect.Right - 1, rect.Bottom - 2), new Point(rect.Right - 2, rect.Bottom - 1), new Point(rect.Left + 1, rect.Bottom - 1), new Point(rect.Left, rect.Bottom - 2), new Point(rect.Left, rect.Top + 1), new Point(rect.Left + 1, rect.Top) };

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!this.tracking && ((e.Button & MouseButtons.Left) == MouseButtons.Left))
            {
                this.tracking = true;
                this.OnMouseMove(e);
                base.Invalidate();
                base.Update();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.hovering = true;
            base.Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.hovering = false;
            base.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.tracking && (base.ClientSize.Width != 0))
            {
                int num = base.ClientSize.Width - 4;
                Math.Min(e.X - 2, num);
                this.Tolerance = ((float) e.X) / ((float) num);
                base.Update();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (this.tracking && ((e.Button & MouseButtons.Left) == MouseButtons.Left))
            {
                this.tracking = false;
                base.Invalidate();
                base.Update();
            }
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(this.penBrushCache.GetSolidBrush(SystemColors.Window), e.ClipRectangle);
            Font font = new Font(this.Font.FontFamily, 8f, this.Font.Style);
            int num = (int) (this.tolerance * 100f);
            string s = string.Format(this.percentageFormat, num);
            e.Graphics.DrawString(s, font, SystemBrushes.WindowText, (float) 2f, (float) 1f);
            Rectangle rect = new Rectangle(0, 0, base.ClientSize.Width, base.ClientSize.Height);
            e.Graphics.DrawLines(SystemPens.WindowText, GetOutline1px(rect));
            Rectangle rectangle2 = new Rectangle(2, 2, (int) ((base.ClientRectangle.Width - 4) * this.tolerance), base.ClientRectangle.Height - 4);
            Brush brush = this.hovering ? SystemBrushes.HotTrack : SystemBrushes.Highlight;
            e.Graphics.FillRectangle(brush, rectangle2);
            Region clip = e.Graphics.Clip;
            e.Graphics.SetClip(rectangle2, CombineMode.Replace);
            e.Graphics.DrawString(s, font, SystemBrushes.HighlightText, (float) 2f, (float) 1f);
            e.Graphics.SetClip(clip, CombineMode.Replace);
            clip.Dispose();
            font.Dispose();
            base.OnPaint(e);
        }

        protected void OnToleranceChanged()
        {
            base.Invalidate();
            if (this.ToleranceChanged != null)
            {
                this.ToleranceChanged(this, EventArgs.Empty);
            }
        }

        public void PerformToleranceChanged()
        {
            this.OnToleranceChanged();
        }

        public float Tolerance
        {
            get => 
                this.tolerance;
            set
            {
                if (this.tolerance != value)
                {
                    this.tolerance = value.Clamp(0f, 1f);
                    this.OnToleranceChanged();
                }
            }
        }
    }
}

