namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class LayerPropertyHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;
        private int layerIndex;
        private object properties;

        public LayerPropertyHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
            this.properties = ((Layer) this.historyWorkspace.Document.Layers[layerIndex]).SaveProperties();
        }

        protected override HistoryMemento OnUndo()
        {
            HistoryMemento memento = new LayerPropertyHistoryMemento(base.Name, base.Image, this.historyWorkspace, this.layerIndex);
            Layer layer = (Layer) this.historyWorkspace.Document.Layers[this.layerIndex];
            layer.LoadProperties(this.properties, true);
            layer.PerformPropertyChanged();
            return memento;
        }
    }
}

