namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Windows.Forms;

    internal sealed class MagicWandTool : FloodToolBase
    {
        private SelectionCombineMode combineMode;
        private Cursor cursorMouseUp;
        private Cursor cursorMouseUpMinus;
        private Cursor cursorMouseUpPlus;

        public MagicWandTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.MagicWandToolIcon.png"), PdnResources.GetString2("MagicWandTool.Name"), PdnResources.GetString2("MagicWandTool.HelpText"), 's', false, ToolBarConfigItems.None | ToolBarConfigItems.SelectionCombineMode)
        {
            base.ClipToSelection = false;
        }

        private Cursor GetCursor() => 
            this.GetCursor((base.ModifierKeys & Keys.Control) != Keys.None, (base.ModifierKeys & Keys.Alt) != Keys.None);

        private Cursor GetCursor(bool ctrlDown, bool altDown)
        {
            if (ctrlDown)
            {
                return this.cursorMouseUpPlus;
            }
            if (altDown)
            {
                return this.cursorMouseUpMinus;
            }
            return this.cursorMouseUp;
        }

        protected override void OnActivate()
        {
            base.DocumentWorkspace.EnableSelectionTinting = true;
            this.cursorMouseUp = PdnResources.GetCursor2("Cursors.MagicWandToolCursor.cur");
            this.cursorMouseUpMinus = PdnResources.GetCursor2("Cursors.MagicWandToolCursorMinus.cur");
            this.cursorMouseUpPlus = PdnResources.GetCursor2("Cursors.MagicWandToolCursorPlus.cur");
            base.Cursor = this.GetCursor();
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            if (this.cursorMouseUp != null)
            {
                this.cursorMouseUp.Dispose();
                this.cursorMouseUp = null;
            }
            if (this.cursorMouseUpMinus != null)
            {
                this.cursorMouseUpMinus.Dispose();
                this.cursorMouseUpMinus = null;
            }
            if (this.cursorMouseUpPlus != null)
            {
                this.cursorMouseUpPlus.Dispose();
                this.cursorMouseUpPlus = null;
            }
            base.DocumentWorkspace.EnableSelectionTinting = false;
            base.OnDeactivate();
        }

        protected override void OnFillRegionComputed(GeometryList geometry)
        {
            SelectionHistoryMemento memento = new SelectionHistoryMemento(base.Name, base.Image, base.DocumentWorkspace);
            base.Selection.PerformChanging();
            base.Selection.SetContinuation(geometry, this.combineMode);
            base.Selection.CommitContinuation();
            base.Selection.PerformChanged();
            base.HistoryStack.PushNewMemento(memento);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.Cursor = this.GetCursor();
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.Cursor = this.GetCursor();
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.Cursor = Cursors.WaitCursor;
            if (((base.ModifierKeys & Keys.Control) != Keys.None) && (e.Button == MouseButtons.Left))
            {
                this.combineMode = SelectionCombineMode.Union;
            }
            else if (((base.ModifierKeys & Keys.Alt) != Keys.None) && (e.Button == MouseButtons.Left))
            {
                this.combineMode = SelectionCombineMode.Exclude;
            }
            else if (((base.ModifierKeys & Keys.Control) != Keys.None) && (e.Button == MouseButtons.Right))
            {
                this.combineMode = SelectionCombineMode.Xor;
            }
            else if (((base.ModifierKeys & Keys.Alt) != Keys.None) && (e.Button == MouseButtons.Right))
            {
                this.combineMode = SelectionCombineMode.Intersect;
            }
            else
            {
                this.combineMode = base.AppEnvironment.SelectionCombineMode;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.Cursor = this.GetCursor();
            base.OnMouseUp(e);
        }
    }
}

