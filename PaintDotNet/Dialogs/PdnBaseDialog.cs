namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class PdnBaseDialog : PdnBaseForm
    {
        protected Button baseCancelButton;
        protected Button baseOkButton;
        private IContainer components;

        public PdnBaseDialog()
        {
            this.InitializeComponent();
            if (!base.DesignMode)
            {
                this.baseOkButton.Text = PdnResources.GetString2("Form.OkButton.Text");
                this.baseCancelButton.Text = PdnResources.GetString2("Form.CancelButton.Text");
            }
        }

        private void baseCancelButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void baseOkButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
            base.Close();
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

        private void InitializeComponent()
        {
            this.baseOkButton = new Button();
            this.baseCancelButton = new Button();
            base.SuspendLayout();
            this.baseOkButton.Location = new Point(0x4d, 0x80);
            this.baseOkButton.Name = "baseOkButton";
            this.baseOkButton.TabIndex = 1;
            this.baseOkButton.FlatStyle = FlatStyle.System;
            this.baseOkButton.Click += new EventHandler(this.baseOkButton_Click);
            this.baseCancelButton.DialogResult = DialogResult.Cancel;
            this.baseCancelButton.Location = new Point(0xa5, 0x80);
            this.baseCancelButton.Name = "baseCancelButton";
            this.baseCancelButton.TabIndex = 2;
            this.baseCancelButton.FlatStyle = FlatStyle.System;
            this.baseCancelButton.Click += new EventHandler(this.baseCancelButton_Click);
            base.AcceptButton = this.baseOkButton;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.baseCancelButton;
            base.ClientSize = new Size(0xf8, 0x9e);
            base.Controls.Add(this.baseCancelButton);
            base.Controls.Add(this.baseOkButton);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.MinimizeBox = false;
            base.Name = "PdnBaseDialog";
            base.ShowInTaskbar = false;
            this.Text = "PdnBaseDialog";
            base.Controls.SetChildIndex(this.baseOkButton, 0);
            base.Controls.SetChildIndex(this.baseCancelButton, 0);
            base.ResumeLayout(false);
        }
    }
}

