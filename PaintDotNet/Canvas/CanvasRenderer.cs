namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;

    internal sealed class CanvasRenderer : IRenderer<ColorBgra>
    {
        private int[] c2DLookupX;
        private int[] c2DLookupY;
        private Int32Size canvasSize;
        private int[] d2CLookupX;
        private int[] d2CLookupY;
        private CanvasLayer[] list = ArrayUtil.Empty<CanvasLayer>();
        private object lockObject = new object();
        private Int32Size renderDstSize;
        private PaintDotNet.ScaleFactor scaleFactor;
        private CanvasLayer[] topList = ArrayUtil.Empty<CanvasLayer>();

        public event EventHandler<RectsEventArgs> CanvasInvalidated;

        public CanvasRenderer(Int32Size canvasSize, Int32Size renderToDstSize)
        {
            this.canvasSize = canvasSize;
            this.renderDstSize = renderToDstSize;
        }

        public void Add(CanvasLayer addMe, bool alwaysOnTop)
        {
            CanvasLayer[] layerArray = alwaysOnTop ? this.topList : this.list;
            CanvasLayer[] layerArray2 = new CanvasLayer[layerArray.Length + 1];
            for (int i = 0; i < layerArray.Length; i++)
            {
                layerArray2[i] = layerArray[i];
            }
            layerArray2[layerArray2.Length - 1] = addMe;
            if (alwaysOnTop)
            {
                this.topList = layerArray2;
            }
            else
            {
                this.list = layerArray2;
            }
            this.Invalidate();
        }

        public Point CanvasToRenderDst(Int32Point pt) => 
            this.scaleFactor.Scale(pt);

        public Rect CanvasToRenderDst(Int32Rect rect) => 
            this.scaleFactor.Scale(rect);

        public Point CanvasToRenderDst(Point pt) => 
            this.scaleFactor.Scale(pt);

        public Rect CanvasToRenderDst(Rect rect) => 
            this.scaleFactor.Scale(rect);

        private void ComputeScaleFactor()
        {
            this.scaleFactor = new PaintDotNet.ScaleFactor(this.RenderDstSize.Width, this.CanvasSize.Width);
        }

        private void CreateC2DLookupX()
        {
            if ((this.c2DLookupX == null) || (this.c2DLookupX.Length != (this.CanvasSize.Width + 1)))
            {
                this.c2DLookupX = CreateScaleXLookup(this.scaleFactor, this.CanvasSize.Width, this.RenderDstSize.Width);
            }
        }

        private void CreateC2DLookupY()
        {
            if ((this.c2DLookupY == null) || (this.c2DLookupY.Length != (this.CanvasSize.Height + 1)))
            {
                this.c2DLookupY = CreateScaleYLookup(this.scaleFactor, this.CanvasSize.Height, this.renderDstSize.Height);
            }
        }

        private void CreateD2CLookupX()
        {
            if ((this.d2CLookupX == null) || (this.d2CLookupX.Length != (this.RenderDstSize.Width + 1)))
            {
                this.d2CLookupX = CreateUnscaleXLookup(this.scaleFactor, this.CanvasSize.Width, this.RenderDstSize.Width);
            }
        }

        private void CreateD2SLookupY()
        {
            if ((this.d2CLookupY == null) || (this.d2CLookupY.Length != (this.RenderDstSize.Height + 1)))
            {
                this.d2CLookupY = CreateUnscaleYLookup(this.scaleFactor, this.CanvasSize.Height, this.RenderDstSize.Height);
            }
        }

        private static int[] CreateScaleXLookup(PaintDotNet.ScaleFactor scaleFactor, int srcWidth, int dstWidth)
        {
            int[] numArray = new int[srcWidth + 1];
            for (int i = 0; i < numArray.Length; i++)
            {
                Int32Point p = new Int32Point(i, 0);
                Int32Point point2 = Int32Point.Truncate(scaleFactor.Scale(p));
                numArray[i] = point2.X.Clamp(0, dstWidth - 1);
            }
            return numArray;
        }

        private static int[] CreateScaleYLookup(PaintDotNet.ScaleFactor scaleFactor, int srcHeight, int dstHeight)
        {
            int[] numArray = new int[srcHeight + 1];
            for (int i = 0; i < numArray.Length; i++)
            {
                Int32Point p = new Int32Point(0, i);
                Int32Point point2 = Int32Point.Truncate(scaleFactor.Scale(p));
                numArray[i] = point2.Y.Clamp(0, dstHeight - 1);
            }
            return numArray;
        }

        private static int[] CreateUnscaleXLookup(PaintDotNet.ScaleFactor scaleFactor, int srcWidth, int dstWidth)
        {
            int[] numArray = new int[dstWidth + 1];
            for (int i = 0; i < numArray.Length; i++)
            {
                Int32Point p = new Int32Point(i, 0);
                Int32Point point2 = Int32Point.Truncate(scaleFactor.Unscale(p));
                numArray[i] = point2.X.Clamp(0, srcWidth - 1);
            }
            return numArray;
        }

        private static int[] CreateUnscaleYLookup(PaintDotNet.ScaleFactor scaleFactor, int srcHeight, int dstHeight)
        {
            int[] numArray = new int[dstHeight + 1];
            for (int i = 0; i < numArray.Length; i++)
            {
                Int32Point p = new Int32Point(0, i);
                Int32Point point2 = Int32Point.Truncate(scaleFactor.Unscale(p));
                numArray[i] = point2.Y.Clamp(0, srcHeight - 1);
            }
            return numArray;
        }

        public void Invalidate()
        {
            Rect[] canvasRects = new Rect[] { CanvasLayer.MaxBounds.ToRect() };
            this.InvalidateCanvas(ref canvasRects, true);
        }

        public void InvalidateCanvas(Rect[] canvasRects)
        {
            this.InvalidateCanvas(ref canvasRects, false);
        }

        public void InvalidateCanvas(ref Rect[] canvasRects, bool takeOwnership)
        {
            this.OnCanvasInvalidated(ref canvasRects, takeOwnership);
        }

        public void InvalidateLookups()
        {
            this.c2DLookupX = null;
            this.c2DLookupY = null;
            this.d2CLookupX = null;
            this.d2CLookupY = null;
        }

        private void OnCanvasInvalidated(ref Rect[] rects, bool takeOwnership)
        {
            if (this.CanvasInvalidated != null)
            {
                RectsEventArgs e = new RectsEventArgs(ref rects, takeOwnership);
                this.CanvasInvalidated(this, e);
            }
            else if (takeOwnership)
            {
                rects = null;
            }
        }

        private void OnCanvasSizeChanged()
        {
            this.InvalidateLookups();
            if ((this.renderDstSize.Width != 0) && (this.canvasSize.Width != 0))
            {
                this.ComputeScaleFactor();
                for (int i = 0; i < this.list.Length; i++)
                {
                    this.list[i].OnCanvasSizeChanged();
                }
                for (int j = 0; j < this.topList.Length; j++)
                {
                    this.topList[j].OnCanvasSizeChanged();
                }
            }
        }

        private void OnRenderDstSizeChanged()
        {
            this.InvalidateLookups();
            if ((this.renderDstSize.Width != 0) && (this.canvasSize.Width != 0))
            {
                this.ComputeScaleFactor();
                for (int i = 0; i < this.list.Length; i++)
                {
                    this.list[i].OnRenderDstSizeChanged();
                }
                for (int j = 0; j < this.topList.Length; j++)
                {
                    this.topList[j].OnRenderDstSizeChanged();
                }
            }
        }

        public void Remove(CanvasLayer removeMe)
        {
            if ((this.list.Length == 0) && (this.topList.Length == 0))
            {
                throw new InvalidOperationException("zero items left, can't remove anything");
            }
            bool flag = false;
            if (this.list.Length > 0)
            {
                CanvasLayer[] layerArray = new CanvasLayer[this.list.Length - 1];
                bool flag2 = false;
                int index = 0;
                for (int i = 0; i < this.list.Length; i++)
                {
                    if (this.list[i] == removeMe)
                    {
                        if (flag2)
                        {
                            throw new ArgumentException("removeMe appeared multiple times in the list");
                        }
                        flag2 = true;
                    }
                    else if (index != (this.list.Length - 1))
                    {
                        layerArray[index] = this.list[i];
                        index++;
                    }
                }
                if (flag2)
                {
                    this.list = layerArray;
                    flag = true;
                }
            }
            if (this.topList.Length > 0)
            {
                CanvasLayer[] layerArray2 = new CanvasLayer[this.topList.Length - 1];
                int num3 = 0;
                bool flag3 = false;
                for (int j = 0; j < this.topList.Length; j++)
                {
                    if (this.topList[j] == removeMe)
                    {
                        if (flag || flag3)
                        {
                            throw new ArgumentException("removeMe appeared multiple times in the list");
                        }
                        flag3 = true;
                    }
                    else if (num3 != (this.topList.Length - 1))
                    {
                        layerArray2[num3] = this.topList[j];
                        num3++;
                    }
                }
                if (flag3)
                {
                    this.topList = layerArray2;
                    flag = true;
                }
            }
            if (!flag)
            {
                throw new ArgumentException("removeMe was not found", "removeMe");
            }
            this.Invalidate();
        }

        public void Render(ISurface<ColorBgra> dst, Int32Point renderDstOffset)
        {
            foreach (CanvasLayer layer in this.list)
            {
                if (layer.Visible)
                {
                    layer.Render(dst, renderDstOffset);
                }
            }
            foreach (CanvasLayer layer2 in this.topList)
            {
                if (layer2.Visible)
                {
                    layer2.Render(dst, renderDstOffset);
                }
            }
        }

        public Point RenderDstToCanvas(Int32Point pt) => 
            this.scaleFactor.Unscale(pt);

        public Rect RenderDstToCanvas(Int32Rect rect) => 
            this.scaleFactor.Unscale(rect);

        public Point RenderDstToCanvas(Point pt) => 
            this.scaleFactor.Unscale(pt);

        public Rect RenderDstToCanvas(Rect rect) => 
            this.scaleFactor.Unscale(rect);

        public int[] Canvas2DstLookupX
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.CreateC2DLookupX();
                }
                return this.c2DLookupX;
            }
        }

        public int[] Canvas2DstLookupY
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.CreateC2DLookupY();
                }
                return this.c2DLookupY;
            }
        }

        public IEnumerable<CanvasLayer> CanvasLayers =>
            this.list.Concat<CanvasLayer>(this.topList).ToArrayEx<CanvasLayer>();

        public Int32Size CanvasSize
        {
            get => 
                this.canvasSize;
            set
            {
                if (this.canvasSize != value)
                {
                    this.canvasSize = value;
                    this.OnCanvasSizeChanged();
                }
            }
        }

        public int[] Dst2CanvasLookupX
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.CreateD2CLookupX();
                }
                return this.d2CLookupX;
            }
        }

        public int[] Dst2CanvasLookupY
        {
            get
            {
                lock (this.SyncRoot)
                {
                    this.CreateD2SLookupY();
                }
                return this.d2CLookupY;
            }
        }

        int IRenderer<ColorBgra>.Height =>
            this.RenderDstSize.Height;

        int IRenderer<ColorBgra>.Width =>
            this.RenderDstSize.Width;

        public Int32Size RenderDstSize
        {
            get => 
                this.renderDstSize;
            set
            {
                if (this.renderDstSize != value)
                {
                    this.renderDstSize = value;
                    this.OnRenderDstSizeChanged();
                }
            }
        }

        public PaintDotNet.ScaleFactor ScaleFactor =>
            this.scaleFactor;

        public object SyncRoot =>
            this.lockObject;
    }
}

