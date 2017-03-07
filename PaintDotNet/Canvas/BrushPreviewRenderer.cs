namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;

    internal class BrushPreviewRenderer : CanvasLayer
    {
        private int brushAlpha;
        private System.Windows.Point brushLocation;
        private double brushSize;

        public BrushPreviewRenderer(CanvasRenderer ownerCanvas) : base(ownerCanvas)
        {
            this.brushLocation = new System.Windows.Point(-500.0, -500.0);
            this.brushAlpha = 0xff;
        }

        private Rect GetInvalidateBrushRect()
        {
            double ratio = base.OwnerCanvas.ScaleFactor.Ratio;
            Rect rect = RectUtil.FromCenter(this.BrushLocation, this.brushSize);
            rect.Inflate(Math.Max((double) 4.0, (double) (4.0 / ratio)), Math.Max((double) 4.0, (double) (4.0 / ratio)));
            return rect;
        }

        private void InvalidateBrushLocation()
        {
            Rect invalidateBrushRect = this.GetInvalidateBrushRect();
            base.InvalidateCanvas(invalidateBrushRect);
        }

        protected override void OnRender(ISurface<ColorBgra> dst, Int32Point renderOffset)
        {
            using (RenderArgs args = new RenderArgs(dst))
            {
                Graphics g = args.Graphics;
                renderOffset = this.RenderToGraphics(g, renderOffset);
            }
        }

        protected override void OnVisibleChanged()
        {
            this.InvalidateBrushLocation();
        }

        private Int32Point RenderToGraphics(Graphics g, Int32Point renderOffset)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            System.Windows.Point brushLocation = this.BrushLocation;
            brushLocation.X *= base.OwnerCanvas.ScaleFactor.Ratio;
            brushLocation.Y *= base.OwnerCanvas.ScaleFactor.Ratio;
            g.TranslateTransform((float) -renderOffset.X, (float) -renderOffset.Y, MatrixOrder.Append);
            RectangleF rect = Utility.RectangleFromCenter(brushLocation.ToGdipPointF(), (float) (this.brushSize * base.OwnerCanvas.ScaleFactor.Ratio));
            g.PixelOffsetMode = PixelOffsetMode.Half;
            using (Pen pen = new Pen(Color.FromArgb(this.brushAlpha, Color.White), -1f))
            {
                using (Pen pen2 = new Pen(Color.FromArgb(this.brushAlpha, Color.Black), -1f))
                {
                    rect.Inflate(-2f, -2f);
                    g.DrawEllipse(pen, rect);
                    rect.Inflate(1f, 1f);
                    g.DrawEllipse(pen2, rect);
                    rect.Inflate(1f, 1f);
                    g.DrawEllipse(pen, rect);
                }
            }
            return renderOffset;
        }

        public int BrushAlpha
        {
            get => 
                this.brushAlpha;
            set
            {
                if (value != this.brushAlpha)
                {
                    this.brushAlpha = value;
                    this.InvalidateBrushLocation();
                }
            }
        }

        public System.Windows.Point BrushLocation
        {
            get => 
                this.brushLocation;
            set
            {
                if (value != this.brushLocation)
                {
                    Rect invalidateBrushRect = this.GetInvalidateBrushRect();
                    this.brushLocation = value;
                    Rect rect2 = this.GetInvalidateBrushRect();
                    base.InvalidateCanvas(Rect.Union(invalidateBrushRect, rect2));
                }
            }
        }

        public double BrushSize
        {
            get => 
                this.brushSize;
            set
            {
                if (value != this.brushSize)
                {
                    Rect invalidateBrushRect = this.GetInvalidateBrushRect();
                    this.brushSize = value;
                    Rect rect2 = this.GetInvalidateBrushRect();
                    base.InvalidateCanvas(Rect.Union(invalidateBrushRect, rect2));
                }
            }
        }
    }
}

