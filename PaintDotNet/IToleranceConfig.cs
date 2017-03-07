namespace PaintDotNet
{
    using System;

    internal interface IToleranceConfig
    {
        event EventHandler ToleranceChanged;

        void PerformToleranceChanged();

        float Tolerance { get; set; }
    }
}

