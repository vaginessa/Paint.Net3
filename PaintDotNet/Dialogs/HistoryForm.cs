namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class HistoryForm : FloatingToolForm
    {
        private IContainer components;
        private ToolStripButton fastForwardButton;
        private PaintDotNet.Controls.HistoryControl historyControl;
        private ToolStripButton redoButton;
        private ToolStripButton rewindButton;
        private ToolStripEx toolStrip;
        private ToolStripButton undoButton;

        public event EventHandler FastForwardButtonClicked;

        public event EventHandler RedoButtonClicked;

        public event EventHandler RewindButtonClicked;

        public event EventHandler UndoButtonClicked;

        public HistoryForm()
        {
            this.InitializeComponent();
            this.toolStrip.Renderer = new PdnToolStripRenderer();
            this.rewindButton.Image = PdnResources.GetImageResource2("Icons.HistoryRewindIcon.png").Reference;
            this.undoButton.Image = PdnResources.GetImageResource2("Icons.MenuEditUndoIcon.png").Reference;
            this.redoButton.Image = PdnResources.GetImageResource2("Icons.MenuEditRedoIcon.png").Reference;
            this.fastForwardButton.Image = PdnResources.GetImageResource2("Icons.HistoryFastForwardIcon.png").Reference;
            this.Text = PdnResources.GetString2("HistoryForm.Text");
            this.rewindButton.ToolTipText = PdnResources.GetString2("HistoryForm.RewindButton.ToolTipText");
            this.undoButton.ToolTipText = PdnResources.GetString2("HistoryForm.UndoButton.ToolTipText");
            this.redoButton.ToolTipText = PdnResources.GetString2("HistoryForm.RedoButton.ToolTipText");
            this.fastForwardButton.ToolTipText = PdnResources.GetString2("HistoryForm.FastForwardButton.ToolTipText");
            this.MinimumSize = base.Size;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                this.components = null;
            }
            base.Dispose(disposing);
        }

        private void HistoryControl_HistoryChanged(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
            this.UpdateHistoryButtons();
        }

        private void HistoryControl_RelinquishFocus(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
        }

        private void HistoryForm_Enter(object sender, EventArgs e)
        {
            base.PerformLayout();
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.historyControl = new PaintDotNet.Controls.HistoryControl();
            this.toolStrip = new ToolStripEx();
            this.rewindButton = new ToolStripButton();
            this.undoButton = new ToolStripButton();
            this.redoButton = new ToolStripButton();
            this.fastForwardButton = new ToolStripButton();
            this.toolStrip.SuspendLayout();
            base.SuspendLayout();
            this.historyControl.Dock = DockStyle.Top;
            this.historyControl.HistoryStack = null;
            this.historyControl.Location = new Point(0, 0);
            this.historyControl.Name = "historyControl";
            this.historyControl.Size = new Size(160, 0x98);
            this.historyControl.TabIndex = 0;
            this.historyControl.HistoryChanged += new EventHandler(this.HistoryControl_HistoryChanged);
            this.historyControl.RelinquishFocus += new EventHandler(this.HistoryControl_RelinquishFocus);
            this.historyControl.ManagedFocus = true;
            this.toolStrip.Dock = DockStyle.Bottom;
            this.toolStrip.Items.AddRange(new ToolStripItem[] { this.rewindButton, this.undoButton, this.redoButton, this.fastForwardButton });
            this.toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            this.toolStrip.Location = new Point(0, 0x8b);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new Size(160, 0x13);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip1";
            this.toolStrip.RelinquishFocus += new EventHandler(this.ToolStrip_RelinquishFocus);
            this.rewindButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.rewindButton.Name = "rewindButton";
            this.rewindButton.Size = new Size(0x17, 4);
            this.rewindButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.undoButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.undoButton.Name = "undoButton";
            this.undoButton.Size = new Size(0x17, 4);
            this.undoButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.redoButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.redoButton.Name = "redoButton";
            this.redoButton.Size = new Size(0x17, 4);
            this.redoButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.fastForwardButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.fastForwardButton.Name = "fastForwardButton";
            this.fastForwardButton.Size = new Size(0x17, 4);
            this.fastForwardButton.Click += new EventHandler(this.OnToolStripButtonClick);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0xa5, 0x9e);
            base.Controls.Add(this.toolStrip);
            base.Controls.Add(this.historyControl);
            base.Name = "HistoryForm";
            base.Enter += new EventHandler(this.HistoryForm_Enter);
            base.Controls.SetChildIndex(this.historyControl, 0);
            base.Controls.SetChildIndex(this.toolStrip, 0);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected virtual void OnFastForwardButtonClicked()
        {
            if (this.FastForwardButtonClicked != null)
            {
                this.FastForwardButtonClicked(this, EventArgs.Empty);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (this.historyControl != null)
            {
                this.historyControl.Size = new Size(base.ClientRectangle.Width, base.ClientRectangle.Height - (this.toolStrip.Height + (base.ClientRectangle.Height - this.toolStrip.Bottom)));
            }
        }

        protected virtual void OnRedoButtonClicked()
        {
            if (this.RedoButtonClicked != null)
            {
                this.RedoButtonClicked(this, EventArgs.Empty);
            }
        }

        protected virtual void OnRewindButtonClicked()
        {
            if (this.RewindButtonClicked != null)
            {
                this.RewindButtonClicked(this, EventArgs.Empty);
            }
        }

        private void OnToolStripButtonClick(object sender, EventArgs e)
        {
            if (sender == this.undoButton)
            {
                this.OnUndoButtonClicked();
            }
            else if (sender == this.redoButton)
            {
                this.OnRedoButtonClicked();
            }
            else if (sender == this.rewindButton)
            {
                this.OnRewindButtonClicked();
            }
            else if (sender == this.fastForwardButton)
            {
                this.OnFastForwardButtonClicked();
            }
            this.OnRelinquishFocus();
        }

        protected virtual void OnUndoButtonClicked()
        {
            if (this.UndoButtonClicked != null)
            {
                this.UndoButtonClicked(this, EventArgs.Empty);
            }
        }

        public void PerformFastForwardClick()
        {
            this.OnFastForwardButtonClicked();
        }

        public void PerformRedoClick()
        {
            this.OnRedoButtonClicked();
        }

        public void PerformRewindClick()
        {
            this.OnRewindButtonClicked();
        }

        public void PerformUndoClick()
        {
            this.OnUndoButtonClicked();
        }

        private void ToolStrip_RelinquishFocus(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
        }

        private void UpdateHistoryButtons()
        {
            if (this.historyControl.HistoryStack == null)
            {
                this.rewindButton.Enabled = false;
                this.undoButton.Enabled = false;
                this.fastForwardButton.Enabled = false;
                this.redoButton.Enabled = false;
            }
            else
            {
                if (this.historyControl.HistoryStack.UndoStack.Count <= 1)
                {
                    this.rewindButton.Enabled = false;
                    this.undoButton.Enabled = false;
                }
                else
                {
                    this.rewindButton.Enabled = true;
                    this.undoButton.Enabled = true;
                }
                if (this.historyControl.HistoryStack.RedoStack.Count == 0)
                {
                    this.fastForwardButton.Enabled = false;
                    this.redoButton.Enabled = false;
                }
                else
                {
                    this.fastForwardButton.Enabled = true;
                    this.redoButton.Enabled = true;
                }
            }
        }

        public PaintDotNet.Controls.HistoryControl HistoryControl =>
            this.historyControl;
    }
}

