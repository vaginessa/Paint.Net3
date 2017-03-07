namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class PencilTool : PaintDotNet.Tools.Tool
    {
        private BitmapLayer bitmapLayer;
        private BinaryPixelOp blendOp;
        private GeometryList clipMask;
        private BinaryPixelOp copyOp;
        private Int32Point difference;
        private Int32Point? lastPoint;
        private MouseButtons mouseButton;
        private bool mouseDown;
        private ColorBgra pencilColor;
        private Cursor pencilToolCursor;
        private RenderArgs renderArgs;
        private SegmentedList<Int32Rect> savedRects;
        private SegmentedList<Int32Point> tracePoints;

        public PencilTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.PencilToolIcon.png"), PdnResources.GetString2("PencilTool.Name"), PdnResources.GetString2("PencilTool.HelpText"), 'p', true, ToolBarConfigItems.AlphaBlending)
        {
            this.lastPoint = null;
            this.blendOp = UserBlendOps.NormalBlendOp.Static;
            this.copyOp = new BinaryPixelOps.AssignFromRhs();
            this.mouseDown = false;
        }

        private void DrawLines<TList>(RenderArgs ra, TList points, int startIndex, int length, ColorBgra color) where TList: IList<Int32Point>
        {
            Func<System.Drawing.Point, Int32Point> selector = null;
            if (points.Count != 0)
            {
                if (points.Count == 1)
                {
                    Int32Point pt = points[0];
                    if (ra.Surface.Bounds<ColorBgra>().Contains(pt))
                    {
                        this.DrawPoint(ra, pt, color);
                    }
                }
                else
                {
                    for (int i = startIndex + 1; i < (startIndex + length); i++)
                    {
                        if (selector == null)
                        {
                            selector = p => p.ToInt32Point();
                        }
                        Int32Point[] pointArray = Utility.GetLinePoints(points[i - 1], points[i]).Select<System.Drawing.Point, Int32Point>(selector).ToArrayEx<Int32Point>();
                        int num2 = 0;
                        if (i != 1)
                        {
                            num2 = 1;
                        }
                        for (int j = num2; j < pointArray.Length; j++)
                        {
                            Int32Point point2 = pointArray[j];
                            this.DrawPoint(ra, point2, color);
                        }
                    }
                }
            }
        }

        private void DrawPoint(RenderArgs ra, Int32Point p, ColorBgra color)
        {
            if (ra.Surface.Bounds<ColorBgra>().Contains(p) && ra.Graphics.IsVisible((System.Drawing.Point) p))
            {
                ra.Surface[p.X, p.Y] = (base.AppEnvironment.AlphaBlending ? this.blendOp : this.copyOp).Apply(ra.Surface[p.X, p.Y], color);
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            this.pencilToolCursor = PdnResources.GetCursor2("Cursors.PencilToolCursor.cur");
            base.Cursor = this.pencilToolCursor;
            this.savedRects = new SegmentedList<Int32Rect>();
            if (base.ActiveLayer != null)
            {
                this.bitmapLayer = (BitmapLayer) base.ActiveLayer;
                this.renderArgs = new RenderArgs(this.bitmapLayer.Surface);
                this.tracePoints = new SegmentedList<Int32Point>();
            }
            else
            {
                this.bitmapLayer = null;
                DisposableUtil.Free<RenderArgs>(ref this.renderArgs);
            }
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            DisposableUtil.Free<Cursor>(ref this.pencilToolCursor);
            if (this.mouseDown)
            {
                System.Drawing.Point point = this.tracePoints[this.tracePoints.Count - 1];
                this.OnMouseUp(new MouseEventArgsF(this.mouseButton, 0, (double) point.X, (double) point.Y, 0));
            }
            this.savedRects = null;
            this.tracePoints = null;
            this.bitmapLayer = null;
            DisposableUtil.Free<RenderArgs>(ref this.renderArgs);
            DisposableUtil.Free<GeometryList>(ref this.clipMask);
            this.mouseDown = false;
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            if (!this.mouseDown && (((e.Button & MouseButtons.Left) == MouseButtons.Left) || ((e.Button & MouseButtons.Right) == MouseButtons.Right)))
            {
                this.mouseDown = true;
                this.mouseButton = e.Button;
                this.tracePoints = new SegmentedList<Int32Point>();
                this.bitmapLayer = (BitmapLayer) base.ActiveLayer;
                this.renderArgs = new RenderArgs(this.bitmapLayer.Surface);
                DisposableUtil.Free<GeometryList>(ref this.clipMask);
                this.clipMask = base.Selection.CreateGeometryListClippingMask();
                this.renderArgs.Graphics.SetClip(this.clipMask, CombineMode.Replace);
                this.OnMouseMove(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            if (this.mouseDown && ((e.Button & this.mouseButton) != MouseButtons.None))
            {
                Int32Point point = new Int32Point((int) Math.Truncate(e.Fx), (int) Math.Truncate(e.Fy));
                if (!this.lastPoint.HasValue)
                {
                    this.lastPoint = new Int32Point?(point);
                }
                this.difference = new Int32Point(point.X - this.lastPoint.Value.X, point.Y - this.lastPoint.Value.Y);
                if (this.tracePoints.Count > 0)
                {
                    Int32Point point2 = this.tracePoints[this.tracePoints.Count - 1];
                    if (point2 == point)
                    {
                        return;
                    }
                }
                if ((this.mouseButton & MouseButtons.Left) == MouseButtons.Left)
                {
                    this.pencilColor = base.AppEnvironment.PrimaryColor;
                }
                else
                {
                    this.pencilColor = base.AppEnvironment.SecondaryColor;
                }
                if ((this.tracePoints.Count <= 0) || (point != this.tracePoints[this.tracePoints.Count - 1]))
                {
                    this.tracePoints.Add(point);
                }
                if (base.ActiveLayer is BitmapLayer)
                {
                    Int32Rect rect;
                    if (this.tracePoints.Count == 1)
                    {
                        rect = Int32RectUtil.FromPixelPoints(point, point);
                    }
                    else
                    {
                        Int32Point a = this.tracePoints[this.tracePoints.Count - 1];
                        Int32Point b = this.tracePoints[this.tracePoints.Count - 2];
                        rect = Int32RectUtil.FromPixelPoints(a, b);
                    }
                    rect = rect.InflateCopy(2, 2).IntersectCopy(base.ActiveLayer.Bounds());
                    if (!rect.HasZeroArea() && this.renderArgs.Graphics.IsVisible(rect))
                    {
                        int num;
                        int num2;
                        base.SaveRegion(null, rect);
                        this.savedRects.Add(rect);
                        if (this.tracePoints.Count == 1)
                        {
                            num = 0;
                            num2 = 1;
                        }
                        else
                        {
                            num = this.tracePoints.Count - 2;
                            num2 = 2;
                        }
                        this.DrawLines<SegmentedList<Int32Point>>(this.renderArgs, this.tracePoints, num, num2, this.pencilColor);
                        this.bitmapLayer.Invalidate(rect);
                        base.Update();
                    }
                }
                this.lastPoint = new Int32Point?(point);
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            if (this.mouseDown)
            {
                this.OnMouseMove(e);
                this.mouseDown = false;
                if (this.savedRects.Count > 0)
                {
                    GeometryList saveMeGeometry = GeometryList.FromScans(Int32RectUtil.SimplifyRegion(this.savedRects));
                    base.SaveRegion(saveMeGeometry, saveMeGeometry.Bounds.Int32Bound());
                    HistoryMemento memento = new BitmapHistoryMemento(base.Name, base.Image, base.DocumentWorkspace, base.ActiveLayerIndex, saveMeGeometry, base.ScratchSurface);
                    base.HistoryStack.PushNewMemento(memento);
                    saveMeGeometry.Dispose();
                    this.savedRects.Clear();
                    this.savedRects.TrimExcess();
                    base.ClearSavedMemory();
                }
                this.tracePoints = null;
            }
        }
    }
}

