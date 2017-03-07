namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class ControlShadow : GdiBufferedPaintControl
    {
        private Color aeroBackColor = Color.FromArgb(0xff, 0xc9, 0xd3, 0xe2);
        private Color classicBackColor = Color.FromArgb(0xc0, 0xc0, 0xc0);
        private Control occludingControl;
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;

        public event Action<ControlShadow, ISurface<ColorBgra>, Rectangle> GdiPaint;

        public ControlShadow()
        {
            base.SetStyle(ControlStyles.Selectable, false);
            this.Dock = DockStyle.Fill;
            base.ResizeRedraw = true;
        }

        private void DrawOutlineAndShadow(ISurface<ColorBgra> dst, Rectangle clipRect)
        {
            if (this.occludingControl != null)
            {
                Rectangle r = new Rectangle(new Point(0, 0), this.occludingControl.Size);
                r = this.occludingControl.RectangleToScreen(r);
                r = base.RectangleToClient(r);
                int recommendedExtent = DropShadow.GetRecommendedExtent(r.Size);
                if (Rectangle.Intersect(clipRect, Rectangle.Inflate(r, recommendedExtent, recommendedExtent)).HasPositiveArea())
                {
                    using (RenderArgs args = new RenderArgs(dst))
                    {
                        args.Graphics.TranslateTransform((float) -clipRect.X, (float) -clipRect.Y);
                        DropShadow.DrawOutside(args.Graphics, this.penBrushCache, r, recommendedExtent);
                    }
                }
            }
        }

        private static Rectangle[] Exclude(Rectangle rect, Rectangle excludeRect)
        {
            PdnRegion region = new PdnRegion(rect);
            region.Exclude(excludeRect);
            Rectangle[] regionScansReadOnlyInt = region.GetRegionScansReadOnlyInt();
            region.Dispose();
            return regionScansReadOnlyInt;
        }

        protected override void OnGdiPaint(GdiPaintContext ctx)
        {
            Rectangle[] updateRegion;
            Rectangle excludeRect = base.RectangleToClient(this.occludingControl.RectangleToScreen(this.occludingControl.ClientRectangle));
            if (ctx.UpdateRegion.Length > 9)
            {
                updateRegion = new Rectangle[] { ctx.UpdateRect };
            }
            else
            {
                updateRegion = ctx.UpdateRegion;
            }
            if (updateRegion == null)
            {
                updateRegion = new Rectangle[] { base.ClientRectangle };
            }
            foreach (Rectangle rectangle2 in updateRegion)
            {
                foreach (Rectangle rectangle3 in Exclude(rectangle2, excludeRect))
                {
                    if (rectangle3.HasPositiveArea())
                    {
                        using (Surface surface = base.GetDoubleBuffer(rectangle3.Size))
                        {
                            Color c = (ThemeConfig.EffectiveTheme == PdnTheme.Aero) ? this.aeroBackColor : this.classicBackColor;
                            surface.Clear(ColorBgra.FromColor(c));
                            this.DrawOutlineAndShadow(surface, rectangle3);
                            if (this.GdiPaint != null)
                            {
                                this.GdiPaint(this, surface, rectangle3);
                            }
                            base.DrawDoubleBuffer(ctx.Hdc, surface, rectangle3);
                        }
                    }
                }
            }
            base.OnGdiPaint(ctx);
        }

        [Browsable(false)]
        public Control OccludingControl
        {
            get => 
                this.occludingControl;
            set
            {
                this.occludingControl = value;
                base.Invalidate();
            }
        }
    }
}

