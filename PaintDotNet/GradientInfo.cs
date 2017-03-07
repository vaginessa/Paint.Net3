namespace PaintDotNet
{
    using PaintDotNet.Rendering;
    using System;
    using System.ComponentModel;

    [Serializable]
    internal sealed class GradientInfo : ICloneable
    {
        private bool alphaOnly;
        private PaintDotNet.GradientType gradientType;

        public GradientInfo(PaintDotNet.GradientType gradientType, bool alphaOnly)
        {
            this.gradientType = gradientType;
            this.alphaOnly = alphaOnly;
        }

        public GradientInfo Clone() => 
            new GradientInfo(this.gradientType, this.alphaOnly);

        public GradientRenderer CreateGradientRenderer()
        {
            UserBlendOps.NormalBlendOp @static = UserBlendOps.NormalBlendOp.Static;
            switch (this.gradientType)
            {
                case PaintDotNet.GradientType.LinearClamped:
                    return new GradientRenderers.LinearClamped(this.alphaOnly, @static);

                case PaintDotNet.GradientType.LinearReflected:
                    return new GradientRenderers.LinearReflected(this.alphaOnly, @static);

                case PaintDotNet.GradientType.LinearDiamond:
                    return new GradientRenderers.LinearDiamond(this.alphaOnly, @static);

                case PaintDotNet.GradientType.Radial:
                    return new GradientRenderers.Radial(this.alphaOnly, @static);

                case PaintDotNet.GradientType.Conical:
                    return new GradientRenderers.Conical(this.alphaOnly, @static);
            }
            throw new InvalidEnumArgumentException();
        }

        public override bool Equals(object obj)
        {
            GradientInfo info = obj as GradientInfo;
            return ((info?.GradientType == this.GradientType) && (info.AlphaOnly == this.AlphaOnly));
        }

        public override int GetHashCode() => 
            (this.gradientType.GetHashCode() + this.alphaOnly.GetHashCode());

        object ICloneable.Clone() => 
            this.Clone();

        public bool AlphaOnly =>
            this.alphaOnly;

        public PaintDotNet.GradientType GradientType =>
            this.gradientType;
    }
}

