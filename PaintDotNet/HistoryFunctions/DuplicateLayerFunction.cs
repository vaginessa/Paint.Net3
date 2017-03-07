namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class DuplicateLayerFunction : HistoryFunction
    {
        private int layerIndex;

        public DuplicateLayerFunction(int layerIndex) : base(ActionFlags.None)
        {
            this.layerIndex = layerIndex;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if ((this.layerIndex < 0) || (this.layerIndex >= historyWorkspace.Document.Layers.Count))
            {
                throw new ArgumentOutOfRangeException(string.Concat(new object[] { "layerIndex = ", this.layerIndex, ", expected [0, ", historyWorkspace.Document.Layers.Count, ")" }));
            }
            Layer layer = null;
            layer = (Layer) historyWorkspace.ActiveLayer.Clone();
            layer.IsBackground = false;
            int layerIndex = 1 + this.layerIndex;
            HistoryMemento memento = new NewLayerHistoryMemento(StaticName, StaticImage, historyWorkspace, layerIndex);
            base.EnterCriticalRegion();
            historyWorkspace.Document.Layers.Insert(layerIndex, layer);
            layer.Invalidate();
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuLayersDuplicateLayerIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("DuplicateLayer.HistoryMementoName");
    }
}

