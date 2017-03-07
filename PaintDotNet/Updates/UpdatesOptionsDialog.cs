namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class UpdatesOptionsDialog : PdnBaseForm
    {
        private Label allUsersNoticeLabel;
        private CheckBox autoCheckBox;
        private CheckBox betaCheckBox;
        private Button cancelButton;
        public const string CommandLineParameter = "/updateOptions";
        private IContainer components;
        private PaintDotNet.Controls.HeadingLabel headerLabel1;
        private Button saveButton;

        private UpdatesOptionsDialog()
        {
            this.InitializeComponent();
        }

        private void AutoCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.betaCheckBox.Enabled = this.autoCheckBox.Checked;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.saveButton = new Button();
            this.autoCheckBox = new CheckBox();
            this.betaCheckBox = new CheckBox();
            this.allUsersNoticeLabel = new Label();
            this.cancelButton = new Button();
            this.headerLabel1 = new PaintDotNet.Controls.HeadingLabel();
            base.SuspendLayout();
            this.saveButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.saveButton.Location = new Point(0xec, 0x5f);
            this.saveButton.Name = "saveButton4";
            this.saveButton.Size = new Size(0x4b, 0x17);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = ".save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.FlatStyle = FlatStyle.System;
            this.saveButton.Click += new EventHandler(this.SaveButton_Click);
            this.autoCheckBox.AutoSize = true;
            this.autoCheckBox.Location = new Point(8, 9);
            this.autoCheckBox.Name = "autoCheckBox";
            this.autoCheckBox.Size = new Size(80, 0x11);
            this.autoCheckBox.TabIndex = 1;
            this.autoCheckBox.Text = "checkBox1";
            this.autoCheckBox.UseVisualStyleBackColor = true;
            this.autoCheckBox.FlatStyle = FlatStyle.System;
            this.autoCheckBox.CheckedChanged += new EventHandler(this.AutoCheckBox_CheckedChanged);
            this.betaCheckBox.AutoSize = true;
            this.betaCheckBox.Location = new Point(0x1a, 0x21);
            this.betaCheckBox.Name = "betaCheckBox";
            this.betaCheckBox.Size = new Size(80, 0x11);
            this.betaCheckBox.TabIndex = 2;
            this.betaCheckBox.Text = "checkBox1";
            this.betaCheckBox.FlatStyle = FlatStyle.System;
            this.betaCheckBox.UseVisualStyleBackColor = true;
            this.allUsersNoticeLabel.AutoSize = true;
            this.allUsersNoticeLabel.Location = new Point(7, 0x3f);
            this.allUsersNoticeLabel.Name = "allUsersNoticeLabel";
            this.allUsersNoticeLabel.Size = new Size(0x4e, 13);
            this.allUsersNoticeLabel.TabIndex = 4;
            this.allUsersNoticeLabel.Text = ".allUsersNotice";
            this.cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Location = new Point(0x13c, 0x5f);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(0x4b, 0x17);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = ".cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.FlatStyle = FlatStyle.System;
            this.cancelButton.Click += new EventHandler(this.CancelButton_Click);
            this.headerLabel1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.headerLabel1.Location = new Point(7, 80);
            this.headerLabel1.Name = "headerLabel1";
            this.headerLabel1.RightMargin = 0;
            this.headerLabel1.Size = new Size(0x180, 14);
            this.headerLabel1.TabIndex = 6;
            this.headerLabel1.TabStop = false;
            base.AcceptButton = this.saveButton;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.cancelButton;
            base.ClientSize = new Size(0x18e, 0x7d);
            base.Controls.Add(this.headerLabel1);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.betaCheckBox);
            base.Controls.Add(this.autoCheckBox);
            base.Controls.Add(this.saveButton);
            base.Controls.Add(this.allUsersNoticeLabel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "UpdatesOptionsDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "UpdatesOptionsDialog";
            base.Controls.SetChildIndex(this.allUsersNoticeLabel, 0);
            base.Controls.SetChildIndex(this.saveButton, 0);
            base.Controls.SetChildIndex(this.autoCheckBox, 0);
            base.Controls.SetChildIndex(this.betaCheckBox, 0);
            base.Controls.SetChildIndex(this.cancelButton, 0);
            base.Controls.SetChildIndex(this.headerLabel1, 0);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString2("UpdatesOptionsDialog.Text");
            Image reference = PdnResources.GetImageResource2("Icons.SettingsIcon.png").Reference;
            base.Icon = Utility.ImageToIcon(reference, Utility.TransparentKey, false);
            this.saveButton.Text = PdnResources.GetString2("UpdatesOptionsDialog.SaveButton.Text");
            this.autoCheckBox.Text = PdnResources.GetString2("UpdatesOptionsDialog.AutoCheckBox.Text");
            this.betaCheckBox.Text = PdnResources.GetString2("UpdatesOptionsDialog.BetaCheckBox.Text");
            this.allUsersNoticeLabel.Text = PdnResources.GetString2("UpdatesOptionsDialog.AllUsersNoticeLabel.Text");
            this.cancelButton.Text = PdnResources.GetString2("Form.CancelButton.Text");
            base.LoadResources();
        }

        private void LoadSettings()
        {
            bool flag = Settings.SystemWide.GetString("CHECKFORUPDATES", "0") == "1";
            this.autoCheckBox.Checked = flag;
            bool flag2 = Settings.SystemWide.GetString("CHECKFORBETAS", "0") == "1";
            this.betaCheckBox.Checked = flag2;
            this.betaCheckBox.Enabled = this.autoCheckBox.Checked;
        }

        protected override void OnLoad(EventArgs e)
        {
            this.LoadSettings();
            this.LoadResources();
            base.OnLoad(e);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            this.SaveSettings();
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        private void SaveSettings()
        {
            string str = this.autoCheckBox.Checked ? "1" : "0";
            Settings.SystemWide.SetString("CHECKFORUPDATES", str);
            string str2 = this.betaCheckBox.Checked ? "1" : "0";
            Settings.SystemWide.SetString("CHECKFORBETAS", str2);
        }

        public static void ShowUpdateOptionsDialog(IWin32Window owner)
        {
            ShowUpdateOptionsDialog(owner, false);
        }

        public static void ShowUpdateOptionsDialog(IWin32Window owner, bool allowNewInstance)
        {
            if (Security.IsAdministrator)
            {
                UpdatesOptionsDialog dialog = new UpdatesOptionsDialog();
                if (owner == null)
                {
                    dialog.ShowInTaskbar = true;
                }
                dialog.ShowDialog(owner);
            }
            else if (Security.CanElevateToAdministrator && allowNewInstance)
            {
                Startup.StartNewInstance(owner, true, new string[] { "/updateOptions" });
            }
            else
            {
                Utility.ShowNonAdminErrorBox(owner);
            }
        }
    }
}

