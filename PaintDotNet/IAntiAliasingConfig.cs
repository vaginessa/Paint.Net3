namespace PaintDotNet
{
    using System;

    internal interface IAntiAliasingConfig
    {
        event EventHandler AntiAliasingChanged;

        void PerformAntiAliasingChanged();

        bool AntiAliasing { get; set; }
    }
}

