namespace PaintDotNet
{
    using PaintDotNet.Typography;
    using System;
    using System.Drawing;
    using System.Runtime.Serialization;

    [Serializable]
    internal class FontInfo : IDisposable, ISerializable, ICloneable
    {
        private string familyName;
        private const string fontFamilyNameTag = "family.Name";
        private float size;
        private const string sizeTag = "size";
        private System.Drawing.FontStyle style;
        private const string styleTag = "style";

        protected FontInfo(SerializationInfo info, StreamingContext context)
        {
            this.familyName = info.GetString("family.Name");
            this.size = info.GetSingle("size");
            int num = info.GetInt32("style");
            this.style = (System.Drawing.FontStyle) num;
        }

        public FontInfo(string fontFamilyName, float size, System.Drawing.FontStyle fontStyle)
        {
            this.FontFamilyName = fontFamilyName;
            this.Size = size;
            this.FontStyle = fontStyle;
        }

        public bool CanCreateFont() => 
            true;

        public FontInfo Clone() => 
            new FontInfo(this.familyName, this.size, this.style);

        public PaintDotNet.Typography.Font CreateFont(ITypographyService typographyService)
        {
            PaintDotNet.Typography.FontFamily family = typographyService.SystemFontFamilies.CreateFontFamily(this.familyName);
            FontStyleFlags styleFlags = this.style.ToFontStyleFlags();
            return family.CreateFont((double) this.size, styleFlags);
        }

        public void Dispose()
        {
        }

        public override bool Equals(object obj) => 
            (this == ((FontInfo) obj));

        public override int GetHashCode() => 
            HashCodeUtil.CombineHashCodes(this.familyName.GetHashCode(), this.size.GetHashCode(), (int) this.style);

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("family.Name", this.familyName);
            info.AddValue("size", this.size);
            info.AddValue("style", (int) this.style);
        }

        public static bool operator ==(FontInfo lhs, FontInfo rhs) => 
            (((lhs.familyName == rhs.familyName) && (lhs.size == rhs.size)) && (lhs.style == rhs.style));

        public static bool operator !=(FontInfo lhs, FontInfo rhs) => 
            !(lhs == rhs);

        object ICloneable.Clone() => 
            this.Clone();

        public string FontFamilyName
        {
            get => 
                this.familyName;
            set
            {
                this.familyName = value;
            }
        }

        public System.Drawing.FontStyle FontStyle
        {
            get => 
                this.style;
            set
            {
                this.style = value;
            }
        }

        public float Size
        {
            get => 
                this.size;
            set
            {
                this.size = value;
            }
        }
    }
}

