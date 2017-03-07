namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Windows;

    internal sealed class CropToSelectionFunction : HistoryFunction
    {
        public CropToSelectionFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            Int32Rect[] interiorScans;
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }
            GeometryList geometryRhs = historyWorkspace.Selection.CreateGeometryListClippingMask();
            if (geometryRhs.Bounds.Area() < 1.0)
            {
                geometryRhs.Dispose();
                return null;
            }
            SelectionHistoryMemento memento = new SelectionHistoryMemento(StaticName, null, historyWorkspace);
            ReplaceDocumentHistoryMemento memento2 = new ReplaceDocumentHistoryMemento(StaticName, null, historyWorkspace);
            Int32Rect rect = geometryRhs.GetInteriorScans().Bounds();
            using (GeometryList list2 = new GeometryList(rect))
            {
                list2.CombineWith(geometryRhs, GeometryCombineMode.Exclude);
                list2.Translate((double) -rect.X, (double) -rect.Y);
                interiorScans = list2.GetInteriorScans();
            }
            geometryRhs.Dispose();
            geometryRhs = null;
            Document other = historyWorkspace.Document;
            Document document2 = new Document(rect.Width, rect.Height);
            document2.ReplaceMetaDataFrom(other);
            foreach (Layer layer in other.Layers)
            {
                if (!(layer is BitmapLayer))
                {
                    throw new InvalidOperationException("Crop does not support Layers that are not BitmapLayers");
                }
                BitmapLayer layer2 = (BitmapLayer) layer;
                BitmapLayer layer3 = new BitmapLayer(layer2.Surface.CreateWindow(rect.ToGdipRectangle()));
                ColorBgra color = ColorBgra.White.NewAlpha(0);
                foreach (Int32Rect rect2 in interiorScans)
                {
                    layer3.Surface.Clear(rect2, color);
                }
                layer3.LoadProperties(layer2.SaveProperties());
                document2.Layers.Add(layer3);
            }
            CompoundHistoryMemento memento3 = new CompoundHistoryMemento(StaticName, PdnResources.GetImageResource2("Icons.MenuImageCropIcon.png"), new HistoryMemento[] { memento, memento2 });
            base.EnterCriticalRegion();
            historyWorkspace.Document = document2;
            return memento3;
        }

        public static string StaticName =>
            PdnResources.GetString2("CropAction.Name");
    }
}

