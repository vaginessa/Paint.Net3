namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class SwapLayerFunction : HistoryFunction
    {
        private int layer1Index;
        private int layer2Index;

        public SwapLayerFunction(int layer1Index, int layer2Index) : base(ActionFlags.None)
        {
            this.layer1Index = layer1Index;
            this.layer2Index = layer2Index;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (((this.layer1Index < 0) || (this.layer1Index >= historyWorkspace.Document.Layers.Count)) || ((this.layer2Index < 0) || (this.layer2Index >= historyWorkspace.Document.Layers.Count)))
            {
                throw new ArgumentOutOfRangeException(string.Concat(new object[] { "layer1Index = ", this.layer1Index, ", layer2Index = ", this.layer2Index, ", expected [0,", historyWorkspace.Document.Layers.Count, ")" }));
            }
            SwapLayerHistoryMemento memento = new SwapLayerHistoryMemento(StaticName, StaticImage, historyWorkspace, this.layer1Index, this.layer2Index);
            Layer at = historyWorkspace.Document.Layers.GetAt(this.layer1Index);
            Layer layer2 = historyWorkspace.Document.Layers.GetAt(this.layer2Index);
            base.EnterCriticalRegion();
            historyWorkspace.Document.Layers[this.layer1Index] = layer2;
            historyWorkspace.Document.Layers[this.layer2Index] = at;
            at.Invalidate();
            layer2.Invalidate();
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuLayersMoveLayerUpIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("SwapLayerFunction.Name");
    }
}

