namespace PaintDotNet
{
    using System;
    using System.Windows.Forms;

    internal class MouseEventArgsF : MouseEventArgs
    {
        private double fx;
        private double fy;

        public MouseEventArgsF(MouseButtons buttons, int clicks, double fx, double fy, int delta) : base(buttons, clicks, (int) fx, (int) fy, delta)
        {
            this.fx = fx;
            this.fy = fy;
        }

        public static MouseEventArgsF From(MouseEventArgs e) => 
            new MouseEventArgsF(e.Button, e.Clicks, (double) e.X, (double) e.Y, e.Delta);

        public double Fx =>
            this.fx;

        public double Fy =>
            this.fy;
    }
}

