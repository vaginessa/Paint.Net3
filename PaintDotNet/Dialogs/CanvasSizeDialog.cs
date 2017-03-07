namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class CanvasSizeDialog : ResizeDialog
    {
        private AnchorChooserControl anchorChooserControl;
        private ComboBox anchorEdgeCB;
        private EnumLocalizer anchorEdgeNames = EnumLocalizer.Create(typeof(PaintDotNet.AnchorEdge));
        private PaintDotNet.Controls.HeadingLabel anchorHeader;
        private IContainer components;
        private Label newSpaceLabel;

        public CanvasSizeDialog()
        {
            this.InitializeComponent();
            base.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuImageCanvasSizeIcon.png").Reference);
            this.Text = PdnResources.GetString2("CanvasSizeDialog.Text");
            this.anchorHeader.Text = PdnResources.GetString2("CanvasSizeDialog.AnchorHeader.Text");
            this.newSpaceLabel.Text = PdnResources.GetString2("CanvasSizeDialog.NewSpaceLabel.Text");
            foreach (LocalizedEnumValue value2 in this.anchorEdgeNames.GetLocalizedEnumValues())
            {
                PaintDotNet.AnchorEdge enumValue = (PaintDotNet.AnchorEdge) value2.EnumValue;
                this.anchorEdgeCB.Items.Add(value2);
                if (enumValue == this.AnchorEdge)
                {
                    this.anchorEdgeCB.SelectedItem = value2;
                }
            }
            this.anchorChooserControl_AnchorEdgeChanged(this.anchorChooserControl, EventArgs.Empty);
        }

        private void anchorChooserControl_AnchorEdgeChanged(object sender, EventArgs e)
        {
            LocalizedEnumValue localizedEnumValue = this.anchorEdgeNames.GetLocalizedEnumValue(this.anchorChooserControl.AnchorEdge);
            this.anchorEdgeCB.SelectedItem = localizedEnumValue;
        }

        private void anchorEdgeCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            LocalizedEnumValue selectedItem = (LocalizedEnumValue) this.anchorEdgeCB.SelectedItem;
            this.AnchorEdge = (PaintDotNet.AnchorEdge) selectedItem.EnumValue;
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
            this.anchorChooserControl = new AnchorChooserControl();
            this.newSpaceLabel = new Label();
            this.anchorHeader = new PaintDotNet.Controls.HeadingLabel();
            this.anchorEdgeCB = new ComboBox();
            base.percentUpDown.BeginInit();
            base.resolutionUpDown.BeginInit();
            base.pixelWidthUpDown.BeginInit();
            base.pixelHeightUpDown.BeginInit();
            base.printWidthUpDown.BeginInit();
            base.printHeightUpDown.BeginInit();
            base.SuspendLayout();
            base.constrainCheckBox.Location = new Point(0x1b, 0x4a);
            base.constrainCheckBox.Name = "constrainCheckBox";
            base.okButton.Location = new Point(0x8e, 0x16e);
            base.okButton.Name = "okButton";
            base.okButton.TabIndex = 0x12;
            base.cancelButton.Location = new Point(220, 0x16e);
            base.cancelButton.Name = "cancelButton";
            base.cancelButton.TabIndex = 0x13;
            base.percentSignLabel.Location = new Point(200, 0x1c);
            base.percentSignLabel.Name = "percentSignLabel";
            base.percentSignLabel.TabIndex = 0x17;
            base.percentUpDown.Location = new Point(120, 0x1b);
            base.percentUpDown.Name = "percentUpDown";
            base.percentUpDown.TabIndex = 0x16;
            base.absoluteRB.FlatStyle = FlatStyle.System;
            base.absoluteRB.Location = new Point(8, 0x33);
            base.absoluteRB.Name = "absoluteRB";
            base.percentRB.FlatStyle = FlatStyle.System;
            base.percentRB.Location = new Point(8, 0x18);
            base.percentRB.Name = "percentRB";
            base.percentRB.TabIndex = 0x15;
            base.asteriskTextLabel.Enabled = false;
            base.asteriskTextLabel.Location = new Point(400, 0x48);
            base.asteriskTextLabel.Name = "asteriskTextLabel";
            base.asteriskTextLabel.Visible = true;
            base.asteriskLabel.Enabled = false;
            base.asteriskLabel.Location = new Point(0x288, 0x20);
            base.asteriskLabel.Name = "asteriskLabel";
            base.asteriskLabel.Visible = true;
            base.resizedImageHeader.Name = "resizedImageHeader";
            base.resizedImageHeader.TabIndex = 20;
            base.resolutionLabel.Location = new Point(0x20, 0xa6);
            base.resolutionLabel.Name = "resolutionLabel";
            base.unitsComboBox2.Location = new Point(200, 0xa5);
            base.unitsComboBox2.Name = "unitsComboBox2";
            base.unitsComboBox1.Location = new Point(200, 0xd0);
            base.unitsComboBox1.Name = "unitsComboBox1";
            base.resolutionUpDown.Location = new Point(120, 0xa5);
            base.resolutionUpDown.Name = "resolutionUpDown";
            base.newWidthLabel1.Location = new Point(0x20, 0x76);
            base.newWidthLabel1.Name = "newWidthLabel1";
            base.newHeightLabel1.Location = new Point(0x20, 0x8e);
            base.newHeightLabel1.Name = "newHeightLabel1";
            base.pixelsLabel1.Location = new Point(200, 0x76);
            base.pixelsLabel1.Name = "pixelsLabel1";
            base.newWidthLabel2.Location = new Point(0x20, 0xd1);
            base.newWidthLabel2.Name = "newWidthLabel2";
            base.newHeightLabel2.Location = new Point(0x20, 0xe9);
            base.newHeightLabel2.Name = "newHeightLabel2";
            base.pixelsLabel2.Location = new Point(200, 0x8e);
            base.pixelsLabel2.Name = "pixelsLabel2";
            base.unitsLabel1.Location = new Point(200, 0xea);
            base.unitsLabel1.Name = "unitsLabel1";
            base.pixelWidthUpDown.Location = new Point(120, 0x75);
            base.pixelWidthUpDown.Name = "pixelWidthUpDown";
            base.pixelHeightUpDown.Location = new Point(120, 0x8d);
            base.pixelHeightUpDown.Name = "pixelHeightUpDown";
            base.printWidthUpDown.Location = new Point(120, 0xd0);
            base.printWidthUpDown.Name = "printWidthUpDown";
            base.printHeightUpDown.Location = new Point(120, 0xe8);
            base.printHeightUpDown.Name = "printHeightUpDown";
            base.pixelSizeHeader.Location = new Point(0x19, 0x62);
            base.pixelSizeHeader.Name = "pixelSizeHeader";
            base.printSizeHeader.Location = new Point(0x19, 0xbd);
            base.printSizeHeader.Name = "printSizeHeader";
            base.resamplingLabel.Enabled = false;
            base.resamplingLabel.Location = new Point(0x180, 40);
            base.resamplingLabel.Name = "resamplingLabel";
            base.resamplingLabel.Visible = false;
            base.resamplingAlgorithmComboBox.Enabled = false;
            base.resamplingAlgorithmComboBox.Location = new Point(0x1f0, 0x20);
            base.resamplingAlgorithmComboBox.Name = "resamplingAlgorithmComboBox";
            base.resamplingAlgorithmComboBox.Visible = false;
            this.anchorChooserControl.Location = new Point(0xb1, 0x113);
            this.anchorChooserControl.Name = "anchorChooserControl";
            this.anchorChooserControl.Size = new Size(0x51, 0x51);
            this.anchorChooserControl.TabIndex = 0x11;
            this.anchorChooserControl.TabStop = false;
            this.anchorChooserControl.AnchorEdgeChanged += new EventHandler(this.anchorChooserControl_AnchorEdgeChanged);
            this.newSpaceLabel.Location = new Point(0x178, 0x128);
            this.newSpaceLabel.Name = "newSpaceLabel";
            this.newSpaceLabel.Size = new Size(0xea, 0x20);
            this.newSpaceLabel.TabIndex = 20;
            this.anchorHeader.Location = new Point(8, 0x100);
            this.anchorHeader.Name = "anchorHeader";
            this.anchorHeader.Size = new Size(0x120, 14);
            this.anchorHeader.TabIndex = 15;
            this.anchorHeader.TabStop = false;
            this.anchorEdgeCB.DropDownStyle = ComboBoxStyle.DropDownList;
            this.anchorEdgeCB.Location = new Point(0x20, 0x113);
            this.anchorEdgeCB.Name = "anchorEdgeCB";
            this.anchorEdgeCB.Size = new Size(120, 0x15);
            this.anchorEdgeCB.TabIndex = 0x10;
            this.anchorEdgeCB.SelectedIndexChanged += new EventHandler(this.anchorEdgeCB_SelectedIndexChanged);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0x12a, 0x196);
            base.Controls.Add(this.anchorEdgeCB);
            base.Controls.Add(this.anchorHeader);
            base.Controls.Add(this.anchorChooserControl);
            base.Controls.Add(this.newSpaceLabel);
            base.Location = new Point(0, 0);
            base.Name = "CanvasSizeDialog";
            base.Controls.SetChildIndex(base.pixelsLabel1, 0);
            base.Controls.SetChildIndex(base.unitsLabel1, 0);
            base.Controls.SetChildIndex(base.newWidthLabel1, 0);
            base.Controls.SetChildIndex(base.resamplingLabel, 0);
            base.Controls.SetChildIndex(base.resolutionLabel, 0);
            base.Controls.SetChildIndex(base.asteriskTextLabel, 0);
            base.Controls.SetChildIndex(base.asteriskLabel, 0);
            base.Controls.SetChildIndex(base.pixelsLabel2, 0);
            base.Controls.SetChildIndex(base.percentSignLabel, 0);
            base.Controls.SetChildIndex(this.newSpaceLabel, 0);
            base.Controls.SetChildIndex(base.newHeightLabel1, 0);
            base.Controls.SetChildIndex(base.newWidthLabel2, 0);
            base.Controls.SetChildIndex(base.newHeightLabel2, 0);
            base.Controls.SetChildIndex(base.resizedImageHeader, 0);
            base.Controls.SetChildIndex(base.resolutionUpDown, 0);
            base.Controls.SetChildIndex(base.unitsComboBox2, 0);
            base.Controls.SetChildIndex(base.unitsComboBox1, 0);
            base.Controls.SetChildIndex(base.printWidthUpDown, 0);
            base.Controls.SetChildIndex(base.printHeightUpDown, 0);
            base.Controls.SetChildIndex(base.pixelSizeHeader, 0);
            base.Controls.SetChildIndex(base.printSizeHeader, 0);
            base.Controls.SetChildIndex(base.pixelHeightUpDown, 0);
            base.Controls.SetChildIndex(base.pixelWidthUpDown, 0);
            base.Controls.SetChildIndex(this.anchorChooserControl, 0);
            base.Controls.SetChildIndex(base.constrainCheckBox, 0);
            base.Controls.SetChildIndex(base.resamplingAlgorithmComboBox, 0);
            base.Controls.SetChildIndex(base.percentRB, 0);
            base.Controls.SetChildIndex(base.absoluteRB, 0);
            base.Controls.SetChildIndex(base.percentUpDown, 0);
            base.Controls.SetChildIndex(this.anchorHeader, 0);
            base.Controls.SetChildIndex(this.anchorEdgeCB, 0);
            base.Controls.SetChildIndex(base.okButton, 0);
            base.Controls.SetChildIndex(base.cancelButton, 0);
            base.percentUpDown.EndInit();
            base.resolutionUpDown.EndInit();
            base.pixelWidthUpDown.EndInit();
            base.pixelHeightUpDown.EndInit();
            base.printWidthUpDown.EndInit();
            base.printHeightUpDown.EndInit();
            base.ResumeLayout(false);
        }

        [DefaultValue(0)]
        public PaintDotNet.AnchorEdge AnchorEdge
        {
            get => 
                this.anchorChooserControl.AnchorEdge;
            set
            {
                this.anchorChooserControl.AnchorEdge = value;
            }
        }
    }
}

