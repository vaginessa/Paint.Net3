namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Media;

    internal class MoveNubRenderer : CanvasControl
    {
        private int alpha;
        private MoveNubShape shape;
        private System.Windows.Media.Matrix transform;
        private double transformAngle;

        public MoveNubRenderer(CanvasRenderer ownerCanvas) : base(ownerCanvas)
        {
            this.shape = MoveNubShape.Square;
            this.transform = System.Windows.Media.Matrix.Identity;
            this.alpha = 0xff;
            base.Size = new System.Windows.Size(5.0, 5.0);
        }

        private Rect GetOurRectangle()
        {
            System.Windows.Point location = this.transform.Transform(base.Location);
            double d = 1.0 / base.OwnerCanvas.ScaleFactor.Ratio;
            double num2 = UI.ScaleWidth(base.Size.Width);
            double num3 = UI.ScaleHeight(base.Size.Height);
            if (!double.IsNaN(d))
            {
                Rect rect = new Rect(location, new System.Windows.Size(0.0, 0.0));
                rect.Inflate((double) (d * num2), (double) (d * num3));
                return rect;
            }
            return new Rect(0.0, 0.0, 0.0, 0.0);
        }

        private void InvalidateOurself()
        {
            this.InvalidateOurself(false);
        }

        private void InvalidateOurself(bool force)
        {
            if (base.Visible || force)
            {
                Int32Rect rect = this.GetOurRectangle().Int32Bound().InflateCopy(1, 1);
                base.InvalidateCanvas(rect);
            }
        }

        public bool IsPointTouching(System.Windows.Point ptF, bool pad)
        {
            Rect ourRectangle = this.GetOurRectangle();
            if (pad)
            {
                double num = 2.0 / base.OwnerCanvas.ScaleFactor.Ratio;
                ourRectangle.Inflate((double) (num + 1.0), (double) (num + 1.0));
            }
            return ourRectangle.Contains(ptF);
        }

        protected override void OnLocationChanged()
        {
            this.InvalidateOurself();
            base.OnLocationChanged();
        }

        protected override void OnLocationChanging()
        {
            this.InvalidateOurself();
            base.OnLocationChanging();
        }

        protected override void OnRender(RenderArgs ra, Int32Point offset)
        {
            Graphics graphics = ra.Graphics;
            System.Windows.Media.Matrix transform = this.transform;
            double scalar = UI.ScaleWidth(Math.Min(base.Width, base.Height));
            SmoothingMode smoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TranslateTransform((float) -offset.X, (float) -offset.Y, MatrixOrder.Append);
            System.Windows.Point location = base.Location;
            location = transform.Transform(location);
            location.X *= base.OwnerCanvas.ScaleFactor.Ratio;
            location.Y *= base.OwnerCanvas.ScaleFactor.Ratio;
            Vector[] vecs = new Vector[] { new Vector(-1.0, -1.0), new Vector(1.0, -1.0), new Vector(1.0, 1.0), new Vector(-1.0, 1.0), new Vector(-1.0, 0.0), new Vector(1.0, 0.0), new Vector(0.0, -1.0), new Vector(0.0, 1.0) };
            vecs.RotateInPlace(this.transformAngle);
            vecs.NormalizeInPlace();
            using (Pen pen = new Pen(Color.FromArgb(this.alpha, Color.White), -1f))
            {
                using (Pen pen2 = new Pen(Color.FromArgb(this.alpha, Color.Black), -1f))
                {
                    PixelOffsetMode pixelOffsetMode = graphics.PixelOffsetMode;
                    graphics.PixelOffsetMode = PixelOffsetMode.None;
                    if (this.shape != MoveNubShape.Circle)
                    {
                        PointF[] points = new PointF[] { (location + Vector.Multiply(scalar, vecs[0])).ToGdipPointF(), (location + Vector.Multiply(scalar, vecs[1])).ToGdipPointF(), (location + Vector.Multiply(scalar, vecs[2])).ToGdipPointF(), (location + Vector.Multiply(scalar, vecs[3])).ToGdipPointF() };
                        PointF[] tfArray2 = new PointF[] { (location + Vector.Multiply((double) (scalar - 1.0), vecs[0])).ToGdipPointF(), (location + Vector.Multiply((double) (scalar - 1.0), vecs[1])).ToGdipPointF(), (location + Vector.Multiply((double) (scalar - 1.0), vecs[2])).ToGdipPointF(), (location + Vector.Multiply((double) (scalar - 1.0), vecs[3])).ToGdipPointF() };
                        PointF[] tfArray3 = new PointF[] { (location + Vector.Multiply((double) (scalar - 2.0), vecs[0])).ToGdipPointF(), (location + Vector.Multiply((double) (scalar - 2.0), vecs[1])).ToGdipPointF(), (location + Vector.Multiply((double) (scalar - 2.0), vecs[2])).ToGdipPointF(), (location + Vector.Multiply((double) (scalar - 2.0), vecs[3])).ToGdipPointF() };
                        graphics.DrawPolygon(pen, points);
                        graphics.DrawPolygon(pen2, tfArray2);
                        graphics.DrawPolygon(pen, tfArray3);
                    }
                    else if (this.shape == MoveNubShape.Circle)
                    {
                        Rect rect = new Rect(new System.Windows.Point(location.X, location.Y), new System.Windows.Size(0.0, 0.0));
                        rect.Inflate((double) (scalar - 1.0), (double) (scalar - 1.0));
                        graphics.DrawEllipse(pen, rect.ToGdipRectangleF());
                        rect.Inflate((double) -1.0, (double) -1.0);
                        graphics.DrawEllipse(pen2, rect.ToGdipRectangleF());
                        rect.Inflate((double) -1.0, (double) -1.0);
                        graphics.DrawEllipse(pen, rect.ToGdipRectangleF());
                    }
                    if (this.shape == MoveNubShape.Compass)
                    {
                        pen2.SetLineCap(LineCap.Round, LineCap.DiamondAnchor, DashCap.Flat);
                        pen2.EndCap = LineCap.ArrowAnchor;
                        pen2.StartCap = LineCap.ArrowAnchor;
                        pen.SetLineCap(LineCap.Round, LineCap.DiamondAnchor, DashCap.Flat);
                        pen.EndCap = LineCap.ArrowAnchor;
                        pen.StartCap = LineCap.ArrowAnchor;
                        System.Windows.Point point2 = location + Vector.Multiply((double) (scalar - 1.0), vecs[0]);
                        System.Windows.Point point3 = location + Vector.Multiply((double) (scalar - 1.0), vecs[1]);
                        System.Windows.Point point4 = location + Vector.Multiply((double) (scalar - 1.0), vecs[2]);
                        System.Windows.Point point5 = location + Vector.Multiply((double) (scalar - 1.0), vecs[3]);
                        System.Windows.Point pt = point2 + ((System.Windows.Point) ((point3 - point2) / 2.0));
                        System.Windows.Point point7 = point2 + ((System.Windows.Point) ((point5 - point2) / 2.0));
                        System.Windows.Point point8 = point3 + ((System.Windows.Point) ((point4 - point3) / 2.0));
                        System.Windows.Point point9 = point5 + ((System.Windows.Point) ((point4 - point5) / 2.0));
                        using (SolidBrush brush = new SolidBrush(pen.Color))
                        {
                            System.Windows.Point[] pointArray = new System.Windows.Point[] { point2, point3, point4, point5 };
                            PointF[] tfArray4 = (from p in pointArray select p.ToGdipPointF()).ToArrayEx<PointF>();
                            graphics.FillPolygon(brush, tfArray4, FillMode.Winding);
                        }
                        graphics.DrawLine(pen2, pt.ToGdipPointF(), point9.ToGdipPointF());
                        graphics.DrawLine(pen2, point7.ToGdipPointF(), point8.ToGdipPointF());
                    }
                    graphics.PixelOffsetMode = pixelOffsetMode;
                }
            }
            graphics.SmoothingMode = smoothingMode;
        }

        protected override void OnSizeChanged()
        {
            this.InvalidateOurself();
            base.OnSizeChanged();
        }

        protected override void OnSizeChanging()
        {
            this.InvalidateOurself();
            base.OnSizeChanging();
        }

        protected override void OnVisibleChanged()
        {
            this.InvalidateOurself(true);
        }

        public int Alpha
        {
            get => 
                this.alpha;
            set
            {
                if ((value < 0) || (value > 0xff))
                {
                    throw new ArgumentOutOfRangeException("value", value, "value must be [0, 255]");
                }
                if (this.alpha != value)
                {
                    this.alpha = value;
                    this.InvalidateOurself();
                }
            }
        }

        public MoveNubShape Shape
        {
            get => 
                this.shape;
            set
            {
                this.InvalidateOurself();
                this.shape = value;
                this.InvalidateOurself();
            }
        }

        public System.Windows.Media.Matrix Transform
        {
            get => 
                this.transform;
            set
            {
                this.InvalidateOurself();
                this.transform = value;
                this.transformAngle = this.transform.GetAngleOfTransform();
                this.InvalidateOurself();
            }
        }
    }
}

