namespace PaintDotNet.Controls
{
    using System;
    using System.Windows.Forms;

    internal interface IGlassyControl
    {
        void SetGlassWndProcFilter(IMessageFilter wndProcFilter);

        Padding GlassInset { get; }

        bool IsGlassDesired { get; }
    }
}

