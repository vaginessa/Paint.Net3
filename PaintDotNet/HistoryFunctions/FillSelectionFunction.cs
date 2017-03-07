namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class FillSelectionFunction : HistoryFunction
    {
        private ColorBgra fillColor;

        public FillSelectionFunction(ColorBgra fillColor) : base(ActionFlags.None)
        {
            this.fillColor = fillColor;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }
            GeometryList changedRegion = historyWorkspace.Selection.CreateGeometryListClippingMask();
            BitmapLayer activeLayer = (BitmapLayer) historyWorkspace.ActiveLayer;
            HistoryMemento memento = new BitmapHistoryMemento(StaticName, StaticImage, historyWorkspace, historyWorkspace.ActiveLayerIndex, changedRegion);
            base.EnterCriticalRegion();
            activeLayer.Surface.Clear(changedRegion.GetInteriorScans(), this.fillColor);
            activeLayer.Invalidate(changedRegion);
            changedRegion.Dispose();
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuEditFillSelectionIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("FillSelectionAction.Name");
    }
}

