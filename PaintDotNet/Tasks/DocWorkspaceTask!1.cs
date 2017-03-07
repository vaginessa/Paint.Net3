namespace PaintDotNet.Tasks
{
    using PaintDotNet.Controls;
    using PaintDotNet.Threading;
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class DocWorkspaceTask<T> : UITask<T>
    {
        public DocWorkspaceTask(DocumentWorkspace dw) : base(dw.TaskManager, dw.AppWorkspace)
        {
            this.DocWorkspace = dw;
        }

        public void Start()
        {
            base.Start(this.DocWorkspace.Dispatcher);
        }

        protected DocumentWorkspace DocWorkspace { get; private set; }
    }
}

