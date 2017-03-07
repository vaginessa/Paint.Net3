namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class ZoomTool : PaintDotNet.Tools.Tool
    {
        private Cursor cursorZoom;
        private Cursor cursorZoomIn;
        private Cursor cursorZoomOut;
        private Cursor cursorZoomPan;
        private System.Drawing.Point downPt;
        private System.Drawing.Point lastPt;
        private MouseButtons mouseDown;
        private bool moveOffsetMode;
        private Selection outline;
        private SelectionRenderer outlineRenderer;
        private Rectangle rect;

        public ZoomTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.ZoomToolIcon.png"), PdnResources.GetString2("ZoomTool.Name"), PdnResources.GetString2("ZoomTool.HelpText"), 'z', false, ToolBarConfigItems.None)
        {
            this.rect = Rectangle.Empty;
            this.mouseDown = MouseButtons.None;
        }

        protected override void OnActivate()
        {
            this.cursorZoom = PdnResources.GetCursor2("Cursors.ZoomToolCursor.cur");
            this.cursorZoomIn = PdnResources.GetCursor2("Cursors.ZoomInToolCursor.cur");
            this.cursorZoomOut = PdnResources.GetCursor2("Cursors.ZoomOutToolCursor.cur");
            this.cursorZoomPan = PdnResources.GetCursor2("Cursors.ZoomOutToolCursor.cur");
            base.Cursor = this.cursorZoom;
            base.OnActivate();
            this.outline = new Selection();
            this.outlineRenderer = new SelectionRenderer(base.CanvasRenderer, this.outline, base.DocumentWorkspace);
            this.outlineRenderer.TintColor = ColorBgra.FromBgra(0xff, 0xff, 0xff, 0x80);
            base.CanvasRenderer.Add(this.outlineRenderer, true);
        }

        protected override void OnDeactivate()
        {
            if (this.cursorZoom != null)
            {
                this.cursorZoom.Dispose();
                this.cursorZoom = null;
            }
            if (this.cursorZoomIn != null)
            {
                this.cursorZoomIn.Dispose();
                this.cursorZoomIn = null;
            }
            if (this.cursorZoomOut != null)
            {
                this.cursorZoomOut.Dispose();
                this.cursorZoomOut = null;
            }
            if (this.cursorZoomPan != null)
            {
                this.cursorZoomPan.Dispose();
                this.cursorZoomPan = null;
            }
            base.CanvasRenderer.Remove(this.outlineRenderer);
            this.outlineRenderer.Dispose();
            this.outlineRenderer = null;
            base.OnDeactivate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!e.Handled && (this.mouseDown != MouseButtons.None))
            {
                e.Handled = true;
            }
            base.OnKeyPress(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            if (this.mouseDown != MouseButtons.None)
            {
                this.moveOffsetMode = true;
            }
            else
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        base.Cursor = this.cursorZoomIn;
                        break;

                    case MouseButtons.Right:
                        base.Cursor = this.cursorZoomOut;
                        break;

                    case MouseButtons.Middle:
                        base.Cursor = this.cursorZoomPan;
                        break;
                }
                this.mouseDown = e.Button;
                this.lastPt = new System.Drawing.Point(e.X, e.Y);
                this.downPt = this.lastPt;
                this.OnMouseMove(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            System.Drawing.Point b = new System.Drawing.Point(e.X, e.Y);
            if (this.moveOffsetMode)
            {
                System.Drawing.Size size = new System.Drawing.Size(b.X - this.lastPt.X, b.Y - this.lastPt.Y);
                this.downPt.X += size.Width;
                this.downPt.Y += size.Height;
            }
            if ((((e.Button == MouseButtons.Left) && (this.mouseDown == MouseButtons.Left)) && (PointFUtil.Distance((PointF) b, (PointF) this.downPt) > 10f)) || !this.rect.IsEmpty)
            {
                this.rect = Utility.PointsToRectangle(this.downPt, b);
                this.rect.Intersect(base.ActiveLayer.Bounds);
                this.UpdateDrawnRect();
            }
            else if ((e.Button == MouseButtons.Middle) && (this.mouseDown == MouseButtons.Middle))
            {
                System.Windows.Point documentScrollPosition = base.DocumentWorkspace.DocumentScrollPosition;
                documentScrollPosition.X += b.X - this.lastPt.X;
                documentScrollPosition.Y += b.Y - this.lastPt.Y;
                base.DocumentWorkspace.DocumentScrollPosition = documentScrollPosition;
                base.Update();
            }
            else
            {
                this.rect = Rectangle.Empty;
            }
            this.lastPt = b;
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            this.OnMouseMove(e);
            bool flag = true;
            base.Cursor = this.cursorZoom;
            if (this.moveOffsetMode)
            {
                this.moveOffsetMode = false;
                flag = false;
            }
            else if ((this.mouseDown == MouseButtons.Left) || (this.mouseDown == MouseButtons.Right))
            {
                Rectangle rect = this.rect;
                this.rect = Rectangle.Empty;
                this.UpdateDrawnRect();
                if (e.Button == MouseButtons.Left)
                {
                    Vector vector = new Vector((double) rect.Width, (double) rect.Height);
                    if (vector.Length < 10.0)
                    {
                        base.DocumentWorkspace.ZoomIn();
                        base.DocumentWorkspace.RecenterView(new System.Windows.Point((double) e.X, (double) e.Y));
                    }
                    else
                    {
                        base.DocumentWorkspace.ZoomToRectangle(rect);
                    }
                }
                else
                {
                    base.DocumentWorkspace.ZoomOut();
                    base.DocumentWorkspace.RecenterView(new System.Windows.Point((double) e.X, (double) e.Y));
                }
                this.outline.Reset();
            }
            if (flag)
            {
                this.mouseDown = MouseButtons.None;
            }
        }

        private void UpdateDrawnRect()
        {
            if (!this.rect.IsEmpty)
            {
                this.outline.PerformChanging();
                this.outline.Reset();
                this.outline.SetContinuation(this.rect.ToInt32Rect(), SelectionCombineMode.Replace);
                this.outline.CommitContinuation();
                this.outline.PerformChanged();
                base.Update();
            }
        }
    }
}

