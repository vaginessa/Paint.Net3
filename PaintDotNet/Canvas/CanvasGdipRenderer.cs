namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Windows;

    internal abstract class CanvasGdipRenderer : CanvasLayer
    {
        public CanvasGdipRenderer(CanvasRenderer ownerCanvas) : base(ownerCanvas)
        {
        }

        protected sealed override void OnRender(ISurface<ColorBgra> dst, Int32Point renderOffset)
        {
            if (this.ShouldRender(Int32RectUtil.From(renderOffset, dst.Size<ColorBgra>())))
            {
                using (RenderArgs args = new RenderArgs(dst))
                {
                    this.RenderToGraphics(args, renderOffset);
                }
            }
        }

        public abstract void RenderToGraphics(RenderArgs ra, Int32Point offset);
        public virtual bool ShouldRender(Int32Rect renderBounds) => 
            base.Visible;

        public override bool ClipsToCanvas =>
            false;
    }
}

