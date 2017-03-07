namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Functional;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal class SaveConfigDialog : PdnBaseDialog
    {
        private volatile bool callbackBusy;
        private ManualResetEvent callbackDoneEvent = new ManualResetEvent(true);
        private IContainer components;
        private Button defaultsButton;
        private bool disposeDocument;
        private PaintDotNet.Document document;
        private bool documentMouseDown;
        private DocumentView documentView;
        private string fileSizeTextFormat;
        private System.Threading.Timer fileSizeTimer;
        private PaintDotNet.FileType fileType;
        private Hashtable fileTypeToSaveToken = new Hashtable();
        private PaintDotNet.Controls.SeparatorLine footerSeparator;
        private Cursor handIcon = PdnResources.GetCursor2("Cursors.PanToolCursor.cur");
        private Cursor handIconMouseDown = PdnResources.GetCursor2("Cursors.PanToolCursorMouseDown.cur");
        private System.Drawing.Point lastMouseXY;
        private PaintDotNet.Controls.HeadingLabel previewHeader;
        private Panel saveConfigPanel;
        private SaveConfigWidget saveConfigWidget;
        private Surface scratchSurface;
        private PaintDotNet.Controls.HeadingLabel settingsHeader;
        private const int timerDelayTime = 100;
        private static readonly System.Drawing.Size unscaledMinSize = new System.Drawing.Size(600, 380);

        public event ProgressEventHandler Progress;

        public SaveConfigDialog()
        {
            base.SuspendLayout();
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = true;
            this.fileSizeTimer = new System.Threading.Timer(new TimerCallback(this.FileSizeTimerCallback), null, 0x3e8, -1);
            this.InitializeComponent();
            this.Text = PdnResources.GetString2("SaveConfigDialog.Text");
            this.fileSizeTextFormat = PdnResources.GetString2("SaveConfigDialog.PreviewHeader.Text.Format");
            this.settingsHeader.Text = PdnResources.GetString2("SaveConfigDialog.SettingsHeader.Text");
            this.defaultsButton.Text = PdnResources.GetString2("SaveConfigDialog.DefaultsButton.Text");
            this.previewHeader.Text = PdnResources.GetString2("SaveConfigDialog.PreviewHeader.Text");
            base.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuFileSaveIcon.png").Reference);
            this.documentView.Cursor = this.handIcon;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void BaseCancelButton_Click(object sender, EventArgs e)
        {
            this.UIWaitForCallbackDoneEvent(WaitActionType.Cancel);
            this.CleanupTimer();
        }

        private void BaseOkButton_Click(object sender, EventArgs e)
        {
            this.UIWaitForCallbackDoneEvent(WaitActionType.Ok);
            this.CleanupTimer();
        }

        private void CleanupTimer()
        {
            if (this.fileSizeTimer != null)
            {
                new Action(this.fileSizeTimer.Dispose).Try().Observe();
                this.fileSizeTimer = null;
            }
        }

        private void DefaultsButton_Click(object sender, EventArgs e)
        {
            this.SaveConfigToken = this.FileType.CreateDefaultSaveConfigToken();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.disposeDocument && (this.documentView.Document != null))
                {
                    PaintDotNet.Document document = this.documentView.Document;
                    this.documentView.Document = null;
                    document.Dispose();
                }
                this.CleanupTimer();
                if (this.handIcon != null)
                {
                    this.handIcon.Dispose();
                    this.handIcon = null;
                }
                if (this.handIconMouseDown != null)
                {
                    this.handIconMouseDown.Dispose();
                    this.handIconMouseDown = null;
                }
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DocumentView_DocumentMouseDown(object sender, MouseEventArgsF e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.documentMouseDown = true;
                this.documentView.Cursor = this.handIconMouseDown;
                this.lastMouseXY = new System.Drawing.Point(e.X, e.Y);
            }
        }

        private void DocumentView_DocumentMouseMove(object sender, MouseEventArgsF e)
        {
            if (this.documentMouseDown)
            {
                System.Drawing.Point point = new System.Drawing.Point(e.X, e.Y);
                System.Drawing.Size size = new System.Drawing.Size(point.X - this.lastMouseXY.X, point.Y - this.lastMouseXY.Y);
                if ((size.Width != 0) || (size.Height != 0))
                {
                    System.Windows.Point documentScrollPosition = this.documentView.DocumentScrollPosition;
                    System.Windows.Point point3 = new System.Windows.Point(documentScrollPosition.X - size.Width, documentScrollPosition.Y - size.Height);
                    this.documentView.DocumentScrollPosition = point3;
                    this.documentView.Update();
                    this.lastMouseXY = point;
                    this.lastMouseXY.X -= size.Width;
                    this.lastMouseXY.Y -= size.Height;
                }
            }
        }

        private void DocumentView_DocumentMouseUp(object sender, MouseEventArgsF e)
        {
            this.documentMouseDown = false;
            this.documentView.Cursor = this.handIcon;
        }

        private void FileSizeProgressEventHandler(object state, ProgressEventArgs e)
        {
            if (base.IsHandleCreated)
            {
                base.BeginInvoke(new Action<int>(this.SetFileSizeProgress), new object[] { (int) e.Percent });
            }
        }

        private void FileSizeTimerCallback(object state)
        {
            try
            {
                if (base.IsHandleCreated)
                {
                    if (this.callbackBusy)
                    {
                        base.Invoke(new Action(this.QueueFileSizeTextUpdate));
                    }
                    else
                    {
                        try
                        {
                            this.FileSizeTimerCallbackImpl(state);
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void FileSizeTimerCallbackImpl(object state)
        {
            if (this.fileSizeTimer != null)
            {
                this.callbackBusy = true;
                try
                {
                    if (this.Document != null)
                    {
                        string tempFileName = Path.GetTempFileName();
                        FileStream output = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                        this.FileType.Save(this.Document, output, this.SaveConfigToken, this.scratchSurface, new ProgressEventHandler(this.FileSizeProgressEventHandler), true);
                        output.Flush();
                        output.Close();
                        base.BeginInvoke(new Action<string>(this.UpdateFileSizeAndPreview), new object[] { tempFileName });
                    }
                }
                catch (Exception)
                {
                    base.BeginInvoke(new Action<string>(this.UpdateFileSizeAndPreview), new object[1]);
                }
                finally
                {
                    this.callbackDoneEvent.Set();
                    this.callbackBusy = false;
                }
            }
        }

        private void InitializeComponent()
        {
            this.saveConfigPanel = new Panel();
            this.defaultsButton = new Button();
            this.saveConfigWidget = new SaveConfigWidget();
            this.previewHeader = new PaintDotNet.Controls.HeadingLabel();
            this.documentView = new DocumentView();
            this.settingsHeader = new PaintDotNet.Controls.HeadingLabel();
            this.footerSeparator = new PaintDotNet.Controls.SeparatorLine();
            base.SuspendLayout();
            base.baseOkButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            base.baseOkButton.FlatStyle = FlatStyle.System;
            base.baseOkButton.Name = "baseOkButton";
            base.baseOkButton.TabIndex = 2;
            base.baseOkButton.Click += new EventHandler(this.BaseOkButton_Click);
            base.baseCancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            base.baseCancelButton.FlatStyle = FlatStyle.System;
            base.baseCancelButton.Name = "baseCancelButton";
            base.baseCancelButton.TabIndex = 3;
            base.baseCancelButton.Click += new EventHandler(this.BaseCancelButton_Click);
            this.footerSeparator.Name = "footerSeparator";
            this.saveConfigPanel.AutoScroll = true;
            this.saveConfigPanel.Name = "saveConfigPanel";
            this.saveConfigPanel.TabIndex = 0;
            this.saveConfigPanel.TabStop = false;
            this.defaultsButton.Name = "defaultsButton";
            this.defaultsButton.AutoSize = true;
            this.defaultsButton.FlatStyle = FlatStyle.System;
            this.defaultsButton.TabIndex = 1;
            this.defaultsButton.Click += new EventHandler(this.DefaultsButton_Click);
            this.saveConfigWidget.Name = "saveConfigWidget";
            this.saveConfigWidget.TabIndex = 9;
            this.saveConfigWidget.Token = null;
            this.previewHeader.Name = "previewHeader";
            this.previewHeader.RightMargin = 0;
            this.previewHeader.TabIndex = 11;
            this.previewHeader.TabStop = false;
            this.previewHeader.Text = "Header";
            this.documentView.BorderStyle = BorderStyle.Fixed3D;
            this.documentView.Document = null;
            this.documentView.Name = "documentView";
            this.documentView.PanelAutoScroll = true;
            this.documentView.RulersEnabled = false;
            this.documentView.TabIndex = 12;
            this.documentView.TabStop = false;
            this.documentView.DocumentMouseMove += new EventHandler<MouseEventArgsF>(this.DocumentView_DocumentMouseMove);
            this.documentView.DocumentMouseDown += new EventHandler<MouseEventArgsF>(this.DocumentView_DocumentMouseDown);
            this.documentView.DocumentMouseUp += new EventHandler<MouseEventArgsF>(this.DocumentView_DocumentMouseUp);
            this.documentView.Visible = false;
            this.settingsHeader.Name = "settingsHeader";
            this.settingsHeader.TabIndex = 13;
            this.settingsHeader.TabStop = false;
            this.settingsHeader.Text = "Header";
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.Controls.Add(this.defaultsButton);
            base.Controls.Add(this.settingsHeader);
            base.Controls.Add(this.previewHeader);
            base.Controls.Add(this.documentView);
            base.Controls.Add(this.footerSeparator);
            base.Controls.Add(this.saveConfigPanel);
            base.FormBorderStyle = FormBorderStyle.Sizable;
            base.MinimizeBox = false;
            base.MaximizeBox = true;
            base.Name = "SaveConfigDialog";
            base.StartPosition = FormStartPosition.Manual;
            base.Controls.SetChildIndex(this.saveConfigPanel, 0);
            base.Controls.SetChildIndex(this.documentView, 0);
            base.Controls.SetChildIndex(base.baseOkButton, 0);
            base.Controls.SetChildIndex(base.baseCancelButton, 0);
            base.Controls.SetChildIndex(this.previewHeader, 0);
            base.Controls.SetChildIndex(this.settingsHeader, 0);
            base.Controls.SetChildIndex(this.defaultsButton, 0);
            base.ResumeLayout(false);
        }

        private void LoadPositions()
        {
            Rectangle bounds;
            Rectangle rectangle3;
            FormWindowState normal;
            System.Drawing.Size size = UI.ScaleSize(unscaledMinSize);
            Form owner = base.Owner;
            if (owner != null)
            {
                bounds = owner.Bounds;
            }
            else
            {
                bounds = Screen.PrimaryScreen.WorkingArea;
            }
            Rectangle rectangle2 = new Rectangle((bounds.Width - size.Width) / 2, (bounds.Height - size.Height) / 2, size.Width, size.Height);
            try
            {
                string str = Settings.CurrentUser.GetString("SaveConfigDialog.WindowState", FormWindowState.Normal.ToString());
                normal = (FormWindowState) Enum.Parse(typeof(FormWindowState), str);
                int x = Settings.CurrentUser.GetInt32("SaveConfigDialog.Left", rectangle2.Left);
                int y = Settings.CurrentUser.GetInt32("SaveConfigDialog.Top", rectangle2.Top);
                int width = Math.Max(size.Width, Settings.CurrentUser.GetInt32("SaveConfigDialog.Width", rectangle2.Width));
                int height = Math.Max(size.Height, Settings.CurrentUser.GetInt32("SaveConfigDialog.Height", rectangle2.Height));
                rectangle3 = new Rectangle(x, y, width, height);
            }
            catch (Exception)
            {
                rectangle3 = rectangle2;
                normal = FormWindowState.Normal;
            }
            Rectangle newClientBounds = new Rectangle(rectangle3.Left + bounds.Left, rectangle3.Top + owner.Top, rectangle3.Width, rectangle3.Height);
            Rectangle defaultClientBounds = new Rectangle(rectangle2.Left + bounds.Left, rectangle2.Top + bounds.Top, rectangle2.Width, rectangle2.Height);
            base.SuspendLayout();
            try
            {
                Rectangle clientBounds = this.ValidateAndAdjustNewBounds(owner, newClientBounds, defaultClientBounds);
                Rectangle rectangle7 = base.ClientBoundsToWindowBounds(clientBounds);
                base.Bounds = rectangle7;
                base.WindowState = normal;
            }
            finally
            {
                base.ResumeLayout(true);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (base.IsShown)
            {
                this.SavePositions();
            }
            base.OnClosing(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UI.ScaleHeight(8);
            int num2 = Math.Max(0, num - (base.IsGlassEffectivelyEnabled ? UI.ScaleHeight(5) : 0));
            int x = base.IsGlassEffectivelyEnabled ? -1 : UI.ScaleWidth(8);
            int num4 = UI.ScaleWidth(8);
            base.baseCancelButton.PerformLayout();
            base.baseOkButton.PerformLayout();
            base.baseCancelButton.Location = new System.Drawing.Point((base.ClientSize.Width - base.baseOkButton.Width) - x, (base.ClientSize.Height - num2) - base.baseCancelButton.Height);
            base.baseOkButton.Location = new System.Drawing.Point((base.baseCancelButton.Left - num4) - base.baseOkButton.Width, (base.ClientSize.Height - num2) - base.baseOkButton.Height);
            this.footerSeparator.Size = this.footerSeparator.GetPreferredSize(new System.Drawing.Size(base.ClientSize.Width - (2 * x), 1));
            this.footerSeparator.Location = new System.Drawing.Point(x, (base.baseOkButton.Top - num) - this.footerSeparator.Height);
            if (base.IsGlassEffectivelyEnabled)
            {
                base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.footerSeparator.Top);
                this.footerSeparator.Visible = false;
                base.SizeGripStyle = SizeGripStyle.Hide;
            }
            else
            {
                base.GlassInset = new Padding(0);
                this.footerSeparator.Visible = true;
                base.SizeGripStyle = SizeGripStyle.Show;
            }
            int num5 = UI.ScaleHeight(8);
            int y = UI.ScaleHeight(6);
            int num7 = UI.ScaleWidth(8);
            int num8 = UI.ScaleWidth(200);
            int num9 = UI.ScaleWidth(8);
            int num10 = UI.ScaleWidth(8);
            int num11 = (num7 + num8) + num9;
            int width = (base.ClientSize.Width - num11) - num10;
            int num13 = UI.ScaleHeight(12);
            int num14 = -3;
            this.settingsHeader.Location = new System.Drawing.Point(num7 + num14, y);
            this.settingsHeader.Width = num8 - num14;
            this.settingsHeader.PerformLayout();
            this.saveConfigPanel.Location = new System.Drawing.Point(num7, this.settingsHeader.Bottom + num);
            this.saveConfigPanel.Width = num8;
            this.saveConfigPanel.PerformLayout();
            this.saveConfigWidget.Width = this.saveConfigPanel.Width - SystemInformation.VerticalScrollBarWidth;
            this.previewHeader.Location = new System.Drawing.Point(num11 + num14, y);
            this.previewHeader.Width = width - num14;
            this.previewHeader.PerformLayout();
            this.documentView.Location = new System.Drawing.Point(num11, this.previewHeader.Bottom + num);
            this.documentView.Size = new System.Drawing.Size(width, (this.footerSeparator.Top - num5) - this.documentView.Top);
            this.saveConfigPanel.Height = ((this.documentView.Bottom - this.saveConfigPanel.Top) - this.defaultsButton.Height) - num13;
            this.saveConfigWidget.PerformLayout();
            int num15 = Math.Min(this.saveConfigPanel.Height, this.saveConfigWidget.Height);
            this.defaultsButton.PerformLayout();
            this.defaultsButton.Location = new System.Drawing.Point(num7 + ((num8 - this.defaultsButton.Width) / 2), (this.saveConfigPanel.Top + num15) + num13);
            this.MinimumSize = UI.ScaleSize(unscaledMinSize);
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.scratchSurface == null)
            {
                throw new InvalidOperationException("ScratchSurface was never set: it is null");
            }
            this.LoadPositions();
            base.OnLoad(e);
        }

        protected virtual void OnProgress(int percent)
        {
            if (this.Progress != null)
            {
                this.Progress(this, new ProgressEventArgs((double) percent));
            }
        }

        protected override void OnResize(EventArgs e)
        {
            if (base.IsShown)
            {
                this.SavePositions();
            }
            base.OnResize(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (base.IsShown)
            {
                this.SavePositions();
            }
            base.OnSizeChanged(e);
        }

        private void QueueFileSizeTextUpdate()
        {
            this.callbackDoneEvent.Reset();
            string str = PdnResources.GetString2("SaveConfigDialog.FileSizeText.Text.Computing");
            this.previewHeader.Text = string.Format(this.fileSizeTextFormat, str);
            this.fileSizeTimer.Change(100, 0);
            this.OnProgress(0);
        }

        private void SavePositions()
        {
            if (base.WindowState != FormWindowState.Minimized)
            {
                if (base.WindowState != FormWindowState.Maximized)
                {
                    System.Drawing.Point location;
                    Form owner = base.Owner;
                    if (owner != null)
                    {
                        location = owner.Bounds.Location;
                    }
                    else
                    {
                        location = new System.Drawing.Point(0, 0);
                    }
                    Rectangle rectangle2 = base.WindowBoundsToClientBounds(base.Bounds);
                    int num = rectangle2.Left - location.X;
                    int num2 = rectangle2.Top - location.Y;
                    Settings.CurrentUser.SetInt32("SaveConfigDialog.Left", num);
                    Settings.CurrentUser.SetInt32("SaveConfigDialog.Top", num2);
                    Settings.CurrentUser.SetInt32("SaveConfigDialog.Width", rectangle2.Width);
                    Settings.CurrentUser.SetInt32("SaveConfigDialog.Height", rectangle2.Height);
                }
                Settings.CurrentUser.SetString("SaveConfigDialog.WindowState", base.WindowState.ToString());
            }
        }

        private void SetFileSizeProgress(int percent)
        {
            string str2 = string.Format(PdnResources.GetString2("SaveConfigDialog.FileSizeText.Text.Computing.Format"), percent);
            this.previewHeader.Text = string.Format(this.fileSizeTextFormat, str2);
            int num = percent.Clamp(0, 100);
            this.OnProgress(num);
        }

        private void TokenChangedHandler(object sender, EventArgs e)
        {
            this.QueueFileSizeTextUpdate();
        }

        private void UIWaitForCallbackDoneEvent(WaitActionType wat)
        {
            if (!this.callbackDoneEvent.WaitOne(0, false))
            {
                using (TaskManager manager = new TaskManager())
                {
                    VirtualTask<Unit> cancelTask = manager.CreateVirtualTask();
                    TaskProgressDialog dialog = new TaskProgressDialog {
                        Task = cancelTask,
                        CloseOnFinished = true,
                        ShowCancelButton = false,
                        Icon = base.Icon,
                        Text = this.Text
                    };
                    switch (wat)
                    {
                        case WaitActionType.Ok:
                            dialog.HeaderText = PdnResources.GetString2("SaveConfigDialog.Finishing.Text");
                            break;

                        case WaitActionType.Cancel:
                            dialog.HeaderText = PdnResources.GetString2("TaskProgressDialog.Canceling.Text");
                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }
                    dialog.Shown += (, ) => ThreadPool.QueueUserWorkItem(delegate {
                        try
                        {
                            this.callbackDoneEvent.WaitOne();
                        }
                        finally
                        {
                            cancelTask.SetState(TaskState.Finished);
                        }
                    });
                    dialog.ShowDialog(this);
                }
            }
        }

        private void UpdateFileSizeAndPreview(string tempFileName)
        {
            if (!base.IsDisposed)
            {
                if (tempFileName == null)
                {
                    string str = PdnResources.GetString2("SaveConfigDialog.FileSizeText.Text.Error");
                    this.previewHeader.Text = string.Format(this.fileSizeTextFormat, str);
                }
                else
                {
                    FileInfo info = new FileInfo(tempFileName);
                    long length = info.Length;
                    this.previewHeader.Text = string.Format(this.fileSizeTextFormat, Utility.SizeStringFromBytes(length));
                    this.documentView.Visible = true;
                    this.documentView.ResumeRefresh();
                    PaintDotNet.Document document = null;
                    try
                    {
                        if (this.disposeDocument && (this.documentView.Document != null))
                        {
                            document = this.documentView.Document;
                        }
                        if (this.fileType.IsReflexive(this.SaveConfigToken))
                        {
                            this.documentView.Document = this.Document;
                            this.documentView.Document.Invalidate();
                            this.disposeDocument = false;
                        }
                        else
                        {
                            PaintDotNet.Document document2;
                            FileStream input = new FileStream(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                            try
                            {
                                Utility.GCFullCollect();
                                document2 = this.fileType.Load(input);
                            }
                            catch
                            {
                                document2 = null;
                                this.TokenChangedHandler(this, EventArgs.Empty);
                            }
                            input.Close();
                            if (document2 != null)
                            {
                                this.documentView.Document = document2;
                                this.disposeDocument = true;
                            }
                            Utility.GCFullCollect();
                        }
                        try
                        {
                            info.Delete();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    finally
                    {
                        this.documentView.SuspendRefresh();
                        if (document != null)
                        {
                            document.Dispose();
                        }
                    }
                }
            }
        }

        private Rectangle ValidateAndAdjustNewBounds(Form owner, Rectangle newClientBounds, Rectangle defaultClientBounds)
        {
            Rectangle rectangle3;
            Screen primaryScreen;
            Rectangle rect = base.ClientBoundsToWindowBounds(newClientBounds);
            bool flag = false;
            foreach (Screen screen in Screen.AllScreens)
            {
                flag |= screen.Bounds.IntersectsWith(rect);
            }
            if (flag)
            {
                rectangle3 = newClientBounds;
            }
            else
            {
                rectangle3 = defaultClientBounds;
            }
            if (owner != null)
            {
                primaryScreen = Screen.FromControl(owner);
            }
            else
            {
                primaryScreen = Screen.PrimaryScreen;
            }
            Rectangle bounds = base.ClientBoundsToWindowBounds(rectangle3);
            Rectangle windowBounds = PdnBaseForm.EnsureRectIsOnScreen(primaryScreen, bounds);
            return base.WindowBoundsToClientBounds(windowBounds);
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                UI.AddCompositedToCP(createParams);
                return createParams;
            }
        }

        [Browsable(false)]
        public PaintDotNet.Document Document
        {
            get => 
                this.document;
            set
            {
                this.document = value;
            }
        }

        [Browsable(false)]
        public PaintDotNet.FileType FileType
        {
            get => 
                this.fileType;
            set
            {
                if ((this.fileType == null) || (this.fileType.Name != value.Name))
                {
                    if (this.fileType != null)
                    {
                        this.fileTypeToSaveToken[this.fileType] = this.SaveConfigToken;
                    }
                    this.fileType = value;
                    PaintDotNet.SaveConfigToken lastSaveConfigToken = (PaintDotNet.SaveConfigToken) this.fileTypeToSaveToken[this.fileType];
                    if (lastSaveConfigToken == null)
                    {
                        lastSaveConfigToken = this.fileType.GetLastSaveConfigToken();
                    }
                    PaintDotNet.SaveConfigToken token2 = this.fileType.CreateDefaultSaveConfigToken();
                    if (lastSaveConfigToken.GetType() != token2.GetType())
                    {
                        lastSaveConfigToken = null;
                    }
                    if (lastSaveConfigToken == null)
                    {
                        lastSaveConfigToken = this.fileType.CreateDefaultSaveConfigToken();
                    }
                    SaveConfigWidget widget = this.fileType.CreateSaveConfigWidget();
                    widget.Token = lastSaveConfigToken;
                    widget.Location = this.saveConfigWidget.Location;
                    this.TokenChangedHandler(this, EventArgs.Empty);
                    this.saveConfigWidget.TokenChanged -= new EventHandler(this.TokenChangedHandler);
                    base.SuspendLayout();
                    this.saveConfigPanel.Controls.Remove(this.saveConfigWidget);
                    this.saveConfigWidget = widget;
                    this.saveConfigPanel.Controls.Add(this.saveConfigWidget);
                    base.ResumeLayout(true);
                    this.saveConfigWidget.TokenChanged += new EventHandler(this.TokenChangedHandler);
                    if (this.saveConfigWidget is NoSaveConfigWidget)
                    {
                        this.defaultsButton.Enabled = false;
                    }
                    else
                    {
                        this.defaultsButton.Enabled = true;
                    }
                }
            }
        }

        [Browsable(false)]
        public PaintDotNet.SaveConfigToken SaveConfigToken
        {
            get => 
                this.saveConfigWidget.Token;
            set
            {
                this.saveConfigWidget.Token = value;
            }
        }

        public Surface ScratchSurface
        {
            set
            {
                if (this.scratchSurface != null)
                {
                    throw new InvalidOperationException("May only set ScratchSurface once, and only before the dialog is shown");
                }
                this.scratchSurface = value;
            }
        }

        private static class SettingNames
        {
            public const string Height = "SaveConfigDialog.Height";
            public const string Left = "SaveConfigDialog.Left";
            public const string Top = "SaveConfigDialog.Top";
            public const string Width = "SaveConfigDialog.Width";
            public const string WindowState = "SaveConfigDialog.WindowState";
        }

        private enum WaitActionType
        {
            Ok,
            Cancel
        }
    }
}

