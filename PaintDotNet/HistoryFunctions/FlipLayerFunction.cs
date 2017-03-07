namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;

    internal abstract class FlipLayerFunction : HistoryFunction
    {
        private FlipType flipType;
        private string historyName;
        private int layerIndex;
        private ImageResource undoImage;

        public FlipLayerFunction(string historyName, ImageResource image, FlipType flipType, int layerIndex) : base(ActionFlags.None)
        {
            this.historyName = historyName;
            this.flipType = flipType;
            this.undoImage = image;
            this.layerIndex = layerIndex;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            CompoundHistoryMemento memento = new CompoundHistoryMemento(this.historyName, this.undoImage);
            if (!historyWorkspace.Selection.IsEmpty)
            {
                DeselectFunction function = new DeselectFunction();
                base.EnterCriticalRegion();
                HistoryMemento memento2 = function.Execute(historyWorkspace);
                memento.PushNewAction(memento2);
            }
            FlipLayerHistoryMemento newHA = new FlipLayerHistoryMemento(null, null, historyWorkspace, this.layerIndex, this.flipType);
            base.EnterCriticalRegion();
            newHA.PerformUndo();
            memento.PushNewAction(newHA);
            return memento;
        }
    }
}

