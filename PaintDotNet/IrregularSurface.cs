namespace PaintDotNet
{
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.Serialization;
    using System.Windows;

    [Serializable]
    internal sealed class IrregularSurface : ISurfaceDraw, IDisposable, ICloneable, IDeserializationCallback
    {
        private bool disposed;
        [NonSerialized]
        private GeometryList geometry;
        private SegmentedList<PlacedSurface> placedSurfaces;

        private IrregularSurface(IrregularSurface cloneMe)
        {
            this.placedSurfaces = new SegmentedList<PlacedSurface>();
            this.placedSurfaces.EnsureCapacity(cloneMe.placedSurfaces.Count);
            foreach (PlacedSurface surface in cloneMe.placedSurfaces)
            {
                this.placedSurfaces.Add(surface.CloneT<PlacedSurface>());
            }
            this.geometry = cloneMe.geometry.Clone();
        }

        public IrregularSurface(Surface source, GeometryList roi)
        {
            GeometryList list = GeometryList.ClipToRect(roi, source.Bounds.ToInt32Rect());
            UnsafeList<Int32Rect> interiorScansUnsafeList = list.GetInteriorScansUnsafeList();
            this.placedSurfaces = new SegmentedList<PlacedSurface>();
            this.placedSurfaces.EnsureCapacity(interiorScansUnsafeList.Count);
            foreach (Int32Rect rect in interiorScansUnsafeList)
            {
                this.placedSurfaces.Add(new PlacedSurface(source, rect));
            }
            this.geometry = list;
        }

        public IrregularSurface(Surface source, Int32Rect[] roi) : this(source, GeometryList.FromScans(roi))
        {
        }

        public object Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }
            return new IrregularSurface(this);
        }

        public void Dispose()
        {
            foreach (PlacedSurface surface in this.placedSurfaces)
            {
                surface.Dispose();
            }
            this.placedSurfaces.Clear();
            this.placedSurfaces = null;
            if (this.geometry != null)
            {
                this.geometry.Dispose();
                this.geometry = null;
            }
            this.disposed = true;
        }

        public void Draw(Surface dst)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }
            foreach (PlacedSurface surface in this.placedSurfaces)
            {
                surface.Draw(dst);
            }
        }

        public void Draw(Surface dst, IPixelOp pixelOp)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }
            foreach (PlacedSurface surface in this.placedSurfaces)
            {
                surface.Draw(dst, pixelOp);
            }
        }

        public void Draw(Surface dst, int tX, int tY)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }
            foreach (PlacedSurface surface in this.placedSurfaces)
            {
                surface.Draw(dst, tX, tY);
            }
        }

        public void Draw(Surface dst, int tX, int tY, IPixelOp pixelOp)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("IrregularSurface");
            }
            foreach (PlacedSurface surface in this.placedSurfaces)
            {
                surface.Draw(dst, tX, tY, pixelOp);
            }
        }

        public void OnDeserialization(object sender)
        {
            Int32Rect[] scans = new Int32Rect[this.placedSurfaces.Count];
            for (int i = 0; i < scans.Length; i++)
            {
                scans[i] = Int32RectUtil.From(this.placedSurfaces[i].Where, this.placedSurfaces[i].What.Size<ColorBgra>());
            }
            this.geometry = GeometryList.FromScans(scans);
        }

        public GeometryList Geometry
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("IrregularSurface");
                }
                return this.geometry;
            }
        }
    }
}

