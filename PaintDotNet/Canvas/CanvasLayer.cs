namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Windows;

    internal abstract class CanvasLayer : Disposable
    {
        public const int MaxXCoordinate = 0x20000;
        public const int MaxYCoordinate = 0x20000;
        public const int MinXCoordinate = -131072;
        public const int MinYCoordinate = -131072;
        private CanvasRenderer ownerCanvas;
        private bool visible;

        public CanvasLayer(CanvasRenderer canvasRenderer)
        {
            this.ownerCanvas = canvasRenderer;
            this.visible = true;
        }

        protected void Invalidate()
        {
            Rect rect = RectUtil.FromEdges(-131072.0, -131072.0, 131073.0, 131073.0);
            this.InvalidateCanvas(rect);
        }

        public void InvalidateCanvas()
        {
            this.InvalidateCanvas(MaxBounds);
        }

        protected void InvalidateCanvas(Int32Rect rect)
        {
            this.InvalidateCanvas(rect.ToRect());
        }

        protected void InvalidateCanvas(Int32Rect[] rects)
        {
            Rect[] rectArray = new Rect[rects.Length];
            for (int i = 0; i < rects.Length; i++)
            {
                rectArray[i] = rects[i].ToRect();
            }
            this.InvalidateCanvas(ref rectArray, true);
        }

        protected void InvalidateCanvas(Rect rect)
        {
            Rect[] rects = new Rect[] { rect };
            this.InvalidateCanvas(ref rects, true);
        }

        protected void InvalidateCanvas(Rect[] rects)
        {
            this.InvalidateCanvas(ref rects, false);
        }

        protected void InvalidateCanvas(ref Rect[] rects, bool takeOwnership)
        {
            this.OwnerCanvas.InvalidateCanvas(ref rects, takeOwnership);
        }

        public virtual void OnCanvasSizeChanged()
        {
        }

        protected abstract void OnRender(ISurface<ColorBgra> dst, Int32Point offset);
        public virtual void OnRenderDstSizeChanged()
        {
        }

        protected abstract void OnVisibleChanged();
        protected virtual void OnVisibleChanging()
        {
        }

        public void Render(ISurface<ColorBgra> dst, Int32Point offset)
        {
            this.OnRender(dst, offset);
        }

        public Int32Size CanvasSize =>
            this.OwnerCanvas.CanvasSize;

        public virtual bool ClipsToCanvas =>
            true;

        public static Int32Rect MaxBounds =>
            Int32RectUtil.FromEdges(-131072, -131072, 0x20001, 0x20001);

        protected CanvasRenderer OwnerCanvas =>
            this.ownerCanvas;

        public Int32Size RenderDstSize =>
            this.OwnerCanvas.RenderDstSize;

        protected object SyncRoot =>
            this.OwnerCanvas.SyncRoot;

        public bool Visible
        {
            get => 
                this.visible;
            set
            {
                if (this.visible != value)
                {
                    this.OnVisibleChanging();
                    this.visible = value;
                    this.OnVisibleChanged();
                }
            }
        }
    }
}

