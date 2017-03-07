namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Windows;

    [Serializable]
    internal sealed class PlacedSurface : ISurfaceDraw, IDisposable, ICloneable
    {
        private bool disposed;
        private Surface what;
        private Int32Point where;

        private PlacedSurface(PlacedSurface ps)
        {
            this.where = ps.Where;
            this.what = ps.What.Clone();
        }

        public PlacedSurface(Surface source, Int32Rect roi)
        {
            this.where = roi.Location();
            using (ISurface<ColorBgra> surface = source.CreateWindow<ColorBgra>(roi))
            {
                this.what = new Surface(surface.Size<ColorBgra>());
                surface.Render<ColorBgra>(this.what);
            }
        }

        public object Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }
            return new PlacedSurface(this);
        }

        public void Dispose()
        {
            this.disposed = true;
            this.what.Dispose();
            this.what = null;
        }

        public void Draw(Surface dst)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }
            dst.CopySurface(this.what, (System.Drawing.Point) this.where);
        }

        public void Draw(Surface dst, IPixelOp pixelOp)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }
            Int32Rect rect = this.Bounds.IntersectCopy(dst.Bounds<ColorBgra>());
            if ((rect.Width > 0) && (rect.Height > 0))
            {
                int x = rect.X - this.where.X;
                int y = rect.Y - this.where.Y;
                pixelOp.Apply(dst, rect.Location().ToGdipPoint(), this.what, new System.Drawing.Point(x, y), rect.Size().ToGdipSize());
            }
        }

        public void Draw(Surface dst, int tX, int tY)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }
            System.Drawing.Point where = (System.Drawing.Point) this.where;
            try
            {
                this.where.X += tX;
                this.where.Y += tY;
                this.Draw(dst);
            }
            finally
            {
                this.where = where;
            }
        }

        public void Draw(Surface dst, int tX, int tY, IPixelOp pixelOp)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("PlacedSurface");
            }
            System.Drawing.Point where = (System.Drawing.Point) this.where;
            try
            {
                this.where.X += tX;
                this.where.Y += tY;
                this.Draw(dst, pixelOp);
            }
            finally
            {
                this.where = where;
            }
        }

        public Int32Rect Bounds
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("PlacedSurface");
                }
                return Int32RectUtil.From(this.Where, this.What.Size<ColorBgra>());
            }
        }

        public Int32Size Size
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("PlacedSurface");
                }
                return this.Size;
            }
        }

        public Surface What
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("PlacedSurface");
                }
                return this.what;
            }
        }

        public Int32Point Where
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("PlacedSurface");
                }
                return this.where;
            }
        }
    }
}

