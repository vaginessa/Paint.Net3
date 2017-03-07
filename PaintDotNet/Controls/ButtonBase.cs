namespace PaintDotNet.Controls
{
    using System;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal abstract class ButtonBase : Control, IButtonControl
    {
        private System.Windows.Forms.DialogResult dialogResult;
        private bool drawHover;
        private bool drawPressed;
        private bool isDefault;

        public event EventHandler DialogResultChanged;

        public event EventHandler IsDefaultChanged;

        public ButtonBase()
        {
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            base.SetStyle(ControlStyles.ResizeRedraw, true);
            base.SetStyle(ControlStyles.Selectable, true);
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            base.SetStyle(ControlStyles.UserPaint, true);
            base.SetStyle(ControlStyles.StandardDoubleClick, false);
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            base.AccessibleRole = AccessibleRole.PushButton;
            base.Name = "ButtonBase";
            this.DoubleBuffered = true;
            base.TabStop = true;
        }

        public void NotifyDefault(bool value)
        {
            if (this.isDefault != value)
            {
                this.isDefault = value;
                this.OnIsDefaultChanged();
                base.Invalidate(true);
            }
        }

        protected virtual void OnDialogResultChanged()
        {
            if (this.DialogResultChanged != null)
            {
                this.DialogResultChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.Invalidate(true);
            base.OnEnabledChanged(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.Invalidate(true);
            base.OnGotFocus(e);
        }

        protected virtual void OnIsDefaultChanged()
        {
            if (this.IsDefaultChanged != null)
            {
                this.IsDefaultChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                this.drawPressed = true;
                this.Refresh();
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                this.drawPressed = false;
                this.Refresh();
                this.PerformClick();
            }
            base.OnKeyUp(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            this.drawPressed = false;
            base.Invalidate(true);
            base.OnLostFocus(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            this.drawPressed = true;
            base.Invalidate(true);
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.drawHover = true;
            base.Invalidate(true);
            base.Update();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.drawHover = false;
            base.Invalidate(true);
            base.Update();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs mevent)
        {
            base.Invalidate(true);
            base.OnMouseMove(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            this.drawPressed = false;
            base.Invalidate(true);
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            PushButtonState disabled;
            if (!base.Enabled)
            {
                disabled = PushButtonState.Disabled;
            }
            else if (this.drawPressed && this.ContainsMouseCursor)
            {
                disabled = PushButtonState.Pressed;
            }
            else if (this.drawHover)
            {
                disabled = PushButtonState.Hot;
            }
            else if (this.IsDefault)
            {
                disabled = PushButtonState.Default;
            }
            else
            {
                disabled = PushButtonState.Normal;
            }
            bool drawFocusCues = this.ShowFocusCues && this.Focused;
            bool showKeyboardCues = this.ShowKeyboardCues;
            this.OnPaintButton(e.Graphics, disabled, drawFocusCues, showKeyboardCues);
            base.OnPaint(e);
        }

        protected abstract void OnPaintButton(Graphics g, PushButtonState buttonState, bool drawFocusCues, bool drawKeyboardCues);
        public void PerformClick()
        {
            this.OnClick(EventArgs.Empty);
        }

        protected override bool ProcessMnemonic(char charCode)
        {
            if (base.CanSelect && Control.IsMnemonic(charCode, this.Text))
            {
                this.OnClick(EventArgs.Empty);
            }
            return base.ProcessMnemonic(charCode);
        }

        private bool ContainsMouseCursor
        {
            get
            {
                Point mousePosition = Control.MousePosition;
                return base.RectangleToScreen(base.ClientRectangle).Contains(mousePosition);
            }
        }

        public System.Windows.Forms.DialogResult DialogResult
        {
            get => 
                this.dialogResult;
            set
            {
                if (this.dialogResult != value)
                {
                    this.dialogResult = value;
                    this.OnDialogResultChanged();
                }
            }
        }

        public bool IsDefault =>
            this.isDefault;
    }
}

