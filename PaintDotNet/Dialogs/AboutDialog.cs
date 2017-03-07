namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    internal class AboutDialog : PdnBaseForm
    {
        private Button closeButton;
        private TextBox copyrightLabel;
        private Label creditsLabel;
        private PdnBanner pdnBanner;
        private RichTextBox richCreditsBox;
        private PaintDotNet.Controls.SeparatorLine separator;
        private TextBox versionLabel;

        public AboutDialog()
        {
            this.DoubleBuffered = true;
            base.SuspendLayout();
            this.InitializeComponent();
            this.richCreditsBox.BackColor = SystemColors.Window;
            string format = PdnResources.GetString2("AboutDialog.Text.Format");
            this.Text = string.Format(format, PdnInfo.BareProductName);
            try
            {
                using (Stream stream = PdnResources.CreateResourceStream("Files.AboutCredits.rtf"))
                {
                    this.richCreditsBox.LoadFile(stream, RichTextBoxStreamType.RichText);
                }
            }
            catch (Exception)
            {
            }
            this.copyrightLabel.Text = PdnInfo.CopyrightString;
            base.Icon = PdnResources.GetIconFromImage("Icons.MenuHelpAboutIcon.png");
            this.closeButton.Text = PdnResources.GetString2("Form.CloseButton.Text");
            this.creditsLabel.Text = PdnResources.GetString2("AboutDialog.CreditsLabel.Text");
            this.versionLabel.Text = PdnInfo.FullAppName;
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = true;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeComponent()
        {
            this.closeButton = new Button();
            this.creditsLabel = new Label();
            this.richCreditsBox = new RichTextBox();
            this.copyrightLabel = new TextBox();
            this.pdnBanner = new PdnBanner();
            this.versionLabel = new TextBox();
            this.separator = new PaintDotNet.Controls.SeparatorLine();
            base.SuspendLayout();
            this.closeButton.DialogResult = DialogResult.Cancel;
            this.closeButton.AutoSize = true;
            this.closeButton.FlatStyle = FlatStyle.System;
            this.closeButton.Name = "okButton";
            this.closeButton.TabIndex = 0;
            this.creditsLabel.Location = new Point(7, 0x84);
            this.creditsLabel.Name = "creditsLabel";
            this.creditsLabel.Size = new Size(200, 0x10);
            this.creditsLabel.TabIndex = 5;
            this.richCreditsBox.CausesValidation = false;
            this.richCreditsBox.Location = new Point(10, 0x99);
            this.richCreditsBox.Name = "richCreditsBox";
            this.richCreditsBox.ReadOnly = true;
            this.richCreditsBox.Size = new Size(0x1dc, 0xe8);
            this.richCreditsBox.TabIndex = 6;
            this.richCreditsBox.Text = "";
            this.richCreditsBox.LinkClicked += new LinkClickedEventHandler(this.RichCreditsBox_LinkClicked);
            this.copyrightLabel.BorderStyle = BorderStyle.None;
            this.copyrightLabel.Location = new Point(10, 0x5f);
            this.copyrightLabel.Multiline = true;
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.ReadOnly = true;
            this.copyrightLabel.Size = new Size(0x1e1, 0x24);
            this.copyrightLabel.TabIndex = 4;
            this.pdnBanner.Location = new Point(0, 0);
            this.pdnBanner.Name = "pdnBanner";
            this.pdnBanner.Size = new Size(0x1ef, 0x47);
            this.pdnBanner.TabIndex = 7;
            this.versionLabel.BorderStyle = BorderStyle.None;
            this.versionLabel.Multiline = false;
            this.versionLabel.ReadOnly = true;
            this.versionLabel.Location = new Point(10, 0x4d);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new Size(0x1e1, 13);
            this.versionLabel.TabIndex = 8;
            this.separator.Name = "separator";
            base.AcceptButton = this.closeButton;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.closeButton;
            base.ClientSize = new Size(0x1ef, 440);
            base.Controls.Add(this.versionLabel);
            base.Controls.Add(this.copyrightLabel);
            base.Controls.Add(this.richCreditsBox);
            base.Controls.Add(this.creditsLabel);
            base.Controls.Add(this.pdnBanner);
            base.Controls.Add(this.separator);
            base.Controls.Add(this.closeButton);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Location = new Point(0, 0);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "AboutDialog";
            base.ShowInTaskbar = false;
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.StartPosition = FormStartPosition.CenterParent;
            base.Controls.SetChildIndex(this.closeButton, 0);
            base.Controls.SetChildIndex(this.pdnBanner, 0);
            base.Controls.SetChildIndex(this.creditsLabel, 0);
            base.Controls.SetChildIndex(this.richCreditsBox, 0);
            base.Controls.SetChildIndex(this.copyrightLabel, 0);
            base.Controls.SetChildIndex(this.versionLabel, 0);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num3;
            int num = UI.ScaleHeight(8);
            int num2 = Math.Max(0, num - base.ExtendedFramePadding.Bottom);
            if (base.IsGlassEffectivelyEnabled)
            {
                num3 = -1;
                this.separator.Visible = false;
            }
            else
            {
                num3 = UI.ScaleWidth(8);
                this.separator.Visible = true;
            }
            this.closeButton.Size = UI.ScaleSize(0x55, 0x17);
            this.closeButton.PerformLayout();
            this.closeButton.Location = new Point((base.ClientSize.Width - num3) - this.closeButton.Width, (base.ClientSize.Height - num2) - this.closeButton.Height);
            this.separator.Size = this.separator.GetPreferredSize(new Size(base.ClientSize.Width - (2 * num3), 1));
            this.separator.Location = new Point(num3, (this.closeButton.Top - num) - this.separator.Height);
            base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.separator.Top);
            base.OnLayout(levent);
        }

        private void RichCreditsBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if ((e.LinkText != null) && e.LinkText.StartsWith("http://"))
            {
                PdnInfo.OpenUrl2(this, e.LinkText);
            }
        }
    }
}

