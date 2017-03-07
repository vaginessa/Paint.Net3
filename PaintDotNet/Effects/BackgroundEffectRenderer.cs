namespace PaintDotNet.Effects
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Threading;
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class BackgroundEffectRenderer : IDisposable
    {
        private volatile bool aborted;
        private volatile bool disposed;
        private RenderArgs dstArgs;
        private Effect effect;
        private EffectConfigToken effectToken;
        private EffectConfigToken effectTokenCopy;
        private ArrayList exceptions = ArrayList.Synchronized(new ArrayList());
        private PdnRegion renderRegion;
        private RenderArgs srcArgs;
        private Thread thread;
        private ManualResetEvent threadInitialized;
        private PrivateThreadPool threadPool;
        private volatile bool threadShouldStop;
        private int tileCount;
        private PdnRegion[] tilePdnRegions;
        private Rectangle[][] tileRegions;
        private int workerThreads;

        public event EventHandler FinishedRendering;

        public event RenderedTileEventHandler RenderedTile;

        public event EventHandler StartingRendering;

        public BackgroundEffectRenderer(Effect effect, EffectConfigToken effectToken, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion renderRegion, int tileCount, int workerThreads)
        {
            this.effect = effect;
            this.effectToken = effectToken;
            this.dstArgs = dstArgs;
            this.srcArgs = srcArgs;
            this.renderRegion = renderRegion;
            this.renderRegion.Intersect(dstArgs.Bounds);
            this.tileRegions = this.SliceUpRegion(renderRegion, tileCount, dstArgs.Bounds);
            this.tilePdnRegions = new PdnRegion[this.tileRegions.Length];
            for (int i = 0; i < this.tileRegions.Length; i++)
            {
                this.tilePdnRegions[i] = Utility.RectanglesToRegion(this.tileRegions[i]);
            }
            this.tileCount = tileCount;
            this.workerThreads = workerThreads;
            if (effect.CheckForEffectFlags(EffectFlags.None | EffectFlags.SingleThreaded))
            {
                this.workerThreads = 1;
            }
            this.threadPool = new PrivateThreadPool(this.workerThreads, false);
        }

        public void Abort()
        {
            if (this.thread != null)
            {
                this.threadShouldStop = true;
                if (this.effect != null)
                {
                    try
                    {
                        this.effect.SignalCancelRequest();
                    }
                    catch (Exception)
                    {
                    }
                }
                this.Join();
                this.threadPool.Drain();
            }
        }

        public void AbortAsync()
        {
            this.threadShouldStop = true;
            Effect effect = this.effect;
            if (effect != null)
            {
                try
                {
                    effect.SignalCancelRequest();
                }
                catch (Exception)
                {
                }
            }
        }

        private Rectangle[] ConsolidateRects(Rectangle[] scans)
        {
            if (scans.Length == 0)
            {
                return scans;
            }
            SegmentedList<Rectangle> items = new SegmentedList<Rectangle>();
            int num = 0;
            items.Add(scans[0]);
            for (int i = 1; i < scans.Length; i++)
            {
                Rectangle rectangle2 = items[num];
                if (scans[i].Left == rectangle2.Left)
                {
                    Rectangle rectangle3 = items[num];
                    if (scans[i].Right == rectangle3.Right)
                    {
                        Rectangle rectangle4 = items[num];
                        if (scans[i].Top == rectangle4.Bottom)
                        {
                            Rectangle rectangle = items[num];
                            Rectangle rectangle5 = items[num];
                            rectangle.Height = scans[i].Bottom - rectangle5.Top;
                            items[num] = rectangle;
                            continue;
                        }
                    }
                }
                items.Add(scans[i]);
                num = items.Count - 1;
            }
            return items.ToArrayEx<Rectangle>();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this.disposed = true;
            if (disposing)
            {
                if (this.srcArgs != null)
                {
                    this.srcArgs.Dispose();
                    this.srcArgs = null;
                }
                if (this.dstArgs != null)
                {
                    this.dstArgs.Dispose();
                    this.dstArgs = null;
                }
                if (this.threadPool != null)
                {
                    this.threadPool.Dispose();
                    this.threadPool = null;
                }
            }
        }

        private void DrainExceptions()
        {
            if (this.exceptions.Count > 0)
            {
                Exception innerException = (Exception) this.exceptions[0];
                this.exceptions.Clear();
                throw new WorkerThreadException("Worker thread threw an exception", innerException);
            }
        }

        ~BackgroundEffectRenderer()
        {
            this.Dispose(false);
        }

        public void Join()
        {
            this.thread.Join();
            this.DrainExceptions();
        }

        private void OnFinishedRendering()
        {
            if (this.FinishedRendering != null)
            {
                this.FinishedRendering(this, EventArgs.Empty);
            }
        }

        private void OnRenderedTile(RenderedTileEventArgs e)
        {
            if (this.RenderedTile != null)
            {
                this.RenderedTile(this, e);
            }
        }

        private void OnStartingRendering()
        {
            if (this.StartingRendering != null)
            {
                this.StartingRendering(this, EventArgs.Empty);
            }
        }

        private Rectangle[][] SliceUpRegion(PdnRegion region, int sliceCount, Rectangle layerBounds)
        {
            Rectangle[][] rectangleArray = new Rectangle[sliceCount][];
            Scanline[] regionScans = Utility.GetRegionScans(region.GetRegionScansReadOnlyInt());
            for (int i = 0; i < sliceCount; i++)
            {
                int num2 = (regionScans.Length * i) / sliceCount;
                int num3 = Math.Min(regionScans.Length, (regionScans.Length * (i + 1)) / sliceCount);
                switch (i)
                {
                    case 0:
                        num3 = Math.Min(num3, num2 + 1);
                        break;

                    case 1:
                        num2 = Math.Min(num2, 1);
                        break;
                }
                Rectangle[] scans = Utility.ScanlinesToRectangles(regionScans, num2, num3 - num2);
                for (int j = 0; j < scans.Length; j++)
                {
                    scans[j].Intersect(layerBounds);
                }
                rectangleArray[i] = this.ConsolidateRects(scans);
            }
            return rectangleArray;
        }

        public void Start()
        {
            this.Abort();
            this.aborted = false;
            if (this.effectToken != null)
            {
                try
                {
                    this.effectTokenCopy = (EffectConfigToken) this.effectToken.Clone();
                }
                catch (Exception exception)
                {
                    this.exceptions.Add(exception);
                    this.effectTokenCopy = null;
                }
            }
            this.threadShouldStop = false;
            this.OnStartingRendering();
            this.thread = new Thread(new ThreadStart(this.ThreadFunction));
            this.threadInitialized = new ManualResetEvent(false);
            this.thread.Start();
            this.threadInitialized.WaitOne();
            this.threadInitialized.Close();
            this.threadInitialized = null;
        }

        public void ThreadFunction()
        {
            if (this.srcArgs.Surface.Scan0.MaySetAllowWrites)
            {
                this.srcArgs.Surface.Scan0.AllowWrites = false;
            }
            try
            {
                this.threadInitialized.Set();
                this.effect.SetRenderInfo(this.effectTokenCopy, this.dstArgs, this.srcArgs);
                if (this.threadShouldStop)
                {
                    this.effect.SignalCancelRequest();
                }
                else if (this.tileCount > 0)
                {
                    Rectangle[] rois = this.tileRegions[0];
                    this.effect.Render(this.effectTokenCopy, this.dstArgs, this.srcArgs, rois);
                    PdnRegion renderedRegion = this.tilePdnRegions[0];
                    if (!this.threadShouldStop)
                    {
                        this.OnRenderedTile(new RenderedTileEventArgs(renderedRegion, this.tileCount, 0));
                    }
                }
                RendererContext context = new RendererContext(this);
                WaitCallback callback = new WaitCallback(context.Renderer2);
                for (int i = 0; i < this.workerThreads; i++)
                {
                    EffectConfigToken token;
                    if (this.threadShouldStop)
                    {
                        this.effect.SignalCancelRequest();
                        break;
                    }
                    if (this.effectTokenCopy == null)
                    {
                        token = null;
                    }
                    else
                    {
                        token = this.effectTokenCopy.CloneT<EffectConfigToken>();
                    }
                    this.threadPool.QueueUserWorkItem(callback, token);
                }
                this.threadPool.Drain();
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }
            finally
            {
                PrivateThreadPool threadPool = this.threadPool;
                if (!this.disposed && (threadPool != null))
                {
                    try
                    {
                        threadPool.Drain();
                    }
                    catch (Exception)
                    {
                    }
                }
                this.OnFinishedRendering();
                RenderArgs srcArgs = this.srcArgs;
                if (srcArgs != null)
                {
                    Surface surface = srcArgs.Surface;
                    if (surface != null)
                    {
                        MemoryBlock block = surface.Scan0;
                        if (((block != null) && !this.disposed) && block.MaySetAllowWrites)
                        {
                            try
                            {
                                block.AllowWrites = true;
                            }
                            catch (ObjectDisposedException)
                            {
                            }
                        }
                    }
                }
            }
        }

        public bool Aborted =>
            this.aborted;

        private sealed class RendererContext
        {
            private BackgroundEffectRenderer ber;
            private int nextTile;

            public RendererContext(BackgroundEffectRenderer ber)
            {
                this.ber = ber;
            }

            public void Renderer(EffectConfigToken token)
            {
                int tileCount = this.ber.tileCount;
                try
                {
                Label_000C:
                    if (this.ber.threadShouldStop)
                    {
                        this.ber.effect.SignalCancelRequest();
                        this.ber.aborted = true;
                    }
                    else
                    {
                        int index = Interlocked.Increment(ref this.nextTile);
                        if (index < tileCount)
                        {
                            Rectangle[] rois = this.ber.tileRegions[index];
                            this.ber.effect.Render(token, this.ber.dstArgs, this.ber.srcArgs, rois);
                            PdnRegion renderedRegion = this.ber.tilePdnRegions[index];
                            if (!this.ber.threadShouldStop)
                            {
                                this.ber.OnRenderedTile(new RenderedTileEventArgs(renderedRegion, this.ber.tileCount, index));
                            }
                            goto Label_000C;
                        }
                    }
                }
                catch (Exception exception)
                {
                    this.ber.exceptions.Add(exception);
                }
            }

            public void Renderer2(object token)
            {
                if (token == null)
                {
                    this.Renderer(null);
                }
                else
                {
                    this.Renderer((EffectConfigToken) token);
                }
            }
        }
    }
}

