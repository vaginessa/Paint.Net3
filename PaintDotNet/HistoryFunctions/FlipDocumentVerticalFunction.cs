namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using System;

    internal sealed class FlipDocumentVerticalFunction : FlipDocumentFunction
    {
        public FlipDocumentVerticalFunction() : base(StaticName, PdnResources.GetImageResource2("Icons.MenuImageFlipVerticalIcon.png"), FlipType.Vertical)
        {
        }

        public static string StaticName =>
            PdnResources.GetString2("FlipDocumentVerticalAction.Name");
    }
}

