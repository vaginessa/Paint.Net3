namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.HistoryFunctions;
    using System;

    internal class FlipLayerHorizontalFunction : FlipLayerFunction
    {
        public FlipLayerHorizontalFunction(int layerIndex) : base(StaticName, PdnResources.GetImageResource2("Icons.MenuLayersFlipHorizontalIcon.png"), FlipType.Horizontal, layerIndex)
        {
        }

        public static string StaticName =>
            PdnResources.GetString2("FlipLayerHorizontalAction.Name");
    }
}

