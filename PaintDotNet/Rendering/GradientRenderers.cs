namespace PaintDotNet.Rendering
{
    using PaintDotNet;
    using System;
    using System.Windows;

    internal static class GradientRenderers
    {
        public sealed class Conical : GradientRenderer
        {
            private const double invPi = 0.31830988618379069;
            private double tOffset;

            public Conical(bool alphaOnly, BinaryPixelOp normalBlendOp) : base(alphaOnly, normalBlendOp)
            {
            }

            public override void BeforeRender()
            {
                this.tOffset = -this.ComputeUnboundedLerp((int) base.EndPoint.X, (int) base.EndPoint.Y);
                base.BeforeRender();
            }

            public override double BoundLerp(double t)
            {
                if (t > 1.0)
                {
                    t -= 2.0;
                }
                else if (t < -1.0)
                {
                    t += 2.0;
                }
                return Math.Abs(t).Clamp(0.0, 1.0);
            }

            public override double ComputeUnboundedLerp(int x, int y)
            {
                double num = x - base.StartPoint.X;
                double num2 = y - base.StartPoint.Y;
                double num4 = Math.Atan2(num2, num) * 0.31830988618379069;
                return (num4 + this.tOffset);
            }
        }

        public abstract class LinearBase : GradientRenderer
        {
            protected double dtdx;
            protected double dtdy;

            protected LinearBase(bool alphaOnly, BinaryPixelOp normalBlendOp) : base(alphaOnly, normalBlendOp)
            {
            }

            public override void BeforeRender()
            {
                Vector vector = (Vector) (base.EndPoint - base.StartPoint);
                double length = vector.Length;
                if (base.EndPoint.X == base.StartPoint.X)
                {
                    this.dtdx = 0.0;
                }
                else
                {
                    this.dtdx = vector.X / (length * length);
                }
                if (base.EndPoint.Y == base.StartPoint.Y)
                {
                    this.dtdy = 0.0;
                }
                else
                {
                    this.dtdy = vector.Y / (length * length);
                }
                base.BeforeRender();
            }
        }

        public sealed class LinearClamped : GradientRenderers.LinearStraight
        {
            public LinearClamped(bool alphaOnly, BinaryPixelOp normalBlendOp) : base(alphaOnly, normalBlendOp)
            {
            }

            public override double BoundLerp(double t) => 
                t.Clamp(0.0, 1.0);
        }

        public sealed class LinearDiamond : GradientRenderers.LinearStraight
        {
            public LinearDiamond(bool alphaOnly, BinaryPixelOp normalBlendOp) : base(alphaOnly, normalBlendOp)
            {
            }

            public override double BoundLerp(double t) => 
                t.Clamp(0.0, 1.0);

            public override double ComputeUnboundedLerp(int x, int y)
            {
                double num = x - base.StartPoint.X;
                double num2 = y - base.StartPoint.Y;
                double num3 = (num * base.dtdx) + (num2 * base.dtdy);
                double num4 = (num * base.dtdy) - (num2 * base.dtdx);
                double num5 = Math.Abs(num3);
                double num6 = Math.Abs(num4);
                return (num5 + num6);
            }
        }

        public sealed class LinearReflected : GradientRenderers.LinearStraight
        {
            public LinearReflected(bool alphaOnly, BinaryPixelOp normalBlendOp) : base(alphaOnly, normalBlendOp)
            {
            }

            public override double BoundLerp(double t) => 
                Math.Abs(t).Clamp(0.0, 1.0);
        }

        public abstract class LinearStraight : GradientRenderers.LinearBase
        {
            protected LinearStraight(bool alphaOnly, BinaryPixelOp normalBlendOp) : base(alphaOnly, normalBlendOp)
            {
            }

            public override double ComputeUnboundedLerp(int x, int y)
            {
                double num = x - base.StartPoint.X;
                double num2 = y - base.StartPoint.Y;
                return ((num * base.dtdx) + (num2 * base.dtdy));
            }
        }

        public sealed class Radial : GradientRenderer
        {
            private double invDistanceScale;

            public Radial(bool alphaOnly, BinaryPixelOp normalBlendOp) : base(alphaOnly, normalBlendOp)
            {
            }

            public override void BeforeRender()
            {
                double num = base.StartPoint.DistanceTo(base.EndPoint);
                if (num == 0.0)
                {
                    this.invDistanceScale = 0.0;
                }
                else
                {
                    this.invDistanceScale = 1.0 / num;
                }
                base.BeforeRender();
            }

            public override double BoundLerp(double t) => 
                t.Clamp(0.0, 1.0);

            public override double ComputeUnboundedLerp(int x, int y)
            {
                double num = x - base.StartPoint.X;
                double num2 = y - base.StartPoint.Y;
                return (Math.Sqrt((num * num) + (num2 * num2)) * this.invDistanceScale);
            }
        }
    }
}

