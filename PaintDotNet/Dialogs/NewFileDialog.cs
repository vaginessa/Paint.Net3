namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class NewFileDialog : ResizeDialog
    {
        private Container components;

        public NewFileDialog()
        {
            this.InitializeComponent();
            base.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuFileNewIcon.png").Reference, Utility.TransparentKey);
            this.Text = PdnResources.GetString2("NewFileDialog.Text");
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
            base.percentUpDown.BeginInit();
            base.resolutionUpDown.BeginInit();
            base.pixelWidthUpDown.BeginInit();
            base.pixelHeightUpDown.BeginInit();
            base.printWidthUpDown.BeginInit();
            base.printHeightUpDown.BeginInit();
            base.SuspendLayout();
            base.constrainCheckBox.Location = new Point(8, 0x1c);
            base.constrainCheckBox.Name = "constrainCheckBox";
            base.okButton.Location = new Point(0x7b, 0xd9);
            base.okButton.Name = "okButton";
            base.cancelButton.Location = new Point(0xc9, 0xd9);
            base.cancelButton.Name = "cancelButton";
            base.percentSignLabel.Enabled = false;
            base.percentSignLabel.Location = new Point(520, 0x30);
            base.percentSignLabel.Name = "percentSignLabel";
            base.percentSignLabel.Visible = false;
            base.percentUpDown.Location = new Point(440, 0x30);
            base.percentUpDown.Name = "percentUpDown";
            base.percentUpDown.Visible = false;
            base.absoluteRB.Enabled = false;
            base.absoluteRB.FlatStyle = FlatStyle.System;
            base.absoluteRB.Location = new Point(0x148, 0x48);
            base.absoluteRB.Name = "absoluteRB";
            base.absoluteRB.Visible = false;
            base.percentRB.Enabled = false;
            base.percentRB.FlatStyle = FlatStyle.System;
            base.percentRB.Location = new Point(0x148, 40);
            base.percentRB.Name = "percentRB";
            base.percentRB.Visible = false;
            base.asteriskTextLabel.Enabled = false;
            base.asteriskTextLabel.Location = new Point(0x148, 280);
            base.asteriskTextLabel.Name = "asteriskTextLabel";
            base.asteriskLabel.Enabled = false;
            base.asteriskLabel.Location = new Point(0x250, 0x10);
            base.asteriskLabel.Name = "asteriskLabel";
            base.resizedImageHeader.Name = "resizedImageHeader";
            base.resizedImageHeader.Size = new Size(0x112, 0x10);
            base.resolutionLabel.Location = new Point(0x10, 0x76);
            base.resolutionLabel.Name = "resolutionLabel";
            base.unitsComboBox2.Location = new Point(0xb8, 0x75);
            base.unitsComboBox2.Name = "unitsComboBox2";
            base.unitsComboBox1.Location = new Point(0xb8, 160);
            base.unitsComboBox1.Name = "unitsComboBox1";
            base.resolutionUpDown.Location = new Point(0x68, 0x75);
            base.resolutionUpDown.Name = "resolutionUpDown";
            base.newWidthLabel1.Location = new Point(0x10, 70);
            base.newWidthLabel1.Name = "newWidthLabel1";
            base.newHeightLabel1.Location = new Point(0x10, 0x5e);
            base.newHeightLabel1.Name = "newHeightLabel1";
            base.pixelsLabel1.Location = new Point(0xb8, 70);
            base.pixelsLabel1.Name = "pixelsLabel1";
            base.newWidthLabel2.Location = new Point(0x10, 0xa1);
            base.newWidthLabel2.Name = "newWidthLabel2";
            base.newHeightLabel2.Location = new Point(0x10, 0xb9);
            base.newHeightLabel2.Name = "newHeightLabel2";
            base.pixelsLabel2.Location = new Point(0xb8, 0x5e);
            base.pixelsLabel2.Name = "pixelsLabel2";
            base.unitsLabel1.Location = new Point(0xb8, 0xba);
            base.unitsLabel1.Name = "unitsLabel1";
            base.pixelWidthUpDown.Location = new Point(0x68, 0x45);
            base.pixelWidthUpDown.Name = "pixelWidthUpDown";
            base.pixelHeightUpDown.Location = new Point(0x68, 0x5d);
            base.pixelHeightUpDown.Name = "pixelHeightUpDown";
            base.printWidthUpDown.Location = new Point(0x68, 160);
            base.printWidthUpDown.Name = "printWidthUpDown";
            base.printHeightUpDown.Location = new Point(0x68, 0xb8);
            base.printHeightUpDown.Name = "printHeightUpDown";
            base.pixelSizeHeader.Location = new Point(6, 50);
            base.pixelSizeHeader.Name = "pixelSizeHeader";
            base.printSizeHeader.Location = new Point(6, 0x8d);
            base.printSizeHeader.Name = "printSizeHeader";
            base.resamplingLabel.Enabled = false;
            base.resamplingLabel.Location = new Point(320, 0x18);
            base.resamplingLabel.Name = "resamplingLabel";
            base.resamplingLabel.Visible = false;
            base.resamplingAlgorithmComboBox.Enabled = false;
            base.resamplingAlgorithmComboBox.Location = new Point(440, 0x10);
            base.resamplingAlgorithmComboBox.Name = "resamplingAlgorithmComboBox";
            base.resamplingAlgorithmComboBox.Visible = false;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0x117, 0x100);
            base.Location = new Point(0, 0);
            base.Name = "NewFileDialog";
            base.Controls.SetChildIndex(base.printWidthUpDown, 0);
            base.Controls.SetChildIndex(base.printHeightUpDown, 0);
            base.percentUpDown.EndInit();
            base.resolutionUpDown.EndInit();
            base.pixelWidthUpDown.EndInit();
            base.pixelHeightUpDown.EndInit();
            base.printWidthUpDown.EndInit();
            base.printHeightUpDown.EndInit();
            base.ResumeLayout(false);
        }
    }
}

