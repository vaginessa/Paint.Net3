namespace PaintDotNet.Tasks
{
    using PaintDotNet.Controls;
    using System;

    internal abstract class DocWorkspaceTask : DocWorkspaceTask<Unit>
    {
        public DocWorkspaceTask(DocumentWorkspace dw) : base(dw)
        {
        }
    }
}

