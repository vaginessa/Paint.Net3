namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class PaintBrushTool : PaintDotNet.Tools.Tool
    {
        private BitmapLayer bitmapLayer;
        private Brush brush;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;
        private Vector lastDir;
        private System.Windows.Point lastMouseXY;
        private System.Windows.Point lastNorm;
        private MouseButtons mouseButton;
        private bool mouseDown;
        private BrushPreviewRenderer previewRenderer;
        private RenderArgs renderArgs;
        private SegmentedList<Int32Rect> savedRects;

        public PaintBrushTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.PaintBrushToolIcon.png"), PdnResources.GetString2("PaintBrushTool.Name"), PdnResources.GetString2("PaintBrushTool.HelpText"), 'b', false, ToolBarConfigItems.AlphaBlending | ToolBarConfigItems.Antialiasing | ToolBarConfigItems.Brush | ToolBarConfigItems.Pen)
        {
            this.mouseDown = false;
        }

        private double GetWidth(double pressure) => 
            (((pressure * pressure) * base.AppEnvironment.PenInfo.Width) * 0.5);

        private System.Windows.Point[] MakePolygon(System.Windows.Point a, System.Windows.Point b, System.Windows.Point c, System.Windows.Point d)
        {
            System.Windows.Point point = new System.Windows.Point(a.X - b.X, a.Y - b.Y);
            System.Windows.Point point2 = new System.Windows.Point(c.X - d.X, c.Y - d.Y);
            if (((point.X * point2.X) + (point.Y * point2.Y)) > 0.0)
            {
                return new System.Windows.Point[] { a, b, d, c };
            }
            return new System.Windows.Point[] { a, b, c, d };
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            this.cursorMouseUp = PdnResources.GetCursor2("Cursors.PaintBrushToolCursor.cur");
            this.cursorMouseDown = PdnResources.GetCursor2("Cursors.PaintBrushToolCursorMouseDown.cur");
            base.Cursor = this.cursorMouseUp;
            this.savedRects = new SegmentedList<Int32Rect>();
            if (base.ActiveLayer != null)
            {
                this.bitmapLayer = (BitmapLayer) base.ActiveLayer;
                this.renderArgs = new RenderArgs(this.bitmapLayer.Surface);
            }
            else
            {
                this.bitmapLayer = null;
                this.renderArgs = null;
            }
            this.previewRenderer = new BrushPreviewRenderer(base.CanvasRenderer);
            base.CanvasRenderer.Add(this.previewRenderer, false);
            this.mouseDown = false;
        }

        protected override void OnDeactivate()
        {
            if (this.mouseDown)
            {
                this.OnMouseUp(new MouseEventArgsF(this.mouseButton, 0, this.lastMouseXY.X, this.lastMouseXY.Y, 0));
            }
            base.CanvasRenderer.Remove(this.previewRenderer);
            this.previewRenderer.Dispose();
            this.previewRenderer = null;
            this.savedRects = null;
            if (this.renderArgs != null)
            {
                this.renderArgs.Dispose();
                this.renderArgs = null;
            }
            this.bitmapLayer = null;
            if (this.cursorMouseUp != null)
            {
                this.cursorMouseUp.Dispose();
                this.cursorMouseUp = null;
            }
            if (this.cursorMouseDown != null)
            {
                this.cursorMouseDown.Dispose();
                this.cursorMouseDown = null;
            }
            base.OnDeactivate();
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            if (!this.mouseDown)
            {
                base.ClearSavedMemory();
                this.previewRenderer.Visible = false;
                base.Cursor = this.cursorMouseDown;
                if (((e.Button & MouseButtons.Left) == MouseButtons.Left) || ((e.Button & MouseButtons.Right) == MouseButtons.Right))
                {
                    this.mouseButton = e.Button;
                    if ((this.mouseButton & MouseButtons.Left) == MouseButtons.Left)
                    {
                        this.brush = base.AppEnvironment.CreateBrush(false);
                    }
                    else if ((this.mouseButton & MouseButtons.Right) == MouseButtons.Right)
                    {
                        this.brush = base.AppEnvironment.CreateBrush(true);
                    }
                    this.mouseDown = true;
                    this.mouseButton = e.Button;
                    using (GeometryList list = base.Selection.CreateGeometryListClippingMask())
                    {
                        this.renderArgs.Graphics.SetClip(list, CombineMode.Replace);
                    }
                    this.lastMouseXY = new System.Windows.Point(e.Fx, e.Fy);
                    this.OnMouseMoveImpl(e, true);
                }
            }
        }

        protected override void OnMouseEnter()
        {
            this.previewRenderer.Visible = true;
            base.OnMouseEnter();
        }

        protected override void OnMouseLeave()
        {
            this.previewRenderer.Visible = false;
            base.OnMouseLeave();
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            this.OnMouseMoveImpl(e, false);
        }

        private void OnMouseMoveImpl(MouseEventArgsF e, bool forceRender)
        {
            System.Windows.Point center = new System.Windows.Point(e.Fx, e.Fy);
            if (this.mouseDown && ((e.Button & this.mouseButton) != MouseButtons.None))
            {
                double width = this.GetWidth(1.0);
                System.Windows.Point lastMouseXY = this.lastMouseXY;
                System.Windows.Point point3 = center;
                Vector vector = (Vector) (point3 - lastMouseXY);
                Rect rect = RectUtil.FromCenter(center, width);
                if ((base.CanvasRenderer.ScaleFactor.Ratio > 1.0) || (width > 0.5))
                {
                    this.renderArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                }
                else
                {
                    this.renderArgs.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                }
                this.lastDir = vector;
                double length = vector.Length;
                if (length == 0.0)
                {
                    vector.X = 0.0;
                    vector.Y = 0.0;
                }
                else
                {
                    vector.X /= length;
                    vector.Y /= length;
                }
                System.Windows.Point point4 = new System.Windows.Point(vector.Y, -vector.X);
                point4.X *= width;
                point4.Y *= width;
                lastMouseXY.X -= vector.X * 0.1666;
                lastMouseXY.Y -= vector.Y * 0.1666;
                this.lastNorm = point4;
                System.Windows.Point[] pts = this.MakePolygon(new System.Windows.Point(lastMouseXY.X - this.lastNorm.X, lastMouseXY.Y - this.lastNorm.Y), new System.Windows.Point(lastMouseXY.X + this.lastNorm.X, lastMouseXY.Y + this.lastNorm.Y), new System.Windows.Point(point3.X + point4.X, point3.Y + point4.Y), new System.Windows.Point(point3.X - point4.X, point3.Y - point4.Y));
                Rect rect2 = Rect.Union(rect, Rect.Union(RectUtil.FromPixelPoints(pts[0], pts[1]), RectUtil.FromPixelPoints(pts[2], pts[3])));
                rect2.Inflate((double) 2.0, (double) 2.0);
                rect2.Intersect(base.ActiveLayer.Bounds().ToRect());
                if (((rect2.Width > 0.0) && (rect2.Height > 0.0)) && this.renderArgs.Graphics.IsVisible(rect2.ToGdipRectangleF()))
                {
                    Int32Rect saveMeBounds = rect2.Int32Bound().IntersectCopy(base.ActiveLayer.Bounds());
                    if ((saveMeBounds.Width > 0) && (saveMeBounds.Height > 0))
                    {
                        base.SaveRegion(null, saveMeBounds);
                        this.savedRects.Add(saveMeBounds);
                        if (base.AppEnvironment.AntiAliasing)
                        {
                            this.renderArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        }
                        else
                        {
                            this.renderArgs.Graphics.SmoothingMode = SmoothingMode.None;
                        }
                        this.renderArgs.Graphics.CompositingMode = base.AppEnvironment.GetCompositingMode();
                        this.renderArgs.Graphics.FillEllipse(this.brush, rect.ToGdipRectangleF());
                        if (this.lastMouseXY != center)
                        {
                            this.renderArgs.Graphics.FillPolygon(this.brush, pts.ToPointFArray(), FillMode.Winding);
                        }
                    }
                    this.bitmapLayer.Invalidate(saveMeBounds);
                    base.Update();
                }
                this.lastNorm = point4;
                this.lastMouseXY = center;
            }
            else
            {
                this.lastMouseXY = center;
                this.lastNorm = new System.Windows.Point(0.0, 0.0);
                this.lastDir = new Vector(0.0, 0.0);
                this.previewRenderer.BrushSize = base.AppEnvironment.PenInfo.Width / 2f;
            }
            this.previewRenderer.BrushLocation = center;
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            base.Cursor = this.cursorMouseUp;
            if (this.mouseDown)
            {
                this.previewRenderer.Visible = true;
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
                this.brush.Dispose();
                this.brush = null;
            }
        }
    }
}

