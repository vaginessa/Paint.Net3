namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using System;
    using System.Windows.Forms;

    internal class UpdatesStateMachine : StateMachine
    {
        private Control uiContext;

        public UpdatesStateMachine() : base(new StartupState(), new object[] { UpdatesAction.Continue, UpdatesAction.Cancel })
        {
        }

        public Control UIContext
        {
            get => 
                this.uiContext;
            set
            {
                this.uiContext = value;
            }
        }
    }
}

