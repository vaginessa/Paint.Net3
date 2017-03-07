namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class RectangleTool : ShapeTool
    {
        private Cursor rectangleToolCursor;
        private ImageResource rectangleToolIcon;
        private string statusTextFormat;

        public RectangleTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.RectangleToolIcon.png"), PdnResources.GetString2("RectangleTool.Name"), PdnResources.GetString2("RectangleTool.HelpText"))
        {
            this.statusTextFormat = PdnResources.GetString2("RectangleTool.StatusText.Format");
        }

        protected override PdnGraphicsPath CreateShapePath(Point[] points)
        {
            Rect rect;
            string str;
            string str2;
            Point a = points[0];
            Point b = points[points.Length - 1];
            if ((base.ModifierKeys & Keys.Shift) != Keys.None)
            {
                rect = RectUtil.FromPixelPointsConstrained(a, b);
            }
            else
            {
                rect = RectUtil.FromPixelPoints(a, b);
            }
            PdnGraphicsPath path = new PdnGraphicsPath();
            path.AddRectangle(rect.ToGdipRectangleF());
            path.CloseFigure();
            MeasurementUnit units = base.AppWorkspace.Units;
            double num = Math.Abs(base.Document.PixelToPhysicalX(rect.Width, units));
            double num2 = Math.Abs(base.Document.PixelToPhysicalY(rect.Height, units));
            double num3 = num * num2;
            if (units != MeasurementUnit.Pixel)
            {
                str2 = PdnResources.GetString2("MeasurementUnit." + units.ToString() + ".Abbreviation");
                str = "F2";
            }
            else
            {
                str2 = string.Empty;
                str = "F0";
            }
            string str4 = PdnResources.GetString2("MeasurementUnit." + units.ToString() + ".Plural");
            string statusText = string.Format(this.statusTextFormat, new object[] { num.ToString(str), str2, num2.ToString(str), str2, num3.ToString(str), str4 });
            base.SetStatus(this.rectangleToolIcon, statusText);
            return path;
        }

        public override PixelOffsetMode GetPixelOffsetMode()
        {
            float width = base.AppEnvironment.PenInfo.Width;
            int num2 = (int) width;
            if ((num2 == width) && ((num2 & 1) == 1))
            {
                return PixelOffsetMode.None;
            }
            return base.GetPixelOffsetMode();
        }

        protected override void OnActivate()
        {
            this.rectangleToolCursor = PdnResources.GetCursor2("Cursors.RectangleToolCursor.cur");
            this.rectangleToolIcon = base.Image;
            base.Cursor = this.rectangleToolCursor;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.rectangleToolCursor != null)
            {
                this.rectangleToolCursor.Dispose();
                this.rectangleToolCursor = null;
            }
            base.OnDeactivate();
        }

        protected override SegmentedList<Point> TrimShapePath(SegmentedList<Point> points)
        {
            SegmentedList<Point> list = new SegmentedList<Point>();
            if (points.Count > 0)
            {
                list.Add(points[0]);
                if (points.Count > 1)
                {
                    list.Add(points[points.Count - 1]);
                }
            }
            return list;
        }
    }
}

