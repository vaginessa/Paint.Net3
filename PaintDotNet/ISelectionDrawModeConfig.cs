namespace PaintDotNet
{
    using System;

    internal interface ISelectionDrawModeConfig
    {
        event EventHandler SelectionDrawModeInfoChanged;

        void PerformSelectionDrawModeInfoChanged();

        PaintDotNet.SelectionDrawModeInfo SelectionDrawModeInfo { get; set; }
    }
}

