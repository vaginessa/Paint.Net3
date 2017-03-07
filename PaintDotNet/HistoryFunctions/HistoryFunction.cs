namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class HistoryFunction
    {
        private PaintDotNet.ActionFlags actionFlags;
        private int criticalRegionCount;
        private ISynchronizeInvoke eventSink;
        private bool executed;
        private volatile bool pleaseCancel;

        public event EventHandler CancelRequested;

        public event EventHandler<EventArgs<HistoryMemento>> Finished;

        public HistoryFunction(PaintDotNet.ActionFlags actionFlags)
        {
            this.actionFlags = actionFlags;
        }

        public void BeginExecute(ISynchronizeInvoke eventSink, IHistoryWorkspace historyWorkspace, EventHandler<EventArgs<HistoryMemento>> finishedCallback)
        {
            if (finishedCallback == null)
            {
                throw new ArgumentNullException("finishedCallback");
            }
            if (this.eventSink != null)
            {
                throw new InvalidOperationException("already executing this function");
            }
            this.eventSink = eventSink;
            this.Finished += finishedCallback;
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.ExecuteTrampoline), historyWorkspace);
        }

        protected void EnterCriticalRegion()
        {
            Interlocked.Increment(ref this.criticalRegionCount);
        }

        public HistoryMemento Execute(IHistoryWorkspace historyWorkspace)
        {
            HistoryMemento memento = null;
            Exception exception = null;
            HistoryMemento memento2;
            try
            {
                try
                {
                    if (this.executed)
                    {
                        throw new InvalidOperationException("Already executed this HistoryFunction");
                    }
                    this.executed = true;
                    memento = this.OnExecute(historyWorkspace);
                    memento2 = memento;
                }
                catch (ArgumentOutOfRangeException exception2)
                {
                    if (this.criticalRegionCount > 0)
                    {
                        throw;
                    }
                    throw new HistoryFunctionNonFatalException(null, exception2);
                }
                catch (OutOfMemoryException exception3)
                {
                    if (this.criticalRegionCount > 0)
                    {
                        throw;
                    }
                    throw new HistoryFunctionNonFatalException(null, exception3);
                }
            }
            catch (Exception exception4)
            {
                if (!this.IsAsync)
                {
                    throw;
                }
                exception = exception4;
                memento2 = memento;
            }
            finally
            {
                if (this.IsAsync)
                {
                    this.OnFinished(memento, exception);
                }
            }
            return memento2;
        }

        private void ExecuteTrampoline(object context)
        {
            this.Execute((IHistoryWorkspace) context);
        }

        protected virtual void OnCancelRequested()
        {
            if (!this.pleaseCancel)
            {
                throw new InvalidOperationException("OnCancelRequested() was called when pleaseCancel equaled false");
            }
            if (this.CancelRequested != null)
            {
                this.eventSink.BeginInvoke(this.CancelRequested, new object[] { this, EventArgs.Empty });
            }
        }

        public abstract HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace);
        private void OnFinished(HistoryMemento memento, Exception exception)
        {
            if (this.eventSink.InvokeRequired)
            {
                this.eventSink.BeginInvoke(new Action<HistoryMemento, Exception>(this.OnFinished), new object[] { memento, exception });
            }
            else
            {
                if (exception != null)
                {
                    throw new WorkerThreadException(exception);
                }
                if (this.Finished != null)
                {
                    this.Finished(this, new EventArgs<HistoryMemento>(memento));
                }
            }
        }

        public PaintDotNet.ActionFlags ActionFlags =>
            this.actionFlags;

        public ISynchronizeInvoke EventSink
        {
            get
            {
                if (!this.IsAsync)
                {
                    throw new InvalidOperationException("EventSink property is only accessible when IsAsync is true");
                }
                return this.eventSink;
            }
        }

        public bool IsAsync =>
            (this.eventSink != null);

        protected bool PleaseCancel =>
            this.pleaseCancel;
    }
}

