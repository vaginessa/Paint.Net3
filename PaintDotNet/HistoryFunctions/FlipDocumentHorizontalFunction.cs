namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using System;

    internal sealed class FlipDocumentHorizontalFunction : FlipDocumentFunction
    {
        public FlipDocumentHorizontalFunction() : base(StaticName, PdnResources.GetImageResource2("Icons.MenuImageFlipHorizontalIcon.png"), FlipType.Horizontal)
        {
        }

        public static string StaticName =>
            PdnResources.GetString2("FlipDocumentHorizontalAction.Name");
    }
}

