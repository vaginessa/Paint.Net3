namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class ColorGradientControl : UserControl
    {
        private Color[] customGradient;
        private bool drawFarNub = true;
        private bool drawNearNub = true;
        private int highlight = -1;
        private Point lastTrackingMouseXY = new Point(-1, -1);
        private Color maxColor;
        private Color minColor;
        private System.Windows.Forms.Orientation orientation = System.Windows.Forms.Orientation.Vertical;
        private int tracking = -1;
        private const int triangleHalfLength = 3;
        private const int triangleSize = 7;
        private int[] vals;

        public event IndexEventHandler ValueChanged;

        public ColorGradientControl()
        {
            this.InitializeComponent();
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            this.Count = 1;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void DrawGradient(Graphics g)
        {
            double num;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            switch (this.orientation)
            {
                case System.Windows.Forms.Orientation.Horizontal:
                    num = 180.0;
                    break;

                case System.Windows.Forms.Orientation.Vertical:
                    num = 90.0;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
            Rectangle clientRectangle = base.ClientRectangle;
            switch (this.orientation)
            {
                case System.Windows.Forms.Orientation.Horizontal:
                    clientRectangle.Inflate(-3, -4);
                    break;

                case System.Windows.Forms.Orientation.Vertical:
                    clientRectangle.Inflate(-4, -3);
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
            if (((this.customGradient != null) && (clientRectangle.Width > 1)) && (clientRectangle.Height > 1))
            {
                Surface surface = new Surface(clientRectangle.Size.ToInt32Size());
                using (RenderArgs args = new RenderArgs(surface))
                {
                    Utility.DrawColorRectangle(args.Graphics, args.Bounds, Color.Transparent, false);
                    if (this.Orientation == System.Windows.Forms.Orientation.Horizontal)
                    {
                        for (int j = 0; j < surface.Width; j++)
                        {
                            double num16;
                            double num17;
                            double num18;
                            double d = ((double) (j * (this.customGradient.Length - 1))) / ((double) (surface.Width - 1));
                            int index = (int) Math.Floor(d);
                            double num5 = 1.0 - (d - index);
                            int num6 = (int) Math.Min((double) (this.customGradient.Length - 1), Math.Ceiling(d));
                            Color color = this.customGradient[index];
                            Color color2 = this.customGradient[num6];
                            double num7 = ((double) color.A) / 255.0;
                            double num8 = ((double) color.R) / 255.0;
                            double num9 = ((double) color.G) / 255.0;
                            double num10 = ((double) color.B) / 255.0;
                            double num11 = ((double) color2.A) / 255.0;
                            double num12 = ((double) color2.R) / 255.0;
                            double num13 = ((double) color2.G) / 255.0;
                            double num14 = ((double) color2.B) / 255.0;
                            double num15 = (num5 * num7) + ((1.0 - num5) * num11);
                            if (num15 == 0.0)
                            {
                                num16 = 0.0;
                                num17 = 0.0;
                                num18 = 0.0;
                            }
                            else
                            {
                                num16 = (((num5 * num7) * num8) + (((1.0 - num5) * num11) * num12)) / num15;
                                num17 = (((num5 * num7) * num9) + (((1.0 - num5) * num11) * num13)) / num15;
                                num18 = (((num5 * num7) * num10) + (((1.0 - num5) * num11) * num14)) / num15;
                            }
                            int num19 = ((int) Math.Round((double) (num15 * 255.0))).Clamp(0, 0xff);
                            int num20 = ((int) Math.Round((double) (num16 * 255.0))).Clamp(0, 0xff);
                            int num21 = ((int) Math.Round((double) (num17 * 255.0))).Clamp(0, 0xff);
                            int num22 = ((int) Math.Round((double) (num18 * 255.0))).Clamp(0, 0xff);
                            for (int k = 0; k < surface.Height; k++)
                            {
                                ColorBgra bgra = surface[j, k];
                                int num24 = ((num20 * num19) + (bgra.R * (0xff - num19))) / 0xff;
                                int num25 = ((num21 * num19) + (bgra.G * (0xff - num19))) / 0xff;
                                int num26 = ((num22 * num19) + (bgra.B * (0xff - num19))) / 0xff;
                                surface[j, k] = ColorBgra.FromBgra((byte) num26, (byte) num25, (byte) num24, 0xff);
                            }
                        }
                        g.DrawImage(args.Bitmap, clientRectangle, args.Bounds, GraphicsUnit.Pixel);
                    }
                    else if (this.Orientation != System.Windows.Forms.Orientation.Vertical)
                    {
                        throw new InvalidEnumArgumentException();
                    }
                }
                surface.Dispose();
            }
            else
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(base.ClientRectangle, this.maxColor, this.minColor, (float) num, false))
                {
                    g.FillRectangle(brush, clientRectangle);
                }
            }
            using (PdnRegion region = new PdnRegion())
            {
                region.MakeInfinite();
                region.Exclude(clientRectangle);
                using (SolidBrush brush2 = new SolidBrush(this.BackColor))
                {
                    g.FillRegion(brush2, region.GetRegionReadOnly());
                }
            }
            for (int i = 0; i < this.vals.Length; i++)
            {
                Brush blue;
                Pen pen;
                Point point;
                Point point2;
                Point point3;
                Point point4;
                Point point5;
                Point point6;
                int x = this.ValueToPosition(this.vals[i]);
                if (i == this.highlight)
                {
                    blue = Brushes.Blue;
                    pen = (Pen) Pens.White.Clone();
                }
                else
                {
                    blue = Brushes.Black;
                    pen = (Pen) Pens.Gray.Clone();
                }
                g.SmoothingMode = SmoothingMode.AntiAlias;
                switch (this.orientation)
                {
                    case System.Windows.Forms.Orientation.Horizontal:
                        point = new Point(x - 3, 0);
                        point2 = new Point(x, 6);
                        point3 = new Point(x + 3, 0);
                        point4 = new Point(point.X, (base.Height - 1) - point.Y);
                        point5 = new Point(point2.X, (base.Height - 1) - point2.Y);
                        point6 = new Point(point3.X, (base.Height - 1) - point3.Y);
                        break;

                    case System.Windows.Forms.Orientation.Vertical:
                        point = new Point(0, x - 3);
                        point2 = new Point(6, x);
                        point3 = new Point(0, x + 3);
                        point4 = new Point((base.Width - 1) - point.X, point.Y);
                        point5 = new Point((base.Width - 1) - point2.X, point2.Y);
                        point6 = new Point((base.Width - 1) - point3.X, point3.Y);
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }
                if (this.drawNearNub)
                {
                    Point[] points = new Point[] { point, point2, point3, point };
                    g.FillPolygon(blue, points);
                }
                if (this.drawFarNub)
                {
                    Point[] pointArray2 = new Point[] { point4, point5, point6, point4 };
                    g.FillPolygon(blue, pointArray2);
                }
                if (pen != null)
                {
                    if (this.drawNearNub)
                    {
                        Point[] pointArray3 = new Point[] { point, point2, point3, point };
                        g.DrawPolygon(pen, pointArray3);
                    }
                    if (this.drawFarNub)
                    {
                        Point[] pointArray4 = new Point[] { point4, point5, point6, point4 };
                        g.DrawPolygon(pen, pointArray4);
                    }
                    pen.Dispose();
                }
            }
        }

        private int GetOrientedValue(Point pt)
        {
            switch (this.orientation)
            {
                case System.Windows.Forms.Orientation.Horizontal:
                    return pt.X;

                case System.Windows.Forms.Orientation.Vertical:
                    return pt.Y;
            }
            throw new InvalidEnumArgumentException();
        }

        private int GetOrientedValue(MouseEventArgs me) => 
            this.GetOrientedValue(new Point(me.X, me.Y));

        public int GetValue(int index)
        {
            if ((index < 0) || (index >= this.vals.Length))
            {
                throw new ArgumentOutOfRangeException("index", index, "Index must be within the bounds of the array");
            }
            return this.vals[index];
        }

        private void InitializeComponent()
        {
        }

        private void InvalidateTriangle(int index)
        {
            if ((index >= 0) && (index < this.vals.Length))
            {
                Rectangle rectangle;
                int num = this.ValueToPosition(this.vals[index]);
                switch (this.orientation)
                {
                    case System.Windows.Forms.Orientation.Horizontal:
                        rectangle = new Rectangle(num - 3, 0, 7, base.Height);
                        break;

                    case System.Windows.Forms.Orientation.Vertical:
                        rectangle = new Rectangle(0, num - 3, base.Width, 7);
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }
                base.Invalidate(rectangle, true);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                int orientedValue = this.GetOrientedValue(e);
                this.tracking = this.WhichTriangle(orientedValue);
                base.Invalidate();
                this.OnMouseMove(e);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            int highlight = this.highlight;
            this.highlight = -1;
            this.InvalidateTriangle(highlight);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int orientedValue = this.GetOrientedValue(e);
            Point point = new Point(e.X, e.Y);
            if ((this.tracking >= 0) && (point != this.lastTrackingMouseXY))
            {
                int val = this.PositionToValue(orientedValue);
                this.SetValue(this.tracking, val);
                this.lastTrackingMouseXY = point;
            }
            else
            {
                int highlight = this.highlight;
                this.highlight = this.WhichTriangle(orientedValue);
                if (this.highlight != highlight)
                {
                    this.InvalidateTriangle(highlight);
                    this.InvalidateTriangle(this.highlight);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                this.OnMouseMove(e);
                this.tracking = -1;
                base.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            this.DrawGradient(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            this.DrawGradient(pevent.Graphics);
        }

        private void OnValueChanged(int index)
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, new IndexEventArgs(index));
            }
        }

        private int PositionToValue(int pos)
        {
            int width;
            switch (this.orientation)
            {
                case System.Windows.Forms.Orientation.Horizontal:
                    width = base.Width;
                    break;

                case System.Windows.Forms.Orientation.Vertical:
                    width = base.Height;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
            int num2 = (((width - 7) - (pos - 3)) * 0xff) / (width - 7);
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                num2 = 0xff - num2;
            }
            return num2;
        }

        public void SetValue(int index, int val)
        {
            int num = -1;
            int num2 = 0x100;
            if ((index < 0) || (index >= this.vals.Length))
            {
                throw new ArgumentOutOfRangeException("index", index, "Index must be within the bounds of the array");
            }
            if ((index - 1) >= 0)
            {
                num = this.vals[index - 1];
            }
            if ((index + 1) < this.vals.Length)
            {
                num2 = this.vals[index + 1];
            }
            if (this.vals[index] != val)
            {
                this.vals[index] = val.Clamp(num + 1, num2 - 1);
                this.OnValueChanged(index);
                base.Invalidate();
            }
            base.Update();
        }

        private int ValueToPosition(int val)
        {
            int width;
            if (this.orientation == System.Windows.Forms.Orientation.Horizontal)
            {
                val = 0xff - val;
            }
            switch (this.orientation)
            {
                case System.Windows.Forms.Orientation.Horizontal:
                    width = base.Width;
                    break;

                case System.Windows.Forms.Orientation.Vertical:
                    width = base.Height;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
            return (3 + ((width - 7) - ((val * (width - 7)) / 0xff)));
        }

        private int WhichTriangle(int val)
        {
            int num = -1;
            int num2 = 0x7fffffff;
            int num3 = this.PositionToValue(val);
            for (int i = 0; i < this.vals.Length; i++)
            {
                int num5 = Math.Abs((int) (this.vals[i] - num3));
                if (num5 < num2)
                {
                    num2 = num5;
                    num = i;
                }
            }
            return num;
        }

        [Obsolete("Use MinColor property instead", true), EditorBrowsable(EditorBrowsableState.Never)]
        public Color BottomColor
        {
            get => 
                this.MinColor;
            set
            {
                this.MinColor = value;
            }
        }

        public int Count
        {
            get => 
                this.vals.Length;
            set
            {
                if ((value < 0) || (value > 0x10))
                {
                    throw new ArgumentOutOfRangeException("value", value, "Count must be between 0 and 16");
                }
                this.vals = new int[value];
                if (value > 1)
                {
                    for (int i = 0; i < value; i++)
                    {
                        this.vals[i] = (i * 0xff) / (value - 1);
                    }
                }
                else if (value == 1)
                {
                    this.vals[0] = 0x80;
                }
                this.OnValueChanged(0);
                base.Invalidate();
            }
        }

        public Color[] CustomGradient
        {
            get
            {
                if (this.customGradient == null)
                {
                    return null;
                }
                return (Color[]) this.customGradient.Clone();
            }
            set
            {
                if (value != this.customGradient)
                {
                    if (value == null)
                    {
                        this.customGradient = null;
                    }
                    else
                    {
                        this.customGradient = (Color[]) value.Clone();
                    }
                    base.Invalidate();
                }
            }
        }

        public bool DrawFarNub
        {
            get => 
                this.drawFarNub;
            set
            {
                this.drawFarNub = value;
                base.Invalidate();
            }
        }

        public bool DrawNearNub
        {
            get => 
                this.drawNearNub;
            set
            {
                this.drawNearNub = value;
                base.Invalidate();
            }
        }

        public Color MaxColor
        {
            get => 
                this.maxColor;
            set
            {
                if (this.maxColor != value)
                {
                    this.maxColor = value;
                    base.Invalidate();
                }
            }
        }

        public Color MinColor
        {
            get => 
                this.minColor;
            set
            {
                if (this.minColor != value)
                {
                    this.minColor = value;
                    base.Invalidate();
                }
            }
        }

        public System.Windows.Forms.Orientation Orientation
        {
            get => 
                this.orientation;
            set
            {
                if (value != this.orientation)
                {
                    this.orientation = value;
                    base.Invalidate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("Use MaxColor property instead", true)]
        public Color TopColor
        {
            get => 
                this.MaxColor;
            set
            {
                this.MaxColor = value;
            }
        }

        public int Value
        {
            get => 
                this.GetValue(0);
            set
            {
                this.SetValue(0, value);
            }
        }
    }
}

