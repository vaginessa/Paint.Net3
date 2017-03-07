namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ColorRectangleControl : UserControl
    {
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;
        private Color rectangleColor;

        public ColorRectangleControl()
        {
            base.ResizeRedraw = true;
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            int num = 2;
            Rectangle rect = Rectangle.Inflate(base.ClientRectangle, -num, -num);
            Utility.DrawColorRectangle(e.Graphics, rect, this.rectangleColor, false);
            Rectangle clientRectangle = base.ClientRectangle;
            clientRectangle.Width--;
            clientRectangle.Height--;
            e.Graphics.DrawRectangle(this.penBrushCache.GetPen(Color.Black), clientRectangle);
            clientRectangle.Inflate(-1, -1);
            e.Graphics.DrawRectangle(this.penBrushCache.GetPen(Color.White), clientRectangle);
            base.OnPaint(e);
        }

        public Color RectangleColor
        {
            get => 
                this.rectangleColor;
            set
            {
                this.rectangleColor = value;
                base.Invalidate(true);
            }
        }
    }
}

