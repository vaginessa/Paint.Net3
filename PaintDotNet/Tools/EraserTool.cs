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

    internal sealed class EraserTool : PaintDotNet.Tools.Tool
    {
        private BitmapLayer bitmapLayer;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;
        private System.Windows.Point lastMouseXY;
        private MouseButtons mouseButton;
        private bool mouseDown;
        private BrushPreviewRenderer previewRenderer;
        private RenderArgs renderArgs;
        private SegmentedList<Int32Rect> savedRects;

        public EraserTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.EraserToolIcon.png"), PdnResources.GetString2("EraserTool.Name"), PdnResources.GetString2("EraserTool.HelpText"), 'e', false, ToolBarConfigItems.Antialiasing | ToolBarConfigItems.Pen)
        {
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            this.cursorMouseUp = PdnResources.GetCursor2("Cursors.EraserToolCursor.cur");
            this.cursorMouseDown = PdnResources.GetCursor2("Cursors.EraserToolCursorMouseDown.cur");
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
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
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
            if (this.mouseDown)
            {
                this.OnMouseUp(new MouseEventArgsF(this.mouseButton, 0, this.lastMouseXY.X, this.lastMouseXY.Y, 0));
            }
            base.CanvasRenderer.Remove(this.previewRenderer);
            this.previewRenderer.Dispose();
            this.previewRenderer = null;
            this.savedRects.Clear();
            this.savedRects.TrimExcess();
            if (this.renderArgs != null)
            {
                this.renderArgs.Dispose();
                this.renderArgs = null;
            }
            this.bitmapLayer = null;
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            base.Cursor = this.cursorMouseDown;
            if (((e.Button & MouseButtons.Left) == MouseButtons.Left) || ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                this.previewRenderer.Visible = false;
                this.mouseDown = true;
                this.mouseButton = e.Button;
                this.lastMouseXY = new System.Windows.Point(e.Fx, e.Fy);
                using (GeometryList list = base.Selection.CreateGeometryListClippingMask())
                {
                    this.renderArgs.Graphics.SetClip(list);
                }
                this.OnMouseMove(e);
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
            if (this.mouseDown && ((e.Button & this.mouseButton) != MouseButtons.None))
            {
                int a;
                if (e.Button == MouseButtons.Left)
                {
                    a = base.AppEnvironment.PrimaryColor.A;
                }
                else
                {
                    a = base.AppEnvironment.SecondaryColor.A;
                }
                Pen pen = base.AppEnvironment.PenInfo.CreatePen(base.AppEnvironment.BrushInfo, Color.FromArgb(a, 0, 0, 0), Color.FromArgb(a, 0, 0, 0));
                System.Windows.Point lastMouseXY = this.lastMouseXY;
                System.Windows.Point b = new System.Windows.Point(e.Fx, e.Fy);
                Rect rect = RectUtil.FromPixelPoints(lastMouseXY, b);
                rect.Inflate((double) ((int) Math.Ceiling((double) pen.Width)), (double) ((int) Math.Ceiling((double) pen.Width)));
                if (this.renderArgs.Graphics.SmoothingMode == SmoothingMode.AntiAlias)
                {
                    rect.Inflate((double) 1.0, (double) 1.0);
                }
                Int32Rect saveMeBounds = rect.Int32Bound().IntersectCopy(base.ActiveLayer.Bounds());
                if (((rect.Width > 0.0) && (rect.Height > 0.0)) && this.renderArgs.Graphics.IsVisible(rect.ToGdipRectangleF()))
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
                    new UnaryPixelOps.InvertWithAlpha().Apply(this.renderArgs.Surface, saveMeBounds.ToGdipRectangle());
                    this.renderArgs.Graphics.CompositingMode = CompositingMode.SourceOver;
                    if ((base.CanvasRenderer.ScaleFactor.Ratio > 1.0) || (pen.Width > 1.0))
                    {
                        this.renderArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    }
                    else
                    {
                        this.renderArgs.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                    }
                    PointF tf = lastMouseXY.ToGdipPointF();
                    PointF tf2 = b.ToGdipPointF();
                    pen.EndCap = LineCap.Round;
                    pen.StartCap = LineCap.Round;
                    this.renderArgs.Graphics.DrawLine(pen, tf, tf2);
                    this.renderArgs.Graphics.FillEllipse(pen.Brush, tf.X - (pen.Width / 2f), tf.Y - (pen.Width / 2f), pen.Width, pen.Width);
                    new UnaryPixelOps.InvertWithAlpha().Apply(this.renderArgs.Surface, saveMeBounds.ToGdipRectangle());
                    new BinaryPixelOps.SetColorChannels().Apply(this.renderArgs.Surface, saveMeBounds.Location().ToGdipPoint(), base.ScratchSurface, saveMeBounds.Location().ToGdipPoint(), saveMeBounds.Size().ToGdipSize());
                    this.bitmapLayer.Invalidate(saveMeBounds);
                    base.Update();
                }
                this.lastMouseXY = b;
                pen.Dispose();
            }
            else
            {
                this.previewRenderer.BrushLocation = new System.Windows.Point(e.Fx, e.Fy);
                this.previewRenderer.BrushSize = base.AppEnvironment.PenInfo.Width / 2f;
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            base.Cursor = this.cursorMouseUp;
            if (this.mouseDown)
            {
                this.OnMouseMove(e);
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
            }
        }
    }
}

