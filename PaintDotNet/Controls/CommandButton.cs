namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class CommandButton : PaintDotNet.Controls.ButtonBase
    {
        private Image actionImage;
        private Image actionImageDisabled;
        private string actionText;
        private Font actionTextFont;
        private string explanationText;
        private Font explanationTextFont;

        public CommandButton()
        {
            this.InitializeComponent();
            this.actionTextFont = FontUtil.CreateGdipFont(this.Font.FontFamily.Name, this.Font.Size * 1.25f, this.Font.Style, this.Font.Unit);
            this.explanationTextFont = this.Font;
        }

        private void InitializeComponent()
        {
            base.AccessibleRole = AccessibleRole.PushButton;
            base.TabStop = true;
            this.DoubleBuffered = true;
            base.Name = "CommandButton";
            base.PerformLayout();
        }

        private Size MeasureAndDraw(Graphics g, bool enableDrawing, PushButtonState state, bool drawFocusCues, bool drawKeyboardCues)
        {
            Rectangle rectangle2;
            if (enableDrawing)
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.CompositingMode = CompositingMode.SourceOver;
                g.InterpolationMode = InterpolationMode.Bilinear;
            }
            int num = UI.ScaleWidth(9);
            int num2 = UI.ScaleHeight(8);
            int num3 = UI.ScaleHeight(9);
            int num4 = UI.ScaleWidth(8);
            int num5 = UI.ScaleHeight(3);
            int x = 0;
            int num7 = 0;
            if (enableDrawing)
            {
                using (Brush brush = new SolidBrush(this.BackColor))
                {
                    CompositingMode compositingMode = g.CompositingMode;
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.FillRectangle(brush, base.ClientRectangle);
                    g.CompositingMode = compositingMode;
                }
                Rectangle rect = new Rectangle(0, 0, base.ClientSize.Width, base.ClientSize.Height);
                if (state == PushButtonState.Pressed)
                {
                    x = 1;
                    num7 = 1;
                }
                UI.DrawCommandButton(g, state, rect, this.BackColor, this);
            }
            if (this.actionImage == null)
            {
                rectangle2 = new Rectangle(x, num7 + num2, 0, 0);
            }
            else
            {
                rectangle2 = new Rectangle(x + num, num7 + num2, UI.ScaleWidth(this.actionImage.Width), UI.ScaleHeight(this.actionImage.Height));
                Rectangle srcRect = new Rectangle(0, 0, this.actionImage.Width, this.actionImage.Height);
                if (enableDrawing)
                {
                    Image image = base.Enabled ? this.actionImage : this.actionImageDisabled;
                    if (base.Enabled)
                    {
                        rectangle2.Y += 3;
                        rectangle2.X++;
                        g.DrawImage(this.actionImageDisabled, rectangle2, srcRect, GraphicsUnit.Pixel);
                        rectangle2.X--;
                        rectangle2.Y -= 3;
                    }
                    rectangle2.Y += 2;
                    g.DrawImage(image, rectangle2, srcRect, GraphicsUnit.Pixel);
                    rectangle2.Y -= 2;
                }
            }
            int num8 = rectangle2.Right + num4;
            int top = rectangle2.Top;
            int width = ((base.ClientSize.Width - num8) - num) + x;
            Color windowText = SystemColors.WindowText;
            StringFormat format = (StringFormat) StringFormat.GenericTypographic.Clone();
            format.HotkeyPrefix = drawKeyboardCues ? HotkeyPrefix.Show : HotkeyPrefix.Hide;
            TextFormatFlags flags = ((((drawKeyboardCues ? TextFormatFlags.Default : TextFormatFlags.HidePrefix) | TextFormatFlags.NoPadding) | TextFormatFlags.PreserveGraphicsClipping) | TextFormatFlags.PreserveGraphicsTranslateTransform) | TextFormatFlags.WordBreak;
            Size size = TextRenderer.MeasureText(g, this.actionText, this.actionTextFont, new Size(width, 0x2710), flags);
            Rectangle layoutRectangle = new Rectangle(num8, top, width, size.Height);
            if (enableDrawing)
            {
                if (state == PushButtonState.Disabled)
                {
                    ControlPaint.DrawStringDisabled(g, this.actionText, this.actionTextFont, this.BackColor, layoutRectangle, format);
                }
                else
                {
                    TextRenderer.DrawText(g, this.actionText, this.actionTextFont, layoutRectangle, windowText, flags);
                }
            }
            int num11 = num8;
            int y = layoutRectangle.Bottom + num5;
            int num13 = width;
            Size size2 = TextRenderer.MeasureText(g, this.explanationText, this.explanationTextFont, new Size(num13, 0x2710), flags);
            Rectangle rectangle5 = new Rectangle(num11, y, num13, size2.Height);
            if (enableDrawing)
            {
                if (state == PushButtonState.Disabled)
                {
                    ControlPaint.DrawStringDisabled(g, this.explanationText, this.explanationTextFont, this.BackColor, rectangle5, format);
                }
                else
                {
                    TextRenderer.DrawText(g, this.explanationText, this.explanationTextFont, rectangle5, windowText, flags);
                }
            }
            if (enableDrawing && drawFocusCues)
            {
                ControlPaint.DrawFocusRectangle(g, new Rectangle(3, 3, base.ClientSize.Width - 5, base.ClientSize.Height - 5));
            }
            format.Dispose();
            format = null;
            return new Size(base.ClientSize.Width, rectangle5.Bottom + num3);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (this.AutoSize)
            {
                Size size;
                using (Graphics graphics = base.CreateGraphics())
                {
                    size = this.MeasureAndDraw(graphics, false, PushButtonState.Normal, false, false);
                }
                base.ClientSize = size;
            }
            base.OnLayout(levent);
        }

        protected override void OnPaintButton(Graphics g, PushButtonState state, bool drawFocusCues, bool drawKeyboardCues)
        {
            this.MeasureAndDraw(g, true, state, drawFocusCues, drawKeyboardCues);
        }

        public Image ActionImage
        {
            get => 
                this.actionImage;
            set
            {
                if (this.actionImage != null)
                {
                    this.actionImageDisabled.Dispose();
                    this.actionImageDisabled = null;
                    this.actionImage.Dispose();
                    this.actionImage = null;
                }
                if (value != null)
                {
                    this.actionImage = value;
                    this.actionImageDisabled = ToolStripRenderer.CreateDisabledImage(this.actionImage);
                }
                base.PerformLayout();
                base.Invalidate(true);
            }
        }

        public string ActionText
        {
            get => 
                this.actionText;
            set
            {
                if (this.actionText != value)
                {
                    this.actionText = value;
                    this.Text = value;
                    base.PerformLayout();
                    base.Invalidate(true);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Browsable(true)]
        public override bool AutoSize
        {
            get => 
                base.AutoSize;
            set
            {
                base.AutoSize = value;
                base.PerformLayout();
                base.Invalidate(true);
            }
        }

        public string ExplanationText
        {
            get => 
                this.explanationText;
            set
            {
                if (this.explanationText != value)
                {
                    this.explanationText = value;
                    base.PerformLayout();
                    base.Invalidate(true);
                }
            }
        }
    }
}

