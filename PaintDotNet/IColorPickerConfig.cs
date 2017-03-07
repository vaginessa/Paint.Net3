namespace PaintDotNet
{
    using System;

    internal interface IColorPickerConfig
    {
        event EventHandler ColorPickerClickBehaviorChanged;

        void PerformColorPickerClickBehaviorChanged();

        PaintDotNet.ColorPickerClickBehavior ColorPickerClickBehavior { get; set; }
    }
}

