﻿namespace PaintDotNet.Controls
{
    using PaintDotNet.SystemLayer;
    using System;
    using System.Windows.Forms;

    internal class PdnToolStripSplitButton : ToolStripSplitButton
    {
        public PdnToolStripSplitButton()
        {
            base.DropDownButtonWidth += UI.ScaleWidth(2);
        }
    }
}

