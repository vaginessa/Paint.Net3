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

    internal class ResizeDialog : PdnBaseForm
    {
        protected RadioButton absoluteRB;
        protected Label asteriskLabel;
        protected Label asteriskTextLabel;
        protected Button cancelButton;
        private Container components;
        protected CheckBox constrainCheckBox;
        private ResizeConstrainer constrainer;
        private int getValueFromText;
        private int ignoreUpDownValueChanged;
        private int layers;
        protected Label newHeightLabel1;
        protected Label newHeightLabel2;
        protected Label newWidthLabel1;
        protected Label newWidthLabel2;
        protected Button okButton;
        private System.Windows.Forms.Timer okTimer;
        private int okTimerInterval = 200;
        private double originalDpu = Document.GetDefaultDpu(Document.DefaultDpuUnit);
        private MeasurementUnit originalDpuUnit = Document.DefaultDpuUnit;
        protected RadioButton percentRB;
        protected Label percentSignLabel;
        protected NumericUpDown percentUpDown;
        protected NumericUpDown pixelHeightUpDown;
        protected PaintDotNet.Controls.HeadingLabel pixelSizeHeader;
        protected Label pixelsLabel1;
        protected Label pixelsLabel2;
        protected NumericUpDown pixelWidthUpDown;
        protected NumericUpDown printHeightUpDown;
        protected PaintDotNet.Controls.HeadingLabel printSizeHeader;
        protected NumericUpDown printWidthUpDown;
        protected ComboBox resamplingAlgorithmComboBox;
        protected Label resamplingLabel;
        protected PaintDotNet.Controls.HeadingLabel resizedImageHeader;
        protected Label resolutionLabel;
        protected NumericUpDown resolutionUpDown;
        protected PaintDotNet.Controls.SeparatorLine separatorLine;
        protected UnitsComboBox unitsComboBox1;
        protected UnitsComboBox unitsComboBox2;
        protected Label unitsLabel1;
        private EventHandler upDownValueChangedDelegate;

        public ResizeDialog()
        {
            base.SuspendLayout();
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = true;
            this.DoubleBuffered = true;
            this.InitializeComponent();
            this.Text = PdnResources.GetString2("ResizeDialog.Text");
            this.asteriskLabel.Text = PdnResources.GetString2("ResizeDialog.AsteriskLabel.Text");
            this.percentSignLabel.Text = PdnResources.GetString2("ResizeDialog.PercentSignLabel.Text");
            this.pixelSizeHeader.Text = PdnResources.GetString2("ResizeDialog.PixelSizeHeader.Text");
            this.printSizeHeader.Text = PdnResources.GetString2("ResizeDialog.PrintSizeHeader.Text");
            this.pixelsLabel1.Text = PdnResources.GetString2("ResizeDialog.PixelsLabel1.Text");
            this.pixelsLabel2.Text = PdnResources.GetString2("ResizeDialog.PixelsLabel2.Text");
            this.resolutionLabel.Text = PdnResources.GetString2("ResizeDialog.ResolutionLabel.Text");
            this.percentRB.Text = PdnResources.GetString2("ResizeDialog.PercentRB.Text");
            this.absoluteRB.Text = PdnResources.GetString2("ResizeDialog.AbsoluteRB.Text");
            this.resamplingLabel.Text = PdnResources.GetString2("ResizeDialog.ResamplingLabel.Text");
            this.cancelButton.Text = PdnResources.GetString2("Form.CancelButton.Text");
            this.okButton.Text = PdnResources.GetString2("Form.OkButton.Text");
            this.newWidthLabel1.Text = PdnResources.GetString2("ResizeDialog.NewWidthLabel1.Text");
            this.newHeightLabel1.Text = PdnResources.GetString2("ResizeDialog.NewHeightLabel1.Text");
            this.newWidthLabel2.Text = PdnResources.GetString2("ResizeDialog.NewWidthLabel1.Text");
            this.newHeightLabel2.Text = PdnResources.GetString2("ResizeDialog.NewHeightLabel1.Text");
            this.constrainCheckBox.Text = PdnResources.GetString2("ResizeDialog.ConstrainCheckBox.Text");
            this.unitsLabel1.Text = this.unitsComboBox1.UnitsText;
            this.upDownValueChangedDelegate = new EventHandler(this.upDown_ValueChanged);
            this.constrainer = new ResizeConstrainer(new Size((int) this.pixelWidthUpDown.Value, (int) this.pixelHeightUpDown.Value));
            this.SetupConstrainerEvents();
            this.resamplingAlgorithmComboBox.Items.Clear();
            this.resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(PaintDotNet.ResamplingAlgorithm.Bicubic));
            this.resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(PaintDotNet.ResamplingAlgorithm.Bilinear));
            this.resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(PaintDotNet.ResamplingAlgorithm.NearestNeighbor));
            this.resamplingAlgorithmComboBox.Items.Add(new ResampleMethod(PaintDotNet.ResamplingAlgorithm.SuperSampling));
            this.resamplingAlgorithmComboBox.SelectedItem = new ResampleMethod(PaintDotNet.ResamplingAlgorithm.SuperSampling);
            this.layers = 1;
            this.percentUpDown.Enabled = false;
            base.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuImageResizeIcon.png").Reference, Utility.TransparentKey);
            this.PopulateAsteriskLabels();
            this.OnRadioButtonCheckedChanged(this, EventArgs.Empty);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void constrainCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.constrainer.ConstrainToAspect = this.constrainCheckBox.Checked;
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
            this.constrainCheckBox = new CheckBox();
            this.newWidthLabel1 = new Label();
            this.newHeightLabel1 = new Label();
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.pixelWidthUpDown = new NumericUpDown();
            this.pixelHeightUpDown = new NumericUpDown();
            this.resizedImageHeader = new PaintDotNet.Controls.HeadingLabel();
            this.asteriskLabel = new Label();
            this.asteriskTextLabel = new Label();
            this.absoluteRB = new RadioButton();
            this.percentRB = new RadioButton();
            this.pixelsLabel1 = new Label();
            this.percentUpDown = new NumericUpDown();
            this.percentSignLabel = new Label();
            this.resolutionLabel = new Label();
            this.resolutionUpDown = new NumericUpDown();
            this.unitsComboBox2 = new UnitsComboBox();
            this.unitsComboBox1 = new UnitsComboBox();
            this.printWidthUpDown = new NumericUpDown();
            this.printHeightUpDown = new NumericUpDown();
            this.newWidthLabel2 = new Label();
            this.newHeightLabel2 = new Label();
            this.pixelsLabel2 = new Label();
            this.unitsLabel1 = new Label();
            this.pixelSizeHeader = new PaintDotNet.Controls.HeadingLabel();
            this.printSizeHeader = new PaintDotNet.Controls.HeadingLabel();
            this.resamplingLabel = new Label();
            this.resamplingAlgorithmComboBox = new ComboBox();
            this.separatorLine = new PaintDotNet.Controls.SeparatorLine();
            this.pixelWidthUpDown.BeginInit();
            this.pixelHeightUpDown.BeginInit();
            this.percentUpDown.BeginInit();
            this.resolutionUpDown.BeginInit();
            this.printWidthUpDown.BeginInit();
            this.printHeightUpDown.BeginInit();
            base.SuspendLayout();
            this.constrainCheckBox.FlatStyle = FlatStyle.System;
            this.constrainCheckBox.Location = new Point(0x1b, 0x65);
            this.constrainCheckBox.Name = "constrainCheckBox";
            this.constrainCheckBox.Size = new Size(0xf8, 0x10);
            this.constrainCheckBox.TabIndex = 0x19;
            this.constrainCheckBox.CheckedChanged += new EventHandler(this.constrainCheckBox_CheckedChanged);
            this.newWidthLabel1.Location = new Point(0x20, 0x91);
            this.newWidthLabel1.Name = "newWidthLabel1";
            this.newWidthLabel1.Size = new Size(0x4f, 0x10);
            this.newWidthLabel1.TabIndex = 0;
            this.newWidthLabel1.TextAlign = ContentAlignment.MiddleLeft;
            this.newHeightLabel1.Location = new Point(0x20, 0xa9);
            this.newHeightLabel1.Name = "newHeightLabel1";
            this.newHeightLabel1.Size = new Size(0x4f, 0x10);
            this.newHeightLabel1.TabIndex = 3;
            this.newHeightLabel1.TextAlign = ContentAlignment.MiddleLeft;
            this.okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.okButton.AutoSize = true;
            this.okButton.FlatStyle = FlatStyle.System;
            this.okButton.Location = new Point(0x8e, 0x13b);
            this.okButton.Name = "okButton";
            this.okButton.Size = new Size(0x48, 0x17);
            this.okButton.TabIndex = 0x11;
            this.okButton.Click += new EventHandler(this.okButton_Click);
            this.cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.cancelButton.AutoSize = true;
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.FlatStyle = FlatStyle.System;
            this.cancelButton.Location = new Point(220, 0x13b);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(0x48, 0x17);
            this.cancelButton.TabIndex = 0x12;
            this.cancelButton.Click += new EventHandler(this.cancelButton_Click);
            this.pixelWidthUpDown.Location = new Point(120, 0x90);
            int[] bits = new int[4];
            bits[0] = 0x7fffffff;
            this.pixelWidthUpDown.Maximum = new decimal(bits);
            int[] numArray2 = new int[4];
            numArray2[0] = 0x7fffffff;
            numArray2[3] = -2147483648;
            this.pixelWidthUpDown.Minimum = new decimal(numArray2);
            this.pixelWidthUpDown.Name = "pixelWidthUpDown";
            this.pixelWidthUpDown.Size = new Size(0x48, 20);
            this.pixelWidthUpDown.TabIndex = 1;
            this.pixelWidthUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray3 = new int[4];
            numArray3[0] = 4;
            this.pixelWidthUpDown.Value = new decimal(numArray3);
            this.pixelWidthUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.pixelWidthUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.pixelWidthUpDown.ValueChanged += new EventHandler(this.upDown_ValueChanged);
            this.pixelWidthUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.pixelHeightUpDown.Location = new Point(120, 0xa8);
            int[] numArray4 = new int[4];
            numArray4[0] = 0x7fffffff;
            this.pixelHeightUpDown.Maximum = new decimal(numArray4);
            int[] numArray5 = new int[4];
            numArray5[0] = 0x7fffffff;
            numArray5[3] = -2147483648;
            this.pixelHeightUpDown.Minimum = new decimal(numArray5);
            this.pixelHeightUpDown.Name = "pixelHeightUpDown";
            this.pixelHeightUpDown.Size = new Size(0x48, 20);
            this.pixelHeightUpDown.TabIndex = 4;
            this.pixelHeightUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray6 = new int[4];
            numArray6[0] = 3;
            this.pixelHeightUpDown.Value = new decimal(numArray6);
            this.pixelHeightUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.pixelHeightUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.pixelHeightUpDown.ValueChanged += new EventHandler(this.upDown_ValueChanged);
            this.pixelHeightUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.resizedImageHeader.Location = new Point(6, 8);
            this.resizedImageHeader.Name = "resizedImageHeader";
            this.resizedImageHeader.Size = new Size(290, 0x10);
            this.resizedImageHeader.TabIndex = 0x13;
            this.resizedImageHeader.TabStop = false;
            this.asteriskLabel.Location = new Point(0x113, 0x1c);
            this.asteriskLabel.Name = "asteriskLabel";
            this.asteriskLabel.Size = new Size(13, 0x10);
            this.asteriskLabel.TabIndex = 15;
            this.asteriskLabel.Visible = false;
            this.asteriskTextLabel.Location = new Point(8, 290);
            this.asteriskTextLabel.Name = "asteriskTextLabel";
            this.asteriskTextLabel.Size = new Size(0xff, 0x10);
            this.asteriskTextLabel.TabIndex = 0x10;
            this.asteriskTextLabel.Visible = false;
            this.absoluteRB.Checked = true;
            this.absoluteRB.Location = new Point(8, 0x4e);
            this.absoluteRB.Name = "absoluteRB";
            this.absoluteRB.Width = 0x108;
            this.absoluteRB.AutoSize = true;
            this.absoluteRB.TabIndex = 0x18;
            this.absoluteRB.TabStop = true;
            this.absoluteRB.FlatStyle = FlatStyle.System;
            this.absoluteRB.CheckedChanged += new EventHandler(this.OnRadioButtonCheckedChanged);
            this.percentRB.Location = new Point(8, 0x33);
            this.percentRB.Name = "percentRB";
            this.percentRB.TabIndex = 0x16;
            this.percentRB.AutoSize = true;
            this.percentRB.Width = 10;
            this.percentRB.FlatStyle = FlatStyle.System;
            this.percentRB.CheckedChanged += new EventHandler(this.OnRadioButtonCheckedChanged);
            this.pixelsLabel1.Location = new Point(200, 0x91);
            this.pixelsLabel1.Name = "pixelsLabel1";
            this.pixelsLabel1.Width = 0x5d;
            this.pixelsLabel1.TabIndex = 2;
            this.pixelsLabel1.TextAlign = ContentAlignment.MiddleLeft;
            this.percentUpDown.Location = new Point(120, 0x36);
            int[] numArray7 = new int[4];
            numArray7[0] = 0x7d0;
            this.percentUpDown.Maximum = new decimal(numArray7);
            this.percentUpDown.Name = "percentUpDown";
            this.percentUpDown.Size = new Size(0x48, 20);
            this.percentUpDown.TabIndex = 0x17;
            this.percentUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray8 = new int[4];
            numArray8[0] = 100;
            this.percentUpDown.Value = new decimal(numArray8);
            this.percentUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.percentUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.percentUpDown.ValueChanged += new EventHandler(this.upDown_ValueChanged);
            this.percentSignLabel.Location = new Point(200, 0x37);
            this.percentSignLabel.Name = "percentSignLabel";
            this.percentSignLabel.Size = new Size(0x20, 0x10);
            this.percentSignLabel.TabIndex = 13;
            this.percentSignLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.resolutionLabel.Location = new Point(0x20, 0xc1);
            this.resolutionLabel.Name = "resolutionLabel";
            this.resolutionLabel.Size = new Size(0x4f, 0x10);
            this.resolutionLabel.TabIndex = 6;
            this.resolutionLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.resolutionUpDown.DecimalPlaces = 2;
            this.resolutionUpDown.Location = new Point(120, 0xc0);
            int[] numArray9 = new int[4];
            numArray9[0] = 0xffff;
            this.resolutionUpDown.Maximum = new decimal(numArray9);
            int[] numArray10 = new int[4];
            numArray10[0] = 1;
            numArray10[3] = 0x50000;
            this.resolutionUpDown.Minimum = new decimal(numArray10);
            this.resolutionUpDown.Name = "resolutionUpDown";
            this.resolutionUpDown.Size = new Size(0x48, 20);
            this.resolutionUpDown.TabIndex = 7;
            this.resolutionUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray11 = new int[4];
            numArray11[0] = 0x48;
            this.resolutionUpDown.Value = new decimal(numArray11);
            this.resolutionUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.resolutionUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.resolutionUpDown.ValueChanged += new EventHandler(this.upDown_ValueChanged);
            this.resolutionUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.unitsComboBox2.Location = new Point(200, 0xc0);
            this.unitsComboBox2.Name = "unitsComboBox2";
            this.unitsComboBox2.PixelsAvailable = false;
            this.unitsComboBox2.Size = new Size(0x58, 0x15);
            this.unitsComboBox2.TabIndex = 8;
            this.unitsComboBox2.Units = MeasurementUnit.Inch;
            this.unitsComboBox2.UnitsDisplayType = UnitsDisplayType.Ratio;
            this.unitsComboBox2.UnitsChanged += new EventHandler(this.OnUnitsComboBox2UnitsChanged);
            this.unitsComboBox1.Location = new Point(200, 0xeb);
            this.unitsComboBox1.Name = "unitsComboBox1";
            this.unitsComboBox1.PixelsAvailable = false;
            this.unitsComboBox1.Size = new Size(0x58, 0x15);
            this.unitsComboBox1.TabIndex = 12;
            this.unitsComboBox1.Units = MeasurementUnit.Inch;
            this.unitsComboBox1.UnitsChanged += new EventHandler(this.OnUnitsComboBox1UnitsChanged);
            this.printWidthUpDown.DecimalPlaces = 2;
            this.printWidthUpDown.Location = new Point(120, 0xeb);
            int[] numArray12 = new int[4];
            numArray12[0] = 0x7fffffff;
            this.printWidthUpDown.Maximum = new decimal(numArray12);
            int[] numArray13 = new int[4];
            numArray13[0] = 0x7fffffff;
            numArray13[3] = -2147483648;
            this.printWidthUpDown.Minimum = new decimal(numArray13);
            this.printWidthUpDown.Name = "printWidthUpDown";
            this.printWidthUpDown.Size = new Size(0x48, 20);
            this.printWidthUpDown.TabIndex = 11;
            this.printWidthUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray14 = new int[4];
            numArray14[0] = 2;
            this.printWidthUpDown.Value = new decimal(numArray14);
            this.printWidthUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.printWidthUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.printWidthUpDown.ValueChanged += new EventHandler(this.upDown_ValueChanged);
            this.printWidthUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.printHeightUpDown.DecimalPlaces = 2;
            this.printHeightUpDown.Location = new Point(120, 0x103);
            int[] numArray15 = new int[4];
            numArray15[0] = 0x7fffffff;
            this.printHeightUpDown.Maximum = new decimal(numArray15);
            int[] numArray16 = new int[4];
            numArray16[0] = 0x7fffffff;
            numArray16[3] = -2147483648;
            this.printHeightUpDown.Minimum = new decimal(numArray16);
            this.printHeightUpDown.Name = "printHeightUpDown";
            this.printHeightUpDown.Size = new Size(0x48, 20);
            this.printHeightUpDown.TabIndex = 14;
            this.printHeightUpDown.TextAlign = HorizontalAlignment.Right;
            int[] numArray17 = new int[4];
            numArray17[0] = 1;
            this.printHeightUpDown.Value = new decimal(numArray17);
            this.printHeightUpDown.Enter += new EventHandler(this.OnUpDownEnter);
            this.printHeightUpDown.KeyUp += new KeyEventHandler(this.OnUpDownKeyUp);
            this.printHeightUpDown.ValueChanged += new EventHandler(this.upDown_ValueChanged);
            this.printHeightUpDown.Leave += new EventHandler(this.OnUpDownLeave);
            this.newWidthLabel2.Location = new Point(0x20, 0xec);
            this.newWidthLabel2.Name = "newWidthLabel2";
            this.newWidthLabel2.Size = new Size(0x4f, 0x10);
            this.newWidthLabel2.TabIndex = 10;
            this.newWidthLabel2.TextAlign = ContentAlignment.MiddleLeft;
            this.newHeightLabel2.Location = new Point(0x20, 260);
            this.newHeightLabel2.Name = "newHeightLabel2";
            this.newHeightLabel2.Size = new Size(0x4f, 0x10);
            this.newHeightLabel2.TabIndex = 13;
            this.newHeightLabel2.TextAlign = ContentAlignment.MiddleLeft;
            this.pixelsLabel2.Location = new Point(200, 0xa9);
            this.pixelsLabel2.Name = "pixelsLabel2";
            this.pixelsLabel2.Size = new Size(0x5d, 0x10);
            this.pixelsLabel2.TabIndex = 5;
            this.pixelsLabel2.TextAlign = ContentAlignment.MiddleLeft;
            this.unitsLabel1.Location = new Point(200, 0x105);
            this.unitsLabel1.Name = "unitsLabel1";
            this.unitsLabel1.Size = new Size(0x5e, 0x10);
            this.unitsLabel1.TabIndex = 15;
            this.pixelSizeHeader.Location = new Point(0x19, 0x7d);
            this.pixelSizeHeader.Name = "pixelSizeHeader";
            this.pixelSizeHeader.Size = new Size(0x10f, 14);
            this.pixelSizeHeader.TabIndex = 0x1a;
            this.pixelSizeHeader.TabStop = false;
            this.printSizeHeader.Location = new Point(0x19, 0xd8);
            this.printSizeHeader.Name = "printSizeHeader";
            this.printSizeHeader.Size = new Size(0x10f, 14);
            this.printSizeHeader.TabIndex = 9;
            this.printSizeHeader.TabStop = false;
            this.resamplingLabel.Location = new Point(6, 30);
            this.resamplingLabel.Name = "resamplingLabel";
            this.resamplingLabel.AutoSize = true;
            this.resamplingLabel.Size = new Size(0x58, 0x10);
            this.resamplingLabel.TabIndex = 20;
            this.resamplingLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.resamplingAlgorithmComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.resamplingAlgorithmComboBox.ItemHeight = 13;
            this.resamplingAlgorithmComboBox.Location = new Point(120, 0x1b);
            this.resamplingAlgorithmComboBox.Name = "resamplingAlgorithmComboBox";
            this.resamplingAlgorithmComboBox.Size = new Size(0x98, 0x15);
            this.resamplingAlgorithmComboBox.Sorted = true;
            this.resamplingAlgorithmComboBox.TabIndex = 0x15;
            this.resamplingAlgorithmComboBox.FlatStyle = FlatStyle.System;
            this.resamplingAlgorithmComboBox.SelectedIndexChanged += new EventHandler(this.OnResamplingAlgorithmComboBoxSelectedIndexChanged);
            base.AcceptButton = this.okButton;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.cancelButton;
            base.ClientSize = new Size(0x12a, 0x164);
            base.Controls.Add(this.printSizeHeader);
            base.Controls.Add(this.pixelSizeHeader);
            base.Controls.Add(this.unitsLabel1);
            base.Controls.Add(this.pixelsLabel2);
            base.Controls.Add(this.newHeightLabel2);
            base.Controls.Add(this.newWidthLabel2);
            base.Controls.Add(this.printHeightUpDown);
            base.Controls.Add(this.printWidthUpDown);
            base.Controls.Add(this.unitsComboBox1);
            base.Controls.Add(this.unitsComboBox2);
            base.Controls.Add(this.resolutionUpDown);
            base.Controls.Add(this.resolutionLabel);
            base.Controls.Add(this.resizedImageHeader);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.okButton);
            base.Controls.Add(this.asteriskLabel);
            base.Controls.Add(this.asteriskTextLabel);
            base.Controls.Add(this.absoluteRB);
            base.Controls.Add(this.percentRB);
            base.Controls.Add(this.pixelWidthUpDown);
            base.Controls.Add(this.pixelHeightUpDown);
            base.Controls.Add(this.pixelsLabel1);
            base.Controls.Add(this.newHeightLabel1);
            base.Controls.Add(this.newWidthLabel1);
            base.Controls.Add(this.resamplingAlgorithmComboBox);
            base.Controls.Add(this.resamplingLabel);
            base.Controls.Add(this.constrainCheckBox);
            base.Controls.Add(this.percentUpDown);
            base.Controls.Add(this.percentSignLabel);
            base.Controls.Add(this.separatorLine);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "ResizeDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.Controls.SetChildIndex(this.percentSignLabel, 0);
            base.Controls.SetChildIndex(this.percentUpDown, 0);
            base.Controls.SetChildIndex(this.constrainCheckBox, 0);
            base.Controls.SetChildIndex(this.resamplingLabel, 0);
            base.Controls.SetChildIndex(this.resamplingAlgorithmComboBox, 0);
            base.Controls.SetChildIndex(this.newWidthLabel1, 0);
            base.Controls.SetChildIndex(this.newHeightLabel1, 0);
            base.Controls.SetChildIndex(this.pixelsLabel1, 0);
            base.Controls.SetChildIndex(this.pixelHeightUpDown, 0);
            base.Controls.SetChildIndex(this.pixelWidthUpDown, 0);
            base.Controls.SetChildIndex(this.percentRB, 0);
            base.Controls.SetChildIndex(this.absoluteRB, 0);
            base.Controls.SetChildIndex(this.asteriskTextLabel, 0);
            base.Controls.SetChildIndex(this.asteriskLabel, 0);
            base.Controls.SetChildIndex(this.okButton, 0);
            base.Controls.SetChildIndex(this.cancelButton, 0);
            base.Controls.SetChildIndex(this.resizedImageHeader, 0);
            base.Controls.SetChildIndex(this.resolutionLabel, 0);
            base.Controls.SetChildIndex(this.resolutionUpDown, 0);
            base.Controls.SetChildIndex(this.unitsComboBox2, 0);
            base.Controls.SetChildIndex(this.unitsComboBox1, 0);
            base.Controls.SetChildIndex(this.printWidthUpDown, 0);
            base.Controls.SetChildIndex(this.printHeightUpDown, 0);
            base.Controls.SetChildIndex(this.newWidthLabel2, 0);
            base.Controls.SetChildIndex(this.newHeightLabel2, 0);
            base.Controls.SetChildIndex(this.pixelsLabel2, 0);
            base.Controls.SetChildIndex(this.unitsLabel1, 0);
            base.Controls.SetChildIndex(this.pixelSizeHeader, 0);
            base.Controls.SetChildIndex(this.printSizeHeader, 0);
            this.pixelWidthUpDown.EndInit();
            this.pixelHeightUpDown.EndInit();
            this.percentUpDown.EndInit();
            this.resolutionUpDown.EndInit();
            this.printWidthUpDown.EndInit();
            this.printHeightUpDown.EndInit();
            base.ResumeLayout(false);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (this.okTimer == null)
            {
                this.okTimer = new System.Windows.Forms.Timer();
                this.okTimer.Interval = this.okTimerInterval;
                this.okTimer.Tick += new EventHandler(this.okTimer_Tick);
                this.okTimer.Enabled = true;
            }
        }

        private void okTimer_Tick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
            base.Close();
            this.okTimer.Dispose();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((base.DialogResult == DialogResult.OK) && (((this.ImageWidth < 0) || (this.ImageHeight < 0)) || (!this.Resolution.IsFinite() || (this.Resolution < 0.0))))
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void OnConstrainerConstrainToAspectChanged(object sender, EventArgs e)
        {
            this.constrainCheckBox.Checked = this.constrainer.ConstrainToAspect;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnConstrainerNewHeightChanged(object sender, EventArgs e)
        {
            double num;
            this.ignoreUpDownValueChanged++;
            if (NumericUpDownUtil.GetValueFromText(this.pixelHeightUpDown, out num) && (num != this.constrainer.NewPixelHeight))
            {
                this.SafeSetNudValue(this.pixelHeightUpDown, this.constrainer.NewPixelHeight);
            }
            if (NumericUpDownUtil.GetValueFromText(this.printHeightUpDown, out num) && (num != this.constrainer.NewHeight))
            {
                this.SafeSetNudValue(this.printHeightUpDown, this.constrainer.NewHeight);
            }
            this.ignoreUpDownValueChanged--;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnConstrainerNewWidthChanged(object sender, EventArgs e)
        {
            this.ignoreUpDownValueChanged++;
            double val = 0.0;
            if (!NumericUpDownUtil.GetValueFromText(this.pixelWidthUpDown, out val) || (val != this.constrainer.NewPixelWidth))
            {
                this.SafeSetNudValue(this.pixelWidthUpDown, this.constrainer.NewPixelWidth);
            }
            if (!NumericUpDownUtil.GetValueFromText(this.printWidthUpDown, out val) || (val != this.constrainer.NewWidth))
            {
                this.SafeSetNudValue(this.printWidthUpDown, this.constrainer.NewWidth);
            }
            this.ignoreUpDownValueChanged--;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnConstrainerResolutionChanged(object sender, EventArgs e)
        {
            double num;
            this.ignoreUpDownValueChanged++;
            if (NumericUpDownUtil.GetValueFromText(this.resolutionUpDown, out num) && (num != this.constrainer.Resolution))
            {
                this.SafeSetNudValue(this.resolutionUpDown, this.constrainer.Resolution);
            }
            this.ignoreUpDownValueChanged--;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnConstrainerUnitsChanged(object sender, EventArgs e)
        {
            this.unitsComboBox1.Units = this.constrainer.Units;
            this.unitsComboBox2.Units = this.constrainer.Units;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UI.ScaleHeight(8);
            int num2 = Math.Max(0, num - base.ExtendedFramePadding.Bottom);
            int x = base.IsGlassEffectivelyEnabled ? -1 : UI.ScaleWidth(8);
            int num4 = UI.ScaleWidth(8);
            this.cancelButton.PerformLayout();
            this.okButton.PerformLayout();
            this.cancelButton.Location = new Point((base.ClientSize.Width - x) - this.cancelButton.Width, (base.ClientSize.Height - num2) - this.cancelButton.Height);
            this.okButton.Location = new Point((this.cancelButton.Left - num4) - this.okButton.Width, (base.ClientSize.Height - num2) - this.okButton.Height);
            this.separatorLine.Size = this.separatorLine.GetPreferredSize(new Size(base.ClientSize.Width - (2 * x), 1));
            this.separatorLine.Location = new Point(x, (this.okButton.Top - num) - this.separatorLine.Height);
            if (base.IsGlassEffectivelyEnabled)
            {
                this.separatorLine.Visible = false;
                base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.separatorLine.Top);
            }
            else
            {
                this.separatorLine.Visible = true;
                base.GlassInset = new Padding(0);
            }
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.ResumeLayout(true);
            base.OnLoad(e);
            this.pixelWidthUpDown.Select();
            this.pixelWidthUpDown.Select(0, this.pixelWidthUpDown.Text.Length);
            this.PopulateAsteriskLabels();
        }

        private void OnRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (this.absoluteRB.Checked)
            {
                this.pixelWidthUpDown.Enabled = true;
                this.pixelHeightUpDown.Enabled = true;
                this.printWidthUpDown.Enabled = true;
                this.printHeightUpDown.Enabled = true;
                this.constrainCheckBox.Enabled = true;
                this.unitsComboBox1.Enabled = true;
                this.unitsComboBox2.Enabled = true;
                this.resolutionUpDown.Enabled = true;
                this.percentUpDown.Enabled = false;
            }
            else if (this.percentRB.Checked)
            {
                this.pixelWidthUpDown.Enabled = false;
                this.pixelHeightUpDown.Enabled = false;
                this.printWidthUpDown.Enabled = false;
                this.printHeightUpDown.Enabled = false;
                this.constrainCheckBox.Enabled = false;
                this.unitsComboBox1.Enabled = false;
                this.unitsComboBox2.Enabled = false;
                this.resolutionUpDown.Enabled = false;
                this.percentUpDown.Enabled = true;
                this.percentUpDown.Select();
            }
        }

        private void OnResamplingAlgorithmComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.PopulateAsteriskLabels();
        }

        private void OnUnitsComboBox1UnitsChanged(object sender, EventArgs e)
        {
            this.constrainer.Units = this.unitsComboBox1.Units;
            this.unitsLabel1.Text = this.unitsComboBox1.UnitsText;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnUnitsComboBox2UnitsChanged(object sender, EventArgs e)
        {
            this.unitsComboBox1.Units = this.unitsComboBox2.Units;
            this.UpdateSizeText();
            this.TryToEnableOkButton();
        }

        private void OnUpDownEnter(object sender, EventArgs e)
        {
            NumericUpDown down = (NumericUpDown) sender;
            down.Select(0, down.Text.Length);
        }

        private void OnUpDownKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Tab)
            {
                double num;
                if (NumericUpDownUtil.GetValueFromText((NumericUpDown) sender, out num))
                {
                    this.UpdateSizeText();
                    this.getValueFromText++;
                    this.upDown_ValueChanged(sender, e);
                    this.getValueFromText--;
                }
                this.TryToEnableOkButton();
            }
        }

        private void OnUpDownLeave(object sender, EventArgs e)
        {
            ((NumericUpDown) sender).Value = ((NumericUpDown) sender).Value;
            this.TryToEnableOkButton();
        }

        private void PopulateAsteriskLabels()
        {
            ResampleMethod selectedItem = this.resamplingAlgorithmComboBox.SelectedItem as ResampleMethod;
            if (selectedItem != null)
            {
                if (selectedItem.method != PaintDotNet.ResamplingAlgorithm.SuperSampling)
                {
                    this.asteriskLabel.Visible = false;
                    this.asteriskTextLabel.Visible = false;
                }
                else
                {
                    if ((this.ImageWidth < this.OriginalSize.Width) && (this.ImageHeight < this.OriginalSize.Height))
                    {
                        this.asteriskTextLabel.Text = PdnResources.GetString2("ResizeDialog.AsteriskTextLabel.SuperSampling");
                    }
                    else
                    {
                        this.asteriskTextLabel.Text = PdnResources.GetString2("ResizeDialog.AsteriskTextLabel.Bicubic");
                    }
                    if (this.resamplingAlgorithmComboBox.Visible)
                    {
                        this.asteriskLabel.Visible = true;
                        this.asteriskTextLabel.Visible = true;
                    }
                }
            }
        }

        private void SafeSetNudValue(NumericUpDown nud, double value)
        {
            try
            {
                decimal num = (decimal) value;
                if ((num >= nud.Minimum) && (num <= nud.Maximum))
                {
                    nud.Value = num;
                }
            }
            catch (OverflowException)
            {
            }
        }

        private void SetupConstrainerEvents()
        {
            this.constrainer.ConstrainToAspectChanged += new EventHandler(this.OnConstrainerConstrainToAspectChanged);
            this.constrainer.NewHeightChanged += new EventHandler(this.OnConstrainerNewHeightChanged);
            this.constrainer.NewWidthChanged += new EventHandler(this.OnConstrainerNewWidthChanged);
            this.constrainer.ResolutionChanged += new EventHandler(this.OnConstrainerResolutionChanged);
            this.constrainer.UnitsChanged += new EventHandler(this.OnConstrainerUnitsChanged);
            this.constrainCheckBox.Checked = this.constrainer.ConstrainToAspect;
            this.SafeSetNudValue(this.pixelWidthUpDown, this.constrainer.NewPixelWidth);
            this.SafeSetNudValue(this.pixelHeightUpDown, this.constrainer.NewPixelHeight);
            this.SafeSetNudValue(this.printWidthUpDown, this.constrainer.NewWidth);
            this.SafeSetNudValue(this.printHeightUpDown, this.constrainer.NewHeight);
            this.SafeSetNudValue(this.resolutionUpDown, this.constrainer.Resolution);
            this.unitsComboBox1.Units = this.constrainer.Units;
        }

        private void TryToEnableOkButton()
        {
            double num;
            double num2;
            double num3;
            double num4;
            double num5;
            double num6;
            bool valueFromText = NumericUpDownUtil.GetValueFromText(this.pixelWidthUpDown, out num);
            bool flag2 = NumericUpDownUtil.GetValueFromText(this.pixelHeightUpDown, out num2);
            bool flag3 = NumericUpDownUtil.GetValueFromText(this.printWidthUpDown, out num3);
            bool flag4 = NumericUpDownUtil.GetValueFromText(this.printHeightUpDown, out num4);
            bool flag5 = NumericUpDownUtil.GetValueFromText(this.resolutionUpDown, out num5);
            bool flag6 = NumericUpDownUtil.GetValueFromText(this.percentUpDown, out num6);
            bool flag7 = (num >= 1.0) && (num <= 65535.0);
            bool flag8 = (num2 >= 1.0) && (num2 <= 65535.0);
            bool flag9 = num3 > 0.0;
            bool flag10 = num4 > 0.0;
            bool flag11 = (num5 >= 0.01) && (num5 < 2000000.0);
            bool flag12 = (num6 >= ((double) this.percentUpDown.Minimum)) && (num6 <= ((double) this.percentUpDown.Maximum));
            bool flag13 = ((((valueFromText && flag2) && (flag3 && flag4)) && ((flag5 && flag6) && (flag7 && flag8))) && ((flag9 && flag10) && flag11)) && flag12;
            this.okButton.Enabled = flag13;
        }

        private void UpdateSizeText()
        {
            long bytes = ((this.layers * 4L) * ((long) this.constrainer.NewPixelWidth)) * ((long) this.constrainer.NewPixelHeight);
            string str = Utility.SizeStringFromBytes(bytes);
            string format = PdnResources.GetString2("ResizeDialog.ResizedImageHeader.Text.Format");
            this.resizedImageHeader.Text = string.Format(format, str);
        }

        private void upDown_ValueChanged(object sender, EventArgs e)
        {
            if (this.ignoreUpDownValueChanged <= 0)
            {
                double num;
                if (sender == this.percentUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if ((NumericUpDownUtil.GetValueFromText(this.percentUpDown, out num) && (num >= ((double) this.percentUpDown.Minimum))) && (num <= ((double) this.percentUpDown.Maximum)))
                        {
                            this.constrainer.SetByPercent(num / 100.0);
                        }
                    }
                    else
                    {
                        this.constrainer.SetByPercent(((double) this.percentUpDown.Value) / 100.0);
                    }
                }
                if (sender == this.pixelWidthUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.pixelWidthUpDown, out num))
                        {
                            this.constrainer.NewPixelWidth = num;
                        }
                    }
                    else
                    {
                        this.constrainer.NewPixelWidth = (double) this.pixelWidthUpDown.Value;
                    }
                }
                if (sender == this.pixelHeightUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.pixelHeightUpDown, out num))
                        {
                            this.constrainer.NewPixelHeight = num;
                        }
                    }
                    else
                    {
                        this.constrainer.NewPixelHeight = (double) this.pixelHeightUpDown.Value;
                    }
                }
                if (sender == this.printWidthUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.printWidthUpDown, out num))
                        {
                            this.constrainer.NewWidth = num;
                        }
                    }
                    else
                    {
                        this.constrainer.NewWidth = (double) this.printWidthUpDown.Value;
                    }
                }
                if (sender == this.printHeightUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.printHeightUpDown, out num))
                        {
                            this.constrainer.NewHeight = num;
                        }
                    }
                    else
                    {
                        this.constrainer.NewHeight = (double) this.printHeightUpDown.Value;
                    }
                }
                if (sender == this.resolutionUpDown)
                {
                    if (this.getValueFromText > 0)
                    {
                        if (NumericUpDownUtil.GetValueFromText(this.resolutionUpDown, out num) && (num >= 0.01))
                        {
                            this.constrainer.Resolution = num;
                        }
                    }
                    else if (((double) this.resolutionUpDown.Value) >= 0.01)
                    {
                        this.constrainer.Resolution = (double) this.resolutionUpDown.Value;
                    }
                }
                this.UpdateSizeText();
                this.PopulateAsteriskLabels();
                this.TryToEnableOkButton();
            }
        }

        public bool ConstrainToAspect
        {
            get => 
                this.constrainer.ConstrainToAspect;
            set
            {
                this.constrainer.ConstrainToAspect = value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                UI.AddCompositedToCP(createParams);
                return createParams;
            }
        }

        public int ImageHeight
        {
            get
            {
                double num;
                if (!NumericUpDownUtil.GetValueFromText(this.pixelHeightUpDown, out num))
                {
                    num = Math.Round(this.constrainer.NewPixelHeight);
                }
                return (int) num.Clamp(-2147483648.0, 2147483647.0);
            }
            set
            {
                this.constrainer.NewPixelHeight = value;
            }
        }

        public int ImageWidth
        {
            get
            {
                double num;
                if (!NumericUpDownUtil.GetValueFromText(this.pixelWidthUpDown, out num))
                {
                    num = Math.Round(this.constrainer.NewPixelWidth);
                }
                return (int) num.Clamp(-2147483648.0, 2147483647.0);
            }
            set
            {
                this.constrainer.NewPixelWidth = value;
            }
        }

        public int LayerCount
        {
            get => 
                this.layers;
            set
            {
                this.layers = value;
                this.UpdateSizeText();
            }
        }

        public double OriginalDpu
        {
            get => 
                this.originalDpu;
            set
            {
                this.originalDpu = value;
                this.UpdateSizeText();
            }
        }

        public MeasurementUnit OriginalDpuUnit
        {
            get => 
                this.originalDpuUnit;
            set
            {
                this.originalDpuUnit = value;
                this.UpdateSizeText();
            }
        }

        public Size OriginalSize
        {
            get => 
                this.constrainer.OriginalPixelSize;
            set
            {
                this.constrainer = new ResizeConstrainer(value);
                this.SetupConstrainerEvents();
                this.UpdateSizeText();
            }
        }

        public PaintDotNet.ResamplingAlgorithm ResamplingAlgorithm
        {
            get => 
                ((ResampleMethod) this.resamplingAlgorithmComboBox.SelectedItem).method;
            set
            {
                this.resamplingAlgorithmComboBox.SelectedItem = new ResampleMethod(value);
                this.PopulateAsteriskLabels();
            }
        }

        public double Resolution
        {
            get => 
                this.constrainer.Resolution;
            set
            {
                this.constrainer.Resolution = Math.Max(0.01, value);
            }
        }

        public MeasurementUnit Units
        {
            get => 
                this.constrainer.Units;
            set
            {
                this.constrainer.Units = value;
            }
        }

        private sealed class ResampleMethod
        {
            public ResamplingAlgorithm method;

            public ResampleMethod(ResamplingAlgorithm method)
            {
                this.method = method;
            }

            public override bool Equals(object obj) => 
                ((obj is ResizeDialog.ResampleMethod) && (((ResizeDialog.ResampleMethod) obj).method == this.method));

            public override int GetHashCode() => 
                this.method.GetHashCode();

            public override string ToString()
            {
                switch (this.method)
                {
                    case ResamplingAlgorithm.NearestNeighbor:
                        return PdnResources.GetString2("ResizeDialog.ResampleMethod.NearestNeighbor");

                    case ResamplingAlgorithm.Bilinear:
                        return PdnResources.GetString2("ResizeDialog.ResampleMethod.Bilinear");

                    case ResamplingAlgorithm.Bicubic:
                        return PdnResources.GetString2("ResizeDialog.ResampleMethod.Bicubic");

                    case ResamplingAlgorithm.SuperSampling:
                        return PdnResources.GetString2("ResizeDialog.ResampleMethod.SuperSampling");
                }
                return this.method.ToString();
            }
        }

        private sealed class ResizeConstrainer
        {
            private bool constrainToAspect = false;
            public const double MinResolution = 0.01;
            private double newHeight;
            private double newWidth;
            private Size originalPixelSize;
            private double resolution;
            private MeasurementUnit units;

            public event EventHandler ConstrainToAspectChanged;

            public event EventHandler NewHeightChanged;

            public event EventHandler NewWidthChanged;

            public event EventHandler ResolutionChanged;

            public event EventHandler UnitsChanged;

            public ResizeConstrainer(Size originalPixelSize)
            {
                this.originalPixelSize = originalPixelSize;
                this.units = Document.DefaultDpuUnit;
                this.resolution = Document.GetDefaultDpu(this.units);
                this.newWidth = ((double) this.originalPixelSize.Width) / this.resolution;
                this.newHeight = ((double) this.originalPixelSize.Height) / this.resolution;
            }

            private void OnConstrainToAspectChanged()
            {
                if (this.ConstrainToAspectChanged != null)
                {
                    this.ConstrainToAspectChanged(this, EventArgs.Empty);
                }
            }

            private void OnNewHeightChanged()
            {
                if (this.NewHeightChanged != null)
                {
                    this.NewHeightChanged(this, EventArgs.Empty);
                }
            }

            private void OnNewWidthChanged()
            {
                if (this.NewWidthChanged != null)
                {
                    this.NewWidthChanged(this, EventArgs.Empty);
                }
            }

            private void OnResolutionChanged()
            {
                if (this.ResolutionChanged != null)
                {
                    this.ResolutionChanged(this, EventArgs.Empty);
                }
            }

            private void OnUnitsChanged()
            {
                if (this.UnitsChanged != null)
                {
                    this.UnitsChanged(this, EventArgs.Empty);
                }
            }

            public void SetByPercent(double scale)
            {
                this.constrainToAspect = false;
                this.NewPixelWidth = this.OriginalPixelSize.Width * scale;
                this.NewPixelHeight = this.OriginalPixelSize.Height * scale;
                this.constrainToAspect = true;
            }

            public bool ConstrainToAspect
            {
                get => 
                    this.constrainToAspect;
                set
                {
                    if (this.constrainToAspect != value)
                    {
                        if (value)
                        {
                            double num = this.newWidth / this.OriginalAspect;
                            if (this.newHeight != num)
                            {
                                this.newHeight = num;
                                this.OnNewHeightChanged();
                            }
                        }
                        this.constrainToAspect = value;
                        this.OnConstrainToAspectChanged();
                    }
                }
            }

            public double NewHeight
            {
                get => 
                    this.newHeight;
                set
                {
                    if (this.newHeight != value)
                    {
                        this.newHeight = value;
                        this.OnNewHeightChanged();
                        if (this.constrainToAspect)
                        {
                            double num = value * this.OriginalAspect;
                            if (this.newWidth != num)
                            {
                                this.newWidth = num;
                                this.OnNewWidthChanged();
                            }
                        }
                    }
                }
            }

            public double NewPixelHeight
            {
                get
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        return this.newHeight;
                    }
                    return (this.newHeight * this.resolution);
                }
                set
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        this.NewHeight = value;
                    }
                    else
                    {
                        this.NewHeight = value / this.resolution;
                    }
                }
            }

            public double NewPixelWidth
            {
                get
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        return this.newWidth;
                    }
                    return (this.newWidth * this.resolution);
                }
                set
                {
                    if (this.Units == MeasurementUnit.Pixel)
                    {
                        this.NewWidth = value;
                    }
                    else
                    {
                        this.NewWidth = value / this.resolution;
                    }
                }
            }

            public double NewWidth
            {
                get => 
                    this.newWidth;
                set
                {
                    if (this.newWidth != value)
                    {
                        this.newWidth = value;
                        this.OnNewWidthChanged();
                        if (this.constrainToAspect)
                        {
                            double num = value / this.OriginalAspect;
                            if (this.newHeight != num)
                            {
                                this.newHeight = num;
                                this.OnNewHeightChanged();
                            }
                        }
                    }
                }
            }

            private double OriginalAspect =>
                (((double) this.originalPixelSize.Width) / ((double) this.originalPixelSize.Height));

            public Size OriginalPixelSize =>
                this.originalPixelSize;

            public double Resolution
            {
                get => 
                    this.resolution;
                set
                {
                    if (value < 0.01)
                    {
                        throw new ArgumentOutOfRangeException("value", value, "value must be >= 0.01");
                    }
                    if (this.resolution != value)
                    {
                        if (this.Units != MeasurementUnit.Pixel)
                        {
                            this.newWidth = (this.newWidth * this.resolution) / value;
                            this.newHeight = (this.newHeight * this.resolution) / value;
                        }
                        this.resolution = value;
                        this.OnResolutionChanged();
                        if (this.Units != MeasurementUnit.Pixel)
                        {
                            this.OnNewWidthChanged();
                            this.OnNewHeightChanged();
                        }
                    }
                }
            }

            public MeasurementUnit Units
            {
                get => 
                    this.units;
                set
                {
                    if (this.units != value)
                    {
                        switch (value)
                        {
                            case MeasurementUnit.Pixel:
                                this.newWidth *= this.resolution;
                                this.newHeight *= this.resolution;
                                this.units = value;
                                this.OnUnitsChanged();
                                this.OnNewWidthChanged();
                                this.OnNewHeightChanged();
                                return;

                            case MeasurementUnit.Inch:
                                if (this.units != MeasurementUnit.Centimeter)
                                {
                                    throw new InvalidEnumArgumentException("this.units is not a valid member of the MeasurementUnit enumeration");
                                }
                                this.newWidth = Document.CentimetersToInches(this.newWidth);
                                this.newHeight = Document.CentimetersToInches(this.newHeight);
                                this.units = value;
                                this.resolution = Document.InchesToCentimeters(this.resolution);
                                this.OnUnitsChanged();
                                this.OnResolutionChanged();
                                this.OnNewWidthChanged();
                                this.OnNewHeightChanged();
                                return;

                            case MeasurementUnit.Centimeter:
                                if (this.units != MeasurementUnit.Inch)
                                {
                                    throw new InvalidEnumArgumentException("this.units is not a valid member of the MeasurementUnit enumeration");
                                }
                                this.newWidth = Document.InchesToCentimeters(this.newWidth);
                                this.newHeight = Document.InchesToCentimeters(this.newHeight);
                                this.units = value;
                                this.resolution = Document.CentimetersToInches(this.resolution);
                                this.OnUnitsChanged();
                                this.OnResolutionChanged();
                                this.OnNewWidthChanged();
                                this.OnNewHeightChanged();
                                break;

                            default:
                                throw new InvalidEnumArgumentException("value is not a valid member of the MeasurementUnit enumeration");
                        }
                    }
                }
            }
        }
    }
}

