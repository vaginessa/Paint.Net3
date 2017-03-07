﻿namespace PaintDotNet
{
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class StateMachineExecutor : IDisposable
    {
        private bool disposed;
        private ManualResetEvent inputAvailable = new ManualResetEvent(false);
        private bool isStarted;
        private bool lowPriorityExecution;
        private volatile bool pleaseAbort;
        private object queuedInput;
        private StateMachine stateMachine;
        private ManualResetEvent stateMachineInitialized = new ManualResetEvent(false);
        private ManualResetEvent stateMachineNotBusy = new ManualResetEvent(false);
        private Thread stateMachineThread;
        private ISynchronizeInvoke syncContext;
        private Exception threadException;

        public event EventHandler<EventArgs<PaintDotNet.State>> StateBegin;

        public event EventHandler StateMachineBegin;

        public event EventHandler StateMachineFinished;

        public event ProgressEventHandler StateProgress;

        public event EventHandler<EventArgs<PaintDotNet.State>> StateWaitingForInput;

        public StateMachineExecutor(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void Abort()
        {
            if (!this.disposed)
            {
                this.pleaseAbort = true;
                PaintDotNet.State currentState = this.stateMachine.CurrentState;
                if ((currentState != null) && currentState.CanAbort)
                {
                    this.stateMachine.CurrentState.Abort();
                }
                this.stateMachineNotBusy.WaitOne();
                this.inputAvailable.Set();
                this.stateMachineThread.Join();
                if (this.threadException != null)
                {
                    throw new WorkerThreadException("State machine thread threw an exception", this.threadException);
                }
            }
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
                this.Abort();
                if (this.stateMachineInitialized != null)
                {
                    this.stateMachineInitialized.Close();
                    this.stateMachineInitialized = null;
                }
                if (this.stateMachineNotBusy != null)
                {
                    this.stateMachineNotBusy.Close();
                    this.stateMachineNotBusy = null;
                }
                if (this.inputAvailable != null)
                {
                    this.inputAvailable.Close();
                    this.inputAvailable = null;
                }
            }
            this.disposed = true;
        }

        ~StateMachineExecutor()
        {
            this.Dispose(false);
        }

        private void OnStateBegin(PaintDotNet.State state)
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Action<PaintDotNet.State>(this.OnStateBegin), new object[] { state });
            }
            else if (this.StateBegin != null)
            {
                this.StateBegin(this, new EventArgs<PaintDotNet.State>(state));
            }
        }

        private void OnStateMachineBegin()
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Action(this.OnStateMachineBegin), null);
            }
            else if (this.StateMachineBegin != null)
            {
                this.StateMachineBegin(this, EventArgs.Empty);
            }
        }

        private void OnStateMachineFinished()
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Action(this.OnStateMachineFinished), null);
            }
            else if (this.StateMachineFinished != null)
            {
                this.StateMachineFinished(this, EventArgs.Empty);
            }
        }

        private void OnStateProgress(double percent)
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Action<double>(this.OnStateProgress), new object[] { percent });
            }
            else if (this.StateProgress != null)
            {
                this.StateProgress(this, new ProgressEventArgs(percent));
            }
        }

        private void OnStateWaitingForInput(PaintDotNet.State state)
        {
            if ((this.syncContext != null) && this.syncContext.InvokeRequired)
            {
                this.syncContext.BeginInvoke(new Action<PaintDotNet.State>(this.OnStateWaitingForInput), new object[] { state });
            }
            else if (this.StateWaitingForInput != null)
            {
                this.StateWaitingForInput(this, new EventArgs<PaintDotNet.State>(state));
            }
        }

        public void ProcessInput(object input)
        {
            this.stateMachineNotBusy.WaitOne();
            this.stateMachineNotBusy.Reset();
            this.queuedInput = input;
            this.inputAvailable.Set();
        }

        public void Start()
        {
            if (this.isStarted)
            {
                throw new InvalidOperationException("State machine thread is already executing");
            }
            this.isStarted = true;
            this.stateMachineThread = new Thread(new ThreadStart(this.StateMachineThread));
            this.stateMachineInitialized.Reset();
            this.stateMachineThread.Start();
            this.stateMachineInitialized.WaitOne();
        }

        private void StateMachineThread()
        {
            ThreadBackground background = null;
            try
            {
                if (this.lowPriorityExecution)
                {
                    background = new ThreadBackground(ThreadBackgroundFlags.Cpu);
                }
                this.StateMachineThreadImpl();
            }
            finally
            {
                if (background != null)
                {
                    background.Dispose();
                    background = null;
                }
            }
        }

        private void StateMachineThreadImpl()
        {
            this.threadException = null;
            EventHandler<EventArgs<PaintDotNet.State>> handler = delegate (object sender, EventArgs<PaintDotNet.State> e) {
                this.stateMachineInitialized.Set();
                this.OnStateBegin(e.Data);
            };
            ProgressEventHandler handler2 = (sender, e) => this.OnStateProgress(e.Percent);
            try
            {
                this.stateMachineNotBusy.Set();
                this.OnStateMachineBegin();
                this.stateMachineNotBusy.Reset();
                this.stateMachine.NewState += handler;
                this.stateMachine.StateProgress += handler2;
                this.stateMachine.Start();
                do
                {
                    this.stateMachineNotBusy.Set();
                    this.OnStateWaitingForInput(this.stateMachine.CurrentState);
                    this.inputAvailable.WaitOne();
                    this.inputAvailable.Reset();
                    if (this.pleaseAbort)
                    {
                        break;
                    }
                    this.stateMachine.ProcessInput(this.queuedInput);
                }
                while (!this.stateMachine.IsInFinalState);
                this.stateMachineNotBusy.Set();
            }
            catch (Exception exception)
            {
                this.threadException = exception;
            }
            finally
            {
                this.stateMachineNotBusy.Set();
                this.stateMachineInitialized.Set();
                this.stateMachine.NewState -= handler;
                this.stateMachine.StateProgress -= handler2;
                this.OnStateMachineFinished();
            }
        }

        public PaintDotNet.State CurrentState =>
            this.stateMachine.CurrentState;

        public bool IsInFinalState =>
            this.stateMachine.IsInFinalState;

        public bool IsStarted =>
            this.isStarted;

        public bool LowPriorityExecution
        {
            get => 
                this.lowPriorityExecution;
            set
            {
                if (this.IsStarted)
                {
                    throw new InvalidOperationException("Can only enable low priority execution before the state machine begins execution");
                }
                this.lowPriorityExecution = value;
            }
        }

        public ISynchronizeInvoke SyncContext
        {
            get => 
                this.syncContext;
            set
            {
                this.syncContext = value;
            }
        }
    }
}

