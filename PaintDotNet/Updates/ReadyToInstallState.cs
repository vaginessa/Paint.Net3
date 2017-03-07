namespace PaintDotNet.Updates
{
    using PaintDotNet;
    using System;
    using System.Runtime.InteropServices;

    internal class ReadyToInstallState : UpdatesState, INewVersionInfo
    {
        private string installerPath;
        private bool installingOnExit;
        private PdnVersionInfo newVersionInfo;

        public ReadyToInstallState(string installerPath, PdnVersionInfo newVersionInfo) : base(false, true, MarqueeStyle.None)
        {
            this.installerPath = installerPath;
            this.newVersionInfo = newVersionInfo;
        }

        public override void OnEnteredState()
        {
        }

        public override void ProcessInput(object input, out PaintDotNet.State newState)
        {
            if (input.Equals(UpdatesAction.Continue))
            {
                newState = new InstallingState(this.installerPath);
            }
            else
            {
                if (!input.Equals(UpdatesAction.Cancel))
                {
                    throw new ArgumentException();
                }
                newState = new DoneState();
            }
        }

        public override string InfoText
        {
            get
            {
                if (this.installingOnExit)
                {
                    return PdnResources.GetString2("UpdatesDialog.InfoText.Text.ReadyToInstallState.InstallOnExit");
                }
                return base.InfoText;
            }
        }

        public bool InstallingOnExit
        {
            get => 
                this.installingOnExit;
            set
            {
                this.installingOnExit = value;
            }
        }

        public PdnVersionInfo NewVersionInfo =>
            this.newVersionInfo;
    }
}

