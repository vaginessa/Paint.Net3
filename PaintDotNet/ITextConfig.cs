namespace PaintDotNet
{
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;

    internal interface ITextConfig
    {
        event EventHandler FontAlignmentChanged;

        event EventHandler FontInfoChanged;

        event EventHandler FontSmoothingChanged;

        TextAlignment FontAlignment { get; set; }

        string FontFamilyName { get; set; }

        PaintDotNet.FontInfo FontInfo { get; set; }

        float FontSize { get; set; }

        PaintDotNet.SystemLayer.FontSmoothing FontSmoothing { get; set; }

        System.Drawing.FontStyle FontStyle { get; set; }
    }
}

