namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using System;

    internal sealed class ToolInfo
    {
        private string helpText;
        private char hotKey;
        private ImageResource image;
        private string name;
        private bool skipIfActiveOnHotKey;
        private PaintDotNet.ToolBarConfigItems toolBarConfigItems;
        private Type toolType;

        public ToolInfo(string name, string helpText, ImageResource image, char hotKey, bool skipIfActiveOnHotKey, PaintDotNet.ToolBarConfigItems toolBarConfigItems, Type toolType)
        {
            this.name = name;
            this.helpText = helpText;
            this.image = image;
            this.hotKey = hotKey;
            this.skipIfActiveOnHotKey = skipIfActiveOnHotKey;
            this.toolBarConfigItems = toolBarConfigItems;
            this.toolType = toolType;
        }

        public override bool Equals(object obj)
        {
            ToolInfo info = obj as ToolInfo;
            if (info == null)
            {
                return false;
            }
            return ((((this.name == info.name) && (this.helpText == info.helpText)) && ((this.hotKey == info.hotKey) && (this.skipIfActiveOnHotKey == info.skipIfActiveOnHotKey))) && (this.toolType == info.toolType));
        }

        public override int GetHashCode() => 
            this.name.GetHashCode();

        public string HelpText =>
            this.helpText;

        public char HotKey =>
            this.hotKey;

        public ImageResource Image =>
            this.image;

        public string Name =>
            this.name;

        public bool SkipIfActiveOnHotKey =>
            this.skipIfActiveOnHotKey;

        public PaintDotNet.ToolBarConfigItems ToolBarConfigItems =>
            this.toolBarConfigItems;

        public Type ToolType =>
            this.toolType;
    }
}

