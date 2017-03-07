namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class DocumentBox : GdiBufferedPaintControl, IDispatcherObject
    {
        private CanvasDocumentRenderer canvasDocumentRenderer;
        private PaintDotNet.Canvas.CanvasRenderer canvasRenderer;
        private IDispatcher dispatcher;
        private PaintDotNet.Document document;
        public const int MaxSideLength = 0x7fff;
        private PaintDotNet.ScaleFactor scaleFactor;

        public event Action GdiPaint;

        public DocumentBox()
        {
            this.dispatcher = new ControlDispatcher(this);
            base.SetStyle(ControlStyles.Selectable, false);
            this.scaleFactor = PaintDotNet.ScaleFactor.OneToOne;
            this.canvasRenderer = new PaintDotNet.Canvas.CanvasRenderer(base.Size.ToInt32Size(), base.Size.ToInt32Size());
            this.canvasRenderer.CanvasInvalidated += new EventHandler<RectsEventArgs>(this.Renderers_Invalidated);
            this.canvasDocumentRenderer = new CanvasDocumentRenderer(this.dispatcher, this.canvasRenderer, null);
            this.canvasRenderer.Add(this.canvasDocumentRenderer, false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.canvasDocumentRenderer != null))
            {
                this.canvasRenderer.Remove(this.canvasDocumentRenderer);
                this.canvasDocumentRenderer.Dispose();
                this.canvasDocumentRenderer = null;
            }
            base.Dispose(disposing);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void DrawArea(ISurface<ColorBgra> dst, System.Drawing.Point offset)
        {
            this.CanvasRenderer.Parallelize(7).Render(dst, offset);
        }

        protected override void OnGdiPaint(GdiPaintContext ctx)
        {
            Rectangle[] updateRegion;
            if (ctx.UpdateRegion.Length > 9)
            {
                updateRegion = new Rectangle[] { ctx.UpdateRect };
            }
            else
            {
                updateRegion = ctx.UpdateRegion;
            }
            foreach (Rectangle rectangle in updateRegion)
            {
                if (rectangle.HasPositiveArea())
                {
                    using (Surface surface = base.GetDoubleBuffer(rectangle.Size))
                    {
                        this.DrawArea(surface, rectangle.Location);
                        base.DrawDoubleBuffer(ctx.Hdc, surface, rectangle);
                    }
                }
            }
            base.OnGdiPaint(ctx);
            if (this.GdiPaint != null)
            {
                this.GdiPaint();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            System.Drawing.Size size = base.Size;
            if ((base.Width == 0x7fff) && (this.document != null))
            {
                size.Height = (int) ((0x8000L * this.document.Height) / ((long) this.document.Width));
            }
            else if (size.Width == 0)
            {
                size.Width = 1;
            }
            if ((base.Width == 0x7fff) && (this.document != null))
            {
                size.Width = (int) ((0x8000L * this.document.Width) / ((long) this.document.Height));
            }
            else if (size.Height == 0)
            {
                size.Height = 1;
            }
            if (size != base.Size)
            {
                base.Size = size;
            }
            if (this.document == null)
            {
                this.scaleFactor = PaintDotNet.ScaleFactor.OneToOne;
            }
            else
            {
                PaintDotNet.ScaleFactor factor = PaintDotNet.ScaleFactor.Max(base.Width, this.document.Width, base.Height, this.document.Height, PaintDotNet.ScaleFactor.OneToOne);
                this.scaleFactor = factor;
            }
            this.canvasRenderer.RenderDstSize = base.Size.ToInt32Size();
        }

        public void PopCacheStandby()
        {
            this.canvasDocumentRenderer.PopCacheStandby();
        }

        public void PrefetchSync()
        {
            this.PrefetchSync(new Int32Rect(0, 0, base.Width, base.Height));
        }

        public void PrefetchSync(Int32Rect bounds)
        {
            this.canvasDocumentRenderer.PrefetchSync(bounds);
        }

        public void PushCacheStandby()
        {
            this.canvasDocumentRenderer.PushCacheStandby();
        }

        private void Renderers_Invalidated(object sender, RectsEventArgs e)
        {
            try
            {
                PaintDotNet.ScaleFactor scaleFactor = this.canvasRenderer.ScaleFactor;
                if (scaleFactor.Denominator != 0)
                {
                    double ratio = scaleFactor.Ratio;
                    if (ratio.IsFinite())
                    {
                        Rect[] rects = e.Rects;
                        for (int i = 0; i < rects.Length; i++)
                        {
                            Int32Rect rect = this.canvasRenderer.CanvasToRenderDst(rects[i]).Int32Bound();
                            this.Invalidate(rect);
                        }
                    }
                }
            }
            catch (ArithmeticException)
            {
            }
        }

        public PaintDotNet.Canvas.CanvasRenderer CanvasRenderer =>
            this.canvasRenderer;

        public IDispatcher Dispatcher =>
            this.dispatcher;

        public PaintDotNet.Document Document
        {
            get => 
                this.document;
            set
            {
                this.document = value;
                this.canvasDocumentRenderer.Document = value;
                if (this.document != null)
                {
                    base.Size = Int32Size.Truncate(this.scaleFactor.Scale(this.document.Size())).ToGdipSize();
                    this.canvasRenderer.CanvasSize = this.document.Size.ToInt32Size();
                    this.canvasRenderer.RenderDstSize = base.Size.ToInt32Size();
                }
                base.Invalidate();
            }
        }

        public bool HighQualityZoomIn
        {
            get => 
                this.canvasDocumentRenderer.HighQualityZoomIn;
            set
            {
                this.canvasDocumentRenderer.HighQualityZoomIn = value;
            }
        }

        public bool HighQualityZoomOut
        {
            get => 
                this.canvasDocumentRenderer.HighQualityZoomOut;
            set
            {
                this.canvasDocumentRenderer.HighQualityZoomOut = value;
            }
        }

        public PaintDotNet.ScaleFactor ScaleFactor =>
            this.scaleFactor;
    }
}

