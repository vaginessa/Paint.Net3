namespace PaintDotNet
{
    using System;

    internal interface IPenConfig
    {
        event EventHandler PenInfoChanged;

        void PerformPenChanged();

        PaintDotNet.PenInfo PenInfo { get; set; }
    }
}

