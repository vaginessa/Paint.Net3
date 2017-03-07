namespace PaintDotNet
{
    using System;

    internal interface ISelectionCombineModeConfig
    {
        event EventHandler SelectionCombineModeChanged;

        void PerformSelectionCombineModeChanged();

        PaintDotNet.SelectionCombineMode SelectionCombineMode { get; set; }
    }
}

