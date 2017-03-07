namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class CanvasSizeAction : DocumentWorkspaceAction
    {
        public CanvasSizeAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            AnchorEdge lastCanvasSizeAnchorEdge = PaintDotNet.SettingNames.GetLastCanvasSizeAnchorEdge();
            Document document = ResizeDocument(documentWorkspace.FindForm(), documentWorkspace.Document, documentWorkspace.Document.Size, lastCanvasSizeAnchorEdge, documentWorkspace.AppWorkspace.AppEnvironment.SecondaryColor, true, true);
            if (document != null)
            {
                using (new PushNullToolMode(documentWorkspace))
                {
                    if (document.DpuUnit != MeasurementUnit.Pixel)
                    {
                        Settings.CurrentUser.SetString("LastNonPixelUnits", document.DpuUnit.ToString());
                        if (documentWorkspace.AppWorkspace.Units != MeasurementUnit.Pixel)
                        {
                            documentWorkspace.AppWorkspace.Units = document.DpuUnit;
                        }
                    }
                    ReplaceDocumentHistoryMemento memento = new ReplaceDocumentHistoryMemento(StaticName, StaticImage, documentWorkspace);
                    documentWorkspace.Document = document;
                    return memento;
                }
            }
            return null;
        }

        public static Document ResizeDocument(Document document, Size newSize, AnchorEdge edge, ColorBgra background)
        {
            Document document2 = new Document(newSize.Width, newSize.Height);
            document2.ReplaceMetaDataFrom(document);
            for (int i = 0; i < document.Layers.Count; i++)
            {
                Layer layer2;
                Layer layer = (Layer) document.Layers[i];
                if (!(layer is BitmapLayer))
                {
                    throw new InvalidOperationException("Canvas Size does not support Layers that are not BitmapLayers");
                }
                try
                {
                    layer2 = ResizeLayer((BitmapLayer) layer, newSize, edge, background);
                }
                catch (OutOfMemoryException)
                {
                    document2.Dispose();
                    throw;
                }
                document2.Layers.Add(layer2);
            }
            return document2;
        }

        public static Document ResizeDocument(IWin32Window parent, Document document, Size initialNewSize, AnchorEdge initialAnchor, ColorBgra background, bool loadAndSaveMaintainAspect, bool saveAnchor)
        {
            Document document3;
            using (CanvasSizeDialog dialog = new CanvasSizeDialog())
            {
                bool boolean;
                if (loadAndSaveMaintainAspect)
                {
                    boolean = Settings.CurrentUser.GetBoolean("LastMaintainAspectRatioCS", false);
                }
                else
                {
                    boolean = false;
                }
                dialog.OriginalSize = document.Size;
                dialog.OriginalDpuUnit = document.DpuUnit;
                dialog.OriginalDpu = document.DpuX;
                dialog.ImageWidth = initialNewSize.Width;
                dialog.ImageHeight = initialNewSize.Height;
                dialog.LayerCount = document.Layers.Count;
                dialog.AnchorEdge = initialAnchor;
                dialog.Units = dialog.OriginalDpuUnit;
                dialog.Resolution = document.DpuX;
                dialog.Units = PaintDotNet.SettingNames.GetLastNonPixelUnits();
                dialog.ConstrainToAspect = boolean;
                DialogResult result = dialog.ShowDialog(parent);
                Size newSize = new Size(dialog.ImageWidth, dialog.ImageHeight);
                MeasurementUnit units = dialog.Units;
                double resolution = dialog.Resolution;
                if (result == DialogResult.Cancel)
                {
                    return null;
                }
                if (loadAndSaveMaintainAspect)
                {
                    Settings.CurrentUser.SetBoolean("LastMaintainAspectRatioCS", dialog.ConstrainToAspect);
                }
                if (saveAnchor)
                {
                    Settings.CurrentUser.SetString("LastCanvasSizeAnchorEdge", dialog.AnchorEdge.ToString());
                }
                if (((newSize == document.Size) && (units == document.DpuUnit)) && (resolution == document.DpuX))
                {
                    document3 = null;
                }
                else
                {
                    try
                    {
                        Utility.GCFullCollect();
                        Document document2 = ResizeDocument(document, newSize, dialog.AnchorEdge, background);
                        document2.DpuUnit = units;
                        document2.DpuX = resolution;
                        document2.DpuY = resolution;
                        document3 = document2;
                    }
                    catch (OutOfMemoryException)
                    {
                        Utility.ErrorBox(parent, PdnResources.GetString2("CanvasSizeAction.ResizeDocument.OutOfMemory"));
                        document3 = null;
                    }
                    catch
                    {
                        document3 = null;
                    }
                }
            }
            return document3;
        }

        public static BitmapLayer ResizeLayer(BitmapLayer layer, Size newSize, AnchorEdge anchor, ColorBgra background)
        {
            BitmapLayer layer2 = new BitmapLayer(newSize.Width, newSize.Height);
            new UnaryPixelOps.Constant(background).Apply(layer2.Surface, layer2.Surface.Bounds);
            if (!layer.IsBackground)
            {
                new UnaryPixelOps.SetAlphaChannel(0).Apply(layer2.Surface, layer2.Surface.Bounds);
            }
            int num = 0;
            int num2 = 0;
            int num3 = newSize.Width - layer.Width;
            int num4 = newSize.Height - layer.Height;
            int num5 = (newSize.Width - layer.Width) / 2;
            int num6 = (newSize.Height - layer.Height) / 2;
            int x = 0;
            int y = 0;
            switch (anchor)
            {
                case AnchorEdge.TopLeft:
                    x = num2;
                    y = num;
                    break;

                case AnchorEdge.Top:
                    x = num5;
                    y = num;
                    break;

                case AnchorEdge.TopRight:
                    x = num3;
                    y = num;
                    break;

                case AnchorEdge.Left:
                    x = num2;
                    y = num6;
                    break;

                case AnchorEdge.Middle:
                    x = num5;
                    y = num6;
                    break;

                case AnchorEdge.Right:
                    x = num3;
                    y = num6;
                    break;

                case AnchorEdge.BottomLeft:
                    x = num2;
                    y = num4;
                    break;

                case AnchorEdge.Bottom:
                    x = num5;
                    y = num4;
                    break;

                case AnchorEdge.BottomRight:
                    x = num3;
                    y = num4;
                    break;
            }
            layer2.Surface.CopySurface(layer.Surface, new Point(x, y));
            layer2.LoadProperties(layer.SaveProperties());
            return layer2;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuImageCanvasSizeIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("CanvasSizeAction.Name");
    }
}

