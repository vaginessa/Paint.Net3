namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Functional;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Updates;
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class CheckForUpdatesMenuItem : PdnMenuItem
    {
        private bool calledFinish;
        private bool installOnExit;
        private System.Windows.Forms.Timer retryDialogTimer;
        private StateMachineExecutor stateMachineExecutor;
        private UpdatesDialog updatesDialog;
        private UpdatesStateMachine updatesStateMachine;

        public CheckForUpdatesMenuItem()
        {
            base.Name = "CheckForUpdates";
        }

        private void AppWorkspace_HandleCreated(object sender, EventArgs e)
        {
            base.AppWorkspace.HandleCreated -= new EventHandler(this.AppWorkspace_HandleCreated);
            this.StartUpdates();
        }

        private DialogResult AskInstallNowOrOnExit(IWin32Window owner, string newVersionName, string moreInfoUrl)
        {
            Image reference;
            Icon icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuUtilitiesCheckForUpdatesIcon.png").Reference, false);
            string str = PdnResources.GetString2("UpdatePromptTaskDialog.Title");
            ImageResource resource = PdnResources.GetImageResource2("Images.UpdatePromptTaskDialog.TaskImage.png");
            try
            {
                reference = resource.Reference;
            }
            catch (Exception)
            {
                reference = null;
            }
            string str2 = PdnResources.GetString2("UpdatePromptTaskDialog.IntroText");
            TaskAuxLabel label = new TaskAuxLabel {
                Text = newVersionName,
                TextFont = new Font(this.Font.FontFamily, this.Font.SizeInPoints * 1.25f, FontStyle.Bold)
            };
            TaskButton button = new TaskButton(PdnResources.GetImageResource2("Icons.Updates.InstallAtExit.png").Reference, PdnResources.GetString2("UpdatePromptTaskDialog.InstallOnExitTB.ActionText"), PdnResources.GetString2("UpdatePromptTaskDialog.InstallOnExitTB.DescriptionText"));
            TaskButton tail = new TaskButton(PdnResources.GetImageResource2("Icons.Updates.InstallNow.png").Reference, PdnResources.GetString2("UpdatePromptTaskDialog.InstallNowTB.ActionText"), PdnResources.GetString2("UpdatePromptTaskDialog.InstallNowTB.DescriptionText"));
            string str3 = PdnResources.GetString2("UpdatePromptTaskDialog.AuxButtonText");
            Action auxButtonClickHandler = delegate {
                () => Shell.LaunchUrl2(this.AppWorkspace, moreInfoUrl).Eval<bool>().Observe();
            };
            TaskAuxButton button3 = new TaskAuxButton {
                Text = str3
            };
            button3.Clicked += delegate (object s, EventArgs e) {
                auxButtonClickHandler();
            };
            TaskButton[] buttonArray = (from tb in Enumerable.Empty<TaskButton>().Concat<TaskButton>(((PdnInfo.IsExpired || Shell.IsActivityQueuedForRestart) ? null : button)).Concat<TaskButton>(tail)
                where tb != null
                select tb).ToArrayEx<TaskButton>();
            TaskDialog dialog2 = new TaskDialog {
                Icon = icon,
                Title = str,
                TaskImage = reference,
                IntroText = str2,
                TaskButtons = buttonArray,
                AcceptButton = tail,
                CancelButton = null,
                PixelWidth96Dpi = (TaskDialog.DefaultPixelWidth96Dpi * 3) / 2,
                AuxControls = new TaskAuxControl[] { 
                    label,
                    button3
                }
            };
            TaskButton button4 = dialog2.Show(owner);
            if (button4 == tail)
            {
                return DialogResult.Yes;
            }
            if (button4 == button)
            {
                return DialogResult.OK;
            }
            return DialogResult.Cancel;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeUpdates();
            }
            base.Dispose(disposing);
        }

        private void DisposeUpdates()
        {
            if (this.stateMachineExecutor != null)
            {
                this.stateMachineExecutor.StateMachineFinished -= new EventHandler(this.OnStateMachineFinished);
                this.stateMachineExecutor.StateBegin -= new EventHandler<EventArgs<PaintDotNet.State>>(this.OnStateBegin);
                this.stateMachineExecutor.StateWaitingForInput -= new EventHandler<EventArgs<PaintDotNet.State>>(this.OnStateWaitingForInput);
                this.stateMachineExecutor.Dispose();
                this.stateMachineExecutor = null;
            }
            this.updatesStateMachine = null;
        }

        private void InitUpdates()
        {
            this.updatesStateMachine = new UpdatesStateMachine();
            this.updatesStateMachine.UIContext = base.AppWorkspace;
            this.stateMachineExecutor = new StateMachineExecutor(this.updatesStateMachine);
            this.stateMachineExecutor.SyncContext = base.AppWorkspace;
            this.stateMachineExecutor.StateMachineFinished += new EventHandler(this.OnStateMachineFinished);
            this.stateMachineExecutor.StateBegin += new EventHandler<EventArgs<PaintDotNet.State>>(this.OnStateBegin);
            this.stateMachineExecutor.StateWaitingForInput += new EventHandler<EventArgs<PaintDotNet.State>>(this.OnStateWaitingForInput);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!e.Cancel)
            {
                switch (e.CloseReason)
                {
                    case CloseReason.WindowsShutDown:
                        return;
                }
                if (this.installOnExit)
                {
                    this.ShowUpdatesDialog(true);
                }
            }
        }

        protected override void OnAppWorkspaceChanged()
        {
            if (((this.updatesStateMachine == null) && !PdnInfo.IsExpired) && (Security.IsAdministrator || Security.CanElevateToAdministrator))
            {
                this.StartUpdates();
            }
            base.OnAppWorkspaceChanged();
        }

        protected override void OnClick(EventArgs e)
        {
            if (!Security.IsAdministrator && !Security.CanElevateToAdministrator)
            {
                Utility.ShowNonAdminErrorBox(base.AppWorkspace);
            }
            else
            {
                if (this.updatesStateMachine == null)
                {
                    this.InitUpdates();
                }
                this.ShowUpdatesDialog();
            }
            base.OnClick(e);
        }

        private void OnStateBegin(object sender, EventArgs<PaintDotNet.State> e)
        {
            EventHandler handler = null;
            if ((e.Data is UpdateAvailableState) && (this.updatesDialog == null))
            {
                bool flag = true;
                PdnBaseForm form2 = base.AppWorkspace.FindForm() as PdnBaseForm;
                if ((form2 != null) && !form2.IsCurrentModalForm)
                {
                    flag = false;
                }
                if (flag)
                {
                    this.ShowUpdatesDialog();
                }
                else
                {
                    if (this.retryDialogTimer != null)
                    {
                        this.retryDialogTimer.Enabled = false;
                        this.retryDialogTimer.Dispose();
                        this.retryDialogTimer = null;
                    }
                    this.retryDialogTimer = new System.Windows.Forms.Timer();
                    this.retryDialogTimer.Interval = 0xbb8;
                    if (handler == null)
                    {
                        handler = delegate (object sender2, EventArgs e2) {
                            bool flag = false;
                            if (base.IsDisposed)
                            {
                                flag = true;
                            }
                            PdnBaseForm form2 = base.AppWorkspace.FindForm() as PdnBaseForm;
                            if (form2 == null)
                            {
                                flag = true;
                            }
                            else if (this.updatesDialog != null)
                            {
                                flag = true;
                            }
                            else if (form2.IsCurrentModalForm && form2.Enabled)
                            {
                                this.ShowUpdatesDialog();
                                flag = true;
                            }
                            if (flag && (this.retryDialogTimer != null))
                            {
                                this.retryDialogTimer.Enabled = false;
                                this.retryDialogTimer.Dispose();
                                this.retryDialogTimer = null;
                            }
                        };
                    }
                    this.retryDialogTimer.Tick += handler;
                    this.retryDialogTimer.Enabled = true;
                }
            }
            else if ((e.Data is ReadyToCheckState) && (this.updatesDialog == null))
            {
                this.DisposeUpdates();
            }
        }

        private void OnStateMachineFinished(object sender, EventArgs e)
        {
            if (!this.installOnExit)
            {
                this.DisposeUpdates();
            }
        }

        private void OnStateWaitingForInput(object sender, EventArgs<PaintDotNet.State> e)
        {
            InstallingState data = e.Data as InstallingState;
            if (data != null)
            {
                data.Finish(base.AppWorkspace);
                this.calledFinish = true;
            }
        }

        private void ShowUpdatesDialog()
        {
            this.ShowUpdatesDialog(false);
        }

        private void ShowUpdatesDialog(bool calledFromExit)
        {
            EventHandler handler = null;
            if (!calledFromExit)
            {
                if (this.installOnExit && Shell.IsActivityQueuedForRestart)
                {
                    Shell.IsActivityQueuedForRestart = false;
                }
                this.installOnExit = false;
            }
            Form form = base.AppWorkspace.FindForm();
            if (form != null)
            {
                form.FormClosing -= new FormClosingEventHandler(this.MainForm_FormClosing);
            }
            if (this.retryDialogTimer != null)
            {
                this.retryDialogTimer.Enabled = false;
                this.retryDialogTimer.Dispose();
                this.retryDialogTimer = null;
            }
            bool flag = true;
            UpdateAvailableState currentState = this.stateMachineExecutor.CurrentState as UpdateAvailableState;
            if (currentState != null)
            {
                PdnVersionInfo newVersionInfo = currentState.NewVersionInfo;
                string friendlyName = newVersionInfo.FriendlyName;
                string infoUrl = newVersionInfo.InfoUrl;
                switch (this.AskInstallNowOrOnExit(base.AppWorkspace, friendlyName, infoUrl))
                {
                    case DialogResult.Yes:
                        this.stateMachineExecutor.ProcessInput(UpdatesAction.Continue);
                        flag = true;
                        goto Label_012D;

                    case DialogResult.OK:
                    {
                        this.stateMachineExecutor.ProcessInput(UpdatesAction.Continue);
                        flag = false;
                        this.installOnExit = true;
                        Shell.IsActivityQueuedForRestart = true;
                        Form form2 = base.AppWorkspace.FindForm();
                        if (form2 != null)
                        {
                            form2.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing);
                        }
                        else
                        {
                            flag = true;
                        }
                        goto Label_012D;
                    }
                }
                this.stateMachineExecutor.ProcessInput(UpdatesAction.Cancel);
                flag = false;
                this.DisposeUpdates();
            }
        Label_012D:
            if (flag)
            {
                if (this.updatesDialog != null)
                {
                    this.updatesDialog.Close();
                    this.updatesDialog = null;
                }
                this.updatesDialog = new UpdatesDialog();
                this.updatesDialog.InstallingOnExit = calledFromExit;
                this.updatesDialog.UpdatesStateMachine = this.stateMachineExecutor;
                if (!this.stateMachineExecutor.IsStarted)
                {
                    this.stateMachineExecutor.Start();
                }
                IWin32Window owner = (IWin32Window) (base.AppWorkspace ?? null);
                this.updatesDialog.StartPosition = calledFromExit ? FormStartPosition.CenterScreen : this.updatesDialog.StartPosition;
                this.updatesDialog.ShowInTaskbar = calledFromExit;
                if (handler == null)
                {
                    handler = (s, e) => UI.FlashForm(this.updatesDialog);
                }
                this.updatesDialog.Shown += handler;
                this.updatesDialog.ShowDialog(owner);
                DialogResult dialogResult = this.updatesDialog.DialogResult;
                this.updatesDialog.Dispose();
                this.updatesDialog = null;
                if (((this.stateMachineExecutor != null) && (dialogResult == DialogResult.Yes)) && (this.stateMachineExecutor.CurrentState is ReadyToInstallState))
                {
                    this.stateMachineExecutor.ProcessInput(UpdatesAction.Continue);
                    while (!this.calledFinish)
                    {
                        Application.DoEvents();
                        Thread.Sleep(10);
                    }
                }
            }
        }

        private void StartUpdates()
        {
            if (!base.AppWorkspace.IsHandleCreated)
            {
                base.AppWorkspace.HandleCreated += new EventHandler(this.AppWorkspace_HandleCreated);
            }
            else
            {
                this.InitUpdates();
                this.stateMachineExecutor.Start();
            }
        }

        public bool InstallingOnExit =>
            this.installOnExit;
    }
}

