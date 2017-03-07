namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class AddNewBlankLayerFunction : HistoryFunction
    {
        public AddNewBlankLayerFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            BitmapLayer layer = null;
            layer = new BitmapLayer(historyWorkspace.Document.Width, historyWorkspace.Document.Height);
            string format = PdnResources.GetString2("AddNewBlankLayer.LayerName.Format");
            layer.Name = string.Format(format, (1 + historyWorkspace.Document.Layers.Count).ToString());
            int layerIndex = historyWorkspace.ActiveLayerIndex + 1;
            NewLayerHistoryMemento memento = new NewLayerHistoryMemento(PdnResources.GetString2("AddNewBlankLayer.HistoryMementoName"), PdnResources.GetImageResource2("Icons.MenuLayersAddNewLayerIcon.png"), historyWorkspace, layerIndex);
            base.EnterCriticalRegion();
            historyWorkspace.Document.Layers.Insert(layerIndex, layer);
            return memento;
        }
    }
}

