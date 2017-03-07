namespace PaintDotNet.Updates
{
    using ICSharpCode.SharpZipLib.Zip;
    using PaintDotNet;
    using PaintDotNet.IO;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class ExtractingState : UpdatesState, INewVersionInfo
    {
        private SiphonStream abortMeStream;
        private Exception exception;
        private string extractMe;
        private string installerPath;
        private PdnVersionInfo newVersionInfo;

        public ExtractingState(string extractMe, PdnVersionInfo newVersionInfo) : base(false, false, MarqueeStyle.Smooth)
        {
            this.extractMe = extractMe;
            this.newVersionInfo = newVersionInfo;
        }

        protected override void OnAbort()
        {
            SiphonStream abortMeStream = this.abortMeStream;
            if (abortMeStream != null)
            {
                abortMeStream.Abort(new Exception());
            }
            base.OnAbort();
        }

        public override void OnEnteredState()
        {
            try
            {
                this.OnEnteredStateImpl();
            }
            catch (Exception exception)
            {
                this.exception = exception;
                base.StateMachine.QueueInput(PrivateInput.GoToError);
            }
        }

        public void OnEnteredStateImpl()
        {
            FileStream baseInputStream = new FileStream(this.extractMe, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream underlyingStream = null;
            try
            {
                ZipEntry nextEntry;
                ZipInputStream input = new ZipInputStream(baseInputStream);
                bool flag = false;
                do
                {
                    nextEntry = input.GetNextEntry();
                    if (nextEntry == null)
                    {
                        goto Label_004D;
                    }
                }
                while (nextEntry.IsDirectory || (string.Compare(".exe", Path.GetExtension(nextEntry.Name), true, CultureInfo.InvariantCulture) != 0));
                flag = true;
            Label_004D:
                if (!flag)
                {
                    this.exception = new FileNotFoundException();
                    base.StateMachine.QueueInput(PrivateInput.GoToError);
                }
                else
                {
                    int maxBytes = (int) nextEntry.Size;
                    int bytesSoFar = 0;
                    this.installerPath = Path.Combine(Path.GetDirectoryName(this.extractMe), nextEntry.Name);
                    underlyingStream = new FileStream(this.installerPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                    SiphonStream output = new SiphonStream(underlyingStream, 0x1000);
                    this.abortMeStream = output;
                    IOEventHandler handler = delegate (object sender, IOEventArgs e) {
                        bytesSoFar += e.Count;
                        double percent = 100.0 * (((double) bytesSoFar) / ((double) maxBytes));
                        this.OnProgress(percent);
                    };
                    base.OnProgress(0.0);
                    if (maxBytes > 0)
                    {
                        output.IOFinished += handler;
                    }
                    StreamUtil.CopyStream(input, output);
                    if (maxBytes > 0)
                    {
                        output.IOFinished -= handler;
                    }
                    this.abortMeStream = null;
                    output = null;
                    underlyingStream.Close();
                    underlyingStream = null;
                    input.Close();
                    input = null;
                    base.StateMachine.QueueInput(PrivateInput.GoToReadyToInstall);
                }
            }
            catch (Exception exception)
            {
                if (base.AbortRequested)
                {
                    base.StateMachine.QueueInput(PrivateInput.GoToAborted);
                }
                else
                {
                    this.exception = exception;
                    base.StateMachine.QueueInput(PrivateInput.GoToError);
                }
            }
            finally
            {
                if (underlyingStream != null)
                {
                    underlyingStream.Close();
                    underlyingStream = null;
                }
                if (baseInputStream != null)
                {
                    baseInputStream.Close();
                    baseInputStream = null;
                }
                if (((this.exception != null) || base.AbortRequested) && (this.installerPath != null))
                {
                    FileSystem.TryDeleteFile(this.installerPath);
                }
                if (this.extractMe != null)
                {
                    FileSystem.TryDeleteFile(this.extractMe);
                }
            }
        }

        public override void ProcessInput(object input, out PaintDotNet.State newState)
        {
            if (input.Equals(PrivateInput.GoToReadyToInstall))
            {
                newState = new ReadyToInstallState(this.installerPath, this.newVersionInfo);
            }
            else if (input.Equals(PrivateInput.GoToError))
            {
                string errorMessage = PdnResources.GetString2("Updates.ExtractingState.GenericError");
                newState = new ErrorState(this.exception, errorMessage);
            }
            else
            {
                if (!input.Equals(PrivateInput.GoToAborted))
                {
                    throw new ArgumentException();
                }
                newState = new AbortedState();
            }
        }

        public override bool CanAbort =>
            true;

        public PdnVersionInfo NewVersionInfo =>
            this.newVersionInfo;
    }
}

