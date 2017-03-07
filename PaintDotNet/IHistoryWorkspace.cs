namespace PaintDotNet
{
    using System;

    internal interface IHistoryWorkspace
    {
        Layer ActiveLayer { get; }

        int ActiveLayerIndex { get; }

        PaintDotNet.Document Document { get; set; }

        PaintDotNet.Selection Selection { get; }
    }
}

