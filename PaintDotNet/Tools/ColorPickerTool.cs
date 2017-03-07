namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal sealed class ColorPickerTool : PaintDotNet.Tools.Tool
    {
        private Cursor colorPickerToolCursor;
        private bool mouseDown;

        public ColorPickerTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.ColorPickerToolIcon.png"), PdnResources.GetString2("ColorPickerTool.Name"), PdnResources.GetString2("ColorPickerTool.HelpText"), 'k', true, ToolBarConfigItems.ColorPickerBehavior)
        {
            this.mouseDown = false;
        }

        private ColorBgra LiftColor(int x, int y) => 
            ((BitmapLayer) base.ActiveLayer).Surface[x, y];

        protected override void OnActivate()
        {
            this.colorPickerToolCursor = PdnResources.GetCursor2("Cursors.ColorPickerToolCursor.cur");
            base.Cursor = this.colorPickerToolCursor;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.colorPickerToolCursor != null)
            {
                this.colorPickerToolCursor.Dispose();
                this.colorPickerToolCursor = null;
            }
            base.OnDeactivate();
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            this.mouseDown = true;
            this.PickColor(e);
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            if (this.mouseDown)
            {
                this.PickColor(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            this.mouseDown = false;
            switch (base.AppEnvironment.ColorPickerClickBehavior)
            {
                case ColorPickerClickBehavior.NoToolSwitch:
                    return;

                case ColorPickerClickBehavior.SwitchToLastTool:
                    base.DocumentWorkspace.SetToolFromType(base.DocumentWorkspace.PreviousActiveToolType);
                    return;

                case ColorPickerClickBehavior.SwitchToPencilTool:
                    base.DocumentWorkspace.SetToolFromType(typeof(PencilTool));
                    return;
            }
            throw new InvalidEnumArgumentException();
        }

        private void PickColor(MouseEventArgsF e)
        {
            if (base.Document.Bounds.Contains(e.X, e.Y))
            {
                ColorBgra bgra = this.LiftColor(e.X, e.Y);
                if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    base.AppEnvironment.PrimaryColor = bgra;
                }
                else if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
                {
                    base.AppEnvironment.SecondaryColor = bgra;
                }
            }
        }
    }
}

