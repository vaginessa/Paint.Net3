namespace PaintDotNet
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class PenInfo : ICloneable, ISerializable
    {
        private float capScale;
        private System.Drawing.Drawing2D.DashStyle dashStyle;
        public const float DefaultCapScale = 1f;
        public const System.Drawing.Drawing2D.DashStyle DefaultDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        public const LineCap2 DefaultLineCap = LineCap2.Flat;
        private LineCap2 endCap;
        public const float MaxCapScale = 5f;
        public const float MinCapScale = 1f;
        private LineCap2 startCap;
        private float width;

        private PenInfo(SerializationInfo info, StreamingContext context)
        {
            this.dashStyle = (System.Drawing.Drawing2D.DashStyle) info.GetValue("dashStyle", typeof(System.Drawing.Drawing2D.DashStyle));
            this.width = info.GetSingle("width");
            try
            {
                this.startCap = (LineCap2) info.GetInt32("startCap");
            }
            catch (SerializationException)
            {
                this.startCap = LineCap2.Flat;
            }
            try
            {
                this.endCap = (LineCap2) info.GetInt32("endCap");
            }
            catch (SerializationException)
            {
                this.endCap = LineCap2.Flat;
            }
            try
            {
                this.capScale = info.GetSingle("capScale").Clamp(1f, 5f);
            }
            catch (SerializationException)
            {
                this.capScale = 1f;
            }
        }

        public PenInfo(System.Drawing.Drawing2D.DashStyle dashStyle, float width, LineCap2 startCap, LineCap2 endCap, float capScale)
        {
            this.dashStyle = dashStyle;
            this.width = width;
            this.capScale = capScale;
            this.startCap = startCap;
            this.endCap = endCap;
        }

        public PenInfo Clone() => 
            new PenInfo(this.dashStyle, this.width, this.startCap, this.endCap, this.capScale);

        public Pen CreatePen(BrushInfo brushInfo, Color foreColor, Color backColor)
        {
            Pen pen;
            LineCap cap;
            CustomLineCap cap2;
            LineCap cap3;
            CustomLineCap cap4;
            if (brushInfo.BrushType == PaintDotNet.BrushType.None)
            {
                pen = new Pen(foreColor, this.width);
            }
            else
            {
                pen = new Pen(brushInfo.CreateBrush(foreColor, backColor), this.width);
            }
            this.LineCapToLineCap2(this.startCap, out cap, out cap2);
            if (cap2 != null)
            {
                pen.CustomStartCap = cap2;
            }
            else
            {
                pen.StartCap = cap;
            }
            this.LineCapToLineCap2(this.endCap, out cap3, out cap4);
            if (cap4 != null)
            {
                pen.CustomEndCap = cap4;
            }
            else
            {
                pen.EndCap = cap3;
            }
            pen.DashStyle = this.dashStyle;
            return pen;
        }

        public override bool Equals(object obj)
        {
            PenInfo info = obj as PenInfo;
            if (info == null)
            {
                return false;
            }
            return (this == info);
        }

        public override int GetHashCode() => 
            ((((this.dashStyle.GetHashCode() ^ this.width.GetHashCode()) ^ this.startCap.GetHashCode()) ^ this.endCap.GetHashCode()) ^ this.capScale.GetHashCode());

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("dashStyle", this.dashStyle);
            info.AddValue("width", this.width);
            info.AddValue("startCap", (int) this.startCap);
            info.AddValue("endCap", (int) this.endCap);
            info.AddValue("capScale", this.capScale);
        }

        private void LineCapToLineCap2(LineCap2 cap2, out LineCap capResult, out CustomLineCap customCapResult)
        {
            switch (cap2)
            {
                case LineCap2.Flat:
                    capResult = LineCap.Flat;
                    customCapResult = null;
                    return;

                case LineCap2.Arrow:
                    capResult = LineCap.ArrowAnchor;
                    customCapResult = new AdjustableArrowCap(5f * this.capScale, 5f * this.capScale, false);
                    return;

                case LineCap2.ArrowFilled:
                    capResult = LineCap.ArrowAnchor;
                    customCapResult = new AdjustableArrowCap(5f * this.capScale, 5f * this.capScale, true);
                    return;

                case LineCap2.Rounded:
                    capResult = LineCap.Round;
                    customCapResult = null;
                    return;
            }
            throw new InvalidEnumArgumentException();
        }

        public static bool operator ==(PenInfo lhs, PenInfo rhs) => 
            ((((lhs.dashStyle == rhs.dashStyle) && (lhs.width == rhs.width)) && ((lhs.startCap == rhs.startCap) && (lhs.endCap == rhs.endCap))) && (lhs.capScale == rhs.capScale));

        public static bool operator !=(PenInfo lhs, PenInfo rhs) => 
            !(lhs == rhs);

        object ICloneable.Clone() => 
            this.Clone();

        private float CapScale
        {
            get => 
                this.capScale.Clamp(1f, 5f);
            set
            {
                this.capScale = value;
            }
        }

        public System.Drawing.Drawing2D.DashStyle DashStyle
        {
            get => 
                this.dashStyle;
            set
            {
                this.dashStyle = value;
            }
        }

        public LineCap2 EndCap
        {
            get => 
                this.endCap;
            set
            {
                this.endCap = value;
            }
        }

        public LineCap2 StartCap
        {
            get => 
                this.startCap;
            set
            {
                this.startCap = value;
            }
        }

        public float Width
        {
            get => 
                this.width;
            set
            {
                this.width = value;
            }
        }
    }
}

