namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal static class DocumentBoxExtensions
    {
        public static Point CanvasToClient(this DocumentBox box, Int32Point surfacePt) => 
            box.ScaleFactor.Scale(surfacePt);

        public static Point CanvasToClient(this DocumentBox box, Point surfacePt) => 
            box.ScaleFactor.Scale(surfacePt);

        public static Rect CanvasToClient(this DocumentBox box, Rect surfaceRect) => 
            new Rect(box.CanvasToClient(surfaceRect.Location), box.CanvasToClient(surfaceRect.Size));

        public static Size CanvasToClient(this DocumentBox box, Size surfaceSize) => 
            box.ScaleFactor.Scale(surfaceSize);

        public static Point ClientToCanvas(this DocumentBox box, Point clientPt) => 
            box.ScaleFactor.Unscale(clientPt);

        public static Rect ClientToCanvas(this DocumentBox box, Rect clientRect) => 
            new Rect(box.ClientToCanvas(clientRect.Location), box.ClientToCanvas(clientRect.Size));

        public static Size ClientToCanvas(this DocumentBox box, Size clientSize) => 
            box.ScaleFactor.Unscale(clientSize);
    }
}

