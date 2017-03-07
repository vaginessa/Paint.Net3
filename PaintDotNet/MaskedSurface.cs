namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    [Serializable]
    internal sealed class MaskedSurface : ICloneable, IIsDisposed, IDisposable
    {
        private bool disposed;
        private const double fp_MaxValue = 131071.0;
        private const double fp_MultFactor = 16384.0;
        private const int fp_RoundFactor = 0x1fff;
        private const int fp_ShiftFactor = 14;
        private GeometryList geometryMask;
        private PaintDotNet.Surface surface;

        private MaskedSurface()
        {
        }

        public MaskedSurface(PaintDotNet.Surface source, GeometryList geometryMask)
        {
            Int32Rect rect4;
            Int32Rect rect2 = geometryMask.Bounds.Int32Bound();
            Int32Rect rect3 = Int32RectUtil.Intersect(rect2, source.Bounds<ColorBgra>());
            if (rect2 != rect3)
            {
                GeometryList list = GeometryList.ClipToRect(geometryMask, source.Bounds<ColorBgra>());
                this.geometryMask = list;
                rect4 = this.geometryMask.Bounds.Int32Bound();
            }
            else
            {
                this.geometryMask = geometryMask.Clone();
                rect4 = rect3;
            }
            if (!rect4.HasZeroArea())
            {
                this.surface = new PaintDotNet.Surface(rect4.Size());
                this.surface.CopySurface(source, rect4);
            }
        }

        public MaskedSurface(ref PaintDotNet.Surface source, bool takeOwnership)
        {
            if (takeOwnership)
            {
                this.surface = source;
                source = null;
            }
            else
            {
                this.surface = source.Clone();
            }
            this.geometryMask = new GeometryList(this.surface.Bounds<ColorBgra>());
        }

        public MaskedSurface(ref PaintDotNet.Surface source, bool takeOwnershipOfSurface, ref GeometryList geometryMaskAndOffset, bool takeOwnershipOfGMAO)
        {
            if (takeOwnershipOfSurface)
            {
                this.surface = source;
                source = null;
            }
            else
            {
                this.surface = source.Clone();
            }
            if (takeOwnershipOfGMAO)
            {
                this.geometryMask = geometryMaskAndOffset;
                geometryMaskAndOffset = null;
            }
            else
            {
                this.geometryMask = geometryMaskAndOffset.CloneT<GeometryList>();
            }
        }

        public MaskedSurface Clone()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            MaskedSurface surface = new MaskedSurface {
                geometryMask = this.geometryMask.Clone()
            };
            if (this.surface != null)
            {
                surface.surface = this.surface.Clone();
            }
            return surface;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free<PaintDotNet.Surface>(ref this.surface);
                DisposableUtil.Free<GeometryList>(ref this.geometryMask);
            }
            this.disposed = true;
        }

        public void Draw(PaintDotNet.Surface dst)
        {
            this.Draw(dst, 0, 0);
        }

        public void Draw(PaintDotNet.Surface dst, int tX, int tY)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            Matrix transform = new Matrix();
            transform.Translate((double) tX, (double) tY);
            this.Draw(dst, transform, ResamplingAlgorithm.Bilinear);
        }

        public void Draw(PaintDotNet.Surface dst, Matrix transform, ResamplingAlgorithm sampling)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            if ((this.surface != null) && transform.HasInverse)
            {
                if ((((sampling == ResamplingAlgorithm.Bilinear) && (transform.M11 == 1.0)) && ((transform.M12 == 0.0) && (transform.M21 == 0.0))) && (((transform.M22 == 1.0) && transform.OffsetX.IsInteger()) && transform.OffsetY.IsInteger()))
                {
                    this.Draw(dst, transform, ResamplingAlgorithm.NearestNeighbor);
                }
                else
                {
                    WaitCallback callback;
                    Int32Rect rect = this.geometryMask.Bounds.Int32Bound();
                    GeometryList list = GeometryList.Transform(this.geometryMask, transform);
                    Int32Rect[] interiorScans = list.GetInteriorScans();
                    DrawContext context = new DrawContext {
                        boundsX = rect.X,
                        boundsY = rect.Y,
                        inverse = transform
                    };
                    context.inverse.Invert();
                    Vector[] vectors = new Vector[] { new Vector(1.0, 0.0), new Vector(0.0, 1.0) };
                    context.inverse.Transform(vectors);
                    context.dsxddx = vectors[0].X;
                    if (Math.Abs(context.dsxddx) > 131071.0)
                    {
                        context.dsxddx = 0.0;
                    }
                    context.dsyddx = vectors[0].Y;
                    if (Math.Abs(context.dsyddx) > 131071.0)
                    {
                        context.dsyddx = 0.0;
                    }
                    context.dsxddy = vectors[1].X;
                    if (Math.Abs(context.dsxddy) > 131071.0)
                    {
                        context.dsxddy = 0.0;
                    }
                    context.dsyddy = vectors[1].Y;
                    if (Math.Abs(context.dsyddy) > 131071.0)
                    {
                        context.dsyddy = 0.0;
                    }
                    context.fp_dsxddx = (int) (context.dsxddx * 16384.0);
                    context.fp_dsyddx = (int) (context.dsyddx * 16384.0);
                    context.fp_dsxddy = (int) (context.dsxddy * 16384.0);
                    context.fp_dsyddy = (int) (context.dsyddy * 16384.0);
                    context.dst = dst;
                    context.src = this.surface;
                    if (interiorScans.Length == 1)
                    {
                        context.dstScans = new Int32Rect[Processor.LogicalCpuCount];
                        Utility.SplitRectangle(interiorScans[0], context.dstScans);
                    }
                    else
                    {
                        context.dstScans = interiorScans;
                    }
                    switch (sampling)
                    {
                        case ResamplingAlgorithm.NearestNeighbor:
                            callback = new WaitCallback(context.DrawScansNearestNeighbor);
                            break;

                        case ResamplingAlgorithm.Bilinear:
                            callback = new WaitCallback(context.DrawScansBilinear);
                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }
                    using (PrivateThreadPool pool = new PrivateThreadPool())
                    {
                        for (int i = 0; i < pool.Threads; i++)
                        {
                            if (i == (pool.Threads - 1))
                            {
                                callback(BoxedConstants.GetInt32(i));
                            }
                            else
                            {
                                pool.QueueUserWorkItem(callback, BoxedConstants.GetInt32(i));
                            }
                        }
                    }
                    context.src = null;
                    list.Dispose();
                    list = null;
                }
            }
        }

        public GeometryList GetGeometryMaskCopy()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            return this.geometryMask.Clone();
        }

        public GeometryList GetGeometryMaskReadOnly()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            return this.geometryMask;
        }

        public Int32Rect[] GetGeometryMaskScans()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            return this.geometryMask.GetInteriorScans();
        }

        public Int32Rect[] GetGeometryMaskScans(Matrix transform)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MaskedSurface");
            }
            return this.geometryMask.GetInteriorScans(transform);
        }

        object ICloneable.Clone() => 
            this.Clone();

        public Rect GeometryMaskBounds =>
            this.geometryMask.Bounds;

        public bool IsDisposed =>
            this.disposed;

        internal PaintDotNet.Surface Surface =>
            this.surface;

        public PaintDotNet.Surface SurfaceReadOnly =>
            this.surface;

        private class DrawContext
        {
            public int boundsX;
            public int boundsY;
            public Surface dst;
            public Int32Rect[] dstScans;
            public double dsxddx;
            public double dsxddy;
            public double dsyddx;
            public double dsyddy;
            public int fp_dsxddx;
            public int fp_dsxddy;
            public int fp_dsyddx;
            public int fp_dsyddy;
            public Matrix inverse;
            public Surface src;

            public void DrawScansBilinear(object cpuNumberObj)
            {
                int num = (int) cpuNumberObj;
                int logicalCpuCount = Processor.LogicalCpuCount;
                Int32Rect rect = this.dst.Bounds<ColorBgra>();
                for (int i = num; i < this.dstScans.Length; i += logicalCpuCount)
                {
                    Int32Rect rect2 = this.dstScans[i].IntersectCopy(rect);
                    Point point = new Point((double) rect2.X, (double) rect2.Y);
                    point.X += 0.5;
                    point.Y += 0.5;
                    point = this.inverse.Transform(point);
                    point.X -= this.boundsX;
                    point.Y -= this.boundsY;
                    point.X -= 0.5;
                    point.Y -= 0.5;
                    Point point2 = point;
                    for (int j = rect2.Y; j < (rect2.Y + rect2.Height); j++)
                    {
                        Point point3 = point2;
                        if (j >= 0)
                        {
                            for (int k = rect2.X; k < (rect2.X + rect2.Width); k++)
                            {
                                float x = (float) point3.X;
                                float y = (float) point3.Y;
                                ColorBgra bilinearSampleClamped = this.src.GetBilinearSampleClamped(x, y);
                                *(this.dst.GetPointAddressUnchecked(k, j)) = bilinearSampleClamped;
                                point3.X += this.dsxddx;
                                point3.Y += this.dsyddx;
                            }
                        }
                        point2.X += this.dsxddy;
                        point2.Y += this.dsyddy;
                    }
                }
            }

            public unsafe void DrawScansNearestNeighbor(object cpuNumberObj)
            {
                int num = (int) cpuNumberObj;
                int logicalCpuCount = Processor.LogicalCpuCount;
                void* voidStar = this.src.Scan0.VoidStar;
                int stride = this.src.Stride;
                Int32Rect rect = this.dst.Bounds<ColorBgra>();
                for (int i = num; i < this.dstScans.Length; i += logicalCpuCount)
                {
                    Int32Rect rect2 = this.dstScans[i].IntersectCopy(rect);
                    if ((rect2.Width != 0) && (rect2.Height != 0))
                    {
                        Point point = new Point((double) rect2.X, (double) rect2.Y);
                        point.X += 0.5;
                        point.Y += 0.5;
                        point = this.inverse.Transform(point);
                        point.X -= this.boundsX;
                        point.Y -= this.boundsY;
                        point.X -= 0.5;
                        point.Y -= 0.5;
                        int num5 = (int) (point.X * 16384.0);
                        int num6 = (int) (point.Y * 16384.0);
                        for (int j = rect2.Y; j < (rect2.Y + rect2.Height); j++)
                        {
                            int num8 = num5;
                            int num9 = num6;
                            num5 += this.fp_dsxddy;
                            num6 += this.fp_dsyddy;
                            if (j >= 0)
                            {
                                int x = rect2.X;
                                ColorBgra* pointAddress = this.dst.GetPointAddress(x, j);
                                ColorBgra* bgraPtr2 = pointAddress + rect2.Width;
                                int num11 = num8 + (this.fp_dsxddx * (rect2.Width - 1));
                                int num12 = num9 + (this.fp_dsyddx * (rect2.Width - 1));
                                while (pointAddress < bgraPtr2)
                                {
                                    int num13 = (num8 + 0x1fff) >> 14;
                                    int num14 = (num9 + 0x1fff) >> 14;
                                    int num15 = Int32Util.Clamp(num13, 0, this.src.Width - 1);
                                    int y = Int32Util.Clamp(num14, 0, this.src.Height - 1);
                                    pointAddress[0] = this.src.GetPointUnchecked(num15, y);
                                    pointAddress++;
                                    num8 += this.fp_dsxddx;
                                    num9 += this.fp_dsyddx;
                                    if ((num15 == num13) && (y == num14))
                                    {
                                        break;
                                    }
                                }
                                ColorBgra* bgraPtr3 = pointAddress;
                                pointAddress = bgraPtr2 - 1;
                                while (pointAddress >= bgraPtr3)
                                {
                                    int num17 = (num11 + 0x1fff) >> 14;
                                    int num18 = (num12 + 0x1fff) >> 14;
                                    int num19 = Int32Util.Clamp(num17, 0, this.src.Width - 1);
                                    int num20 = Int32Util.Clamp(num18, 0, this.src.Height - 1);
                                    pointAddress[0] = this.src.GetPointUnchecked(num19, num20);
                                    if ((num19 == num17) && (num20 == num18))
                                    {
                                        break;
                                    }
                                    pointAddress--;
                                    num11 -= this.fp_dsxddx;
                                    num12 -= this.fp_dsyddx;
                                }
                                ColorBgra* bgraPtr4 = pointAddress;
                                while (bgraPtr3 < bgraPtr4)
                                {
                                    int num21 = (num8 + 0x1fff) >> 14;
                                    int num22 = (num9 + 0x1fff) >> 14;
                                    bgraPtr3->Bgra = (((IntPtr) (num21 * sizeof(ColorBgra))) + (voidStar + (num22 * stride))).Bgra;
                                    bgraPtr3++;
                                    num8 += this.fp_dsxddx;
                                    num9 += this.fp_dsyddx;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

