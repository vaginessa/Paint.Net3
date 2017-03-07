namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class RectangleSelectTool : SelectionTool
    {
        public RectangleSelectTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.RectangleSelectToolIcon.png"), PdnResources.GetString2("RectangleSelectTool.Name"), PdnResources.GetString2("RectangleSelectTool.HelpText"), 's', ToolBarConfigItems.None | ToolBarConfigItems.SelectionDrawMode)
        {
        }

        protected override SegmentedList<Point> CreateShape(SegmentedList<Point> tracePoints)
        {
            Rect rect;
            SelectionDrawModeInfo info2;
            Point a = tracePoints[0];
            Point b = tracePoints[tracePoints.Count - 1];
            SelectionDrawModeInfo selectionDrawModeInfo = base.AppEnvironment.SelectionDrawModeInfo;
            switch (selectionDrawModeInfo.DrawMode)
            {
                case SelectionDrawMode.FixedRatio:
                case SelectionDrawMode.FixedSize:
                    info2 = selectionDrawModeInfo.CloneWithNewWidthAndHeight(Math.Abs(selectionDrawModeInfo.Width), Math.Abs(selectionDrawModeInfo.Height));
                    break;

                default:
                    info2 = selectionDrawModeInfo;
                    break;
            }
            switch (info2.DrawMode)
            {
                case SelectionDrawMode.Normal:
                    if ((base.ModifierKeys & Keys.Shift) == Keys.None)
                    {
                        rect = RectUtil.FromPixelPoints(a, b);
                        break;
                    }
                    rect = RectUtil.FromPixelPointsConstrained(a, b);
                    break;

                case SelectionDrawMode.FixedRatio:
                    try
                    {
                        double num = b.X - a.X;
                        double num2 = b.Y - a.Y;
                        double num3 = num / info2.Width;
                        double num4 = Math.Sign(num3);
                        double num5 = num2 / info2.Height;
                        double num6 = Math.Sign(num5);
                        double num7 = info2.Width / info2.Height;
                        if (num3 < num5)
                        {
                            double x = a.X;
                            double y = a.Y;
                            double right = a.X + num;
                            double bottom = a.Y + (num6 * Math.Abs((double) (num / num7)));
                            rect = RectUtil.FromEdges(x, y, right, bottom);
                        }
                        else
                        {
                            double left = a.X;
                            double top = a.Y;
                            double num14 = a.X + (num4 * Math.Abs((double) (num2 * num7)));
                            double num15 = a.Y + num2;
                            rect = RectUtil.FromEdges(left, top, num14, num15);
                        }
                    }
                    catch (ArithmeticException)
                    {
                        rect = new Rect(a.X, a.Y, 0.0, 0.0);
                    }
                    break;

                case SelectionDrawMode.FixedSize:
                {
                    double width = Document.ConvertMeasurement(info2.Width, info2.Units, base.Document.DpuUnit, base.Document.DpuX, MeasurementUnit.Pixel);
                    double height = Document.ConvertMeasurement(info2.Height, info2.Units, base.Document.DpuUnit, base.Document.DpuY, MeasurementUnit.Pixel);
                    rect = new Rect(b.X, b.Y, width, height);
                    break;
                }
                default:
                    throw new InvalidEnumArgumentException();
            }
            Int32Rect rect2 = rect.Int32Bound().IntersectCopy(base.Document.Bounds());
            if (rect2.HasPositiveArea())
            {
                SegmentedList<Point> list = new SegmentedList<Point>(5, 7) {
                    new Point((double) rect2.Left(), (double) rect2.Top()),
                    new Point((double) rect2.Right(), (double) rect2.Top()),
                    new Point((double) rect2.Right(), (double) rect2.Bottom()),
                    new Point((double) rect2.Left(), (double) rect2.Bottom())
                };
                list.Add(list[0]);
                return list;
            }
            return new SegmentedList<Point>(0, 7);
        }

        protected override void OnActivate()
        {
            base.SetCursors("Cursors.RectangleSelectToolCursor.cur", "Cursors.RectangleSelectToolCursorMinus.cur", "Cursors.RectangleSelectToolCursorPlus.cur", "Cursors.RectangleSelectToolCursorMouseDown.cur");
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        protected override SegmentedList<Point> TrimShapePath(SegmentedList<Point> tracePoints)
        {
            SegmentedList<Point> list = new SegmentedList<Point>();
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

