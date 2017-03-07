namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class PdnStatusBar : StatusStrip, IStatusBarProgress
    {
        private ImageResource contextStatusImage;
        private ToolStripStatusLabel contextStatusLabel;
        private ToolStripStatusLabel cursorInfoStatusLabel;
        private ToolStripStatusLabel imageInfoStatusLabel;
        private ToolStripProgressBar progressStatusBar;
        private ToolStripSeparator progressStatusSeparator;
        private string progressTextFormat = PdnResources.GetString2("StatusBar.Progress.Percentage.Format");

        public PdnStatusBar()
        {
            this.InitializeComponent();
            this.cursorInfoStatusLabel.Image = PdnResources.GetImageResource2("Icons.CursorXYIcon.png").Reference;
            this.cursorInfoStatusLabel.Text = string.Empty;
            this.imageInfoStatusLabel.Image = PdnResources.GetImageResource2("Icons.ImageSizeIcon.png").Reference;
            this.progressStatusBar.Visible = false;
            this.progressStatusSeparator.Visible = false;
            this.progressStatusBar.Height -= 4;
            this.progressStatusBar.ProgressBar.Style = ProgressBarStyle.Continuous;
        }

        public void EraseProgressStatusBar()
        {
            try
            {
                this.progressStatusSeparator.Visible = false;
                this.progressStatusBar.Visible = false;
                this.progressStatusBar.Value = 0;
            }
            catch (NullReferenceException)
            {
            }
        }

        public void EraseProgressStatusBarAsync()
        {
            base.BeginInvoke(new Action(this.EraseProgressStatusBar));
        }

        public double GetProgressStatusBarValue()
        {
            lock (this.progressStatusBar)
            {
                return this.progressStatusBar.Value;
            }
        }

        private void InitializeComponent()
        {
            this.contextStatusLabel = new ToolStripStatusLabel();
            this.progressStatusSeparator = new ToolStripSeparator();
            this.progressStatusBar = new ToolStripProgressBar();
            this.imageInfoStatusLabel = new ToolStripStatusLabel();
            this.cursorInfoStatusLabel = new ToolStripStatusLabel();
            base.SuspendLayout();
            this.contextStatusLabel.Name = "contextStatusLabel";
            this.contextStatusLabel.Width = UI.ScaleWidth(0x1b4);
            this.contextStatusLabel.Spring = true;
            this.contextStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.contextStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.progressStatusBar.Name = "progressStatusBar";
            this.progressStatusBar.Width = 130;
            this.progressStatusBar.AutoSize = false;
            this.imageInfoStatusLabel.Name = "imageInfoStatusLabel";
            this.imageInfoStatusLabel.Width = UI.ScaleWidth(130);
            this.imageInfoStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.imageInfoStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.imageInfoStatusLabel.AutoSize = false;
            this.cursorInfoStatusLabel.Name = "cursorInfoStatusLabel";
            this.cursorInfoStatusLabel.Width = UI.ScaleWidth(130);
            this.cursorInfoStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.cursorInfoStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.cursorInfoStatusLabel.AutoSize = false;
            base.Name = "PdnStatusBar";
            this.Items.Add(this.contextStatusLabel);
            this.Items.Add(this.progressStatusSeparator);
            this.Items.Add(this.progressStatusBar);
            this.Items.Add(new ToolStripSeparator());
            this.Items.Add(this.imageInfoStatusLabel);
            this.Items.Add(new ToolStripSeparator());
            this.Items.Add(this.cursorInfoStatusLabel);
            base.ResumeLayout(false);
        }

        public void ResetProgressStatusBar()
        {
            try
            {
                this.progressStatusBar.Value = 0;
                this.progressStatusSeparator.Visible = true;
                this.progressStatusBar.Visible = true;
            }
            catch (NullReferenceException)
            {
            }
        }

        public void ResetProgressStatusBarAsync()
        {
            base.BeginInvoke(new Action(this.ResetProgressStatusBar));
        }

        public void SetProgressStatusBar(double percent)
        {
            lock (this.progressStatusBar)
            {
                this.progressStatusBar.Value = (int) percent;
                bool flag = percent != 100.0;
                this.progressStatusBar.Visible = flag;
                this.progressStatusSeparator.Visible = flag;
            }
        }

        public ImageResource ContextStatusImage
        {
            get => 
                this.contextStatusImage;
            set
            {
                this.contextStatusImage = value;
                if (this.contextStatusImage == null)
                {
                    this.contextStatusLabel.Image = null;
                }
                else
                {
                    this.contextStatusLabel.Image = this.contextStatusImage.Reference;
                }
                base.Update();
            }
        }

        public string ContextStatusText
        {
            get => 
                this.contextStatusLabel.Text;
            set
            {
                this.contextStatusLabel.Text = value;
                base.Update();
            }
        }

        public string CursorInfoText
        {
            get => 
                this.cursorInfoStatusLabel.Text;
            set
            {
                this.cursorInfoStatusLabel.Text = value;
                base.Update();
            }
        }

        public string ImageInfoStatusText
        {
            get => 
                this.imageInfoStatusLabel.Text;
            set
            {
                this.imageInfoStatusLabel.Text = value;
                base.Update();
            }
        }
    }
}

