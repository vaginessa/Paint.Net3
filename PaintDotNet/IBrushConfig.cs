namespace PaintDotNet
{
    using System;

    internal interface IBrushConfig
    {
        event EventHandler BrushInfoChanged;

        void PerformBrushChanged();

        PaintDotNet.BrushInfo BrushInfo { get; set; }
    }
}

