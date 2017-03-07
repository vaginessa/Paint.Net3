namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class NullHistoryMemento : HistoryMemento
    {
        public NullHistoryMemento(string name, ImageResource image) : base(name, image)
        {
        }

        protected override HistoryMemento OnUndo()
        {
            throw new InvalidOperationException("NullHistoryMementos are not undoable");
        }
    }
}

