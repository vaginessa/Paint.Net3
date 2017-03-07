namespace PaintDotNet
{
    using System;
    using System.Runtime.InteropServices;

    internal abstract class State
    {
        private bool abortedRequested;
        private bool isFinalState;
        private PaintDotNet.StateMachine stateMachine;

        protected State() : this(false)
        {
        }

        protected State(bool isFinalState)
        {
            this.isFinalState = isFinalState;
        }

        public void Abort()
        {
            if (this.CanAbort)
            {
                this.abortedRequested = true;
                this.OnAbort();
            }
        }

        protected virtual void OnAbort()
        {
        }

        public virtual void OnEnteredState()
        {
        }

        protected void OnProgress(double percent)
        {
            if (this.StateMachine != null)
            {
                this.StateMachine.OnStateProgress(percent);
            }
        }

        public abstract void ProcessInput(object input, out PaintDotNet.State newState);

        protected bool AbortRequested =>
            this.abortedRequested;

        public virtual bool CanAbort =>
            false;

        public bool IsFinalState =>
            this.isFinalState;

        public PaintDotNet.StateMachine StateMachine
        {
            get => 
                this.stateMachine;
            set
            {
                this.stateMachine = value;
            }
        }
    }
}

