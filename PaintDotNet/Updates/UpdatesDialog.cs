namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class UpdatesDialog : PdnBaseForm
    {
        private Button closeButton;
        private bool closeOnDoneState;
        private IContainer components;
        private Button continueButton;
        private PaintDotNet.Controls.HeadingLabel headerLabel;
        private Label infoText;
        private LinkLabel moreInfoLink;
        private Uri moreInfoTarget;
        private Label newVersionLabel;
        private Button optionsButton;
        private ProgressBar progressBar;
        private Label progressLabel;
        private StateMachineExecutor updatesStateMachine;
        private Label versionNameLabel;

        public UpdatesDialog()
        {
            this.InstallingOnExit = false;
            this.InitializeComponent();
            Image reference = PdnResources.GetImageResource2("Icons.MenuUtilitiesCheckForUpdatesIcon.png").Reference;
            base.Icon = Utility.ImageToIcon(reference, Utility.TransparentKey);
            if (Security.IsAdministrator)
            {
                this.optionsButton.Enabled = true;
            }
            else if (Security.CanElevateToAdministrator)
            {
                this.optionsButton.Enabled = true;
            }
            else
            {
                this.optionsButton.Enabled = false;
            }
            this.optionsButton.FlatStyle = FlatStyle.System;
            UI.EnableShield(this.optionsButton, true);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (this.updatesStateMachine != null)
            {
                this.updatesStateMachine.Abort();
                this.updatesStateMachine = null;
                this.closeButton.Enabled = false;
            }
            base.Close();
        }

        private void ContinueButton_Click(object sender, EventArgs e)
        {
            if (this.updatesStateMachine.CurrentState is ReadyToInstallState)
            {
                base.DialogResult = DialogResult.Yes;
                base.Hide();
                base.Close();
            }
            else
            {
                this.updatesStateMachine.ProcessInput(UpdatesAction.Continue);
                this.continueButton.Enabled = false;
            }
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
            this.closeButton = new Button();
            this.optionsButton = new Button();
            this.continueButton = new Button();
            this.progressBar = new ProgressBar();
            this.infoText = new Label();
            this.moreInfoLink = new LinkLabel();
            this.versionNameLabel = new Label();
            this.headerLabel = new PaintDotNet.Controls.HeadingLabel();
            this.newVersionLabel = new Label();
            this.progressLabel = new Label();
            base.SuspendLayout();
            this.closeButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.closeButton.AutoSize = true;
            this.closeButton.Location = new Point(0x106, 0x8f);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new Size(0x4b, 0x17);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "_close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.FlatStyle = FlatStyle.System;
            this.closeButton.Click += new EventHandler(this.CloseButton_Click);
            this.optionsButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.optionsButton.AutoSize = true;
            this.optionsButton.Click += new EventHandler(this.OptionsButton_Click);
            this.optionsButton.Location = new Point(0xa6, 0x8f);
            this.optionsButton.Name = "optionsButton";
            this.optionsButton.Size = new Size(0x5b, 0x17);
            this.optionsButton.TabIndex = 1;
            this.optionsButton.Text = "_options...";
            this.optionsButton.FlatStyle = FlatStyle.System;
            this.optionsButton.UseVisualStyleBackColor = true;
            this.continueButton.AutoSize = true;
            this.continueButton.Location = new Point(7, 100);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new Size(0x4b, 0x17);
            this.continueButton.TabIndex = 3;
            this.continueButton.Text = "_continue";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.FlatStyle = FlatStyle.System;
            this.continueButton.Click += new EventHandler(this.ContinueButton_Click);
            this.progressBar.Location = new Point(9, 0x67);
            this.progressBar.MarqueeAnimationSpeed = 40;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(0x126, 0x12);
            this.progressBar.TabIndex = 4;
            this.infoText.Location = new Point(7, 7);
            this.infoText.Name = "infoText";
            this.infoText.Size = new Size(0x149, 0x2d);
            this.infoText.TabIndex = 2;
            this.infoText.Text = ".blahblahblah";
            this.moreInfoLink.AutoSize = true;
            this.moreInfoLink.Location = new Point(7, 0x4b);
            this.moreInfoLink.Name = "moreInfoLink";
            this.moreInfoLink.Size = new Size(0x42, 13);
            this.moreInfoLink.TabIndex = 5;
            this.moreInfoLink.TabStop = true;
            this.moreInfoLink.Text = "_more Info...";
            this.moreInfoLink.Click += new EventHandler(this.MoreInfoLink_Click);
            this.versionNameLabel.AutoSize = true;
            this.versionNameLabel.Location = new Point(0x58, 0x39);
            this.versionNameLabel.Name = "versionNameLabel";
            this.versionNameLabel.Size = new Size(0x54, 13);
            this.versionNameLabel.TabIndex = 6;
            this.versionNameLabel.Text = ".paint.net vX.YZ";
            this.headerLabel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.headerLabel.Location = new Point(9, 0x7e);
            this.headerLabel.Name = "headerLabel";
            this.headerLabel.RightMargin = 0;
            this.headerLabel.Size = new Size(0x147, 15);
            this.headerLabel.TabIndex = 0;
            this.headerLabel.TabStop = false;
            this.newVersionLabel.AutoSize = true;
            this.newVersionLabel.Location = new Point(7, 0x39);
            this.newVersionLabel.Name = "newVersionLabel";
            this.newVersionLabel.Size = new Size(70, 13);
            this.newVersionLabel.TabIndex = 7;
            this.newVersionLabel.Text = ".new version:";
            this.progressLabel.AutoSize = true;
            this.progressLabel.Location = new Point(310, 0x69);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new Size(0, 13);
            this.progressLabel.TabIndex = 8;
            this.progressLabel.TextAlign = ContentAlignment.MiddleRight;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.closeButton;
            base.ClientSize = new Size(0x157, 0xac);
            base.Controls.Add(this.progressLabel);
            base.Controls.Add(this.newVersionLabel);
            base.Controls.Add(this.headerLabel);
            base.Controls.Add(this.versionNameLabel);
            base.Controls.Add(this.moreInfoLink);
            base.Controls.Add(this.continueButton);
            base.Controls.Add(this.infoText);
            base.Controls.Add(this.optionsButton);
            base.Controls.Add(this.closeButton);
            base.Controls.Add(this.progressBar);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "UpdatesDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void MoreInfoLink_Click(object sender, EventArgs e)
        {
            PdnInfo.OpenUrl2(this, this.moreInfoTarget.ToString());
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.updatesStateMachine.CurrentState is ReadyToInstallState)
            {
                this.UpdatesStateMachine_StateBegin(this, new EventArgs<PaintDotNet.State>(this.updatesStateMachine.CurrentState));
            }
            base.OnLoad(e);
        }

        private void OptionsButton_Click(object sender, EventArgs e)
        {
            UpdatesOptionsDialog.ShowUpdateOptionsDialog(this, true);
        }

        private void UpdateDynamicUI()
        {
            this.Text = PdnResources.GetString2("UpdatesDialog.Text");
            string str = PdnResources.GetString2("UpdatesDialog.CloseButton.Text");
            this.optionsButton.Text = PdnResources.GetString2("UpdatesDialog.OptionsButton.Text");
            this.moreInfoLink.Text = PdnResources.GetString2("UpdatesDialog.MoreInfoLink.Text");
            this.newVersionLabel.Text = PdnResources.GetString2("UpdatesDialog.NewVersionLabel.Text");
            if ((this.updatesStateMachine == null) || (this.updatesStateMachine.CurrentState == null))
            {
                this.infoText.Text = string.Empty;
                this.continueButton.Text = string.Empty;
                this.continueButton.Enabled = false;
                this.continueButton.Visible = false;
                this.moreInfoLink.Visible = false;
                this.moreInfoLink.Enabled = false;
                this.versionNameLabel.Visible = false;
                this.versionNameLabel.Enabled = false;
            }
            else
            {
                UpdatesState currentState = (UpdatesState) this.updatesStateMachine.CurrentState;
                if (currentState is ReadyToInstallState)
                {
                    ((ReadyToInstallState) currentState).InstallingOnExit = this.InstallingOnExit;
                    UI.EnableShield(this.continueButton, true);
                }
                else
                {
                    UI.EnableShield(this.continueButton, false);
                }
                this.infoText.Text = currentState.InfoText;
                this.continueButton.Text = currentState.ContinueButtonText;
                this.continueButton.Visible = currentState.ContinueButtonVisible;
                this.continueButton.Enabled = currentState.ContinueButtonVisible;
                this.progressBar.Style = (currentState.MarqueeStyle == MarqueeStyle.Marquee) ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                this.progressBar.Visible = currentState.MarqueeStyle != MarqueeStyle.None;
                this.progressLabel.Visible = this.progressBar.Visible;
                if ((this.continueButton.Enabled || (currentState is ErrorState)) || (currentState is DoneState))
                {
                    str = PdnResources.GetString2("UpdatesDialog.CloseButton.Text");
                }
                else
                {
                    str = PdnResources.GetString2("Form.CancelButton.Text");
                }
                if (currentState is ErrorState)
                {
                    Size proposedSize = new Size(this.infoText.Width, 1);
                    Size preferredSize = this.infoText.GetPreferredSize(proposedSize);
                    this.infoText.Size = preferredSize;
                }
                INewVersionInfo info = currentState as INewVersionInfo;
                if (info != null)
                {
                    this.versionNameLabel.Text = info.NewVersionInfo.FriendlyName;
                    this.versionNameLabel.Visible = true;
                    this.versionNameLabel.Enabled = true;
                    this.moreInfoTarget = new Uri(info.NewVersionInfo.InfoUrl);
                    this.moreInfoLink.Visible = true;
                    this.moreInfoLink.Enabled = true;
                    this.newVersionLabel.Visible = true;
                    this.newVersionLabel.Font = new Font(this.newVersionLabel.Font, this.newVersionLabel.Font.Style | FontStyle.Bold);
                    this.versionNameLabel.Left = this.newVersionLabel.Right;
                    this.moreInfoLink.Left = this.versionNameLabel.Left;
                }
                else
                {
                    this.newVersionLabel.Visible = false;
                    this.versionNameLabel.Visible = false;
                    this.versionNameLabel.Enabled = false;
                    this.moreInfoLink.Visible = false;
                    this.moreInfoLink.Enabled = false;
                }
            }
            this.closeButton.Text = str;
            base.Update();
        }

        private void UpdatesStateMachine_StateBegin(object sender, EventArgs<PaintDotNet.State> e)
        {
            this.progressBar.Value = 0;
            this.UpdateDynamicUI();
            if ((e.Data is DoneState) && this.closeOnDoneState)
            {
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
            else if (e.Data is ReadyToCheckState)
            {
                this.updatesStateMachine.ProcessInput(UpdatesAction.Continue);
            }
            else if (e.Data is ReadyToInstallState)
            {
                base.ClientSize = new Size(base.ClientSize.Width, this.continueButton.Bottom + UI.ScaleHeight(7));
                this.closeButton.Enabled = false;
                this.closeButton.Visible = false;
                this.optionsButton.Enabled = false;
                this.optionsButton.Visible = false;
                this.headerLabel.Visible = false;
                this.continueButton.Location = new Point((base.ClientSize.Width - this.continueButton.Width) - UI.ScaleWidth(7), (base.ClientSize.Height - this.continueButton.Height) - UI.ScaleHeight(8));
            }
            else if (e.Data is AbortedState)
            {
                base.DialogResult = DialogResult.Abort;
                base.Close();
            }
        }

        private void UpdatesStateMachine_StateMachineBegin(object sender, EventArgs e)
        {
            this.UpdateDynamicUI();
        }

        private void UpdatesStateMachine_StateMachineFinished(object sender, EventArgs e)
        {
            this.UpdateDynamicUI();
        }

        private void UpdatesStateMachine_StateProgress(object sender, ProgressEventArgs e)
        {
            int num = ((int) e.Percent).Clamp(this.progressBar.Minimum, this.progressBar.Maximum);
            this.progressBar.Value = num;
            string str2 = string.Format(PdnResources.GetString2("UpdatesDialog.ProgressLabel.Text.Format"), num.ToString());
            this.progressLabel.Text = str2;
            this.UpdateDynamicUI();
        }

        private void UpdatesStateMachine_StateWaitingForInput(object sender, EventArgs<PaintDotNet.State> e)
        {
            this.continueButton.Enabled = true;
            this.UpdateDynamicUI();
        }

        public bool InstallingOnExit { get; set; }

        public StateMachineExecutor UpdatesStateMachine
        {
            get => 
                this.updatesStateMachine;
            set
            {
                if (this.updatesStateMachine != null)
                {
                    this.updatesStateMachine.StateBegin -= new EventHandler<EventArgs<PaintDotNet.State>>(this.UpdatesStateMachine_StateBegin);
                    this.updatesStateMachine.StateMachineBegin -= new EventHandler(this.UpdatesStateMachine_StateMachineBegin);
                    this.updatesStateMachine.StateMachineFinished -= new EventHandler(this.UpdatesStateMachine_StateMachineFinished);
                    this.updatesStateMachine.StateProgress -= new ProgressEventHandler(this.UpdatesStateMachine_StateProgress);
                    this.updatesStateMachine.StateWaitingForInput -= new EventHandler<EventArgs<PaintDotNet.State>>(this.UpdatesStateMachine_StateWaitingForInput);
                }
                this.updatesStateMachine = value;
                if (this.updatesStateMachine != null)
                {
                    this.updatesStateMachine.StateBegin += new EventHandler<EventArgs<PaintDotNet.State>>(this.UpdatesStateMachine_StateBegin);
                    this.updatesStateMachine.StateMachineBegin += new EventHandler(this.UpdatesStateMachine_StateMachineBegin);
                    this.updatesStateMachine.StateMachineFinished += new EventHandler(this.UpdatesStateMachine_StateMachineFinished);
                    this.updatesStateMachine.StateProgress += new ProgressEventHandler(this.UpdatesStateMachine_StateProgress);
                    this.updatesStateMachine.StateWaitingForInput += new EventHandler<EventArgs<PaintDotNet.State>>(this.UpdatesStateMachine_StateWaitingForInput);
                }
                this.UpdateDynamicUI();
            }
        }
    }
}

