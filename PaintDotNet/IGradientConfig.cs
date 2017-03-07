namespace PaintDotNet
{
    using System;

    internal interface IGradientConfig
    {
        event EventHandler GradientInfoChanged;

        void PerformGradientInfoChanged();

        PaintDotNet.GradientInfo GradientInfo { get; set; }
    }
}

