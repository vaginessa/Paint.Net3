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

    internal class UnsavedChangesDialog : PdnBaseForm
    {
        private CommandButton cancelButton;
        private IContainer components;
        private PaintDotNet.Controls.HeadingLabel documentListHeader;
        private DocumentWorkspace[] documents;
        private DocumentStrip documentStrip;
        private CommandButton dontSaveButton;
        private HScrollBar hScrollBar;
        private Label infoLabel;
        private CommandButton saveButton;

        public event EventHandler<EventArgs<DocumentWorkspace>> DocumentClicked;

        public UnsavedChangesDialog()
        {
            this.InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DocumentList_DocumentClicked(object sender, EventArgs<Pair<DocumentWorkspace, DocumentClickAction>> e)
        {
            this.documentStrip.Update();
            this.OnDocumentClicked(e.Data.First);
        }

        private void DocumentList_ScrollOffsetChanged(object sender, EventArgs e)
        {
            this.hScrollBar.Value = this.documentStrip.ScrollOffset;
        }

        private void DontSaveButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.No;
            base.Close();
        }

        private void HScrollBar_ValueChanged(object sender, EventArgs e)
        {
            this.documentStrip.ScrollOffset = this.hScrollBar.Value;
        }

        private void InitializeComponent()
        {
            this.documentStrip = new DocumentStrip();
            this.documentListHeader = new PaintDotNet.Controls.HeadingLabel();
            this.hScrollBar = new HScrollBar();
            this.saveButton = new CommandButton();
            this.dontSaveButton = new CommandButton();
            this.cancelButton = new CommandButton();
            this.infoLabel = new Label();
            base.SuspendLayout();
            this.documentStrip.BackColor = SystemColors.ButtonHighlight;
            this.documentStrip.DocumentClicked += new EventHandler<EventArgs<Pair<DocumentWorkspace, DocumentClickAction>>>(this.DocumentList_DocumentClicked);
            this.documentStrip.DrawDirtyOverlay = false;
            this.documentStrip.EnsureSelectedIsVisible = false;
            this.documentStrip.ManagedFocus = true;
            this.documentStrip.Name = "documentList";
            this.documentStrip.ScrollOffset = 0;
            this.documentStrip.ScrollOffsetChanged += new EventHandler(this.DocumentList_ScrollOffsetChanged);
            this.documentStrip.ShowCloseButtons = false;
            this.documentStrip.ShowScrollButtons = false;
            this.documentStrip.TabIndex = 0;
            this.documentListHeader.Name = "documentListHeader";
            this.documentListHeader.RightMargin = 0;
            this.documentListHeader.TabIndex = 1;
            this.documentListHeader.TabStop = false;
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.TabIndex = 2;
            this.hScrollBar.ValueChanged += new EventHandler(this.HScrollBar_ValueChanged);
            this.saveButton.ActionImage = null;
            this.saveButton.AutoSize = true;
            this.saveButton.Name = "saveButton3";
            this.saveButton.TabIndex = 4;
            this.saveButton.Click += new EventHandler(this.SaveButton_Click);
            this.dontSaveButton.ActionImage = null;
            this.dontSaveButton.AutoSize = true;
            this.dontSaveButton.Name = "dontSaveButton";
            this.dontSaveButton.TabIndex = 5;
            this.dontSaveButton.Click += new EventHandler(this.DontSaveButton_Click);
            this.cancelButton.ActionImage = null;
            this.cancelButton.AutoSize = true;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Click += new EventHandler(this.CancelButton_Click);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.TabIndex = 7;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(450, 100);
            base.Controls.Add(this.infoLabel);
            base.Controls.Add(this.documentListHeader);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.hScrollBar);
            base.Controls.Add(this.dontSaveButton);
            base.Controls.Add(this.documentStrip);
            base.Controls.Add(this.saveButton);
            base.AcceptButton = this.saveButton;
            base.CancelButton = this.cancelButton;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Location = new Point(0, 0);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "UnsavedChangesDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.Controls.SetChildIndex(this.saveButton, 0);
            base.Controls.SetChildIndex(this.documentStrip, 0);
            base.Controls.SetChildIndex(this.dontSaveButton, 0);
            base.Controls.SetChildIndex(this.hScrollBar, 0);
            base.Controls.SetChildIndex(this.cancelButton, 0);
            base.Controls.SetChildIndex(this.documentListHeader, 0);
            base.Controls.SetChildIndex(this.infoLabel, 0);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString2("UnsavedChangesDialog.Text");
            base.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.WarningIcon.png").Reference, false);
            this.infoLabel.Text = PdnResources.GetString2("UnsavedChangesDialog.InfoLabel.Text");
            this.documentListHeader.Text = PdnResources.GetString2("UnsavedChangesDialog.DocumentListHeader.Text");
            this.saveButton.ActionText = PdnResources.GetString2("UnsavedChangesDialog.SaveButton.ActionText");
            this.saveButton.ExplanationText = PdnResources.GetString2("UnsavedChangesDialog.SaveButton.ExplanationText");
            this.saveButton.ActionImage = PdnResources.GetImageResource2("Icons.UnsavedChangesDialog.SaveButton.png").Reference;
            this.dontSaveButton.ActionText = PdnResources.GetString2("UnsavedChangesDialog.DontSaveButton.ActionText");
            this.dontSaveButton.ExplanationText = PdnResources.GetString2("UnsavedChangesDialog.DontSaveButton.ExplanationText");
            this.dontSaveButton.ActionImage = PdnResources.GetImageResource2("Icons.MenuFileCloseIcon.png").Reference;
            this.cancelButton.ActionText = PdnResources.GetString2("UnsavedChangesDialog.CancelButton.ActionText");
            this.cancelButton.ExplanationText = PdnResources.GetString2("UnsavedChangesDialog.CancelButton.ExplanationText");
            this.cancelButton.ActionImage = PdnResources.GetImageResource2("Icons.CancelIcon.png").Reference;
            base.LoadResources();
        }

        protected virtual void OnDocumentClicked(DocumentWorkspace dw)
        {
            if (this.DocumentClicked != null)
            {
                this.DocumentClicked(this, new EventArgs<DocumentWorkspace>(dw));
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int x = UI.ScaleWidth(8);
            int num2 = UI.ScaleWidth(8);
            int num3 = UI.ScaleHeight(8);
            int num4 = UI.ScaleHeight(8);
            int num5 = UI.ScaleHeight(8);
            int num6 = UI.ScaleHeight(8);
            int num7 = UI.ScaleHeight(8);
            int num8 = UI.ScaleHeight(0);
            int width = (base.ClientSize.Width - x) - num2;
            int y = num3;
            this.infoLabel.Location = new Point(x, y);
            this.infoLabel.Width = width;
            this.infoLabel.Height = this.infoLabel.GetPreferredSize(new Size(this.infoLabel.Width, 0)).Height;
            y += this.infoLabel.Height + num5;
            this.documentListHeader.Location = new Point(x, y);
            this.documentListHeader.Width = width;
            y += this.documentListHeader.Height + num6;
            this.documentStrip.Location = new Point(x, y);
            this.documentStrip.Size = new Size(width, UI.ScaleHeight(0x48));
            this.hScrollBar.Location = new Point(x, this.documentStrip.Bottom);
            this.hScrollBar.Width = width;
            y += (this.documentStrip.Height + this.hScrollBar.Height) + num7;
            this.saveButton.Location = new Point(x, y);
            this.saveButton.Width = width;
            this.saveButton.PerformLayout();
            y += this.saveButton.Height + num8;
            this.dontSaveButton.Location = new Point(x, y);
            this.dontSaveButton.Width = width;
            this.dontSaveButton.PerformLayout();
            y += this.dontSaveButton.Height + num8;
            this.cancelButton.Location = new Point(x, y);
            this.cancelButton.Width = width;
            this.cancelButton.PerformLayout();
            y += this.cancelButton.Height + num4;
            base.ClientSize = new Size(base.ClientSize.Width, y);
            base.OnLayout(levent);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Yes;
            base.Close();
        }

        public DocumentWorkspace[] Documents
        {
            get => 
                ((DocumentWorkspace[]) this.documents.Clone());
            set
            {
                this.documents = (DocumentWorkspace[]) value.Clone();
                this.documentStrip.ClearItems();
                foreach (DocumentWorkspace workspace in this.documents)
                {
                    this.documentStrip.AddDocumentWorkspace(workspace);
                }
                this.hScrollBar.Maximum = this.documentStrip.ViewRectangle.Width;
                this.hScrollBar.LargeChange = this.documentStrip.ClientSize.Width;
                if (this.documentStrip.ClientRectangle.Width > this.documentStrip.ViewRectangle.Width)
                {
                    this.hScrollBar.Enabled = false;
                }
                else
                {
                    this.hScrollBar.Enabled = true;
                }
                foreach (ImageStrip.Item item in this.documentStrip.Items)
                {
                    item.Checked = false;
                }
            }
        }

        public DocumentWorkspace SelectedDocument
        {
            get => 
                this.documentStrip.SelectedDocument;
            set
            {
                this.documentStrip.SelectDocumentWorkspace(value);
            }
        }
    }
}

