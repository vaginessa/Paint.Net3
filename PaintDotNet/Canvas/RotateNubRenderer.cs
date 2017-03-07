namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;

    internal class RotateNubRenderer : CanvasGdipRenderer
    {
        private double angle;
        private System.Windows.Point location;
        private const int size = 6;

        public RotateNubRenderer(CanvasRenderer ownerCanvas) : base(ownerCanvas)
        {
            this.location = new System.Windows.Point(0.0, 0.0);
        }

        private Rect GetOurRectangle()
        {
            Rect rect = new Rect(this.Location, new System.Windows.Size(0.0, 0.0));
            double num = 1.0 / base.OwnerCanvas.ScaleFactor.Ratio;
            float num2 = UI.ScaleWidth(6);
            rect.Inflate((double) (num * num2), (double) (num * num2));
            return rect;
        }

        private void InvalidateOurself()
        {
            Int32Rect rect = this.GetOurRectangle().Int32Bound().InflateCopy(2, 2);
            base.InvalidateCanvas(rect);
        }

        public bool IsPointTouching(System.Windows.Point pt) => 
            this.GetOurRectangle().Int32Bound().Contains(pt);

        protected override void OnVisibleChanged()
        {
            this.InvalidateOurself();
        }

        public override void RenderToGraphics(RenderArgs ra, Int32Point offset)
        {
            Graphics graphics = ra.Graphics;
            double a = this.Location.X * base.OwnerCanvas.ScaleFactor.Ratio;
            double num2 = this.Location.Y * base.OwnerCanvas.ScaleFactor.Ratio;
            System.Windows.Point location = new System.Windows.Point(Math.Round(a), Math.Round(num2));
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TranslateTransform((float) -location.X, (float) -location.Y, MatrixOrder.Append);
            graphics.RotateTransform((float) this.angle, MatrixOrder.Append);
            graphics.TranslateTransform((float) (location.X - offset.X), (float) (location.Y - offset.Y), MatrixOrder.Append);
            double num3 = UI.ScaleWidth(6);
            using (Pen pen = new Pen(Color.FromArgb(0x80, Color.White), -1f))
            {
                using (Pen pen2 = new Pen(Color.FromArgb(0x80, Color.Black), -1f))
                {
                    Rect rect = new Rect(location, new System.Windows.Size(0.0, 0.0));
                    rect.Inflate((double) (num3 - 3.0), (double) (num3 - 3.0));
                    graphics.DrawEllipse(pen, rect.TruncateCopy().ToGdipRectangle());
                    rect.Inflate((double) 1.0, (double) 1.0);
                    graphics.DrawEllipse(pen2, rect.TruncateCopy().ToGdipRectangle());
                    rect.Inflate((double) 1.0, (double) 1.0);
                    graphics.DrawEllipse(pen, rect.TruncateCopy().ToGdipRectangle());
                    rect.Inflate((double) -2.0, (double) -2.0);
                    graphics.DrawLine(pen, (float) ((rect.X + (rect.Width / 2.0)) - 1.0), (float) rect.Top, (float) ((rect.X + (rect.Width / 2.0)) - 1.0), (float) rect.Bottom);
                    graphics.DrawLine(pen, (float) ((rect.X + (rect.Width / 2.0)) + 1.0), (float) rect.Top, (float) ((rect.X + (rect.Width / 2.0)) + 1.0), (float) rect.Bottom);
                    graphics.DrawLine(pen2, (float) (rect.X + (rect.Width / 2.0)), (float) rect.Top, (float) (rect.X + (rect.Width / 2.0)), (float) rect.Bottom);
                }
            }
        }

        public double Angle
        {
            get => 
                this.angle;
            set
            {
                this.InvalidateOurself();
                this.angle = value;
                this.InvalidateOurself();
            }
        }

        public System.Windows.Point Location
        {
            get => 
                this.location;
            set
            {
                this.InvalidateOurself();
                this.location = value;
                this.InvalidateOurself();
            }
        }
    }
}

