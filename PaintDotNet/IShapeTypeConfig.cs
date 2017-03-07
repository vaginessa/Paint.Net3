namespace PaintDotNet
{
    using System;

    internal interface IShapeTypeConfig
    {
        event EventHandler ShapeDrawTypeChanged;

        void PerformShapeDrawTypeChanged();

        PaintDotNet.ShapeDrawType ShapeDrawType { get; set; }
    }
}

