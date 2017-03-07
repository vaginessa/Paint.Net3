namespace PaintDotNet
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    [Serializable]
    internal class BrushInfo : ICloneable
    {
        private PaintDotNet.BrushType brushType;
        private System.Drawing.Drawing2D.HatchStyle hatchStyle;

        public BrushInfo(PaintDotNet.BrushType brushType, System.Drawing.Drawing2D.HatchStyle hatchStyle)
        {
            this.brushType = brushType;
            this.hatchStyle = hatchStyle;
        }

        public BrushInfo Clone() => 
            new BrushInfo(this.brushType, this.hatchStyle);

        public Brush CreateBrush(Color foreColor, Color backColor)
        {
            if (this.brushType == PaintDotNet.BrushType.Solid)
            {
                return new SolidBrush(foreColor);
            }
            if (this.brushType != PaintDotNet.BrushType.Hatch)
            {
                throw new InvalidOperationException("BrushType is invalid");
            }
            return new HatchBrush(this.hatchStyle, foreColor, backColor);
        }

        object ICloneable.Clone() => 
            this.Clone();

        public PaintDotNet.BrushType BrushType
        {
            get => 
                this.brushType;
            set
            {
                this.brushType = value;
            }
        }

        public System.Drawing.Drawing2D.HatchStyle HatchStyle
        {
            get => 
                this.hatchStyle;
            set
            {
                this.hatchStyle = value;
            }
        }
    }
}

