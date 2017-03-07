namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class CanvasDocumentRenderer : CanvasLayer, IDispatcherObject
    {
        private int cacheStandbyCount;
        private readonly IDispatcher dispatcher;
        private IRenderer<ColorBgra> docRenderer;
        private CachingRendererBgra docRendererCache;
        private PaintDotNet.Document document;
        private bool highQualityZoomIn;
        private bool highQualityZoomOut;
        private IRenderer<ColorBgra> ourRenderer;
        private const int tileHeightLog2 = 6;
        private const int tileWidthLog2 = 8;

        public CanvasDocumentRenderer(IDispatcher dispatcher, CanvasRenderer ownerCanvas, PaintDotNet.Document document) : base(ownerCanvas)
        {
            this.highQualityZoomIn = true;
            this.highQualityZoomOut = true;
            this.dispatcher = dispatcher;
            this.Document = document;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Document = null;
                if (this.docRendererCache != null)
                {
                    this.docRendererCache.Dispose();
                    this.docRendererCache = null;
                }
            }
            base.Dispose(disposing);
        }

        private void Document_Invalidated(object sender, InvalidateEventArgs e)
        {
            Int32Rect rect = e.InvalidRect.ToInt32Rect();
            base.InvalidateCanvas(rect);
            CachingRendererBgra docRendererCache = this.docRendererCache;
            if (docRendererCache != null)
            {
                docRendererCache.Invalidate(rect);
            }
            this.ourRenderer = null;
        }

        private static bool IsIntegralZoomIn(ScaleFactor sf)
        {
            double ratio = sf.Ratio;
            if (ratio < 1.0)
            {
                return false;
            }
            return (Math.Floor(ratio) == ratio);
        }

        public override void OnCanvasSizeChanged()
        {
            base.OwnerCanvas.InvalidateLookups();
            this.ourRenderer = null;
            base.OnCanvasSizeChanged();
        }

        protected override void OnRender(ISurface<ColorBgra> dst, Int32Point renderOffset)
        {
            if (this.ourRenderer == null)
            {
                IRenderer<ColorBgra> renderer;
                CachingRendererBgra docRendererCache = this.docRendererCache;
                if (this.document.Size.ToInt32Size() == base.RenderDstSize)
                {
                    renderer = new NoResizeCheckersRenderer(docRendererCache);
                }
                else if ((this.document.Width < base.RenderDstSize.Width) && (this.document.Height < base.RenderDstSize.Height))
                {
                    if (this.highQualityZoomIn && !IsIntegralZoomIn(base.OwnerCanvas.ScaleFactor))
                    {
                        renderer = new ResizeRotatedGridMultisamplingCheckersRenderer(docRendererCache, base.RenderDstSize.Width, base.RenderDstSize.Height);
                    }
                    else
                    {
                        renderer = new NearestNeighborCheckersRenderer(docRendererCache, base.OwnerCanvas);
                    }
                }
                else if ((this.document.Width > base.RenderDstSize.Width) && (this.document.Height > base.RenderDstSize.Height))
                {
                    if (this.highQualityZoomOut)
                    {
                        renderer = new ResizeRotatedGridMultisamplingCheckersRenderer(docRendererCache, base.RenderDstSize.Width, base.RenderDstSize.Height);
                    }
                    else
                    {
                        renderer = new NearestNeighborCheckersRenderer(docRendererCache, base.OwnerCanvas);
                    }
                }
                else
                {
                    IRenderer<ColorBgra> sourceRHS = docRendererCache.ResizeNearestNeighbor(base.RenderDstSize.Width, base.RenderDstSize.Height);
                    IRenderer<ColorBgra> sourceLHS = RendererBgra.Checkers(base.RenderDstSize.Width, base.RenderDstSize.Height);
                    UserBlendOps.NormalBlendOp @static = UserBlendOps.NormalBlendOp.Static;
                    renderer = sourceLHS.DrawBlend(@static, sourceRHS);
                }
                this.ourRenderer = renderer;
            }
            this.ourRenderer.Render(dst, renderOffset);
        }

        public override void OnRenderDstSizeChanged()
        {
            base.OwnerCanvas.InvalidateLookups();
            this.ourRenderer = null;
            base.OnRenderDstSizeChanged();
        }

        protected override void OnVisibleChanged()
        {
            base.Invalidate();
        }

        public void PopCacheStandby()
        {
            if (this.docRendererCache != null)
            {
                this.docRendererCache.PopStandby();
            }
            else
            {
                this.cacheStandbyCount--;
            }
        }

        public void PrefetchSync(Int32Rect bounds)
        {
            if (this.docRendererCache != null)
            {
                bounds.IntersectCopy(this.docRendererCache.Bounds<ColorBgra>());
                this.docRendererCache.PrefetchSync(bounds);
            }
        }

        public void PushCacheStandby()
        {
            if (this.docRendererCache != null)
            {
                this.docRendererCache.PushStandby();
            }
            else
            {
                this.cacheStandbyCount++;
            }
        }

        public IDispatcher Dispatcher =>
            this.dispatcher;

        public PaintDotNet.Document Document
        {
            get => 
                this.document;
            set
            {
                this.VerifyAccess();
                if (this.document != null)
                {
                    this.document.Invalidated -= new InvalidateEventHandler(this.Document_Invalidated);
                }
                if (this.docRenderer != null)
                {
                    this.docRenderer = null;
                }
                int standbyCounter = 0;
                if (this.docRendererCache != null)
                {
                    standbyCounter = this.docRendererCache.StandbyCounter;
                    this.docRendererCache.Dispose();
                    this.docRendererCache = null;
                }
                this.ourRenderer = null;
                this.document = value;
                if (this.document != null)
                {
                    this.document.Invalidated += new InvalidateEventHandler(this.Document_Invalidated);
                    this.docRenderer = this.document.CreateRenderer();
                    this.docRendererCache = new CachingRendererBgra(this.dispatcher, this.docRenderer, 8, 6);
                    int num2 = standbyCounter + this.cacheStandbyCount;
                    this.cacheStandbyCount = 0;
                    while (num2 > 0)
                    {
                        this.docRendererCache.PushStandby();
                        num2--;
                    }
                    while (num2 < 0)
                    {
                        this.docRendererCache.PopStandby();
                        num2++;
                    }
                }
            }
        }

        public bool HighQualityZoomIn
        {
            get => 
                this.highQualityZoomIn;
            set
            {
                if (value != this.highQualityZoomIn)
                {
                    this.highQualityZoomIn = value;
                    this.ourRenderer = null;
                    base.Invalidate();
                }
            }
        }

        public bool HighQualityZoomOut
        {
            get => 
                this.highQualityZoomOut;
            set
            {
                if (value != this.highQualityZoomOut)
                {
                    this.highQualityZoomOut = value;
                    this.ourRenderer = null;
                    base.Invalidate();
                }
            }
        }

        private sealed class NearestNeighborCheckersRenderer : RendererBgraBase
        {
            private readonly CanvasRenderer dstCanvas;
            private readonly IRenderer<ColorBgra> source;
            private readonly int sourceHeight;
            private readonly int sourceWidth;

            public NearestNeighborCheckersRenderer(IRenderer<ColorBgra> source, CanvasRenderer dstCanvas) : base(dstCanvas.RenderDstSize.Width, dstCanvas.RenderDstSize.Height)
            {
                this.source = source;
                this.sourceWidth = this.source.Width;
                this.sourceHeight = this.source.Height;
                this.dstCanvas = dstCanvas;
            }

            protected override unsafe void OnRender(ISurface<ColorBgra> dstCropped, Int32Point renderOffset)
            {
                int width = dstCropped.Width;
                int height = dstCropped.Height;
                int x = renderOffset.X;
                int y = renderOffset.Y;
                int[] numArray = this.dstCanvas.Dst2CanvasLookupX;
                int[] numArray2 = this.dstCanvas.Dst2CanvasLookupY;
                int num5 = numArray[x];
                int num6 = numArray2[y];
                int num7 = numArray[(width + x) - 1] + 1;
                int num8 = numArray2[(height + y) - 1] + 1;
                int num9 = num7 - num5;
                int num10 = num8 - num6;
                if (this.source is CachingRendererBgra)
                {
                    CachingRendererBgra source = (CachingRendererBgra) this.source;
                    for (int i = 0; i < height; i++)
                    {
                        int index = i + y;
                        int num13 = numArray2[index];
                        ColorBgra* rowPointer = (ColorBgra*) dstCropped.GetRowPointer<ColorBgra>(i);
                        ColorBgraTileSlice colorBgraMidTile = new ColorBgraTileSlice(-2147483648, -2147483648, null);
                        for (int j = 0; j < width; j++)
                        {
                            int num15 = j + x;
                            int left = numArray[num15];
                            if (left >= colorBgraMidTile.Right)
                            {
                                colorBgraMidTile = source.GetColorBgraMidTile(num13, left);
                            }
                            ColorBgra* bgraPtr2 = (colorBgraMidTile.TilePointer + left) - colorBgraMidTile.Left;
                            int b = bgraPtr2->B;
                            int g = bgraPtr2->G;
                            int r = bgraPtr2->R;
                            int a = bgraPtr2->A;
                            int num21 = (((num15 ^ index) & 8) * 8) + 0xbf;
                            a += a >> 7;
                            int num22 = num21 * (0x100 - a);
                            r = ((r * a) + num22) >> 8;
                            g = ((g * a) + num22) >> 8;
                            b = ((b * a) + num22) >> 8;
                            rowPointer->Bgra = (uint) (((b | (g << 8)) | (r << 0x10)) | -16777216);
                            rowPointer++;
                        }
                    }
                }
                else
                {
                    ISurface<ColorBgra> surface;
                    if ((this.sourceWidth < base.Width) && (this.sourceHeight < base.Height))
                    {
                        surface = dstCropped.CreateWindow<ColorBgra>(new Int32Rect(0, 0, num9, num10));
                    }
                    else
                    {
                        surface = base.SurfaceAllocator.Allocate(num9, num10);
                    }
                    this.source.Render(surface, new Int32Point(num5, num6));
                    for (int k = height - 1; k >= 0; k--)
                    {
                        int num24 = k + y;
                        int num25 = numArray2[num24];
                        int row = num25 - num6;
                        ColorBgra* bgraPtr3 = (ColorBgra*) ((dstCropped.GetRowPointer<ColorBgra>(k).ToPointer() + (width * sizeof(ColorBgra))) - sizeof(ColorBgra));
                        ColorBgra* bgraPtr4 = (ColorBgra*) surface.GetRowPointer<ColorBgra>(row).ToPointer();
                        for (int m = width - 1; m >= 0; m--)
                        {
                            int num28 = m + x;
                            int num29 = numArray[num28];
                            int num30 = num29 - num5;
                            ColorBgra bgra2 = bgraPtr4[num30];
                            int num31 = bgra2.B;
                            int num32 = bgra2.G;
                            int num33 = bgra2.R;
                            int num34 = bgra2.A;
                            int num35 = (((num28 ^ num24) & 8) * 8) + 0xbf;
                            num34 += num34 >> 7;
                            int num36 = num35 * (0x100 - num34);
                            num33 = ((num33 * num34) + num36) >> 8;
                            num32 = ((num32 * num34) + num36) >> 8;
                            num31 = ((num31 * num34) + num36) >> 8;
                            bgraPtr3->Bgra = (uint) (((num31 | (num32 << 8)) | (num33 << 0x10)) | -16777216);
                            bgraPtr3--;
                        }
                    }
                    surface.Dispose();
                }
            }
        }

        private sealed class NoResizeCheckersRenderer : RendererBgraBase
        {
            private IRenderer<ColorBgra> source;

            public NoResizeCheckersRenderer(IRenderer<ColorBgra> source) : base(source.Width, source.Height)
            {
                this.source = source;
            }

            protected override unsafe void OnRender(ISurface<ColorBgra> dstCropped, Int32Point renderOffset)
            {
                int height = dstCropped.Height;
                int width = dstCropped.Width;
                int x = renderOffset.X;
                int y = renderOffset.Y;
                this.source.Render(dstCropped, renderOffset);
                for (int i = 0; i < height; i++)
                {
                    ColorBgra* rowPointer = (ColorBgra*) dstCropped.GetRowPointer<ColorBgra>(i);
                    int num6 = i + y;
                    for (int j = 0; j < width; j++)
                    {
                        int b = rowPointer->B;
                        int g = rowPointer->G;
                        int r = rowPointer->R;
                        int a = rowPointer->A;
                        int num12 = j + x;
                        int num13 = (((num12 ^ num6) & 8) << 3) + 0xbf;
                        a += a >> 7;
                        int num14 = num13 * (0x100 - a);
                        r = ((r * a) + num14) >> 8;
                        g = ((g * a) + num14) >> 8;
                        b = ((b * a) + num14) >> 8;
                        rowPointer->Bgra = (uint) (((b | (g << 8)) | (r << 0x10)) | -16777216);
                        rowPointer++;
                    }
                }
            }
        }

        private sealed class ResizeRotatedGridMultisamplingCheckersRenderer : RendererBgraBase
        {
            private const int maxTileSize = 0x4000;
            private readonly IRenderer<ColorBgra> source;
            private readonly int sourceHeight;
            private readonly int sourceWidth;

            public ResizeRotatedGridMultisamplingCheckersRenderer(IRenderer<ColorBgra> source, int newWidth, int newHeight) : base(newWidth, newHeight)
            {
                this.source = source;
                this.sourceWidth = this.source.Width;
                this.sourceHeight = this.source.Height;
            }

            protected override void OnRender(ISurface<ColorBgra> dstCropped, Int32Point renderOffset)
            {
                try
                {
                    this.OnRenderImpl(dstCropped, renderOffset);
                }
                catch (AccessViolationException exception)
                {
                    throw new AccessViolationException($"AV detected. this.Width={base.Width}, this.Height={base.Height}, this.sourceWidth={this.sourceWidth}, this.sourceHeight={this.sourceHeight}, dstCropped.Width={dstCropped.Width}, dstCropped.Height={dstCropped.Height}, renderOffset.X={renderOffset.X}, renderOffset.Y={renderOffset.Y}, this.IsDisposed={base.IsDisposed}, dstCropped.IsDisposed={dstCropped.IsDisposed}", exception);
                }
            }

            private unsafe void OnRenderImpl(ISurface<ColorBgra> dstCropped, Int32Point renderOffset)
            {
                int width = dstCropped.Width;
                int height = dstCropped.Height;
                int x = renderOffset.X;
                int y = renderOffset.Y;
                long num5 = ((x * 0x1000L) * this.sourceWidth) / ((long) base.Width);
                long num6 = ((y * 0x1000L) * this.sourceHeight) / ((long) base.Height);
                long num7 = (((x + width) * 0x1000L) * this.sourceWidth) / ((long) base.Width);
                long num8 = (((y + height) * 0x1000L) * this.sourceHeight) / ((long) base.Height);
                int num9 = (int) num5;
                int num10 = (int) num6;
                int num11 = (int) num7;
                int num12 = (int) num8;
                int num13 = (num11 - num9) / width;
                int num14 = (num12 - num10) / height;
                int num15 = num9 >> 12;
                int num16 = num10 >> 12;
                int num17 = (((num9 + (num13 * width)) + (num13 >> 1)) + (num13 >> 2)) >> 12;
                int num18 = (((num10 + (num13 * height)) + (num14 >> 1)) + (num14 >> 2)) >> 12;
                int num19 = (1 + num17) - num15;
                int num20 = (1 + num18) - num16;
                CachingRendererBgra source = this.source as CachingRendererBgra;
                if (source == null)
                {
                    if ((((num19 * num20) > 0x4000) && (width > 1)) && (height > 1))
                    {
                        MutableSurfaceWindow<ColorBgra> dst = new MutableSurfaceWindow<ColorBgra>(dstCropped, Int32Rect.Empty);
                        Int32Rect rect = new Int32Rect(0, 0, width / 2, height / 2);
                        dst.WindowBounds = rect;
                        this.Render(dst, new Int32Point(x + rect.X, y + rect.Y));
                        Int32Rect rect2 = new Int32Rect(rect.Right(), 0, width - rect.Width, height / 2);
                        dst.WindowBounds = rect2;
                        this.Render(dst, new Int32Point(x + rect2.X, y + rect2.Y));
                        Int32Rect rect3 = new Int32Rect(0, rect.Bottom(), width / 2, dstCropped.Height - rect.Height);
                        dst.WindowBounds = rect3;
                        this.Render(dst, new Int32Point(x + rect3.X, y + rect3.Y));
                        int introduced137 = rect2.Bottom();
                        Int32Rect rect4 = new Int32Rect(rect2.Left(), introduced137, rect2.Width, rect3.Height);
                        dst.WindowBounds = rect4;
                        this.Render(dst, new Int32Point(x + rect4.X, y + rect4.Y));
                    }
                    else
                    {
                        using (ISurface<ColorBgra> surface = base.SurfaceAllocator.Allocate(num19, num20))
                        {
                            this.source.Render(surface, new Int32Point(num15, num16));
                            int row = 0;
                            for (int i = num10; (row < height) && (i < num12); i += num14)
                            {
                                int num65 = (i + (num14 >> 1)) >> 12;
                                int num66 = i >> 12;
                                int num67 = ((i + (num14 >> 1)) + (num14 >> 2)) >> 12;
                                int num68 = (i + (num14 >> 2)) >> 12;
                                int num69 = num65 - num16;
                                int num70 = num66 - num16;
                                int num71 = num67 - num16;
                                int num72 = num68 - num16;
                                ColorBgra* rowPointer = (ColorBgra*) surface.GetRowPointer<ColorBgra>(num69);
                                ColorBgra* bgraPtr7 = (ColorBgra*) surface.GetRowPointer<ColorBgra>(num70);
                                ColorBgra* bgraPtr8 = (ColorBgra*) surface.GetRowPointer<ColorBgra>(num71);
                                ColorBgra* bgraPtr9 = (ColorBgra*) surface.GetRowPointer<ColorBgra>(num72);
                                ColorBgra* bgraPtr10 = (ColorBgra*) dstCropped.GetRowPointer<ColorBgra>(row);
                                int num73 = row + y;
                                int num74 = x;
                                int num75 = num74 + width;
                                for (int j = num9; (num74 < num75) && (j < num11); j += num13)
                                {
                                    int num77 = j >> 12;
                                    int num78 = (j + (num13 >> 2)) >> 12;
                                    int num79 = (j + (num13 >> 1)) >> 12;
                                    int num80 = ((j + (num13 >> 1)) + (num13 >> 2)) >> 12;
                                    int num81 = num77 - num15;
                                    int num82 = num78 - num15;
                                    int num83 = num79 - num15;
                                    int num84 = num80 - num15;
                                    ColorBgra* bgraPtr11 = rowPointer + num81;
                                    ColorBgra* bgraPtr12 = bgraPtr7 + num82;
                                    ColorBgra* bgraPtr13 = bgraPtr8 + num83;
                                    ColorBgra* bgraPtr14 = bgraPtr9 + num84;
                                    byte num85 = (byte) ((((num74 ^ num73) & 8) * 8) + 0xbf);
                                    int a = bgraPtr11->A;
                                    int num87 = a + (a >> 7);
                                    int num88 = num85 * (0x100 - num87);
                                    int num89 = ((bgraPtr11->B * num87) + num88) >> 8;
                                    int num90 = ((bgraPtr11->G * num87) + num88) >> 8;
                                    int num91 = ((bgraPtr11->R * num87) + num88) >> 8;
                                    int num92 = bgraPtr12->A;
                                    int num93 = num92 + (num92 >> 7);
                                    int num94 = num85 * (0x100 - num93);
                                    int num95 = ((bgraPtr12->B * num93) + num94) >> 8;
                                    int num96 = ((bgraPtr12->G * num93) + num94) >> 8;
                                    int num97 = ((bgraPtr12->R * num93) + num94) >> 8;
                                    int num98 = bgraPtr13->A;
                                    int num99 = num98 + (num98 >> 7);
                                    int num100 = num85 * (0x100 - num99);
                                    int num101 = ((bgraPtr13->B * num99) + num100) >> 8;
                                    int num102 = ((bgraPtr13->G * num99) + num100) >> 8;
                                    int num103 = ((bgraPtr13->R * num99) + num100) >> 8;
                                    int num104 = bgraPtr14->A;
                                    int num105 = num104 + (num104 >> 7);
                                    int num106 = num85 * (0x100 - num105);
                                    int num107 = ((bgraPtr14->B * num105) + num106) >> 8;
                                    int num108 = ((bgraPtr14->G * num105) + num106) >> 8;
                                    int num109 = ((bgraPtr14->R * num105) + num106) >> 8;
                                    int num110 = ((((2 + num89) + num95) + num101) + num107) >> 2;
                                    int num111 = ((((2 + num90) + num96) + num102) + num108) >> 2;
                                    int num112 = ((((2 + num91) + num97) + num103) + num109) >> 2;
                                    bgraPtr10->Bgra = (uint) (((num110 | (num111 << 8)) | (num112 << 0x10)) | -16777216);
                                    bgraPtr10++;
                                    num74++;
                                }
                                row++;
                            }
                        }
                    }
                }
                else
                {
                    int num21 = 0;
                    for (int k = num10; (num21 < height) && (k < num12); k += num14)
                    {
                        int num23 = (k + (num14 >> 1)) >> 12;
                        int num24 = k >> 12;
                        int num25 = ((k + (num14 >> 1)) + (num14 >> 2)) >> 12;
                        int num26 = (k + (num14 >> 2)) >> 12;
                        ColorBgra* bgraPtr = (ColorBgra*) dstCropped.GetRowPointer<ColorBgra>(num21);
                        int num27 = num21 + y;
                        int num28 = x;
                        int num29 = num28 + width;
                        for (int m = num9; (num28 < num29) && (m < num11); m += num13)
                        {
                            int left = m >> 12;
                            int num32 = (m + (num13 >> 2)) >> 12;
                            int num33 = (m + (num13 >> 1)) >> 12;
                            int num34 = ((m + (num13 >> 1)) + (num13 >> 2)) >> 12;
                            ColorBgraTileSlice colorBgraMidTile = source.GetColorBgraMidTile(num23, left);
                            ColorBgraTileSlice slice2 = source.GetColorBgraMidTile(num24, num32);
                            ColorBgraTileSlice slice3 = source.GetColorBgraMidTile(num25, num33);
                            ColorBgraTileSlice slice4 = source.GetColorBgraMidTile(num26, num34);
                            do
                            {
                                ColorBgra* bgraPtr2 = (colorBgraMidTile.TilePointer + left) - colorBgraMidTile.Left;
                                ColorBgra* bgraPtr3 = (slice2.TilePointer + num32) - slice2.Left;
                                ColorBgra* bgraPtr4 = (slice3.TilePointer + num33) - slice3.Left;
                                ColorBgra* bgraPtr5 = (slice4.TilePointer + num34) - slice4.Left;
                                byte num35 = (byte) ((((num28 ^ num27) & 8) * 8) + 0xbf);
                                int num36 = bgraPtr2->A;
                                int num37 = num36 + (num36 >> 7);
                                int num38 = num35 * (0x100 - num37);
                                int num39 = ((bgraPtr2->B * num37) + num38) >> 8;
                                int num40 = ((bgraPtr2->G * num37) + num38) >> 8;
                                int num41 = ((bgraPtr2->R * num37) + num38) >> 8;
                                int num42 = bgraPtr3->A;
                                int num43 = num42 + (num42 >> 7);
                                int num44 = num35 * (0x100 - num43);
                                int num45 = ((bgraPtr3->B * num43) + num44) >> 8;
                                int num46 = ((bgraPtr3->G * num43) + num44) >> 8;
                                int num47 = ((bgraPtr3->R * num43) + num44) >> 8;
                                int num48 = bgraPtr4->A;
                                int num49 = num48 + (num48 >> 7);
                                int num50 = num35 * (0x100 - num49);
                                int num51 = ((bgraPtr4->B * num49) + num50) >> 8;
                                int num52 = ((bgraPtr4->G * num49) + num50) >> 8;
                                int num53 = ((bgraPtr4->R * num49) + num50) >> 8;
                                int num54 = bgraPtr5->A;
                                int num55 = num54 + (num54 >> 7);
                                int num56 = num35 * (0x100 - num55);
                                int num57 = ((bgraPtr5->B * num55) + num56) >> 8;
                                int num58 = ((bgraPtr5->G * num55) + num56) >> 8;
                                int num59 = ((bgraPtr5->R * num55) + num56) >> 8;
                                int num60 = ((((2 + num39) + num45) + num51) + num57) >> 2;
                                int num61 = ((((2 + num40) + num46) + num52) + num58) >> 2;
                                int num62 = ((((2 + num41) + num47) + num53) + num59) >> 2;
                                bgraPtr->Bgra = (uint) (((num60 | (num61 << 8)) | (num62 << 0x10)) | -16777216);
                                bgraPtr++;
                                num28++;
                                m += num13;
                                left = m >> 12;
                                num32 = (m + (num13 >> 2)) >> 12;
                                num33 = (m + (num13 >> 1)) >> 12;
                                num34 = ((m + (num13 >> 1)) + (num13 >> 2)) >> 12;
                            }
                            while (((left < colorBgraMidTile.Right) && (num34 < slice4.Right)) && ((num28 < num29) && (m < num11)));
                            num28--;
                            m -= num13;
                            num28++;
                        }
                        num21++;
                    }
                }
            }
        }
    }
}

