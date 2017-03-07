namespace PaintDotNet.Controls
{
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class SeparatorLine : Control
    {
        private Label label;
        private bool selfDrawn;

        public SeparatorLine()
        {
            this.InitForCurrentVisualStyle();
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            base.TabStop = false;
            base.SetStyle(ControlStyles.Selectable, false);
        }

        public override Size GetPreferredSize(Size proposedSize) => 
            new Size(proposedSize.Width, 2);

        public void InitForCurrentVisualStyle()
        {
            switch (UI.VisualStyleClass)
            {
                case VisualStyleClass.Classic:
                case VisualStyleClass.Luna:
                case VisualStyleClass.Other:
                    this.selfDrawn = false;
                    break;

                case VisualStyleClass.Aero:
                    this.selfDrawn = true;
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }
            if ((this.selfDrawn && (this.label != null)) && base.Controls.Contains(this.label))
            {
                base.SuspendLayout();
                base.Controls.Remove(this.label);
                base.ResumeLayout(false);
                base.PerformLayout();
                base.Invalidate(true);
            }
            else if (!this.selfDrawn && ((this.label != null) || !base.Controls.Contains(this.label)))
            {
                if (this.label == null)
                {
                    this.label = new Label();
                    this.label.BorderStyle = BorderStyle.Fixed3D;
                }
                base.SuspendLayout();
                base.Controls.Add(this.label);
                base.ResumeLayout(false);
                base.PerformLayout();
                base.Invalidate(true);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (!this.selfDrawn)
            {
                this.label.Bounds = new Rectangle(0, 0, base.Width, base.Height);
            }
            base.OnLayout(levent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.selfDrawn)
            {
                GroupBoxRenderer.DrawGroupBox(e.Graphics, new Rectangle(0, 0, base.Width, 1), GroupBoxState.Normal);
            }
            base.OnPaint(e);
        }
    }
}

