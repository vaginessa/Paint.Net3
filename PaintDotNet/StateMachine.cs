namespace PaintDotNet
{
    using System;
    using System.Collections;
    using System.Threading;

    internal class StateMachine
    {
        private PaintDotNet.State currentState;
        private PaintDotNet.State initialState;
        private ArrayList inputAlphabet;
        private Queue inputQueue = new Queue();
        private bool processingInput;

        public event EventHandler<EventArgs<PaintDotNet.State>> NewState;

        public event ProgressEventHandler StateProgress;

        public StateMachine(PaintDotNet.State initialState, IEnumerable inputAlphabet)
        {
            this.initialState = initialState;
            this.inputAlphabet = new ArrayList();
            foreach (object obj2 in inputAlphabet)
            {
                this.inputAlphabet.Add(obj2);
            }
        }

        private void OnNewState(PaintDotNet.State newState)
        {
            if (this.NewState != null)
            {
                this.NewState(this, new EventArgs<PaintDotNet.State>(newState));
            }
        }

        public void OnStateProgress(double percent)
        {
            if (this.StateProgress != null)
            {
                this.StateProgress(this, new ProgressEventArgs(percent));
            }
        }

        public void ProcessInput(object input)
        {
            if (this.processingInput)
            {
                throw new InvalidOperationException("already processing input");
            }
            if (this.currentState.IsFinalState)
            {
                throw new InvalidOperationException("state machine is already in a final state");
            }
            if (!this.inputAlphabet.Contains(input))
            {
                throw new ArgumentOutOfRangeException("must be contained in the input alphabet set", "input");
            }
            this.inputQueue.Enqueue(input);
            this.ProcessQueuedInput();
        }

        private void ProcessQueuedInput()
        {
            while (this.inputQueue.Count > 0)
            {
                PaintDotNet.State state;
                object input = this.inputQueue.Dequeue();
                this.currentState.ProcessInput(input, out state);
                if (state == this.currentState)
                {
                    throw new InvalidOperationException("must provide a clean, newly constructed state");
                }
                this.SetCurrentState(state);
            }
        }

        public void QueueInput(object input)
        {
            this.inputQueue.Enqueue(input);
        }

        private void SetCurrentState(PaintDotNet.State newState)
        {
            if ((this.currentState != null) && this.currentState.IsFinalState)
            {
                throw new InvalidOperationException("state machine is already in a final state");
            }
            this.currentState = newState;
            this.currentState.StateMachine = this;
            this.OnNewState(this.currentState);
            this.currentState.OnEnteredState();
            if (!this.currentState.IsFinalState)
            {
                this.ProcessQueuedInput();
            }
        }

        public void Start()
        {
            if (this.currentState != null)
            {
                throw new InvalidOperationException("may only call Start() once after construction");
            }
            this.SetCurrentState(this.initialState);
        }

        public PaintDotNet.State CurrentState =>
            this.currentState;

        public bool IsInFinalState =>
            this.currentState.IsFinalState;
    }
}

