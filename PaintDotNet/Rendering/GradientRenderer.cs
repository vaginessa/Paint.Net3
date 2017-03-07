namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;

    internal abstract class GradientRenderer
    {
        private bool alphaBlending;
        private bool alphaOnly;
        private ColorBgra endColor;
        private Point endPoint;
        private byte[] lerpAlphas;
        private bool lerpCacheIsValid;
        private ColorBgra[] lerpColors;
        private BinaryPixelOp normalBlendOp;
        private ColorBgra startColor;
        private Point startPoint;

        protected GradientRenderer(bool alphaOnly, BinaryPixelOp normalBlendOp)
        {
            this.normalBlendOp = normalBlendOp;
            this.alphaOnly = alphaOnly;
        }

        public virtual void AfterRender()
        {
        }

        public virtual void BeforeRender()
        {
            if (!this.lerpCacheIsValid)
            {
                byte a;
                byte num2;
                if (this.alphaOnly)
                {
                    ComputeAlphaOnlyValuesFromColors(this.startColor, this.endColor, out a, out num2);
                }
                else
                {
                    a = this.startColor.A;
                    num2 = this.endColor.A;
                }
                this.lerpAlphas = new byte[0x100];
                this.lerpColors = new ColorBgra[0x100];
                for (int i = 0; i < 0x100; i++)
                {
                    byte index = (byte) i;
                    this.lerpColors[index] = ColorBgra.Blend(this.startColor, this.endColor, index);
                    this.lerpAlphas[index] = (byte) (a + (((num2 - a) * index) / 0xff));
                }
                this.lerpCacheIsValid = true;
            }
        }

        public abstract double BoundLerp(double t);
        private static void ComputeAlphaOnlyValuesFromColors(ColorBgra startColor, ColorBgra endColor, out byte startAlpha, out byte endAlpha)
        {
            startAlpha = startColor.A;
            endAlpha = (byte) (0xff - endColor.A);
        }

        public abstract double ComputeUnboundedLerp(int x, int y);
        public unsafe void Render(Surface surface, Int32Rect[] rois, int startIndex, int length)
        {
            byte a;
            byte num2;
            if (this.alphaOnly)
            {
                ComputeAlphaOnlyValuesFromColors(this.startColor, this.endColor, out a, out num2);
            }
            else
            {
                a = this.startColor.A;
                num2 = this.endColor.A;
            }
            for (int i = startIndex; i < (startIndex + length); i++)
            {
                Int32Rect rect = rois[i];
                if (this.startPoint == this.endPoint)
                {
                    for (int j = rect.Y; j < (rect.Y + rect.Height); j++)
                    {
                        ColorBgra* pointAddress = surface.GetPointAddress(rect.X, j);
                        for (int k = rect.X; k < (rect.X + rect.Width); k++)
                        {
                            ColorBgra endColor;
                            if (this.alphaOnly && this.alphaBlending)
                            {
                                byte num6 = (byte) ((ushort) (pointAddress->A * num2)).FastDivideByByte(0xff);
                                endColor = pointAddress[0];
                                endColor.A = num6;
                            }
                            else if (this.alphaOnly && !this.alphaBlending)
                            {
                                endColor = pointAddress[0];
                                endColor.A = num2;
                            }
                            else if (!this.alphaOnly && this.alphaBlending)
                            {
                                endColor = this.normalBlendOp.Apply(pointAddress[0], this.endColor);
                            }
                            else
                            {
                                endColor = this.endColor;
                            }
                            pointAddress[0] = endColor;
                            pointAddress++;
                        }
                    }
                }
                else
                {
                    for (int m = rect.Y; m < (rect.Y + rect.Height); m++)
                    {
                        ColorBgra* bgraPtr2 = surface.GetPointAddress(rect.X, m);
                        if (this.alphaOnly && this.alphaBlending)
                        {
                            for (int n = rect.X; n < (rect.X + rect.Width); n++)
                            {
                                double t = this.ComputeUnboundedLerp(n, m);
                                byte index = (byte) (this.BoundLerp(t) * 255.0);
                                byte frac = this.lerpAlphas[index];
                                byte num13 = ByteUtil.FastScale(bgraPtr2->A, frac);
                                bgraPtr2->A = num13;
                                bgraPtr2++;
                            }
                        }
                        else if (this.alphaOnly && !this.alphaBlending)
                        {
                            for (int num14 = rect.X; num14 < (rect.X + rect.Width); num14++)
                            {
                                double num15 = this.ComputeUnboundedLerp(num14, m);
                                byte num17 = (byte) (this.BoundLerp(num15) * 255.0);
                                byte num18 = this.lerpAlphas[num17];
                                bgraPtr2->A = num18;
                                bgraPtr2++;
                            }
                        }
                        else if ((!this.alphaOnly && this.alphaBlending) && ((a != 0xff) || (num2 != 0xff)))
                        {
                            for (int num19 = rect.X; num19 < (rect.X + rect.Width); num19++)
                            {
                                double num20 = this.ComputeUnboundedLerp(num19, m);
                                byte num22 = (byte) (this.BoundLerp(num20) * 255.0);
                                ColorBgra rhs = this.lerpColors[num22];
                                ColorBgra bgra3 = this.normalBlendOp.Apply(bgraPtr2[0], rhs);
                                bgraPtr2[0] = bgra3;
                                bgraPtr2++;
                            }
                        }
                        else
                        {
                            for (int num23 = rect.X; num23 < (rect.X + rect.Width); num23++)
                            {
                                double num24 = this.ComputeUnboundedLerp(num23, m);
                                byte num26 = (byte) (this.BoundLerp(num24) * 255.0);
                                ColorBgra bgra4 = this.lerpColors[num26];
                                bgraPtr2[0] = bgra4;
                                bgraPtr2++;
                            }
                        }
                    }
                }
            }
            this.AfterRender();
        }

        public bool AlphaBlending
        {
            get => 
                this.alphaBlending;
            set
            {
                this.alphaBlending = value;
            }
        }

        public bool AlphaOnly
        {
            get => 
                this.alphaOnly;
            set
            {
                this.alphaOnly = value;
            }
        }

        public ColorBgra EndColor
        {
            get => 
                this.endColor;
            set
            {
                if (this.endColor != value)
                {
                    this.endColor = value;
                    this.lerpCacheIsValid = false;
                }
            }
        }

        public Point EndPoint
        {
            get => 
                this.endPoint;
            set
            {
                this.endPoint = value;
            }
        }

        public ColorBgra StartColor
        {
            get => 
                this.startColor;
            set
            {
                if (this.startColor != value)
                {
                    this.startColor = value;
                    this.lerpCacheIsValid = false;
                }
            }
        }

        public Point StartPoint
        {
            get => 
                this.startPoint;
            set
            {
                this.startPoint = value;
            }
        }
    }
}

