namespace PaintDotNet.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class TransparentLabel : Label
    {
        public TransparentLabel()
        {
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            base.SetStyle(ControlStyles.Opaque, false);
            this.BackColor = Color.Transparent;
        }
    }
}

