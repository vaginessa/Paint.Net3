namespace PaintDotNet.Controls
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class HeadingLabel : Control
    {
        private int leftMargin = 2;
        private int rightMargin = 8;
        private PaintDotNet.Controls.SeparatorLine separatorLine;
        private const TextFormatFlags textFormatFlags = (TextFormatFlags.NoPadding | TextFormatFlags.HidePrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

        public HeadingLabel()
        {
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            base.SetStyle(ControlStyles.Opaque, true);
            base.SetStyle(ControlStyles.ResizeRedraw, true);
            base.SetStyle(ControlStyles.UserPaint, true);
            base.SetStyle(ControlStyles.Selectable, false);
            base.TabStop = false;
            this.ForeColor = SystemColors.Highlight;
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            base.SuspendLayout();
            this.separatorLine = new PaintDotNet.Controls.SeparatorLine();
            base.Controls.Add(this.separatorLine);
            base.Size = new Size(0x90, 14);
            base.ResumeLayout(false);
        }

        public override Size GetPreferredSize(Size proposedSize) => 
            new Size(Math.Max(proposedSize.Width, this.GetPreferredWidth(proposedSize)), this.GetTextSize().Height);

        private int GetPreferredWidth(Size proposedSize)
        {
            Size textSize = this.GetTextSize();
            return (this.leftMargin + textSize.Width);
        }

        private Size GetTextSize()
        {
            string text = string.IsNullOrEmpty(this.Text) ? " " : this.Text;
            Size size = TextRenderer.MeasureText(text, this.Font, base.ClientSize, TextFormatFlags.NoPadding | TextFormatFlags.HidePrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            if (string.IsNullOrEmpty(this.Text))
            {
                size.Width = 0;
            }
            return size;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.PerformLayout();
            base.Invalidate(true);
            base.OnFontChanged(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            Size textSize = this.GetTextSize();
            int x = ((string.IsNullOrEmpty(this.Text) ? 0 : this.leftMargin) + textSize.Width) + (string.IsNullOrEmpty(this.Text) ? 0 : 1);
            int num2 = base.ClientRectangle.Right - this.rightMargin;
            this.separatorLine.Size = this.separatorLine.GetPreferredSize(new Size(num2 - x, 1));
            this.separatorLine.Location = new Point(x, (base.ClientSize.Height - this.separatorLine.Height) / 2);
            base.OnLayout(levent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(brush, e.ClipRectangle);
            }
            this.GetTextSize();
            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, new Point(this.leftMargin, 0), SystemColors.WindowText, TextFormatFlags.NoPadding | TextFormatFlags.HidePrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            base.OnPaint(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.PerformLayout();
            base.Invalidate(true);
            base.OnTextChanged(e);
        }

        [DefaultValue(8)]
        public int RightMargin
        {
            get => 
                this.rightMargin;
            set
            {
                this.rightMargin = value;
                base.PerformLayout();
            }
        }
    }
}

