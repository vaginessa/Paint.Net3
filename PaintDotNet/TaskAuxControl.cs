namespace PaintDotNet
{
    using System;
    using System.Windows.Forms;

    internal abstract class TaskAuxControl
    {
        internal TaskAuxControl()
        {
        }

        public abstract Control CreateControl();
    }
}

