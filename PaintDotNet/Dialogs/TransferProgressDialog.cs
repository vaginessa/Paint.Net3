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

    internal class TransferProgressDialog : PdnBaseForm
    {
        private Button cancelButton;
        private IContainer components;
        private Label itemText;
        private Label operationProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private PaintDotNet.Controls.HeadingLabel separator1;

        public event EventHandler CancelClicked;

        public TransferProgressDialog()
        {
            Func<Keys, bool> callback = null;
            if (callback == null)
            {
                callback = delegate (Keys keys) {
                    this.OnCancelClicked();
                    return true;
                };
            }
            PdnBaseForm.RegisterFormHotKey(Keys.Escape, callback);
            this.InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.OnCancelClicked();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.cancelButton = new Button();
            this.itemText = new Label();
            this.separator1 = new PaintDotNet.Controls.HeadingLabel();
            this.operationProgress = new Label();
            base.SuspendLayout();
            this.progressBar.Location = new Point(10, 0x33);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(0x195, 0x13);
            this.progressBar.TabIndex = 0;
            this.cancelButton.Location = new Point(0x156, 0x5b);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(0x4b, 0x17);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.FlatStyle = FlatStyle.System;
            this.cancelButton.Click += new EventHandler(this.CancelButton_Click);
            this.itemText.AutoEllipsis = true;
            this.itemText.Location = new Point(8, 8);
            this.itemText.Name = "itemText";
            this.itemText.Size = new Size(0x194, 13);
            this.itemText.TabIndex = 2;
            this.itemText.Text = "itemText";
            this.separator1.Location = new Point(9, 0x4d);
            this.separator1.Name = "separator1";
            this.separator1.RightMargin = 0;
            this.separator1.Size = new Size(0x196, 14);
            this.separator1.TabIndex = 4;
            this.separator1.TabStop = false;
            this.operationProgress.AutoEllipsis = true;
            this.operationProgress.Location = new Point(8, 0x1c);
            this.operationProgress.Name = "operationProgress";
            this.operationProgress.Size = new Size(0x193, 13);
            this.operationProgress.TabIndex = 5;
            this.operationProgress.Text = "operationProgress";
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0x1a7, 0x79);
            base.Controls.Add(this.operationProgress);
            base.Controls.Add(this.progressBar);
            base.Controls.Add(this.itemText);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.separator1);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.Location = new Point(0, 0);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "TransferProgressDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "TransferProgressDialog";
            base.Controls.SetChildIndex(this.separator1, 0);
            base.Controls.SetChildIndex(this.cancelButton, 0);
            base.Controls.SetChildIndex(this.itemText, 0);
            base.Controls.SetChildIndex(this.progressBar, 0);
            base.Controls.SetChildIndex(this.operationProgress, 0);
            base.ResumeLayout(false);
        }

        public override void LoadResources()
        {
            this.cancelButton.Text = PdnResources.GetString2("Form.CancelButton.Text");
            base.LoadResources();
        }

        protected virtual void OnCancelClicked()
        {
            if (this.CancelClicked != null)
            {
                this.CancelClicked(this, EventArgs.Empty);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            UI.EnableCloseBox(this, false);
            base.OnLoad(e);
        }

        public bool CancelEnabled
        {
            get => 
                this.cancelButton.Enabled;
            set
            {
                this.cancelButton.Enabled = value;
            }
        }

        public string ItemText
        {
            get => 
                this.itemText.Text;
            set
            {
                this.itemText.Text = value;
            }
        }

        public string OperationProgress
        {
            get => 
                this.operationProgress.Text;
            set
            {
                this.operationProgress.Text = value;
            }
        }

        public System.Windows.Forms.ProgressBar ProgressBar =>
            this.progressBar;
    }
}

