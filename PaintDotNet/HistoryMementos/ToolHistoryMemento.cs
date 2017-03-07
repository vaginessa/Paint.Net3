namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;

    internal abstract class ToolHistoryMemento : HistoryMemento
    {
        private PaintDotNet.Controls.DocumentWorkspace documentWorkspace;
        private Type toolType;

        public ToolHistoryMemento(PaintDotNet.Controls.DocumentWorkspace documentWorkspace, string name, ImageResource image) : base(name, image)
        {
            this.documentWorkspace = documentWorkspace;
            this.toolType = documentWorkspace.GetToolType();
        }

        protected abstract HistoryMemento OnToolUndo();
        protected sealed override HistoryMemento OnUndo()
        {
            if (this.documentWorkspace.GetToolType() != this.toolType)
            {
                this.documentWorkspace.SetToolFromType(this.toolType);
            }
            return this.OnToolUndo();
        }

        protected PaintDotNet.Controls.DocumentWorkspace DocumentWorkspace =>
            this.documentWorkspace;

        public Type ToolType =>
            this.toolType;
    }
}

