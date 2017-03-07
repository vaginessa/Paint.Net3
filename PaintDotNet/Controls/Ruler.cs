namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class Ruler : UserControl
    {
        private double dpu = 96.0;
        private bool highlightEnabled;
        private double highlightLength;
        private double highlightStart;
        private static readonly double[] majorDivisors = new double[] { 2.0, 2.5, 2.0 };
        private PaintDotNet.MeasurementUnit measurementUnit = PaintDotNet.MeasurementUnit.Inch;
        private double offset;
        private System.Windows.Forms.Orientation orientation;
        private double rulerValue;
        private PaintDotNet.ScaleFactor scaleFactor = PaintDotNet.ScaleFactor.OneToOne;

        public Ruler()
        {
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        private void DrawRuler(PaintEventArgs e, bool highlighted)
        {
            Pen highlightText;
            Brush windowText;
            int num;
            Color window;
            StringFormat format = new StringFormat();
            if (highlighted)
            {
                e.Graphics.Clear(SystemColors.Highlight);
                highlightText = SystemPens.HighlightText;
                windowText = SystemBrushes.HighlightText;
                window = SystemColors.Window;
            }
            else
            {
                e.Graphics.Clear(SystemColors.Window);
                highlightText = SystemPens.WindowText;
                windowText = SystemBrushes.WindowText;
                window = SystemColors.Highlight;
            }
            Brush brush = new SolidBrush(Color.FromArgb(0x80, window));
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                num = (int) this.ScaleFactor.Unscale(((double) base.ClientRectangle.Width));
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Far;
            }
            else
            {
                num = (int) this.ScaleFactor.Unscale(((double) base.ClientRectangle.Height));
                format.Alignment = StringAlignment.Near;
                format.LineAlignment = StringAlignment.Near;
                format.FormatFlags |= StringFormatFlags.DirectionVertical;
            }
            double num2 = 1.0;
            int num3 = 0;
            double dpu = this.dpu;
            double num5 = this.ScaleFactor.Scale(dpu);
            int[] subdivs = this.GetSubdivs(this.measurementUnit);
            double num6 = this.ScaleFactor.Scale(this.offset);
            int num7 = ((int) (this.offset / dpu)) - 1;
            int num8 = ((int) ((this.offset + num) / dpu)) + 1;
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                if (!highlighted)
                {
                    System.Windows.Point location = new System.Windows.Point(this.scaleFactor.Scale((double) ((base.ClientRectangle.Left + this.Value) - this.Offset)), (double) base.ClientRectangle.Top);
                    System.Windows.Size size = new System.Windows.Size(Math.Max(1.0, this.scaleFactor.Scale((double) 1.0)), (double) base.ClientRectangle.Height);
                    location.X -= 0.5;
                    CompositingMode compositingMode = e.Graphics.CompositingMode;
                    e.Graphics.CompositingMode = CompositingMode.SourceOver;
                    e.Graphics.FillRectangle(brush, new Rect(location, size).ToGdipRectangleF());
                    e.Graphics.CompositingMode = compositingMode;
                }
                e.Graphics.DrawLine(SystemPens.WindowText, new System.Drawing.Point(base.ClientRectangle.Left, base.ClientRectangle.Bottom - 1), new System.Drawing.Point(base.ClientRectangle.Right - 1, base.ClientRectangle.Bottom - 1));
            }
            else if (this.orientation == System.Windows.Forms.Orientation.Vertical)
            {
                if (!highlighted)
                {
                    System.Windows.Point point2 = new System.Windows.Point((double) base.ClientRectangle.Left, this.scaleFactor.Scale((double) ((base.ClientRectangle.Top + this.Value) - this.Offset)));
                    System.Windows.Size size2 = new System.Windows.Size((double) base.ClientRectangle.Width, Math.Max(1.0, this.scaleFactor.Scale((double) 1.0)));
                    point2.Y -= 0.5;
                    CompositingMode mode2 = e.Graphics.CompositingMode;
                    e.Graphics.CompositingMode = CompositingMode.SourceOver;
                    e.Graphics.FillRectangle(brush, new Rect(point2, size2).ToGdipRectangleF());
                    e.Graphics.CompositingMode = mode2;
                }
                e.Graphics.DrawLine(SystemPens.WindowText, new System.Drawing.Point(base.ClientRectangle.Right - 1, base.ClientRectangle.Top), new System.Drawing.Point(base.ClientRectangle.Right - 1, base.ClientRectangle.Bottom - 1));
            }
            while ((num5 * num2) < 60.0)
            {
                num2 *= majorDivisors[num3 % majorDivisors.Length];
                num3++;
            }
            num7 = (int) (num2 * Math.Floor((double) (((double) num7) / num2)));
            for (int i = num7; i <= num8; i += (int) num2)
            {
                double num10 = (i * num5) - num6;
                string s = i.ToString();
                if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    this.SubdivideX(e.Graphics, highlightText, base.ClientRectangle.Left + num10, num5 * num2, -num3, (double) base.ClientRectangle.Top, (double) base.ClientRectangle.Height, subdivs);
                    e.Graphics.DrawString(s, this.Font, windowText, new System.Windows.Point(base.ClientRectangle.Left + num10, (double) base.ClientRectangle.Bottom).ToGdipPointF(), format);
                }
                else
                {
                    this.SubdivideY(e.Graphics, highlightText, base.ClientRectangle.Top + num10, num5 * num2, -num3, (double) base.ClientRectangle.Left, (double) base.ClientRectangle.Width, subdivs);
                    e.Graphics.DrawString(s, this.Font, windowText, new System.Windows.Point((double) base.ClientRectangle.Left, base.ClientRectangle.Top + num10).ToGdipPointF(), format);
                }
            }
            format.Dispose();
        }

        private int[] GetSubdivs(PaintDotNet.MeasurementUnit unit)
        {
            switch (unit)
            {
                case PaintDotNet.MeasurementUnit.Inch:
                    return new int[] { 2 };

                case PaintDotNet.MeasurementUnit.Centimeter:
                    return new int[] { 2, 5 };
            }
            return null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rect empty;
            Rect rect2;
            double x = this.scaleFactor.Scale((double) (this.rulerValue - this.offset));
            double num2 = this.scaleFactor.Scale((double) ((this.rulerValue + 1.0) - this.offset));
            double num3 = this.scaleFactor.Scale((double) (this.highlightStart - this.offset));
            double num4 = this.scaleFactor.Scale((double) ((this.highlightStart + this.highlightLength) - this.offset));
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                rect2 = new Rect(x, (double) base.ClientRectangle.Top, num2 - x, (double) base.ClientRectangle.Height);
                empty = new Rect(num3, (double) base.ClientRectangle.Top, num4 - num3, (double) base.ClientRectangle.Height);
            }
            else
            {
                rect2 = new Rect((double) base.ClientRectangle.Left, x, (double) base.ClientRectangle.Width, num2 - x);
                empty = new Rect((double) base.ClientRectangle.Left, num3, (double) base.ClientRectangle.Width, num4 - num3);
            }
            if (!this.highlightEnabled)
            {
                empty = Rect.Empty;
            }
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                e.Graphics.DrawLine(SystemPens.WindowText, UI.ScaleWidth(15), base.ClientRectangle.Top, UI.ScaleWidth(15), base.ClientRectangle.Bottom);
                string s = PdnResources.GetString2("MeasurementUnit." + this.MeasurementUnit.ToString() + ".Abbreviation");
                e.Graphics.DrawString(s, this.Font, SystemBrushes.WindowText, (float) UI.ScaleWidth(-2), 0f);
            }
            Region region = new Region(empty.ToGdipRectangleF());
            region.Xor(rect2.ToGdipRectangleF());
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                region.Exclude(new Rectangle(0, 0, UI.ScaleWidth(0x10), base.ClientRectangle.Height));
            }
            e.Graphics.SetClip(region, CombineMode.Replace);
            this.DrawRuler(e, true);
            region.Xor(base.ClientRectangle);
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                region.Exclude(new Rectangle(0, 0, UI.ScaleWidth(0x10), base.ClientRectangle.Height - 1));
            }
            e.Graphics.SetClip(region, CombineMode.Replace);
            this.DrawRuler(e, false);
            region.Dispose();
        }

        private void SubdivideX(Graphics g, Pen pen, double x, double delta, int index, double y, double height, int[] subdivs)
        {
            double num;
            int num2;
            g.DrawLine(pen, (float) x, (float) y, (float) x, (float) (y + height));
            if (index <= 10)
            {
                if ((subdivs != null) && (index >= 0))
                {
                    num = subdivs[index % subdivs.Length];
                    goto Label_004E;
                }
                if (index < 0)
                {
                    num = majorDivisors[(-index - 1) % majorDivisors.Length];
                    goto Label_004E;
                }
            }
            return;
        Label_004E:
            num2 = 0;
            while (num2 < num)
            {
                if ((delta / num) > 3.5)
                {
                    this.SubdivideX(g, pen, x + ((delta * num2) / num), delta / num, index + 1, y, (height / num) + 0.5, subdivs);
                }
                num2++;
            }
        }

        private void SubdivideY(Graphics g, Pen pen, double y, double delta, int index, double x, double width, int[] subdivs)
        {
            double num;
            int num2;
            g.DrawLine(pen, (float) x, (float) y, (float) (x + width), (float) y);
            if (index <= 10)
            {
                if ((subdivs != null) && (index >= 0))
                {
                    num = subdivs[index % subdivs.Length];
                    goto Label_004E;
                }
                if (index < 0)
                {
                    num = majorDivisors[(-index - 1) % majorDivisors.Length];
                    goto Label_004E;
                }
            }
            return;
        Label_004E:
            num2 = 0;
            while (num2 < num)
            {
                if ((delta / num) > 3.5)
                {
                    this.SubdivideY(g, pen, y + ((delta * num2) / num), delta / num, index + 1, x, (width / num) + 0.5, subdivs);
                }
                num2++;
            }
        }

        [DefaultValue((double) 96.0)]
        public double Dpu
        {
            get => 
                this.dpu;
            set
            {
                if (value != this.dpu)
                {
                    this.dpu = value;
                    base.Invalidate();
                }
            }
        }

        public bool HighlightEnabled
        {
            get => 
                this.highlightEnabled;
            set
            {
                if (this.highlightEnabled != value)
                {
                    this.highlightEnabled = value;
                    base.Invalidate();
                }
            }
        }

        public double HighlightLength
        {
            get => 
                this.highlightLength;
            set
            {
                if (this.highlightLength != value)
                {
                    this.highlightLength = value;
                    base.Invalidate();
                }
            }
        }

        public double HighlightStart
        {
            get => 
                this.highlightStart;
            set
            {
                if (this.highlightStart != value)
                {
                    this.highlightStart = value;
                    base.Invalidate();
                }
            }
        }

        public PaintDotNet.MeasurementUnit MeasurementUnit
        {
            get => 
                this.measurementUnit;
            set
            {
                if (value != this.measurementUnit)
                {
                    this.measurementUnit = value;
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0)]
        public double Offset
        {
            get => 
                this.offset;
            set
            {
                if (this.offset != value)
                {
                    this.offset = value;
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0)]
        public System.Windows.Forms.Orientation Orientation
        {
            get => 
                this.orientation;
            set
            {
                if (this.orientation != value)
                {
                    this.orientation = value;
                    base.Invalidate();
                }
            }
        }

        [Browsable(false)]
        public PaintDotNet.ScaleFactor ScaleFactor
        {
            get => 
                this.scaleFactor;
            set
            {
                if (this.scaleFactor != value)
                {
                    this.scaleFactor = value;
                    base.Invalidate();
                }
            }
        }

        [DefaultValue(0)]
        public double Value
        {
            get => 
                this.rulerValue;
            set
            {
                if (this.rulerValue != value)
                {
                    Rect rect;
                    Rect rect2;
                    double x = this.scaleFactor.Scale(((double) (this.rulerValue - this.offset))) - 1.0;
                    double num2 = this.scaleFactor.Scale(((double) ((this.rulerValue + 1.0) - this.offset))) + 1.0;
                    if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        rect = new Rect(x, (double) base.ClientRectangle.Top, num2 - x, (double) base.ClientRectangle.Height);
                    }
                    else
                    {
                        rect = new Rect((double) base.ClientRectangle.Left, x, (double) base.ClientRectangle.Width, num2 - x);
                    }
                    double num3 = this.scaleFactor.Scale((double) (value - this.offset));
                    double num4 = this.scaleFactor.Scale((double) ((value + 1.0) - this.offset));
                    if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        rect2 = new Rect(num3, (double) base.ClientRectangle.Top, num4 - num3, (double) base.ClientRectangle.Height);
                    }
                    else
                    {
                        rect2 = new Rect((double) base.ClientRectangle.Left, num3, (double) base.ClientRectangle.Width, num4 - num3);
                    }
                    this.rulerValue = value;
                    base.Invalidate(rect.Int32Bound().ToGdipRectangle());
                    base.Invalidate(rect2.Int32Bound().ToGdipRectangle());
                }
            }
        }
    }
}

