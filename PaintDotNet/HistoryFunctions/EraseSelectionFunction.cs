namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Windows;

    internal sealed class EraseSelectionFunction : HistoryFunction
    {
        public EraseSelectionFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }
            SelectionHistoryMemento memento = new SelectionHistoryMemento(string.Empty, null, historyWorkspace);
            GeometryList changedRegion = historyWorkspace.Selection.CreateGeometryList();
            changedRegion.CombineWith(historyWorkspace.Document.Bounds().ToRect(), GeometryCombineMode.Intersect);
            BitmapLayer activeLayer = (BitmapLayer) historyWorkspace.ActiveLayer;
            HistoryMemento memento2 = new BitmapHistoryMemento(null, null, historyWorkspace, historyWorkspace.ActiveLayerIndex, changedRegion);
            HistoryMemento memento3 = new CompoundHistoryMemento(StaticName, StaticImage, new HistoryMemento[] { memento, memento2 });
            base.EnterCriticalRegion();
            Int32Rect[] interiorScans = changedRegion.GetInteriorScans();
            activeLayer.Surface.Clear(interiorScans, ColorBgra.FromBgra(0xff, 0xff, 0xff, 0));
            activeLayer.Invalidate(changedRegion);
            historyWorkspace.Document.Invalidate(changedRegion);
            changedRegion.Dispose();
            historyWorkspace.Selection.Reset();
            return memento3;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuEditEraseSelectionIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("EraseSelectionAction.Name");
    }
}

