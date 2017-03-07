namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class EllipseSelectTool : SelectionTool
    {
        public EllipseSelectTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.EllipseSelectToolIcon.png"), PdnResources.GetString2("EllipseSelectTool.Name"), PdnResources.GetString2("EllipseSelectTool.HelpText"), 's', ToolBarConfigItems.None)
        {
        }

        protected override SegmentedList<System.Windows.Point> CreateShape(SegmentedList<System.Windows.Point> tracePoints)
        {
            Rect rect;
            System.Windows.Point a = tracePoints[0];
            System.Windows.Point b = tracePoints[tracePoints.Count - 1];
            System.Windows.Point point3 = new System.Windows.Point(b.X - a.X, b.Y - a.Y);
            double num = (float) Math.Sqrt((point3.X * point3.X) + (point3.Y * point3.Y));
            if ((base.ModifierKeys & Keys.Shift) != Keys.None)
            {
                System.Windows.Point center = new System.Windows.Point((a.X + b.X) / 2.0, (a.Y + b.Y) / 2.0);
                double radius = num / 2.0;
                rect = RectUtil.FromCenter(center, radius);
            }
            else
            {
                rect = RectUtil.FromPixelPoints(a, b);
            }
            PdnGraphicsPath path = new PdnGraphicsPath();
            path.AddEllipse(rect.ToGdipRectangleF());
            using (Matrix matrix = new Matrix())
            {
                path.Flatten(matrix, 0.1f);
            }
            SegmentedList<System.Windows.Point> list = new SegmentedList<System.Windows.Point>(from pt in path.PathPoints select pt.ToWpfPoint(), 7);
            path.Dispose();
            return list;
        }

        protected override void OnActivate()
        {
            base.SetCursors("Cursors.EllipseSelectToolCursor.cur", "Cursors.EllipseSelectToolCursorMinus.cur", "Cursors.EllipseSelectToolCursorPlus.cur", "Cursors.EllipseSelectToolCursorMouseDown.cur");
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        protected override SegmentedList<System.Windows.Point> TrimShapePath(SegmentedList<System.Windows.Point> tracePoints)
        {
            SegmentedList<System.Windows.Point> list = new SegmentedList<System.Windows.Point>();
            if (tracePoints.Count > 0)
            {
                list.Add(tracePoints[0]);
                if (tracePoints.Count > 1)
                {
                    list.Add(tracePoints[tracePoints.Count - 1]);
                }
            }
            return list;
        }
    }
}

