namespace PaintDotNet
{
    using System;

    [Flags]
    internal enum ToolBarConfigItems : uint
    {
        All = 0xffffffff,
        AlphaBlending = 1,
        Antialiasing = 2,
        Brush = 4,
        ColorPickerBehavior = 8,
        FloodMode = 0x1000,
        Gradient = 0x10,
        None = 0,
        Pen = 0x20,
        PenCaps = 0x40,
        Resampling = 0x100,
        SelectionCombineMode = 0x800,
        SelectionDrawMode = 0x2000,
        ShapeType = 0x80,
        Text = 0x200,
        Tolerance = 0x400
    }
}

