namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class DeleteLayerFunction : HistoryFunction
    {
        private int layerIndex;

        public DeleteLayerFunction(int layerIndex) : base(ActionFlags.None)
        {
            this.layerIndex = layerIndex;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if ((this.layerIndex < 0) || (this.layerIndex >= historyWorkspace.Document.Layers.Count))
            {
                throw new ArgumentOutOfRangeException(string.Concat(new object[] { "layerIndex = ", this.layerIndex, ", expected [0, ", historyWorkspace.Document.Layers.Count, ")" }));
            }
            HistoryMemento memento = new DeleteLayerHistoryMemento(StaticName, StaticImage, historyWorkspace, historyWorkspace.Document.Layers.GetAt(this.layerIndex));
            base.EnterCriticalRegion();
            historyWorkspace.Document.Layers.RemoveAt(this.layerIndex);
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuLayersDeleteLayerIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("DeleteLayer.HistoryMementoName");
    }
}

