namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class ViewConfigStrip : ToolStripEx
    {
        private ToolStripButton gridButton;
        private int ignoreZoomChanges;
        private string percentageFormat;
        private ToolStripButton rulersButton;
        private PaintDotNet.ScaleFactor scaleFactor;
        private int scaleFactorRecursionDepth;
        private ToolStripSeparator separator0;
        private ToolStripSeparator separator1;
        private int suspendEvents;
        private UnitsComboBoxStrip unitsComboBox;
        private ToolStripLabel unitsLabel;
        private string windowText;
        private PaintDotNet.ZoomBasis zoomBasis;
        private PdnToolStripComboBox zoomComboBox;
        private ToolStripButton zoomInButton;
        private ToolStripButton zoomOutButton;

        public event EventHandler DrawGridChanged;

        public event EventHandler RulersEnabledChanged;

        public event EventHandler UnitsChanged;

        public event EventHandler ZoomBasisChanged;

        public event EventHandler ZoomIn;

        public event EventHandler ZoomOut;

        public event EventHandler ZoomScaleChanged;

        public ViewConfigStrip()
        {
            base.SuspendLayout();
            this.InitializeComponent();
            this.windowText = EnumLocalizer.GetLocalizedEnumValue(typeof(PaintDotNet.ZoomBasis), PaintDotNet.ZoomBasis.FitToWindow).LocalizedName;
            this.percentageFormat = PdnResources.GetString2("ZoomConfigWidget.Percentage.Format");
            double[] presetValues = PaintDotNet.ScaleFactor.PresetValues;
            this.zoomComboBox.ComboBox.SuspendLayout();
            string str = null;
            for (int i = presetValues.Length - 1; i >= 0; i--)
            {
                string str2 = (presetValues[i] * 100.0).ToString();
                string item = string.Format(this.percentageFormat, str2);
                if (presetValues[i] == 1.0)
                {
                    str = item;
                }
                this.zoomComboBox.Items.Add(item);
            }
            this.zoomComboBox.Items.Add(this.windowText);
            this.zoomComboBox.ComboBox.ResumeLayout(false);
            this.zoomComboBox.Size = new Size(UI.ScaleWidth(this.zoomComboBox.Width), this.zoomComboBox.Height);
            this.unitsLabel.Text = PdnResources.GetString2("WorkspaceOptionsConfigWidget.UnitsLabel.Text");
            this.zoomComboBox.Text = str;
            this.ScaleFactor = PaintDotNet.ScaleFactor.OneToOne;
            this.zoomOutButton.Image = PdnResources.GetImageResource2("Icons.MenuViewZoomOutIcon.png").Reference;
            this.zoomInButton.Image = PdnResources.GetImageResource2("Icons.MenuViewZoomInIcon.png").Reference;
            this.gridButton.Image = PdnResources.GetImageResource2("Icons.MenuViewGridIcon.png").Reference;
            this.rulersButton.Image = PdnResources.GetImageResource2("Icons.MenuViewRulersIcon.png").Reference;
            this.zoomOutButton.ToolTipText = PdnResources.GetString2("ZoomConfigWidget.ZoomOutButton.ToolTipText");
            this.zoomInButton.ToolTipText = PdnResources.GetString2("ZoomConfigWidget.ZoomInButton.ToolTipText");
            this.gridButton.ToolTipText = PdnResources.GetString2("WorkspaceOptionsConfigWidget.DrawGridToggleButton.ToolTipText");
            this.rulersButton.ToolTipText = PdnResources.GetString2("WorkspaceOptionsConfigWidget.RulersToggleButton.ToolTipText");
            this.unitsComboBox.Size = new Size(UI.ScaleWidth(this.unitsComboBox.Width), this.unitsComboBox.Height);
            this.zoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
            this.ScaleFactor = PaintDotNet.ScaleFactor.OneToOne;
            base.ResumeLayout(false);
        }

        public void BeginZoomChanges()
        {
            this.ignoreZoomChanges++;
        }

        public void EndZoomChanges()
        {
            this.ignoreZoomChanges--;
        }

        private void InitializeComponent()
        {
            this.separator0 = new ToolStripSeparator();
            this.zoomOutButton = new ToolStripButton();
            this.zoomComboBox = new PdnToolStripComboBox(false);
            this.zoomInButton = new ToolStripButton();
            this.separator1 = new ToolStripSeparator();
            this.gridButton = new ToolStripButton();
            this.rulersButton = new ToolStripButton();
            this.unitsLabel = new ToolStripLabel();
            this.unitsComboBox = new UnitsComboBoxStrip();
            base.SuspendLayout();
            this.separator0.Name = "separator0";
            this.zoomComboBox.KeyPress += new KeyPressEventHandler(this.ZoomComboBox_KeyPress);
            this.zoomComboBox.Validating += new CancelEventHandler(this.ZoomComboBox_Validating);
            this.zoomComboBox.SelectedIndexChanged += new EventHandler(this.ZoomComboBox_SelectedIndexChanged);
            this.zoomComboBox.Size = new Size(0x4b, this.zoomComboBox.Height);
            this.zoomComboBox.MaxDropDownItems = 0x63;
            this.unitsComboBox.UnitsChanged += new EventHandler(this.UnitsComboBox_UnitsChanged);
            this.unitsComboBox.LowercaseStrings = false;
            this.unitsComboBox.UnitsDisplayType = UnitsDisplayType.Plural;
            this.unitsComboBox.Units = MeasurementUnit.Pixel;
            this.unitsComboBox.Size = new Size(100, this.unitsComboBox.Height);
            this.Items.Add(this.separator0);
            this.Items.Add(this.zoomOutButton);
            this.Items.Add(this.zoomComboBox);
            this.Items.Add(this.zoomInButton);
            this.Items.Add(this.separator1);
            this.Items.Add(this.gridButton);
            this.Items.Add(this.rulersButton);
            this.Items.Add(this.unitsLabel);
            this.Items.Add(this.unitsComboBox);
            base.ResumeLayout(false);
        }

        private void OnDrawGridChanged()
        {
            if (this.DrawGridChanged != null)
            {
                this.DrawGridChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == this.zoomInButton)
            {
                this.OnZoomIn();
            }
            else if (e.ClickedItem == this.zoomOutButton)
            {
                this.OnZoomOut();
            }
            else if (e.ClickedItem == this.rulersButton)
            {
                this.rulersButton.Checked = !this.rulersButton.Checked;
                this.OnRulersEnabledChanged();
            }
            else if (e.ClickedItem == this.gridButton)
            {
                this.gridButton.Checked = !this.gridButton.Checked;
                this.OnDrawGridChanged();
            }
            base.OnItemClicked(e);
        }

        private void OnRulersEnabledChanged()
        {
            if (this.RulersEnabledChanged != null)
            {
                this.RulersEnabledChanged(this, EventArgs.Empty);
            }
        }

        private void OnUnitsChanged()
        {
            if (this.UnitsChanged != null)
            {
                this.UnitsChanged(this, EventArgs.Empty);
            }
        }

        private void OnZoomBasisChanged()
        {
            this.SetZoomText();
            if (this.ZoomBasisChanged != null)
            {
                this.ZoomBasisChanged(this, EventArgs.Empty);
            }
        }

        private void OnZoomIn()
        {
            if (this.ZoomIn != null)
            {
                this.ZoomIn(this, EventArgs.Empty);
            }
        }

        private void OnZoomOut()
        {
            if (this.ZoomOut != null)
            {
                this.ZoomOut(this, EventArgs.Empty);
            }
        }

        private void OnZoomScaleChanged()
        {
            if (this.zoomBasis == PaintDotNet.ZoomBasis.ScaleFactor)
            {
                this.SetZoomText();
                if (this.ZoomScaleChanged != null)
                {
                    this.ZoomScaleChanged(this, EventArgs.Empty);
                }
            }
        }

        public void PerformZoomBasisChanged()
        {
            this.OnZoomBasisChanged();
        }

        public void PerformZoomScaleChanged()
        {
            this.OnZoomScaleChanged();
        }

        public void ResumeEvents()
        {
            this.suspendEvents--;
        }

        private void SetZoomText()
        {
            if (this.ignoreZoomChanges == 0)
            {
                this.zoomComboBox.BackColor = SystemColors.Window;
                string text = this.zoomComboBox.Text;
                switch (this.zoomBasis)
                {
                    case PaintDotNet.ZoomBasis.FitToWindow:
                        text = this.windowText;
                        break;

                    case PaintDotNet.ZoomBasis.ScaleFactor:
                        text = this.scaleFactor.ToString();
                        break;
                }
                if (this.zoomComboBox.Text != text)
                {
                    this.zoomComboBox.Text = text;
                    this.zoomComboBox.ComboBox.Update();
                }
            }
        }

        public void SuspendEvents()
        {
            this.suspendEvents++;
        }

        private void UnitsComboBox_UnitsChanged(object sender, EventArgs e)
        {
            this.OnUnitsChanged();
        }

        private void ZoomComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == '\n') || (e.KeyChar == '\r'))
            {
                this.ZoomComboBox_Validating(sender, new CancelEventArgs(false));
                this.zoomComboBox.Select(0, this.zoomComboBox.Text.Length);
            }
        }

        private void ZoomComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.suspendEvents == 0)
            {
                this.ZoomComboBox_Validating(sender, new CancelEventArgs(false));
            }
        }

        private void ZoomComboBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                int numerator = 1;
                e.Cancel = false;
                if (this.zoomComboBox.Text == this.windowText)
                {
                    this.ZoomBasis = PaintDotNet.ZoomBasis.FitToWindow;
                }
                else
                {
                    try
                    {
                        string text = this.zoomComboBox.Text;
                        if (text.Length == 0)
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            if (text[text.Length - 1] == '%')
                            {
                                text = text.Substring(0, text.Length - 1);
                            }
                            else if (text[0] == '%')
                            {
                                text = text.Substring(1);
                            }
                            numerator = (int) Math.Round(double.Parse(text));
                            this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
                        }
                    }
                    catch (FormatException)
                    {
                        e.Cancel = true;
                    }
                    catch (OverflowException)
                    {
                        e.Cancel = true;
                    }
                    if (e.Cancel)
                    {
                        this.zoomComboBox.BackColor = Color.Red;
                        this.zoomComboBox.ToolTipText = PdnResources.GetString2("ZoomConfigWidget.Error.InvalidNumber");
                    }
                    else if (numerator < 1)
                    {
                        e.Cancel = true;
                        this.zoomComboBox.BackColor = Color.Red;
                        this.zoomComboBox.ToolTipText = PdnResources.GetString2("ZoomConfigWidget.Error.TooSmall");
                    }
                    else if (numerator > 0xc80)
                    {
                        e.Cancel = true;
                        this.zoomComboBox.BackColor = Color.Red;
                        this.zoomComboBox.ToolTipText = PdnResources.GetString2("ZoomConfigWidget.Error.TooLarge");
                    }
                    else
                    {
                        e.Cancel = false;
                        this.zoomComboBox.ToolTipText = string.Empty;
                        this.zoomComboBox.BackColor = SystemColors.Window;
                        this.ScaleFactor = new PaintDotNet.ScaleFactor(numerator, 100);
                        this.SuspendEvents();
                        this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
                        this.ResumeEvents();
                    }
                }
            }
            catch (FormatException)
            {
            }
        }

        public bool DrawGrid
        {
            get => 
                this.gridButton.Checked;
            set
            {
                if (this.gridButton.Checked != value)
                {
                    this.gridButton.Checked = value;
                    this.OnDrawGridChanged();
                }
            }
        }

        public bool RulersEnabled
        {
            get => 
                this.rulersButton.Checked;
            set
            {
                if (this.rulersButton.Checked != value)
                {
                    this.rulersButton.Checked = value;
                    this.OnRulersEnabledChanged();
                }
            }
        }

        public PaintDotNet.ScaleFactor ScaleFactor
        {
            get => 
                this.scaleFactor;
            set
            {
                if (this.scaleFactor.Ratio != value.Ratio)
                {
                    this.scaleFactor = value;
                    this.scaleFactorRecursionDepth++;
                    if (this.scaleFactorRecursionDepth < 100)
                    {
                        this.OnZoomScaleChanged();
                    }
                    this.scaleFactorRecursionDepth--;
                }
            }
        }

        public MeasurementUnit Units
        {
            get => 
                this.unitsComboBox.Units;
            set
            {
                this.unitsComboBox.Units = value;
            }
        }

        public PaintDotNet.ZoomBasis ZoomBasis
        {
            get => 
                this.zoomBasis;
            set
            {
                if (this.zoomBasis != value)
                {
                    this.zoomBasis = value;
                    this.OnZoomBasisChanged();
                }
            }
        }
    }
}

