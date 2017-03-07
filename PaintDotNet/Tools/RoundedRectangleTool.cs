namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class RoundedRectangleTool : ShapeTool
    {
        private Cursor roundedRectangleCursor;
        private ImageResource roundedRectangleToolIcon;
        private string statusTextFormat;

        public RoundedRectangleTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.RoundedRectangleToolIcon.png"), PdnResources.GetString2("RoundedRectangleTool.Name"), PdnResources.GetString2("RoundedRectangleTool.HelpText"))
        {
            this.statusTextFormat = PdnResources.GetString2("RoundedRectangleTool.StatusText.Format");
        }

        protected override PdnGraphicsPath CreateShapePath(System.Windows.Point[] points)
        {
            Rect rect;
            string str;
            string str2;
            System.Windows.Point a = points[0];
            System.Windows.Point b = points[points.Length - 1];
            double num = 10.0;
            if ((base.ModifierKeys & Keys.Shift) != Keys.None)
            {
                rect = RectUtil.FromPixelPointsConstrained(a, b);
            }
            else
            {
                rect = RectUtil.FromPixelPoints(a, b);
            }
            PdnGraphicsPath roundedRect = this.GetRoundedRect(rect.ToGdipRectangleF(), (float) num);
            roundedRect.Flatten();
            if (roundedRect.PathPoints[0] != roundedRect.PathPoints[roundedRect.PathPoints.Length - 1])
            {
                roundedRect.AddLine(roundedRect.PathPoints[0], roundedRect.PathPoints[roundedRect.PathPoints.Length - 1]);
                roundedRect.CloseFigure();
            }
            MeasurementUnit units = base.AppWorkspace.Units;
            double num2 = Math.Abs(base.Document.PixelToPhysicalX(rect.Width, units));
            double num3 = Math.Abs(base.Document.PixelToPhysicalY(rect.Height, units));
            double num4 = num2 * num3;
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
            string statusText = string.Format(this.statusTextFormat, new object[] { num2.ToString(str), str2, num3.ToString(str), str2, num4.ToString(str), str4 });
            base.SetStatus(this.roundedRectangleToolIcon, statusText);
            return roundedRect;
        }

        private PdnGraphicsPath GetCapsule(RectangleF baseRect)
        {
            PdnGraphicsPath path = new PdnGraphicsPath();
            try
            {
                float height;
                RectangleF ef;
                if (baseRect.Width > baseRect.Height)
                {
                    height = baseRect.Height;
                    SizeF size = new SizeF(height, height);
                    ef = new RectangleF(baseRect.Location, size);
                    path.AddArc(ef, 90f, 180f);
                    ef.X = baseRect.Right - height;
                    path.AddArc(ef, 270f, 180f);
                    return path;
                }
                if (baseRect.Width < baseRect.Height)
                {
                    height = baseRect.Width;
                    SizeF ef3 = new SizeF(height, height);
                    ef = new RectangleF(baseRect.Location, ef3);
                    path.AddArc(ef, 180f, 180f);
                    ef.Y = baseRect.Bottom - height;
                    path.AddArc(ef, 0f, 180f);
                    return path;
                }
                path.AddEllipse(baseRect);
                return path;
            }
            catch (Exception)
            {
                path.AddEllipse(baseRect);
            }
            finally
            {
                path.CloseFigure();
            }
            return path;
        }

        private PdnGraphicsPath GetRoundedRect(RectangleF baseRect, float radius)
        {
            if (radius <= 0f)
            {
                PdnGraphicsPath path = new PdnGraphicsPath();
                path.AddRectangle(baseRect);
                path.CloseFigure();
                return path;
            }
            if (radius >= (((double) Math.Min(baseRect.Width, baseRect.Height)) / 2.0))
            {
                return this.GetCapsule(baseRect);
            }
            float width = radius * 2f;
            SizeF size = new SizeF(width, width);
            RectangleF rectF = new RectangleF(baseRect.Location, size);
            PdnGraphicsPath path2 = new PdnGraphicsPath();
            path2.AddArc(rectF, 180f, 90f);
            rectF.X = baseRect.Right - width;
            path2.AddArc(rectF, 270f, 90f);
            rectF.Y = baseRect.Bottom - width;
            path2.AddArc(rectF, 0f, 90f);
            rectF.X = baseRect.Left;
            path2.AddArc(rectF, 90f, 90f);
            path2.CloseFigure();
            return path2;
        }

        protected override void OnActivate()
        {
            this.roundedRectangleCursor = PdnResources.GetCursor2("Cursors.RoundedRectangleToolCursor.cur");
            base.Cursor = this.roundedRectangleCursor;
            this.roundedRectangleToolIcon = base.Image;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.roundedRectangleCursor != null)
            {
                this.roundedRectangleCursor.Dispose();
                this.roundedRectangleCursor = null;
            }
            base.OnDeactivate();
        }

        protected override SegmentedList<System.Windows.Point> TrimShapePath(SegmentedList<System.Windows.Point> points)
        {
            SegmentedList<System.Windows.Point> list = new SegmentedList<System.Windows.Point>();
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

