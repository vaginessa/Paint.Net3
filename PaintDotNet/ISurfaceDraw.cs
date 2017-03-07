namespace PaintDotNet
{
    using System;

    internal interface ISurfaceDraw
    {
        void Draw(Surface dst);
        void Draw(Surface dst, IPixelOp pixelOp);
        void Draw(Surface dst, int tX, int tY);
        void Draw(Surface dst, int tX, int tY, IPixelOp pixelOp);
    }
}

