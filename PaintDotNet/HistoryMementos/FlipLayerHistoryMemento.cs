namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class FlipLayerHistoryMemento : HistoryMemento
    {
        private FlipType flipType;
        private IHistoryWorkspace historyWorkspace;
        private int layerIndex;

        public FlipLayerHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, int layerIndex, FlipType flipType) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            this.layerIndex = layerIndex;
            this.flipType = flipType;
        }

        private void Flip(Surface surface)
        {
            int num3;
            switch (this.flipType)
            {
                case FlipType.Horizontal:
                    for (int i = 0; i < surface.Height; i++)
                    {
                        for (int j = 0; j < (surface.Width / 2); j++)
                        {
                            ColorBgra bgra = surface[j, i];
                            surface[j, i] = surface[(surface.Width - j) - 1, i];
                            surface[(surface.Width - j) - 1, i] = bgra;
                        }
                    }
                    return;

                case FlipType.Vertical:
                    num3 = 0;
                    break;

                default:
                    throw new InvalidOperationException("FlipType was invalid");
            }
            while (num3 < surface.Width)
            {
                for (int k = 0; k < (surface.Height / 2); k++)
                {
                    ColorBgra bgra2 = surface[num3, k];
                    surface[num3, k] = surface[num3, (surface.Height - k) - 1];
                    surface[num3, (surface.Height - k) - 1] = bgra2;
                }
                num3++;
            }
        }

        protected override HistoryMemento OnUndo()
        {
            FlipLayerHistoryMemento memento = new FlipLayerHistoryMemento(base.Name, base.Image, this.historyWorkspace, this.layerIndex, this.flipType);
            BitmapLayer layer = (BitmapLayer) this.historyWorkspace.Document.Layers[this.layerIndex];
            this.Flip(layer.Surface);
            layer.Invalidate();
            return memento;
        }
    }
}

