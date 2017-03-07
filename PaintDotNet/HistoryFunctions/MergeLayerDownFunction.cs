namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Windows;

    internal sealed class MergeLayerDownFunction : HistoryFunction
    {
        private int layerIndex;

        public MergeLayerDownFunction(int layerIndex) : base(ActionFlags.None)
        {
            this.layerIndex = layerIndex;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if ((this.layerIndex < 1) || (this.layerIndex >= historyWorkspace.Document.Layers.Count))
            {
                throw new ArgumentException(string.Concat(new object[] { "layerIndex must be greater than or equal to 1, and a valid layer index. layerIndex=", this.layerIndex, ", allowableRange=[0,", historyWorkspace.Document.Layers.Count, ")" }));
            }
            int layerIndex = this.layerIndex - 1;
            Int32Rect rect = historyWorkspace.Document.Bounds();
            GeometryList changedRegion = new GeometryList();
            changedRegion.AddRect(rect);
            BitmapHistoryMemento memento = new BitmapHistoryMemento(null, null, historyWorkspace, layerIndex, changedRegion);
            BitmapLayer layer = (BitmapLayer) historyWorkspace.Document.Layers[this.layerIndex];
            BitmapLayer layer2 = (BitmapLayer) historyWorkspace.Document.Layers[layerIndex];
            RenderArgs args = new RenderArgs(layer2.Surface);
            base.EnterCriticalRegion();
            foreach (Int32Rect rect2 in changedRegion.GetInteriorScans())
            {
                layer.Render(args, rect2.ToGdipRectangle());
            }
            layer2.Invalidate();
            args.Dispose();
            args = null;
            changedRegion.Dispose();
            changedRegion = null;
            HistoryMemento memento2 = new DeleteLayerFunction(this.layerIndex).Execute(historyWorkspace);
            return new CompoundHistoryMemento(StaticName, StaticImage, new HistoryMemento[] { memento, memento2 });
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuLayersMergeLayerDownIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("MergeLayerDown.HistoryMementoName");
    }
}

