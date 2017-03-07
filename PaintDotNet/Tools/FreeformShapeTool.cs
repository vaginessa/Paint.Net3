namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class FreeformShapeTool : ShapeTool
    {
        private Cursor freeformShapeToolCursor;

        public FreeformShapeTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.FreeformShapeToolIcon.png"), PdnResources.GetString2("FreeformShapeTool.Name"), PdnResources.GetString2("FreeformShapeTool.HelpText"))
        {
        }

        protected override PdnGraphicsPath CreateShapePath(Point[] points)
        {
            if (points.Length < 2)
            {
                return null;
            }
            bool flag = true;
            foreach (Point point in points)
            {
                if (point != points[0])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                return null;
            }
            PdnGraphicsPath path = new PdnGraphicsPath();
            path.AddLines(points.ToPointFArray());
            path.AddLine(points.Last<Point>().ToGdipPointF(), points[0].ToGdipPointF());
            path.CloseAllFigures();
            return path;
        }

        protected override void OnActivate()
        {
            this.freeformShapeToolCursor = PdnResources.GetCursor2("Cursors.FreeformShapeToolCursor.cur");
            base.Cursor = this.freeformShapeToolCursor;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.freeformShapeToolCursor != null)
            {
                this.freeformShapeToolCursor.Dispose();
                this.freeformShapeToolCursor = null;
            }
            base.OnDeactivate();
        }
    }
}

