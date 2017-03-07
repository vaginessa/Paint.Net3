namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class ColorDisplayWidget : UserControl
    {
        private IconBox blackAndWhiteIconBox;
        private IContainer components;
        private ColorRectangleControl primaryColorRectangle;
        private ColorRectangleControl secondaryColorRectangle;
        private IconBox swapIconBox;
        private ToolTip toolTip;
        private ColorBgra userPrimaryColor;
        private ColorBgra userSecondaryColor;

        public event EventHandler BlackAndWhiteButtonClicked;

        public event EventHandler SwapColorsClicked;

        public event EventHandler UserPrimaryColorChanged;

        public event EventHandler UserPrimaryColorClick;

        public event EventHandler UserSecondaryColorChanged;

        public event EventHandler UserSecondaryColorClick;

        public ColorDisplayWidget()
        {
            this.InitializeComponent();
            this.swapIconBox.Icon = new Bitmap(PdnResources.GetImageResource2("Icons.SwapIcon.png").Reference);
            this.blackAndWhiteIconBox.Icon = new Bitmap(PdnResources.GetImageResource2("Icons.BlackAndWhiteIcon.png").Reference);
            if (!base.DesignMode)
            {
                this.toolTip.SetToolTip(this.swapIconBox, PdnResources.GetString2("ColorDisplayWidget.SwapIconBox.ToolTipText"));
                this.toolTip.SetToolTip(this.blackAndWhiteIconBox, PdnResources.GetString2("ColorDisplayWidget.BlackAndWhiteIconBox.ToolTipText"));
                this.toolTip.SetToolTip(this.primaryColorRectangle, PdnResources.GetString2("ColorDisplayWidget.ForeColorRectangle.ToolTipText"));
                this.toolTip.SetToolTip(this.secondaryColorRectangle, PdnResources.GetString2("ColorDisplayWidget.BackColorRectangle.ToolTipText"));
            }
        }

        private void BlackAndWhiteIconBox_Click(object sender, EventArgs e)
        {
            this.OnBlackAndWhiteButtonClicked();
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
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
            this.components = new Container();
            this.primaryColorRectangle = new ColorRectangleControl();
            this.secondaryColorRectangle = new ColorRectangleControl();
            this.swapIconBox = new IconBox();
            this.blackAndWhiteIconBox = new IconBox();
            this.toolTip = new ToolTip(this.components);
            base.SuspendLayout();
            this.primaryColorRectangle.Name = "foreColorRectangle";
            this.primaryColorRectangle.RectangleColor = Color.FromArgb(0, 0, 0xc0);
            this.primaryColorRectangle.Size = new Size(0x1c, 0x1c);
            this.primaryColorRectangle.TabIndex = 0;
            this.primaryColorRectangle.Click += new EventHandler(this.PrimaryColorRectangle_Click);
            this.primaryColorRectangle.KeyUp += new KeyEventHandler(this.Control_KeyUp);
            this.secondaryColorRectangle.Name = "backColorRectangle";
            this.secondaryColorRectangle.RectangleColor = Color.Magenta;
            this.secondaryColorRectangle.Size = new Size(0x1c, 0x1c);
            this.secondaryColorRectangle.TabIndex = 1;
            this.secondaryColorRectangle.Click += new EventHandler(this.SecondaryColorRectangle_Click);
            this.secondaryColorRectangle.KeyUp += new KeyEventHandler(this.Control_KeyUp);
            this.swapIconBox.Icon = null;
            this.swapIconBox.Name = "swapIconBox";
            this.swapIconBox.Size = new Size(0x10, 0x10);
            this.swapIconBox.TabIndex = 2;
            this.swapIconBox.TabStop = false;
            this.swapIconBox.Click += new EventHandler(this.SwapIconBox_Click);
            this.swapIconBox.KeyUp += new KeyEventHandler(this.Control_KeyUp);
            this.swapIconBox.DoubleClick += new EventHandler(this.SwapIconBox_Click);
            this.blackAndWhiteIconBox.Icon = null;
            this.blackAndWhiteIconBox.Name = "blackAndWhiteIconBox";
            this.blackAndWhiteIconBox.Size = new Size(0x10, 0x10);
            this.blackAndWhiteIconBox.TabIndex = 3;
            this.blackAndWhiteIconBox.TabStop = false;
            this.blackAndWhiteIconBox.Click += new EventHandler(this.BlackAndWhiteIconBox_Click);
            this.blackAndWhiteIconBox.KeyUp += new KeyEventHandler(this.Control_KeyUp);
            this.blackAndWhiteIconBox.DoubleClick += new EventHandler(this.BlackAndWhiteIconBox_Click);
            this.toolTip.ShowAlways = true;
            base.Controls.Add(this.blackAndWhiteIconBox);
            base.Controls.Add(this.swapIconBox);
            base.Controls.Add(this.primaryColorRectangle);
            base.Controls.Add(this.secondaryColorRectangle);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.Name = "ColorDisplayWidget";
            base.Size = new Size(0x30, 0x30);
            base.ResumeLayout(false);
        }

        protected virtual void OnBlackAndWhiteButtonClicked()
        {
            if (this.BlackAndWhiteButtonClicked != null)
            {
                this.BlackAndWhiteButtonClicked(this, EventArgs.Empty);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = (base.ClientRectangle.Width - UI.ScaleWidth(this.DefaultSize.Width)) / 2;
            int num2 = (base.ClientRectangle.Height - UI.ScaleHeight(this.DefaultSize.Height)) / 2;
            this.primaryColorRectangle.Location = new Point(UI.ScaleWidth((int) (num + 2)), UI.ScaleHeight((int) (num2 + 2)));
            this.secondaryColorRectangle.Location = new Point(UI.ScaleWidth((int) (num + 0x12)), UI.ScaleHeight((int) (num2 + 0x12)));
            this.swapIconBox.Location = new Point(UI.ScaleWidth((int) (num + 30)), UI.ScaleHeight((int) (num2 + 2)));
            this.blackAndWhiteIconBox.Location = new Point(UI.ScaleWidth((int) (num + 2)), UI.ScaleHeight((int) (num2 + 0x1f)));
            base.OnLayout(levent);
        }

        protected virtual void OnSwapColorsClicked()
        {
            if (this.SwapColorsClicked != null)
            {
                this.SwapColorsClicked(this, EventArgs.Empty);
            }
        }

        protected virtual void OnUserPrimaryColorChanged()
        {
            if (this.UserPrimaryColorChanged != null)
            {
                this.UserPrimaryColorChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnUserPrimaryColorClick()
        {
            if (this.UserPrimaryColorClick != null)
            {
                this.UserPrimaryColorClick(this, EventArgs.Empty);
            }
        }

        protected virtual void OnUserSecondaryColorChanged()
        {
            if (this.UserSecondaryColorChanged != null)
            {
                this.UserSecondaryColorChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnUserSecondaryColorClick()
        {
            if (this.UserSecondaryColorClick != null)
            {
                this.UserSecondaryColorClick(this, EventArgs.Empty);
            }
        }

        private void PrimaryColorRectangle_Click(object sender, EventArgs e)
        {
            this.OnUserPrimaryColorClick();
        }

        private void SecondaryColorRectangle_Click(object sender, EventArgs e)
        {
            this.OnUserSecondaryColorClick();
        }

        private void SwapIconBox_Click(object sender, EventArgs e)
        {
            this.OnSwapColorsClicked();
        }

        protected override Size DefaultSize =>
            new Size(0x30, 0x30);

        public ColorBgra UserPrimaryColor
        {
            get => 
                this.userPrimaryColor;
            set
            {
                this.userPrimaryColor = value;
                this.primaryColorRectangle.RectangleColor = (Color) value;
                base.Invalidate();
                base.Update();
            }
        }

        public ColorBgra UserSecondaryColor
        {
            get => 
                this.userSecondaryColor;
            set
            {
                this.userSecondaryColor = value;
                this.secondaryColorRectangle.RectangleColor = (Color) value;
                base.Invalidate();
                base.Update();
            }
        }
    }
}

