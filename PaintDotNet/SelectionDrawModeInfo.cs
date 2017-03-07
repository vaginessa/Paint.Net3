namespace PaintDotNet
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class SelectionDrawModeInfo : ICloneable, IDeserializationCallback
    {
        private SelectionDrawMode drawMode;
        private double height;
        private MeasurementUnit units;
        private double width;

        public SelectionDrawModeInfo(SelectionDrawMode drawMode, double width, double height, MeasurementUnit units)
        {
            this.drawMode = drawMode;
            this.width = width;
            this.height = height;
            this.units = units;
        }

        public SelectionDrawModeInfo Clone() => 
            new SelectionDrawModeInfo(this.drawMode, this.width, this.height, this.units);

        public SelectionDrawModeInfo CloneWithNewDrawMode(SelectionDrawMode newDrawMode) => 
            new SelectionDrawModeInfo(newDrawMode, this.width, this.height, this.units);

        public SelectionDrawModeInfo CloneWithNewHeight(double newHeight) => 
            new SelectionDrawModeInfo(this.drawMode, this.width, newHeight, this.units);

        public SelectionDrawModeInfo CloneWithNewUnits(MeasurementUnit newUnits) => 
            new SelectionDrawModeInfo(this.drawMode, this.width, this.height, newUnits);

        public SelectionDrawModeInfo CloneWithNewWidth(double newWidth) => 
            new SelectionDrawModeInfo(this.drawMode, newWidth, this.height, this.units);

        public SelectionDrawModeInfo CloneWithNewWidthAndHeight(double newWidth, double newHeight) => 
            new SelectionDrawModeInfo(this.drawMode, newWidth, newHeight, this.units);

        public static SelectionDrawModeInfo CreateDefault() => 
            new SelectionDrawModeInfo(SelectionDrawMode.Normal, 4.0, 3.0, MeasurementUnit.Inch);

        public override bool Equals(object obj)
        {
            SelectionDrawModeInfo info = obj as SelectionDrawModeInfo;
            if (info == null)
            {
                return false;
            }
            return ((((info.drawMode == this.drawMode) && (info.width == this.width)) && (info.height == this.height)) && (info.units == this.units));
        }

        public override int GetHashCode() => 
            HashCodeUtil.CombineHashCodes((int) this.drawMode, this.width.GetHashCode(), this.height.GetHashCode(), (int) this.units);

        object ICloneable.Clone() => 
            this.Clone();

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            switch (this.units)
            {
                case MeasurementUnit.Pixel:
                case MeasurementUnit.Inch:
                case MeasurementUnit.Centimeter:
                    break;

                default:
                    this.units = MeasurementUnit.Pixel;
                    break;
            }
        }

        public SelectionDrawMode DrawMode =>
            this.drawMode;

        public double Height =>
            this.height;

        public MeasurementUnit Units =>
            this.units;

        public double Width =>
            this.width;
    }
}

