namespace PaintDotNet
{
    using System;

    internal interface IFloodModeConfig
    {
        event EventHandler FloodModeChanged;

        void PerformFloodModeChanged();

        PaintDotNet.FloodMode FloodMode { get; set; }
    }
}

