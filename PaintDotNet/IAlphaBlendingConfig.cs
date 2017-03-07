namespace PaintDotNet
{
    using System;

    internal interface IAlphaBlendingConfig
    {
        event EventHandler AlphaBlendingChanged;

        void PerformAlphaBlendingChanged();

        bool AlphaBlending { get; set; }
    }
}

