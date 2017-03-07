namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.HistoryFunctions;
    using System;

    internal class FlipLayerVerticalFunction : FlipLayerFunction
    {
        public FlipLayerVerticalFunction(int layerIndex) : base(StaticName, PdnResources.GetImageResource2("Icons.MenuLayersFlipVerticalIcon.png"), FlipType.Vertical, layerIndex)
        {
        }

        public static string StaticName =>
            PdnResources.GetString2("FlipLayerVerticalAction.Name");
    }
}

