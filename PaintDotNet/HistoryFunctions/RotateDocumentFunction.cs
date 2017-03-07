namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    internal sealed class RotateDocumentFunction : HistoryFunction
    {
        private RotateType rotation;

        public RotateDocumentFunction(RotateType rotation) : base(ActionFlags.None)
        {
            this.rotation = rotation;
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            int height;
            int width;
            string str;
            string str2;
            switch (this.rotation)
            {
                case RotateType.Clockwise90:
                case RotateType.CounterClockwise90:
                    height = historyWorkspace.Document.Height;
                    width = historyWorkspace.Document.Width;
                    break;

                case RotateType.Rotate180:
                    height = historyWorkspace.Document.Width;
                    width = historyWorkspace.Document.Height;
                    break;

                default:
                    throw new InvalidEnumArgumentException("invalid RotateType");
            }
            switch (this.rotation)
            {
                case RotateType.Clockwise90:
                    str = "Icons.MenuImageRotate90CWIcon.png";
                    str2 = PdnResources.GetString2("RotateAction.90CW");
                    break;

                case RotateType.CounterClockwise90:
                    str = "Icons.MenuImageRotate90CCWIcon.png";
                    str2 = PdnResources.GetString2("RotateAction.90CCW");
                    break;

                case RotateType.Rotate180:
                    str = "Icons.MenuImageRotate180Icon.png";
                    str2 = PdnResources.GetString2("RotateAction.180");
                    break;

                default:
                    throw new InvalidEnumArgumentException("invalid RotateType");
            }
            string name = string.Format(PdnResources.GetString2("RotateAction.HistoryMementoName.Format"), StaticName, str2);
            ImageResource image = PdnResources.GetImageResource2(str);
            List<HistoryMemento> actions = new List<HistoryMemento>();
            Document document = new Document(height, width);
            if (!historyWorkspace.Selection.IsEmpty)
            {
                DeselectFunction function = new DeselectFunction();
                base.EnterCriticalRegion();
                HistoryMemento memento = function.Execute(historyWorkspace);
                actions.Add(memento);
            }
            ReplaceDocumentHistoryMemento item = new ReplaceDocumentHistoryMemento(null, null, historyWorkspace);
            actions.Add(item);
            document.ReplaceMetaDataFrom(historyWorkspace.Document);
            for (int i = 0; i < historyWorkspace.Document.Layers.Count; i++)
            {
                Layer at = historyWorkspace.Document.Layers.GetAt(i);
                if (!(at is BitmapLayer))
                {
                    throw new InvalidOperationException("Cannot Rotate non-BitmapLayers");
                }
                Layer layer2 = this.RotateLayer((BitmapLayer) at, this.rotation, height, width);
                document.Layers.Add(layer2);
                if (base.PleaseCancel)
                {
                    break;
                }
            }
            CompoundHistoryMemento memento3 = new CompoundHistoryMemento(name, image, actions);
            if (base.PleaseCancel)
            {
                return null;
            }
            base.EnterCriticalRegion();
            historyWorkspace.Document = document;
            return memento3;
        }

        private BitmapLayer RotateLayer(BitmapLayer layer, RotateType rotationType, int width, int height)
        {
            Surface surface = new Surface(width, height);
            if (rotationType == RotateType.Rotate180)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        surface[j, i] = layer.Surface[(width - j) - 1, (height - i) - 1];
                    }
                }
            }
            else if (rotationType == RotateType.CounterClockwise90)
            {
                for (int k = 0; k < height; k++)
                {
                    for (int m = 0; m < width; m++)
                    {
                        surface[m, k] = layer.Surface[(height - k) - 1, m];
                    }
                }
            }
            else if (rotationType == RotateType.Clockwise90)
            {
                for (int n = 0; n < height; n++)
                {
                    for (int num6 = 0; num6 < width; num6++)
                    {
                        surface[num6, n] = layer.Surface[n, (width - 1) - num6];
                    }
                }
            }
            BitmapLayer layer2 = new BitmapLayer(surface, true);
            layer2.LoadProperties(layer.SaveProperties());
            return layer2;
        }

        public static string StaticName =>
            PdnResources.GetString2("RotateAction.Name");
    }
}

