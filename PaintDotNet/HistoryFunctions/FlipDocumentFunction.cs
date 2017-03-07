namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;
    using System.Collections.Generic;

    internal abstract class FlipDocumentFunction : HistoryFunction
    {
        private FlipType flipType;
        private string historyName;
        private ImageResource undoImage;

        public FlipDocumentFunction(string historyName, ImageResource image, FlipType flipType) : base(ActionFlags.None)
        {
            this.historyName = historyName;
            this.undoImage = image;
            this.flipType = flipType;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            List<HistoryMemento> actions = new List<HistoryMemento>();
            if (!historyWorkspace.Selection.IsEmpty)
            {
                DeselectFunction function = new DeselectFunction();
                base.EnterCriticalRegion();
                HistoryMemento item = function.Execute(historyWorkspace);
                actions.Add(item);
            }
            int count = historyWorkspace.Document.Layers.Count;
            for (int i = 0; i < count; i++)
            {
                HistoryMemento memento2 = new FlipLayerHistoryMemento(this.historyName, this.undoImage, historyWorkspace, i, this.flipType);
                base.EnterCriticalRegion();
                HistoryMemento memento3 = memento2.PerformUndo();
                actions.Add(memento3);
            }
            return new CompoundHistoryMemento(this.historyName, this.undoImage, actions);
        }
    }
}

