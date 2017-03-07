namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    internal sealed class Selection : ThreadAffinitizedObjectBase
    {
        private int alreadyChanging = 0;
        private Int32Rect clipRectangle = new Int32Rect(0, 0, 0xffff, 0xffff);
        private Data data = new Data();

        public event EventHandler Changed;

        public event EventHandler Changing;

        public void CommitContinuation()
        {
            this.VerifyAccess();
            this.OnChanging();
            this.data.baseGeometry = this.CreateGeometryList(true);
            this.data.continuationGeometry.Clear();
            this.data.continuationCombineMode = SelectionCombineMode.Xor;
            this.OnChanged();
        }

        public void CommitInterimTransform()
        {
            this.VerifyAccess();
            if (!this.data.interimTransform.IsIdentity)
            {
                this.OnChanging();
                this.data.baseGeometry.Transform(this.data.interimTransform);
                this.data.continuationGeometry.Transform(this.data.interimTransform);
                this.data.cumulativeTransform = Matrix.Multiply(this.data.cumulativeTransform, this.data.interimTransform);
                this.data.interimTransform = Matrix.Identity;
                this.OnChanged();
            }
        }

        public GeometryList CreateGeometryList() => 
            this.CreateGeometryList(true);

        public GeometryList CreateGeometryList(bool applyInterimTransform)
        {
            GeometryList list;
            this.VerifyAccess();
            if (this.data.continuationCombineMode == SelectionCombineMode.Replace)
            {
                list = this.data.continuationGeometry.Clone();
            }
            else
            {
                list = GeometryList.Combine(this.data.baseGeometry, this.data.continuationCombineMode.ToGeometryCombineMode(), this.data.continuationGeometry);
            }
            if (applyInterimTransform)
            {
                list.Transform(this.data.interimTransform);
            }
            return list;
        }

        public GeometryList CreateGeometryListClippingMask()
        {
            if (this.IsEmpty)
            {
                GeometryList list = new GeometryList();
                list.AddRect(this.ClipRectangle);
                return list;
            }
            GeometryList geometry = this.CreateGeometryList();
            if (this.ClipRectangle.Contains(geometry.Bounds))
            {
                return geometry;
            }
            GeometryList list3 = GeometryList.ClipToRect(geometry, this.ClipRectangle);
            geometry.Dispose();
            return list3;
        }

        [Obsolete]
        public PdnGraphicsPath CreatePath()
        {
            this.VerifyAccess();
            return this.CreatePath(true);
        }

        [Obsolete]
        private PdnGraphicsPath CreatePath(bool applyInterimTransform)
        {
            this.VerifyAccess();
            PdnGraphicsPath path = new PdnGraphicsPath();
            GeometryList geometryList = this.CreateGeometryList(applyInterimTransform);
            path.AddGeometryList(geometryList);
            return path;
        }

        public PdnRegion CreateRegion()
        {
            this.VerifyAccess();
            if (this.IsEmpty)
            {
                return new PdnRegion(this.clipRectangle);
            }
            using (GeometryList list = this.CreateGeometryListClippingMask())
            {
                return Utility.RectanglesToRegion(list.GetInteriorScans());
            }
        }

        [Obsolete]
        public PdnRegion CreateRegionRaw()
        {
            this.VerifyAccess();
            using (PdnGraphicsPath path = this.CreatePath())
            {
                return new PdnRegion(path);
            }
        }

        public Int32Rect GetBounds()
        {
            this.VerifyAccess();
            return this.GetBounds(true);
        }

        public Int32Rect GetBounds(bool applyInterimTransformation)
        {
            this.VerifyAccess();
            return this.GetBoundsF(applyInterimTransformation).Int32Bound();
        }

        public Rect GetBoundsF()
        {
            this.VerifyAccess();
            return this.GetBoundsF(true);
        }

        public Rect GetBoundsF(bool applyInterimTransformation)
        {
            this.VerifyAccess();
            using (GeometryList list = this.CreateGeometryList(applyInterimTransformation))
            {
                return list.Bounds;
            }
        }

        public Matrix GetCumulativeTransformCopy()
        {
            this.VerifyAccess();
            return this.data.cumulativeTransform;
        }

        public Matrix GetInterimTransformCopy()
        {
            this.VerifyAccess();
            return this.data.interimTransform;
        }

        private void OnChanged()
        {
            this.VerifyAccess();
            if (this.alreadyChanging <= 0)
            {
                throw new InvalidOperationException("Changed event was raised without corresponding Changing event beforehand");
            }
            this.alreadyChanging--;
            if ((this.alreadyChanging == 0) && (this.Changed != null))
            {
                this.Changed(this, EventArgs.Empty);
            }
        }

        private void OnChanging()
        {
            this.VerifyAccess();
            if ((this.alreadyChanging == 0) && (this.Changing != null))
            {
                this.Changing(this, EventArgs.Empty);
            }
            this.alreadyChanging++;
        }

        public void PerformChanged()
        {
            this.OnChanged();
        }

        public void PerformChanging()
        {
            this.OnChanging();
        }

        public void Reset()
        {
            this.VerifyAccess();
            this.OnChanging();
            this.data.baseGeometry.Clear();
            this.data.continuationGeometry.Clear();
            this.data.cumulativeTransform = Matrix.Identity;
            this.data.interimTransform = Matrix.Identity;
            this.OnChanged();
        }

        public void ResetContinuation()
        {
            this.VerifyAccess();
            this.OnChanging();
            this.CommitInterimTransform();
            this.ResetCumulativeTransform();
            this.data.continuationGeometry.Clear();
            this.OnChanged();
        }

        private void ResetCumulativeTransform()
        {
            this.VerifyAccess();
            this.data.cumulativeTransform = Matrix.Identity;
        }

        public void ResetInterimTransform()
        {
            this.VerifyAccess();
            this.OnChanging();
            this.data.interimTransform = Matrix.Identity;
            this.OnChanged();
        }

        public void Restore(object state)
        {
            this.VerifyAccess();
            if (!(state is Data))
            {
                throw new InvalidCastException();
            }
            this.OnChanging();
            this.data.Dispose();
            this.data = ((Data) state).Clone();
            this.OnChanged();
        }

        public object Save()
        {
            this.VerifyAccess();
            return this.data.Clone();
        }

        public void SetContinuation(Int32Point[] polygon, SelectionCombineMode combineMode)
        {
            this.VerifyAccess();
            this.OnChanging();
            this.CommitInterimTransform();
            this.ResetCumulativeTransform();
            this.data.continuationCombineMode = combineMode;
            this.data.continuationGeometry.Clear();
            Point[] points = new Point[polygon.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = (Point) polygon[i];
            }
            this.data.continuationGeometry.AddPolygon(points);
            this.OnChanged();
        }

        public void SetContinuation(Point[] polygon, SelectionCombineMode combineMode)
        {
            this.SetContinuation(ref polygon, combineMode, false);
        }

        [Obsolete]
        public void SetContinuation(PdnGraphicsPath path, SelectionCombineMode combineMode)
        {
            this.VerifyAccess();
            if (!this.data.baseGeometry.IsEmpty)
            {
                throw new InvalidOperationException("base path must be empty to use this overload of SetContinuation");
            }
            this.OnChanging();
            this.CommitInterimTransform();
            this.ResetCumulativeTransform();
            this.data.continuationCombineMode = combineMode;
            this.data.continuationGeometry = path.ToGeometryList();
            this.OnChanged();
        }

        public void SetContinuation(GeometryList geometry, SelectionCombineMode combineMode)
        {
            this.SetContinuation(ref geometry, false, combineMode);
        }

        public void SetContinuation(IList<Int32Point[]> polyList, SelectionCombineMode combineMode)
        {
            this.VerifyAccess();
            this.OnChanging();
            this.CommitInterimTransform();
            this.ResetCumulativeTransform();
            this.data.continuationCombineMode = combineMode;
            for (int i = 0; i < polyList.Count; i++)
            {
                Int32Point[] pointArray = polyList[i];
                Point[] poly = new Point[pointArray.Length];
                for (int j = 0; j < poly.Length; j++)
                {
                    poly[j] = (Point) pointArray[j];
                }
                this.data.continuationGeometry.AddPolygon(ref poly, true);
            }
            this.OnChanged();
        }

        public void SetContinuation(Int32Rect rect, SelectionCombineMode combineMode)
        {
            this.SetContinuation(rect.ToRect(), combineMode);
        }

        public void SetContinuation(Rect rect, SelectionCombineMode combineMode)
        {
            this.VerifyAccess();
            this.OnChanging();
            this.CommitInterimTransform();
            this.ResetCumulativeTransform();
            this.data.continuationCombineMode = combineMode;
            this.data.continuationGeometry.Clear();
            this.data.continuationGeometry.AddRect(rect);
            this.OnChanged();
        }

        public void SetContinuation(ref Point[] polygon, SelectionCombineMode combineMode, bool takeOwnership)
        {
            this.VerifyAccess();
            this.OnChanging();
            this.CommitInterimTransform();
            this.ResetCumulativeTransform();
            this.data.continuationCombineMode = combineMode;
            this.data.continuationGeometry.Clear();
            this.data.continuationGeometry.AddPolygon(ref polygon, takeOwnership);
            this.OnChanged();
        }

        public void SetContinuation(IList<Point[]> polyList, SelectionCombineMode combineMode, bool takeOwnership)
        {
            this.VerifyAccess();
            this.OnChanging();
            this.CommitInterimTransform();
            this.ResetCumulativeTransform();
            this.data.continuationCombineMode = combineMode;
            this.data.continuationGeometry.AddPolygonList(polyList, takeOwnership);
            this.OnChanged();
        }

        public void SetContinuation(ref GeometryList geometry, bool takeOwnership, SelectionCombineMode combineMode)
        {
            this.VerifyAccess();
            this.OnChanging();
            this.CommitInterimTransform();
            this.ResetCumulativeTransform();
            this.data.continuationCombineMode = combineMode;
            if (takeOwnership)
            {
                this.data.continuationGeometry = geometry;
                geometry = null;
            }
            else
            {
                this.data.continuationGeometry = geometry.Clone();
            }
            this.OnChanged();
        }

        public void SetInterimTransform(Matrix m)
        {
            this.VerifyAccess();
            VerifyFinite(m);
            this.OnChanging();
            this.data.interimTransform = m;
            this.OnChanged();
        }

        private static void VerifyFinite(Matrix m)
        {
            if (!m.IsFinite())
            {
                throw new ArgumentException("matrix isn't finite, " + m.ToString());
            }
        }

        public Int32Rect ClipRectangle
        {
            get
            {
                this.VerifyAccess();
                return this.clipRectangle;
            }
            set
            {
                this.VerifyAccess();
                this.clipRectangle = value;
            }
        }

        public bool IsEmpty
        {
            get
            {
                this.VerifyAccess();
                return (this.data.baseGeometry.IsEmpty && this.data.continuationGeometry.IsEmpty);
            }
        }

        [Serializable]
        private sealed class Data : ICloneable, IDisposable
        {
            public GeometryList baseGeometry = new GeometryList();
            public SelectionCombineMode continuationCombineMode = SelectionCombineMode.Xor;
            public GeometryList continuationGeometry = new GeometryList();
            public Matrix cumulativeTransform = Matrix.Identity;
            public Matrix interimTransform = Matrix.Identity;

            public Selection.Data Clone() => 
                new Selection.Data { 
                    baseGeometry = this.baseGeometry.Clone(),
                    continuationGeometry = this.continuationGeometry.Clone(),
                    continuationCombineMode = this.continuationCombineMode,
                    cumulativeTransform = this.cumulativeTransform,
                    interimTransform = this.interimTransform
                };

            public void Dispose()
            {
                this.Dispose(true);
            }

            public void Dispose(bool disposing)
            {
                if (disposing)
                {
                    DisposableUtil.Free<GeometryList>(ref this.baseGeometry);
                    DisposableUtil.Free<GeometryList>(ref this.continuationGeometry);
                }
            }

            object ICloneable.Clone() => 
                this.Clone();
        }
    }
}

