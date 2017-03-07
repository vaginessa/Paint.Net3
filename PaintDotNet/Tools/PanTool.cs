namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class PanTool : PaintDotNet.Tools.Tool
    {
        private Cursor cursorMouseDown;
        private Cursor cursorMouseInvalid;
        private Cursor cursorMouseUp;
        private int ignoreMouseMove;
        private System.Drawing.Point lastMouseXY;
        private bool tracking;

        public PanTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.PanToolIcon.png"), PdnResources.GetString2("PanTool.Name"), PdnResources.GetString2("PanTool.HelpText"), 'h', false, ToolBarConfigItems.None)
        {
            base.autoScroll = false;
            this.tracking = false;
        }

        private bool CanPan()
        {
            if (base.DocumentWorkspace.VisibleDocumentRect.Size == base.Document.Size())
            {
                return false;
            }
            return true;
        }

        protected override void OnActivate()
        {
            this.cursorMouseDown = PdnResources.GetCursor2("Cursors.PanToolCursorMouseDown.cur");
            this.cursorMouseUp = PdnResources.GetCursor2("Cursors.PanToolCursor.cur");
            this.cursorMouseInvalid = PdnResources.GetCursor2("Cursors.PanToolCursorInvalid.cur");
            base.Cursor = this.cursorMouseUp;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.cursorMouseDown != null)
            {
                this.cursorMouseDown.Dispose();
                this.cursorMouseDown = null;
            }
            if (this.cursorMouseUp != null)
            {
                this.cursorMouseUp.Dispose();
                this.cursorMouseUp = null;
            }
            if (this.cursorMouseInvalid != null)
            {
                this.cursorMouseInvalid.Dispose();
                this.cursorMouseInvalid = null;
            }
            base.OnDeactivate();
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            this.lastMouseXY = new System.Drawing.Point(e.X, e.Y);
            this.tracking = true;
            if (this.CanPan())
            {
                base.Cursor = this.cursorMouseDown;
            }
            else
            {
                base.Cursor = this.cursorMouseInvalid;
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            if (this.ignoreMouseMove > 0)
            {
                this.ignoreMouseMove--;
            }
            else if (this.tracking)
            {
                System.Drawing.Point point = new System.Drawing.Point(e.X, e.Y);
                System.Drawing.Size size = new System.Drawing.Size(point.X - this.lastMouseXY.X, point.Y - this.lastMouseXY.Y);
                if ((size.Width != 0) || (size.Height != 0))
                {
                    System.Windows.Point documentScrollPosition = base.DocumentWorkspace.DocumentScrollPosition;
                    System.Windows.Point point3 = new System.Windows.Point(documentScrollPosition.X - size.Width, documentScrollPosition.Y - size.Height);
                    this.ignoreMouseMove++;
                    base.DocumentWorkspace.DocumentScrollPosition = point3;
                    this.lastMouseXY = point;
                    this.lastMouseXY.X -= size.Width;
                    this.lastMouseXY.Y -= size.Height;
                }
            }
            else if (this.CanPan())
            {
                base.Cursor = this.cursorMouseUp;
            }
            else
            {
                base.Cursor = this.cursorMouseInvalid;
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            if (this.CanPan())
            {
                base.Cursor = this.cursorMouseUp;
            }
            else
            {
                base.Cursor = this.cursorMouseInvalid;
            }
            this.tracking = false;
        }
    }
}

