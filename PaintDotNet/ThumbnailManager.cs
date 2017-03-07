namespace PaintDotNet
{
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class ThumbnailManager : IIsDisposed, IDisposable
    {
        private bool disposed;
        private volatile bool quitRenderThread;
        private ManualResetEvent renderingInactive;
        private Stack<Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int>> renderQueue;
        private Thread renderThread;
        private ISynchronizeInvoke syncContext;
        private List<Triple<EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, object, EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>> thumbnailReadyInvokeList = new List<Triple<EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, object, EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>>();
        private int updateLatency;
        private object updateLock;
        private bool useBackgroundPriority = true;

        public ThumbnailManager(ISynchronizeInvoke syncContext)
        {
            int logicalCpuCount = Processor.LogicalCpuCount;
            int num2 = 40;
            int num3 = 160;
            while ((logicalCpuCount > 0) && (num3 > 0))
            {
                num3 = num3 >> 1;
                logicalCpuCount--;
            }
            this.updateLatency = num2 + num3;
            this.syncContext = syncContext;
            this.updateLock = new object();
            this.quitRenderThread = false;
            this.renderQueue = new Stack<Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int>>();
            this.renderingInactive = new ManualResetEvent(true);
            this.renderThread = new Thread(new ThreadStart(this.RenderThread));
            this.renderThread.Start();
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
                this.quitRenderThread = true;
                lock (this.updateLock)
                {
                    Monitor.Pulse(this.updateLock);
                }
                if (this.renderThread != null)
                {
                    this.renderThread.Join();
                    this.renderThread = null;
                }
                if (this.renderingInactive != null)
                {
                    this.renderingInactive.Close();
                    this.renderingInactive = null;
                }
            }
        }

        private void DrainThumbnailReadyInvokeList()
        {
            List<Triple<EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, object, EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>> thumbnailReadyInvokeList = null;
            lock (this.thumbnailReadyInvokeList)
            {
                thumbnailReadyInvokeList = this.thumbnailReadyInvokeList;
                this.thumbnailReadyInvokeList = new List<Triple<EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, object, EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>>();
            }
            foreach (Triple<EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, object, EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>> triple in thumbnailReadyInvokeList)
            {
                triple.First(triple.Second, triple.Third);
            }
        }

        ~ThumbnailManager()
        {
            this.Dispose(false);
        }

        private bool OnThumbnailReady(IThumbnailProvider dw, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>> callback, ISurface<ColorBgra> thumb)
        {
            EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>> third = new EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>(Pair.Create<IThumbnailProvider, ISurface<ColorBgra>>(dw, thumb));
            lock (this.thumbnailReadyInvokeList)
            {
                this.thumbnailReadyInvokeList.Add(new Triple<EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, object, EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>(callback, this, third));
            }
            try
            {
                this.syncContext.BeginInvoke(new Action(this.DrainThumbnailReadyInvokeList), null);
                return true;
            }
            catch (Exception exception)
            {
                if (!(exception is ObjectDisposedException) && !(exception is InvalidOperationException))
                {
                    throw;
                }
                return false;
            }
        }

        public void QueueThumbnailUpdate(IThumbnailProvider updateMe, int thumbSideLength, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>> callback)
        {
            if (thumbSideLength < 1)
            {
                throw new ArgumentOutOfRangeException("thumbSideLength", "must be greater than or equal to 1");
            }
            lock (this.updateLock)
            {
                bool flag = false;
                Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int> item = new Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int>(updateMe, callback, thumbSideLength);
                if (this.renderQueue.Count == 0)
                {
                    flag = true;
                }
                else
                {
                    Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int> triple2 = this.renderQueue.Peek();
                    if (item != triple2)
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    this.renderQueue.Push(item);
                }
                Monitor.Pulse(this.updateLock);
            }
        }

        public void RemoveFromQueue(IThumbnailProvider nukeMe)
        {
            lock (this.updateLock)
            {
                Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int>[] tripleArray = this.renderQueue.ToArrayEx<Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int>>();
                List<Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int>> list = new List<Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int>>();
                for (int i = 0; i < tripleArray.Length; i++)
                {
                    if (tripleArray[i].First != nukeMe)
                    {
                        list.Add(tripleArray[i]);
                    }
                }
                this.renderQueue.Clear();
                for (int j = 0; j < list.Count; j++)
                {
                    this.renderQueue.Push(list[j]);
                }
            }
        }

        private void RenderThread()
        {
            try
            {
                while (this.RenderThreadLoop())
                {
                }
            }
            finally
            {
                this.renderingInactive.Set();
            }
        }

        private bool RenderThreadLoop()
        {
            Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int> triple = new Triple<IThumbnailProvider, EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>, int>();
            lock (this.updateLock)
            {
                if (!this.quitRenderThread)
                {
                    goto Label_0046;
                }
                return false;
            Label_0028:
                Monitor.Wait(this.updateLock);
                if (this.quitRenderThread)
                {
                    return false;
                }
            Label_0046:
                if (this.renderQueue.Count == 0)
                {
                    goto Label_0028;
                }
                this.renderingInactive.Reset();
                triple = this.renderQueue.Pop();
            }
            Thread.Sleep(this.updateLatency);
            bool flag = true;
            lock (this.updateLock)
            {
                if (this.quitRenderThread)
                {
                    return false;
                }
                if ((this.renderQueue.Count > 0) && (triple == this.renderQueue.Peek()))
                {
                    flag = false;
                }
            }
            if (flag)
            {
                try
                {
                    ISurface<ColorBgra> surface;
                    ThreadBackground background;
                    if (this.useBackgroundPriority)
                    {
                        background = new ThreadBackground(ThreadBackgroundFlags.All);
                    }
                    else
                    {
                        background = null;
                    }
                    try
                    {
                        surface = triple.First.CreateThumbnailRenderer(triple.Third).Parallelize(4, (Processor.LogicalCpuCount - 1)).ToSurface();
                    }
                    finally
                    {
                        DisposableUtil.Free<ThreadBackground>(ref background);
                    }
                    bool flag2 = false;
                    lock (this.updateLock)
                    {
                        if (this.quitRenderThread)
                        {
                            surface.Dispose();
                            surface = null;
                            return false;
                        }
                        if ((this.renderQueue.Count > 0) && (triple == this.renderQueue.Peek()))
                        {
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        flag2 = !this.OnThumbnailReady(triple.First, triple.Second, surface);
                    }
                    if (flag2)
                    {
                        surface.Dispose();
                        surface = null;
                    }
                }
                catch (Exception)
                {
                }
            }
            this.renderingInactive.Set();
            return true;
        }

        public bool IsDisposed =>
            this.disposed;

        public int UpdateLatency
        {
            get => 
                this.updateLatency;
            set
            {
                this.updateLatency = value;
            }
        }

        public bool UseBackgroundPriority
        {
            get => 
                this.useBackgroundPriority;
            set
            {
                this.useBackgroundPriority = value;
            }
        }
    }
}

