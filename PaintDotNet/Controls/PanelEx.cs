namespace PaintDotNet.Controls
{
    using PaintDotNet.SystemLayer;
    using System;
    using System.Windows.Forms;

    internal class PanelEx : ScrollPanel
    {
        private bool hideHScroll;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.hideHScroll)
            {
                UI.SuspendControlPainting(this);
            }
            base.OnSizeChanged(e);
            if (this.hideHScroll)
            {
                UI.HideHorizontalScrollBar(this);
                UI.ResumeControlPainting(this);
                base.Invalidate(true);
            }
        }

        public bool HideHScroll
        {
            get => 
                this.hideHScroll;
            set
            {
                this.hideHScroll = value;
            }
        }
    }
}

