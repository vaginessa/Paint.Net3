namespace PaintDotNet.Canvas
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class CanvasGridRenderer : CanvasLayer
    {
        public CanvasGridRenderer(CanvasRenderer canvasRenderer) : base(canvasRenderer)
        {
        }

        public override void OnCanvasSizeChanged()
        {
            if (base.Visible)
            {
                base.OwnerCanvas.InvalidateLookups();
            }
            base.OnCanvasSizeChanged();
        }

        protected override unsafe void OnRender(ISurface<ColorBgra> dst, Int32Point renderOffset)
        {
            if (base.OwnerCanvas.ScaleFactor >= new ScaleFactor(2, 1))
            {
                int[] numArray = base.OwnerCanvas.Dst2CanvasLookupX;
                int[] numArray2 = base.OwnerCanvas.Dst2CanvasLookupY;
                int[] numArray3 = base.OwnerCanvas.Canvas2DstLookupX;
                int[] numArray4 = base.OwnerCanvas.Canvas2DstLookupY;
                int num = numArray2[renderOffset.Y];
                int num2 = numArray2[renderOffset.Y + dst.Height];
                int num3 = renderOffset.X & 1;
                for (int i = num; i <= num2; i++)
                {
                    int num5 = numArray4[i];
                    int row = num5 - renderOffset.Y;
                    if (dst.CheckRowValue<ColorBgra>(row))
                    {
                        ColorBgra* rowPointer = (ColorBgra*) dst.GetRowPointer<ColorBgra>(row);
                        ColorBgra* bgraPtr2 = rowPointer + dst.Width;
                        for (rowPointer += num3; rowPointer < bgraPtr2; rowPointer += 2)
                        {
                            rowPointer[0] = ColorBgra.Black;
                        }
                    }
                }
                int num7 = numArray[renderOffset.X];
                int num8 = numArray[renderOffset.X + dst.Width];
                int num9 = renderOffset.Y & 1;
                for (int j = num7; j <= num8; j++)
                {
                    int num11 = numArray3[j];
                    int column = num11 - renderOffset.X;
                    if (dst.CheckColumnValue<ColorBgra>(column))
                    {
                        byte* numPtr = (byte*) dst.GetPointPointer<ColorBgra>(column, 0).ToPointer();
                        byte* numPtr2 = numPtr + ((byte*) (dst.Stride * dst.Height));
                        for (numPtr += (byte*) (num9 * dst.Stride); numPtr < numPtr2; numPtr += dst.Stride + dst.Stride)
                        {
                            numPtr[0] = (byte) ColorBgra.Black;
                        }
                    }
                }
            }
        }

        public override void OnRenderDstSizeChanged()
        {
            if (base.Visible)
            {
                base.OwnerCanvas.InvalidateLookups();
            }
            base.OnRenderDstSizeChanged();
        }

        protected override void OnVisibleChanged()
        {
            if (base.Visible)
            {
                base.OwnerCanvas.InvalidateLookups();
            }
            base.Invalidate();
        }
    }
}

