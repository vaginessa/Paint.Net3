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

    internal sealed class EllipseTool : ShapeTool
    {
        private Cursor ellipseToolCursor;
        private ImageResource ellipseToolIcon;
        private string statusTextFormat;

        public EllipseTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.EllipseToolIcon.png"), PdnResources.GetString2("EllipseTool.Name"), PdnResources.GetString2("EllipseTool.HelpText"))
        {
            this.statusTextFormat = PdnResources.GetString2("EllipseTool.StatusText.Format");
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
                Point point3 = new Point(b.X - a.X, b.Y - a.Y);
                double num = Math.Sqrt((point3.X * point3.X) + (point3.Y * point3.Y));
                Point center = new Point((a.X + b.X) / 2.0, (a.Y + b.Y) / 2.0);
                double radius = num / 2.0;
                rect = RectUtil.FromCenter(center, radius);
            }
            else
            {
                rect = RectUtil.FromPixelPoints(a, b);
            }
            if ((rect.Width == 0.0) || (rect.Height == 0.0))
            {
                return null;
            }
            PdnGraphicsPath path = new PdnGraphicsPath();
            path.AddEllipse(rect.ToGdipRectangleF());
            using (Matrix matrix = new Matrix())
            {
                path.Flatten(matrix, 0.1f);
            }
            MeasurementUnit units = base.AppWorkspace.Units;
            double num3 = Math.Abs(base.Document.PixelToPhysicalX(rect.Width, units));
            double num4 = Math.Abs(base.Document.PixelToPhysicalY(rect.Height, units));
            double num5 = (3.1415926535897931 * (num3 / 2.0)) * (num4 / 2.0);
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
            string statusText = string.Format(this.statusTextFormat, new object[] { num3.ToString(str), str2, num4.ToString(str), str2, num5.ToString(str), str4 });
            base.SetStatus(this.ellipseToolIcon, statusText);
            return path;
        }

        public override PixelOffsetMode GetPixelOffsetMode() => 
            PixelOffsetMode.None;

        protected override void OnActivate()
        {
            this.ellipseToolCursor = PdnResources.GetCursor2("Cursors.EllipseToolCursor.cur");
            this.ellipseToolIcon = base.Image;
            base.Cursor = this.ellipseToolCursor;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.ellipseToolCursor != null)
            {
                this.ellipseToolCursor.Dispose();
                this.ellipseToolCursor = null;
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

