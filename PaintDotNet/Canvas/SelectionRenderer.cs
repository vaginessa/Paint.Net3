namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media;

    internal class SelectionRenderer : CanvasGdipRenderer
    {
        private bool enableSelectionOutline;
        private bool enableSelectionTinting;
        private UserControl2 ownerControl;
        private bool render;
        private GeometryList selectedPath;
        private Selection selection;
        private IValue<IList<System.Windows.Point[]>> selectionPixelatedPolys;
        private IValue<IList<System.Windows.Point[]>> selectionPolys;
        private IValue<UnsafeList<Int32Rect>> selectionScans;
        private ColorBgra tintColor;
        private byte[] tintLookupB;
        private ColorBgra tintLookupColor;
        private byte[] tintLookupG;
        private byte[] tintLookupR;
        private bool useSystemTinting;
        private static byte[][] xorLookup = CreateXorLookup();

        public SelectionRenderer(CanvasRenderer ownerCanvas, Selection selection) : this(ownerCanvas, selection, null)
        {
        }

        public SelectionRenderer(CanvasRenderer ownerCanvas, Selection selection, UserControl2 ownerControl) : base(ownerCanvas)
        {
            this.tintColor = ColorBgra.FromBgra(0xff, 0, 0, 0x20);
            this.render = true;
            this.tintLookupColor = ColorBgra.Transparent;
            this.enableSelectionOutline = true;
            this.enableSelectionTinting = true;
            this.ownerControl = ownerControl;
            this.selection = selection;
            this.selection.Changing += new EventHandler(this.OnSelectionChanging);
            this.selection.Changed += new EventHandler(this.OnSelectionChanged);
        }

        private static void CreateTintLookup(ColorBgra tintColor, out byte[] lookupB, out byte[] lookupG, out byte[] lookupR)
        {
            lookupB = new byte[0x100];
            lookupG = new byte[0x100];
            lookupR = new byte[0x100];
            for (int i = 0; i <= 0xff; i++)
            {
                ColorBgra bgra2 = UserBlendOps.NormalBlendOp.ApplyStatic(ColorBgra.FromBgra((byte) i, (byte) i, (byte) i, 0xff), tintColor);
                lookupB[i] = bgra2.B;
                lookupG[i] = bgra2.G;
                lookupR[i] = bgra2.R;
            }
        }

        private static byte[][] CreateXorLookup()
        {
            byte[][] bufferArray = new byte[][] { new byte[0x100], new byte[0x100] };
            for (int i = 0; i <= 0xff; i++)
            {
                bufferArray[0][i] = (byte) (0xff - ((byte) i));
            }
            for (int j = 0; j <= 0xff; j++)
            {
                bufferArray[1][j] = (byte) (0xff - ByteUtil.FastScale((byte) j, (byte) j));
            }
            return bufferArray;
        }

        private static void DrawLineXiaolin(double x1, double y1, double x2, double y2, Int32Rect bounds, Action<int, int, byte> setPixelFn)
        {
            DrawLineXiaolin(x1, y1, x2, y2, bounds, setPixelFn, 0, 0);
        }

        private static void DrawLineXiaolin(double x1, double y1, double x2, double y2, Int32Rect bounds, Action<int, int, byte> setPixelFn, int sptx, int spty)
        {
            Action<int, int, byte> action = null;
            if (((x1 < 1.0) || (y1 < 1.0)) || ((x2 < 1.0) || (y2 < 1.0)))
            {
                int num = 1;
                if (x1 < 1.0)
                {
                    num += (int) Math.Ceiling(-x1);
                }
                if (x2 < 1.0)
                {
                    num += (int) Math.Ceiling(-x2);
                }
                int num2 = 1;
                if (y1 < 1.0)
                {
                    num2 += (int) Math.Ceiling(-y1);
                }
                if (y2 < 1.0)
                {
                    num2 += (int) Math.Ceiling(-y2);
                }
                DrawLineXiaolin(x1 + num, y1 + num2, x2 + num, y2 + num2, new Int32Rect(bounds.X + num, bounds.Y + num2, bounds.Width, bounds.Height), setPixelFn, sptx - num, spty - num2);
            }
            else
            {
                double num3 = x2 - x1;
                double num4 = y2 - y1;
                if ((num3 != 0.0) || (num4 != 0.0))
                {
                    if (Math.Abs(num3) >= Math.Abs(num4))
                    {
                        if (x2 < x1)
                        {
                            double num5 = x2;
                            x2 = x1;
                            x1 = num5;
                            double num6 = y2;
                            y2 = y1;
                            y1 = num6;
                        }
                        double num7 = num4 / num3;
                        int num8 = (int) (x1 + 0.5);
                        double num9 = y1 + (num7 * (num8 - x1));
                        double num10 = rfpart(x1 + 0.5);
                        int num11 = num8;
                        int num12 = (int) num9;
                        setPixelFn(num11 + sptx, num12 + spty, (byte) ((255.0 * rfpart(num9)) * num10));
                        setPixelFn(num11 + sptx, (num12 + 1) + spty, (byte) ((255.0 * fpart(num9)) * num10));
                        double num13 = num9 + num7;
                        int num14 = (int) (x2 + 0.5);
                        double num15 = y2 + (num7 * (num14 - x2));
                        double num16 = fpart(x2 + 0.5);
                        int num17 = num14;
                        int num18 = (int) num15;
                        setPixelFn(num17 + sptx, num18 + spty, (byte) ((255.0 * rfpart(num15)) * num16));
                        setPixelFn(num17 + sptx, (num18 + 1) + spty, (byte) ((255.0 * fpart(num15)) * num16));
                        int num19 = num11 + 1;
                        int num20 = bounds.X - num19;
                        if (num20 > 0)
                        {
                            num13 += num7 * num20;
                            num19 = bounds.X;
                        }
                        num17 = Math.Min(num17, bounds.Right());
                        while (num19 < num17)
                        {
                            setPixelFn(num19 + sptx, ((int) num13) + spty, (byte) (255.0 * rfpart(num13)));
                            setPixelFn(num19 + sptx, (((int) num13) + 1) + spty, (byte) (255.0 * fpart(num13)));
                            num13 += num7;
                            num19++;
                        }
                    }
                    else
                    {
                        if (action == null)
                        {
                            action = (x, y, b) => setPixelFn(y, x, b);
                        }
                        DrawLineXiaolin(y1, x1, y2, x2, new Int32Rect(bounds.Y, bounds.X, bounds.Height, bounds.Width), action, spty, sptx);
                    }
                }
            }
        }

        private unsafe void DrawSelectionOutline(ISurface<ColorBgra> dst, Int32Point offset, IList<System.Windows.Point[]> polys, double scale)
        {
            int srcZ = (offset.X + offset.Y) & 1;
            int dstWidth = dst.Width;
            int dstHeight = dst.Height;
            ColorBgra* dstScan0 = (ColorBgra*) dst.Scan0.ToPointer();
            int dstStride = dst.Stride;
            Int32Rect bounds = new Int32Rect(0, 0, dstWidth, dstHeight);
            Rect rect2 = new Rect(0.0, 0.0, (double) dstWidth, (double) dstHeight);
            Action<int, int, byte> setPixelFn = delegate (int x, int y, byte alpha) {
                if (((x >= 0) && (y >= 0)) && ((x < dstWidth) && (y < dstHeight)))
                {
                    ColorBgra* bgraPtr = (dstScan0 + (y * dstStride)) + x;
                    if (bgraPtr->A < alpha)
                    {
                        bgraPtr->A = 0;
                    }
                    else
                    {
                        bgraPtr->A = (byte) (bgraPtr->A - alpha);
                    }
                }
            };
            Action<int, int, byte> action2 = delegate (int x, int y, byte alpha) {
                if (((x >= 0) && (y >= 0)) && ((x < dstWidth) && (y < dstHeight)))
                {
                    ColorBgra* bgraPtr = (dstScan0 + (y * dstStride)) + x;
                    int num = (x + y) & 1;
                    int index = srcZ ^ num;
                    byte[] buffer = xorLookup[index];
                    ColorBgra from = bgraPtr[0];
                    byte frac = (byte) (0xff - from.A);
                    ColorBgra to = from;
                    to.B = buffer[to.B];
                    to.G = buffer[to.G];
                    to.R = buffer[to.R];
                    to.A = 0xff;
                    ColorBgra bgra3 = ColorBgra.Lerp(from, to, frac);
                    bgraPtr->Bgra = bgra3.Bgra;
                }
            };
            for (int i = 0; i < polys.Count; i++)
            {
                System.Windows.Point[] pointArray = polys[i];
                for (int k = 0; k < pointArray.Length; k++)
                {
                    int num3 = k + 1;
                    if ((k + 1) == pointArray.Length)
                    {
                        num3 = 0;
                    }
                    System.Windows.Point point = new System.Windows.Point((pointArray[k].X * scale) - offset.X, (pointArray[k].Y * scale) - offset.Y);
                    System.Windows.Point point2 = new System.Windows.Point((pointArray[num3].X * scale) - offset.X, (pointArray[num3].Y * scale) - offset.Y);
                    double num4 = Math.Min(point.X, point2.X);
                    double num5 = Math.Min(point.Y, point2.Y);
                    double num6 = 1.0 + Math.Max(point.X, point2.X);
                    double num7 = 1.0 + Math.Max(point.Y, point2.Y);
                    Rect rect = new Rect(num4, num5, num6 - num4, num7 - num5);
                    if (rect2.IntersectsWith(rect))
                    {
                        DrawLineXiaolin(point.X, point.Y, point2.X, point2.Y, bounds, setPixelFn);
                    }
                }
            }
            for (int j = 0; j < polys.Count; j++)
            {
                System.Windows.Point[] pointArray2 = polys[j];
                for (int m = 0; m < pointArray2.Length; m++)
                {
                    int num10 = m + 1;
                    if ((m + 1) == pointArray2.Length)
                    {
                        num10 = 0;
                    }
                    System.Windows.Point point3 = new System.Windows.Point((pointArray2[m].X * scale) - offset.X, (pointArray2[m].Y * scale) - offset.Y);
                    System.Windows.Point point4 = new System.Windows.Point((pointArray2[num10].X * scale) - offset.X, (pointArray2[num10].Y * scale) - offset.Y);
                    double num11 = Math.Min(point3.X, point4.X);
                    double num12 = Math.Min(point3.Y, point4.Y);
                    double num13 = 1.0 + Math.Max(point3.X, point4.X);
                    double num14 = 1.0 + Math.Max(point3.Y, point4.Y);
                    Rect rect4 = new Rect(num11, num12, num13 - num11, num14 - num12);
                    if (rect2.IntersectsWith(rect4))
                    {
                        DrawLineXiaolin(point3.X, point3.Y, point4.X, point4.Y, bounds, action2);
                    }
                }
            }
        }

        private void DrawSelectionTinting(ISurface<ColorBgra> dst, Int32Point offset, UnsafeList<Int32Rect> rects, Rect selectedPathBounds)
        {
            Int32Rect rect = new Int32Rect(offset.X, offset.Y, dst.Width, dst.Height);
            Int32Rect rect2 = new Int32Rect(0, 0, base.RenderDstSize.Width, base.RenderDstSize.Height);
            Int32Rect rect3 = rect.IntersectCopy(rect2);
            if ((rect3.Width != 0) && (rect3.Height != 0))
            {
                this.DrawSelectionTintingClipped(dst, offset, rects, selectedPathBounds);
            }
        }

        private unsafe void DrawSelectionTintingClipped(ISurface<ColorBgra> dst, Int32Point offset, UnsafeList<Int32Rect> rects, Rect selectedPathBounds)
        {
            Int32Rect[] unsafeArray = rects.UnsafeArray;
            if (rects.Count == 0)
            {
                return;
            }
            int num = Math.Max(0, offset.Y);
            int num2 = Math.Min(base.RenderDstSize.Height, offset.Y + dst.Height);
            ColorBgra effectiveTintColor = this.EffectiveTintColor;
            if ((effectiveTintColor != this.tintLookupColor) || (this.tintLookupB == null))
            {
                CreateTintLookup(effectiveTintColor, out this.tintLookupB, out this.tintLookupG, out this.tintLookupR);
                this.tintLookupColor = effectiveTintColor;
            }
            int index = 0;
            int num4 = 0;
            int num5 = rects.Count - 1;
            while (num4 <= num5)
            {
                int num6 = num4 + ((num5 - num4) / 2);
                Int32Rect rect = unsafeArray[num6];
                int y = rect.Y;
                int num8 = rect.Y + rect.Height;
                if (y > num2)
                {
                    num5 = num6 - 1;
                }
                else
                {
                    if (num8 < num)
                    {
                        num4 = num6 + 1;
                        continue;
                    }
                    index = num6;
                    break;
                }
            }
        Label_00EA:
            index--;
            if (index == -1)
            {
                index = 0;
            }
            else
            {
                Int32Rect rect2 = unsafeArray[index];
                int num1 = rect2.Y;
                int num9 = rect2.Y + rect2.Height;
                if (num9 >= num)
                {
                    goto Label_00EA;
                }
                index++;
            }
            for (int i = index; (i < rects.Count) && (unsafeArray[i].Y < num2); i++)
            {
                Int32Rect rect3 = rects[i];
                int num11 = Math.Max(num, rect3.Y);
                int num12 = Math.Min(num2, rect3.Y + rect3.Height);
                Math.Max(0, rect3.X);
                Math.Min(base.RenderDstSize.Width, rect3.X + rect3.Width);
                int num13 = rect3.X - offset.X;
                int width = num13 + rect3.Width;
                if (num13 < 0)
                {
                    num13 = 0;
                }
                if (width > dst.Width)
                {
                    width = dst.Width;
                }
                if (num13 < width)
                {
                    for (int j = num11; j < num12; j++)
                    {
                        int row = j - offset.Y;
                        ColorBgra* bgraPtr = (ColorBgra*) (((void*) dst.GetRowPointer<ColorBgra>(row)) + (num13 * sizeof(ColorBgra)));
                        ColorBgra* bgraPtr2 = (bgraPtr + width) - num13;
                        while (bgraPtr < bgraPtr2)
                        {
                            ColorBgra bgra2 = bgraPtr[0];
                            bgraPtr[0] = ColorBgra.FromBgra(this.tintLookupB[bgra2.B], this.tintLookupG[bgra2.G], this.tintLookupR[bgra2.R], 0xff);
                            bgraPtr++;
                        }
                    }
                }
            }
        }

        private static double fpart(double x) => 
            (x - ((int) x));

        public override void OnRenderDstSizeChanged()
        {
            this.UpdateRenderData();
            base.OnRenderDstSizeChanged();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            GeometryList list = this.selection.CreateGeometryList();
            GeometryList selectedPath = this.selectedPath;
            this.selectedPath = list;
            this.UpdateRenderData();
            double ratio = base.OwnerCanvas.ScaleFactor.Ratio;
            double num2 = 1.0 / ratio;
            new Matrix().Scale(ratio, ratio);
            Rect bounds = this.selectedPath.Bounds;
            if (selectedPath != null)
            {
                bounds.Union(selectedPath.Bounds);
            }
            double width = Math.Max(1.0, num2);
            bounds.Inflate(width, width);
            base.InvalidateCanvas(bounds);
            this.render = true;
        }

        private void OnSelectionChanging(object sender, EventArgs e)
        {
            this.render = false;
            this.selectionScans = null;
            this.selectionPolys = null;
            this.selectionPixelatedPolys = null;
        }

        protected override void OnVisibleChanged()
        {
            if (this.selection != null)
            {
                Int32Rect bounds = this.selection.GetBounds();
                base.InvalidateCanvas(bounds);
            }
        }

        public override void RenderToGraphics(RenderArgs ra, Int32Point offset)
        {
            if ((this.selectedPath == null) || this.selectedPath.IsEmpty)
            {
                this.render = false;
            }
            else
            {
                double ratio = base.OwnerCanvas.ScaleFactor.Ratio;
                if (this.EnableSelectionTinting)
                {
                    Rect bounds = this.selectedPath.Bounds;
                    Rect selectedPathBounds = new Rect(bounds.X * ratio, bounds.Y * ratio, bounds.Width * ratio, bounds.Height * ratio);
                    UnsafeList<Int32Rect> rects = this.selectionScans.Value;
                    this.DrawSelectionTinting(ra.ISurface, offset, rects, selectedPathBounds);
                }
                if (this.EnableSelectionOutline)
                {
                    IList<System.Windows.Point[]> polys = (this.selectionPixelatedPolys ?? this.selectionPolys).Value;
                    this.DrawSelectionOutline(ra.ISurface, offset, polys, ratio);
                }
            }
        }

        private static double rfpart(double x) => 
            (1.0 - fpart(x));

        public override bool ShouldRender(Int32Rect renderBounds)
        {
            if ((this.selectedPath == null) || !this.render)
            {
                return false;
            }
            Rect rect = base.OwnerCanvas.RenderDstToCanvas(renderBounds);
            bool flag = false;
            if (this.EnableSelectionTinting && this.selectedPath.Bounds.IntersectsWith(rect))
            {
                flag = true;
            }
            if (this.EnableSelectionOutline)
            {
                double ratio = base.OwnerCanvas.ScaleFactor.Ratio;
                double num2 = 1.0 / ratio;
                if (this.selectedPath.Bounds.InflateCopy((num2 * 2.0), (num2 * 2.0)).IntersectsWith(rect))
                {
                    flag = true;
                }
            }
            return flag;
        }

        private void UpdateRenderData()
        {
            this.selectionPolys = null;
            this.selectionScans = null;
            if ((this.selectedPath != null) && this.selectedPath.IsEmpty)
            {
                this.selectionPixelatedPolys = null;
            }
            else if (this.selectedPath != null)
            {
                Func<UnsafeList<Int32Rect>> valueFn = null;
                Func<UnsafeList<Int32Rect>> func2 = null;
                Func<IList<System.Windows.Point[]>> func3 = null;
                GeometryList selectedPathP = this.selectedPath;
                double ratio = base.OwnerCanvas.ScaleFactor.Ratio;
                Matrix matrix = new Matrix();
                matrix.Scale(ratio, ratio);
                this.selectionPolys = Future.Create<IList<System.Windows.Point[]>>(() => selectedPathP.GetPolygonList());
                if (ratio == 1.0)
                {
                    if (valueFn == null)
                    {
                        valueFn = () => selectedPathP.GetInteriorScansUnsafeList();
                    }
                    this.selectionScans = Future.Create<UnsafeList<Int32Rect>>(valueFn);
                    this.selectionPixelatedPolys = null;
                }
                else if (ratio < 1.0)
                {
                    if (func2 == null)
                    {
                        func2 = () => selectedPathP.GetInteriorScansUnsafeList(matrix);
                    }
                    this.selectionScans = Future.Create<UnsafeList<Int32Rect>>(func2);
                    this.selectionPixelatedPolys = null;
                }
                else
                {
                    int[] c2dX = base.OwnerCanvas.Canvas2DstLookupX;
                    int[] c2dY = base.OwnerCanvas.Canvas2DstLookupY;
                    int cWidth = base.CanvasSize.Width;
                    int cHeight = base.CanvasSize.Height;
                    if (this.selectionPixelatedPolys == null)
                    {
                        if (func3 == null)
                        {
                            func3 = delegate {
                                using (GeometryList list2 = GeometryList.FromNonOverlappingScans(selectedPathP.GetInteriorScansUnsafeList()))
                                {
                                    return list2.GetPolygonList();
                                }
                            };
                        }
                        this.selectionPixelatedPolys = Future.Create<IList<System.Windows.Point[]>>(FutureOptions.Prefetch, func3);
                    }
                    this.selectionScans = Future.Create<UnsafeList<Int32Rect>>(FutureOptions.Prefetch, delegate {
                        Int32Rect[] rectArray;
                        int num;
                        UnsafeList<Int32Rect> interiorScansUnsafeList = selectedPathP.GetInteriorScansUnsafeList();
                        interiorScansUnsafeList.GetArrayReadOnly(out rectArray, out num);
                        for (int j = 0; j < num; j++)
                        {
                            Int32Rect rect = rectArray[j];
                            int index = rect.X.Clamp(0, cWidth);
                            int num4 = rect.Y.Clamp(0, cHeight);
                            int num5 = (rect.X + rect.Width).Clamp(0, cWidth);
                            int num6 = (rect.Y + rect.Height).Clamp(0, cHeight);
                            int left = c2dX[index];
                            int top = c2dY[num4];
                            int right = c2dX[num5];
                            int bottom = c2dY[num6];
                            rectArray[j] = Int32RectUtil.FromEdges(left, top, right, bottom);
                        }
                        return interiorScansUnsafeList;
                    });
                }
            }
        }

        public override bool ClipsToCanvas =>
            false;

        private ColorBgra EffectiveTintColor
        {
            get
            {
                if (this.useSystemTinting)
                {
                    return ColorBgra.FromColor(Color.FromArgb(0x38, SystemColors.Highlight));
                }
                return this.tintColor;
            }
        }

        public bool EnableSelectionOutline
        {
            get => 
                this.enableSelectionOutline;
            set
            {
                if (this.enableSelectionOutline != value)
                {
                    this.enableSelectionOutline = value;
                    base.Invalidate();
                }
            }
        }

        public bool EnableSelectionTinting
        {
            get => 
                this.enableSelectionTinting;
            set
            {
                if (this.enableSelectionTinting != value)
                {
                    this.enableSelectionTinting = value;
                    base.Invalidate();
                }
            }
        }

        public ColorBgra TintColor
        {
            get => 
                this.tintColor;
            set
            {
                if (value != this.tintColor)
                {
                    this.tintColor = value;
                    base.Invalidate();
                }
            }
        }

        public bool UseSystemTinting
        {
            get => 
                this.useSystemTinting;
            set
            {
                if (this.useSystemTinting != value)
                {
                    this.useSystemTinting = value;
                    base.Invalidate();
                }
            }
        }
    }
}

