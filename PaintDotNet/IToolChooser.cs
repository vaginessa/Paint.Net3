namespace PaintDotNet
{
    using PaintDotNet.Tools;
    using System;

    internal interface IToolChooser
    {
        event ToolClickedEventHandler ToolClicked;

        void SelectTool(Type toolType);
        void SelectTool(Type toolType, bool raiseEvent);
        void SetTools(ToolInfo[] toolInfos);
    }
}

