namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class PaintBucketTool : FloodToolBase
    {
        private Brush brush;
        private Cursor cursorMouseUp;

        public PaintBucketTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.PaintBucketIcon.png"), PdnResources.GetString2("PaintBucketTool.Name"), PdnResources.GetString2("PaintBucketTool.HelpText"), 'f', false, ToolBarConfigItems.AlphaBlending | ToolBarConfigItems.Antialiasing | ToolBarConfigItems.Brush)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && (this.brush != null))
            {
                this.brush.Dispose();
                this.brush = null;
            }
        }

        protected override void OnActivate()
        {
            this.cursorMouseUp = PdnResources.GetCursor2("Cursors.PaintBucketToolCursor.cur");
            base.Cursor = this.cursorMouseUp;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.cursorMouseUp != null)
            {
                this.cursorMouseUp.Dispose();
                this.cursorMouseUp = null;
            }
            base.OnDeactivate();
        }

        protected override void OnFillRegionComputed(GeometryList geometry)
        {
            geometry.Bounds.Int32Bound();
            using (RenderArgs args = new RenderArgs(((BitmapLayer) base.ActiveLayer).Surface))
            {
                HistoryMemento memento = new BitmapHistoryMemento(base.Name, base.Image, base.DocumentWorkspace, base.DocumentWorkspace.ActiveLayerIndex, geometry);
                args.Graphics.CompositingMode = base.AppEnvironment.GetCompositingMode();
                args.Graphics.FillGeometryList(this.brush, geometry);
                base.HistoryStack.PushNewMemento(memento);
                Int32Rect roi = geometry.Bounds.Int32Bound();
                base.ActiveLayer.Invalidate(roi);
            }
            base.Update();
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            this.brush = base.AppEnvironment.CreateBrush(e.Button != MouseButtons.Left);
            base.Cursor = Cursors.WaitCursor;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.Cursor = this.cursorMouseUp;
            base.OnMouseUp(e);
        }
    }
}

