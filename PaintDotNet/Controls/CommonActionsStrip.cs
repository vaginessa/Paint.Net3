namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class CommonActionsStrip : ToolStripEx
    {
        private ToolStripButton copyButton;
        private ToolStripButton cropButton;
        private ToolStripButton cutButton;
        private ToolStripButton deselectButton;
        private bool itemClickedMutex;
        private ToolStripButton newButton;
        private ToolStripButton openButton;
        private ToolStripButton pasteButton;
        private ToolStripButton printButton;
        private ToolStripButton redoButton;
        private ToolStripButton saveButton;
        private ToolStripSeparator separator0;
        private ToolStripSeparator separator1;
        private ToolStripSeparator separator2;
        private ToolStripButton undoButton;

        public event EventHandler<EventArgs<CommonAction>> ButtonClick;

        public CommonActionsStrip()
        {
            this.InitializeComponent();
            this.newButton.Image = PdnResources.GetImageResource2("Icons.MenuFileNewIcon.png").Reference;
            this.openButton.Image = PdnResources.GetImageResource2("Icons.MenuFileOpenIcon.png").Reference;
            this.saveButton.Image = PdnResources.GetImageResource2("Icons.MenuFileSaveIcon.png").Reference;
            this.printButton.Image = PdnResources.GetImageResource2("Icons.MenuFilePrintIcon.png").Reference;
            this.cutButton.Image = PdnResources.GetImageResource2("Icons.MenuEditCutIcon.png").Reference;
            this.copyButton.Image = PdnResources.GetImageResource2("Icons.MenuEditCopyIcon.png").Reference;
            this.pasteButton.Image = PdnResources.GetImageResource2("Icons.MenuEditPasteIcon.png").Reference;
            this.cropButton.Image = PdnResources.GetImageResource2("Icons.MenuImageCropIcon.png").Reference;
            this.deselectButton.Image = PdnResources.GetImageResource2("Icons.MenuEditDeselectIcon.png").Reference;
            this.undoButton.Image = PdnResources.GetImageResource2("Icons.MenuEditUndoIcon.png").Reference;
            this.redoButton.Image = PdnResources.GetImageResource2("Icons.MenuEditRedoIcon.png").Reference;
            this.newButton.ToolTipText = PdnResources.GetString2("CommonAction.New");
            this.openButton.ToolTipText = PdnResources.GetString2("CommonAction.Open");
            this.saveButton.ToolTipText = PdnResources.GetString2("CommonAction.Save");
            this.printButton.ToolTipText = PdnResources.GetString2("CommonAction.Print");
            this.cutButton.ToolTipText = PdnResources.GetString2("CommonAction.Cut");
            this.copyButton.ToolTipText = PdnResources.GetString2("CommonAction.Copy");
            this.pasteButton.ToolTipText = PdnResources.GetString2("CommonAction.Paste");
            this.cropButton.ToolTipText = PdnResources.GetString2("CommonAction.CropToSelection");
            this.deselectButton.ToolTipText = PdnResources.GetString2("CommonAction.Deselect");
            this.undoButton.ToolTipText = PdnResources.GetString2("CommonAction.Undo");
            this.redoButton.ToolTipText = PdnResources.GetString2("CommonAction.Redo");
            this.newButton.Tag = CommonAction.New;
            this.openButton.Tag = CommonAction.Open;
            this.saveButton.Tag = CommonAction.Save;
            this.printButton.Tag = CommonAction.Print;
            this.cutButton.Tag = CommonAction.Cut;
            this.copyButton.Tag = CommonAction.Copy;
            this.pasteButton.Tag = CommonAction.Paste;
            this.cropButton.Tag = CommonAction.CropToSelection;
            this.deselectButton.Tag = CommonAction.Deselect;
            this.undoButton.Tag = CommonAction.Undo;
            this.redoButton.Tag = CommonAction.Redo;
        }

        private ToolStripButton FindButton(CommonAction action)
        {
            switch (action)
            {
                case CommonAction.New:
                    return this.newButton;

                case CommonAction.Open:
                    return this.openButton;

                case CommonAction.Save:
                    return this.saveButton;

                case CommonAction.Print:
                    return this.printButton;

                case CommonAction.Cut:
                    return this.cutButton;

                case CommonAction.Copy:
                    return this.copyButton;

                case CommonAction.Paste:
                    return this.pasteButton;

                case CommonAction.CropToSelection:
                    return this.cropButton;

                case CommonAction.Deselect:
                    return this.deselectButton;

                case CommonAction.Undo:
                    return this.undoButton;

                case CommonAction.Redo:
                    return this.redoButton;
            }
            throw new InvalidEnumArgumentException();
        }

        public bool GetButtonEnabled(CommonAction action) => 
            this.FindButton(action).Enabled;

        public bool GetButtonVisible(CommonAction action) => 
            this.FindButton(action).Visible;

        private void InitializeComponent()
        {
            this.separator0 = new ToolStripSeparator();
            this.newButton = new ToolStripButton();
            this.openButton = new ToolStripButton();
            this.saveButton = new ToolStripButton();
            this.printButton = new ToolStripButton();
            this.separator1 = new ToolStripSeparator();
            this.cutButton = new ToolStripButton();
            this.copyButton = new ToolStripButton();
            this.pasteButton = new ToolStripButton();
            this.cropButton = new ToolStripButton();
            this.deselectButton = new ToolStripButton();
            this.separator2 = new ToolStripSeparator();
            this.undoButton = new ToolStripButton();
            this.redoButton = new ToolStripButton();
            base.SuspendLayout();
            this.Items.Add(this.separator0);
            this.Items.Add(this.newButton);
            this.Items.Add(this.openButton);
            this.Items.Add(this.saveButton);
            this.Items.Add(this.printButton);
            this.Items.Add(this.separator1);
            this.Items.Add(this.cutButton);
            this.Items.Add(this.copyButton);
            this.Items.Add(this.pasteButton);
            this.Items.Add(this.cropButton);
            this.Items.Add(this.deselectButton);
            this.Items.Add(this.separator2);
            this.Items.Add(this.undoButton);
            this.Items.Add(this.redoButton);
            base.ResumeLayout(false);
        }

        private void OnButtonClick(CommonAction action)
        {
            if (this.ButtonClick != null)
            {
                this.ButtonClick(this, new EventArgs<CommonAction>(action));
            }
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (!this.itemClickedMutex)
            {
                this.itemClickedMutex = true;
                try
                {
                    if (e.ClickedItem is ToolStripButton)
                    {
                        CommonAction tag = (CommonAction) e.ClickedItem.Tag;
                        this.OnButtonClick(tag);
                    }
                }
                finally
                {
                    this.itemClickedMutex = false;
                }
            }
            base.OnItemClicked(e);
        }

        public void SetButtonEnabled(CommonAction action, bool enabled)
        {
            this.FindButton(action).Enabled = enabled;
        }

        public void SetButtonVisible(CommonAction action, bool visible)
        {
            this.FindButton(action).Visible = visible;
        }
    }
}

