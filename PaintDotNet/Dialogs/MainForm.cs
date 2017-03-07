namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class MainForm : PdnBaseForm
    {
        private AppWorkspace appWorkspace;
        private IContainer components;
        private Button defaultButton;
        private System.Windows.Forms.Timer deferredInitializationTimer;
        private System.Windows.Forms.Timer floaterOpacityTimer;
        private FloatingToolForm[] floaters;
        private bool killAfterInit;
        private bool processingOpen;
        private List<string> queuedInstanceMessages;
        private PaintDotNet.SystemLayer.SingleInstanceManager singleInstanceManager;

        public MainForm() : this(new string[0])
        {
        }

        public MainForm(string[] args)
        {
            this.queuedInstanceMessages = new List<string>();
            bool flag = true;
            base.StartPosition = FormStartPosition.WindowsDefaultLocation;
            List<string> list = new List<string>();
            foreach (string str in args)
            {
                if (string.Compare(str, "/dontForceGC") == 0)
                {
                    Utility.AllowGCFullCollect = false;
                }
                else if (string.Compare(str, "/test", true) == 0)
                {
                    PdnInfo.IsTestMode = true;
                }
                else if ((str.Length > 0) && (str[0] != '/'))
                {
                    try
                    {
                        string fullPath = Path.GetFullPath(str);
                        list.Add(fullPath);
                    }
                    catch (Exception)
                    {
                        list.Add(str);
                        flag = false;
                    }
                }
            }
            if (flag)
            {
                try
                {
                    Environment.CurrentDirectory = PdnInfo.ApplicationDir2;
                }
                catch (Exception)
                {
                }
            }
            this.InitializeComponent();
            base.IsGlassDesired = this.appWorkspace.IsGlassDesired;
            base.Icon = PdnInfo.AppIcon;
            this.LoadSettings();
            foreach (string str3 in list)
            {
                this.queuedInstanceMessages.Add(str3);
            }
            if (list.Count == 0)
            {
                MeasurementUnit defaultDpuUnit = Document.DefaultDpuUnit;
                double defaultDpu = Document.GetDefaultDpu(defaultDpuUnit);
                Int32Size newDocumentSize = this.appWorkspace.GetNewDocumentSize();
                this.appWorkspace.CreateBlankDocumentInNewWorkspace(newDocumentSize, defaultDpuUnit, defaultDpu, true);
                this.appWorkspace.ActiveDocumentWorkspace.Document.Dirty = false;
            }
            this.LoadWindowState();
            this.deferredInitializationTimer.Enabled = true;
            Application.Idle += new EventHandler(this.Application_Idle);
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            if (!base.IsDisposed && ((this.queuedInstanceMessages.Count > 0) || ((this.singleInstanceManager != null) && this.singleInstanceManager.AreMessagesPending)))
            {
                this.ProcessQueuedInstanceMessages();
            }
        }

        private void AppWorkspace_ActiveDocumentWorkspaceChanged(object sender, EventArgs e)
        {
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.ScaleFactorChanged += new EventHandler(this.DocumentWorkspace_ScaleFactorChanged);
                this.appWorkspace.ActiveDocumentWorkspace.DocumentChanged += new EventHandler(this.DocumentWorkspace_DocumentChanged);
                this.appWorkspace.ActiveDocumentWorkspace.SaveOptionsChanged += new EventHandler(this.DocumentWorkspace_SaveOptionsChanged);
            }
            this.SetTitleText();
        }

        private void AppWorkspace_ActiveDocumentWorkspaceChanging(object sender, EventArgs e)
        {
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.ScaleFactorChanged -= new EventHandler(this.DocumentWorkspace_ScaleFactorChanged);
                this.appWorkspace.ActiveDocumentWorkspace.DocumentChanged -= new EventHandler(this.DocumentWorkspace_DocumentChanged);
                this.appWorkspace.ActiveDocumentWorkspace.SaveOptionsChanged -= new EventHandler(this.DocumentWorkspace_SaveOptionsChanged);
            }
        }

        private Keys CharToKeys(char c)
        {
            Keys none = Keys.None;
            c = char.ToLower(c);
            if ((c >= 'a') && (c <= 'z'))
            {
                none = (Keys) (('A' + c) - 0x61);
            }
            return none;
        }

        private void DefaultButton_Click(object sender, EventArgs e)
        {
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.Focus();
                if (this.appWorkspace.ActiveDocumentWorkspace.Tool != null)
                {
                    this.appWorkspace.ActiveDocumentWorkspace.Tool.PerformKeyPress(new KeyPressEventArgs('\r'));
                    this.appWorkspace.ActiveDocumentWorkspace.Tool.PerformKeyPress(Keys.Enter);
                }
            }
        }

        private void DeferredInitialization(object sender, EventArgs e)
        {
            this.deferredInitializationTimer.Enabled = false;
            this.deferredInitializationTimer.Tick -= new EventHandler(this.DeferredInitialization);
            this.deferredInitializationTimer.Dispose();
            this.deferredInitializationTimer = null;
            this.appWorkspace.ToolBar.MainMenu.PopulateEffects();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.singleInstanceManager != null)
                {
                    PaintDotNet.SystemLayer.SingleInstanceManager singleInstanceManager = this.singleInstanceManager;
                    this.SingleInstanceManager = null;
                    singleInstanceManager.Dispose();
                    singleInstanceManager = null;
                }
                if (this.floaterOpacityTimer != null)
                {
                    this.floaterOpacityTimer.Tick -= new EventHandler(this.FloaterOpacityTimer_Tick);
                    this.floaterOpacityTimer.Dispose();
                    this.floaterOpacityTimer = null;
                }
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
            }
            try
            {
                base.Dispose(disposing);
            }
            catch (RankException)
            {
            }
        }

        private void DocumentWorkspace_DocumentChanged(object sender, EventArgs e)
        {
            this.SetTitleText();
            this.OnResize(EventArgs.Empty);
        }

        private void DocumentWorkspace_SaveOptionsChanged(object sender, EventArgs e)
        {
            this.SetTitleText();
        }

        private void DocumentWorkspace_ScaleFactorChanged(object sender, EventArgs e)
        {
            this.SetTitleText();
        }

        private void FloaterOpacityTimer_Tick(object sender, EventArgs e)
        {
            if (((base.WindowState != FormWindowState.Minimized) && (this.floaters != null)) && (PdnBaseForm.EnableOpacity && (this.appWorkspace.ActiveDocumentWorkspace != null)))
            {
                Rect visibleDocumentBounds;
                try
                {
                    visibleDocumentBounds = this.appWorkspace.ActiveDocumentWorkspace.VisibleDocumentBounds;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                Int32Rect rect2 = visibleDocumentBounds.Int32Bound();
                for (int i = 0; i < this.floaters.Length; i++)
                {
                    FloatingToolForm control = this.floaters[i];
                    Int32Rect rect3 = Int32RectUtil.Intersect(rect2, control.Bounds());
                    double num2 = -1.0;
                    try
                    {
                        if ((((rect3.Width == 0) || (rect3.Height == 0)) || (control.Bounds.Contains(Control.MousePosition) && !this.appWorkspace.ActiveDocumentWorkspace.IsMouseCaptured())) || control.IsMouseCapturedSlow())
                        {
                            num2 = Math.Min((double) 1.0, (double) (control.Opacity + 0.125));
                        }
                        else
                        {
                            num2 = Math.Max((double) 0.75, (double) (control.Opacity - 0.0625));
                        }
                        if (num2 != control.Opacity)
                        {
                            control.Opacity = num2;
                        }
                    }
                    catch (Win32Exception)
                    {
                    }
                }
            }
        }

        private Keys GetMenuCmdKey(string text)
        {
            Keys none = Keys.None;
            for (int i = 0; i < (text.Length - 1); i++)
            {
                if (text[i] == '&')
                {
                    return (Keys.Alt | this.CharToKeys(text[i + 1]));
                }
            }
            return none;
        }

        private void HideInsteadOfCloseHandler(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            ((Form) sender).Hide();
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.defaultButton = new Button();
            this.appWorkspace = new AppWorkspace();
            this.floaterOpacityTimer = new System.Windows.Forms.Timer(this.components);
            this.deferredInitializationTimer = new System.Windows.Forms.Timer(this.components);
            base.SuspendLayout();
            this.appWorkspace.Dock = DockStyle.Fill;
            this.appWorkspace.Location = new System.Drawing.Point(0, 0);
            this.appWorkspace.Name = "appWorkspace";
            this.appWorkspace.Size = new System.Drawing.Size(0x2f0, 0x288);
            this.appWorkspace.TabIndex = 2;
            this.appWorkspace.ActiveDocumentWorkspaceChanging += new EventHandler(this.AppWorkspace_ActiveDocumentWorkspaceChanging);
            this.appWorkspace.ActiveDocumentWorkspaceChanged += new EventHandler(this.AppWorkspace_ActiveDocumentWorkspaceChanged);
            this.floaterOpacityTimer.Enabled = false;
            this.floaterOpacityTimer.Interval = 0x19;
            this.floaterOpacityTimer.Tick += new EventHandler(this.FloaterOpacityTimer_Tick);
            this.deferredInitializationTimer.Interval = 250;
            this.deferredInitializationTimer.Tick += new EventHandler(this.DeferredInitialization);
            this.defaultButton.Size = new System.Drawing.Size(1, 1);
            this.defaultButton.Text = "";
            this.defaultButton.Location = new System.Drawing.Point(-100, -100);
            this.defaultButton.TabStop = false;
            this.defaultButton.Click += new EventHandler(this.DefaultButton_Click);
            try
            {
                this.AllowDrop = true;
            }
            catch (InvalidOperationException)
            {
            }
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new System.Drawing.Size(950, 0x2e2);
            base.Controls.Add(this.appWorkspace);
            base.Controls.Add(this.defaultButton);
            base.AcceptButton = this.defaultButton;
            base.Name = "MainForm";
            base.StartPosition = FormStartPosition.WindowsDefaultLocation;
            base.ForceActiveTitleBar = true;
            base.KeyPreview = true;
            base.Controls.SetChildIndex(this.appWorkspace, 0);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void LoadSettings()
        {
            try
            {
                PdnBaseForm.EnableAutoGlass = Settings.CurrentUser.GetBoolean("GlassDialogButtons", true);
                PdnBaseForm.EnableOpacity = Settings.CurrentUser.GetBoolean("TranslucentWindows", true);
            }
            catch (Exception)
            {
                try
                {
                    Settings.CurrentUser.Delete(new string[] { "GlassDialogButtons", "TranslucentWindows" });
                }
                catch
                {
                }
            }
        }

        private void LoadWindowState()
        {
            try
            {
                FormWindowState state = (FormWindowState) Enum.Parse(typeof(FormWindowState), Settings.CurrentUser.GetString("WindowState", base.WindowState.ToString()), true);
                if (state != FormWindowState.Minimized)
                {
                    if (state != FormWindowState.Maximized)
                    {
                        Rectangle empty = Rectangle.Empty;
                        empty.Width = Settings.CurrentUser.GetInt32("Width", base.Width);
                        empty.Height = Settings.CurrentUser.GetInt32("Height", base.Height);
                        int x = Settings.CurrentUser.GetInt32("Left", base.Left);
                        int y = Settings.CurrentUser.GetInt32("Top", base.Top);
                        empty.Location = new System.Drawing.Point(x, y);
                        base.Bounds = empty;
                    }
                    base.WindowState = state;
                }
            }
            catch
            {
                try
                {
                    Settings.CurrentUser.Delete(new string[] { "Width", "Height", "WindowState", "Top", "Left" });
                }
                catch
                {
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.SetTool(null);
            }
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((!e.Cancel && (this.appWorkspace != null)) && !this.appWorkspace.IsDisposed)
            {
                CloseAllWorkspacesAction performMe = new CloseAllWorkspacesAction();
                this.appWorkspace.PerformAction(performMe);
                e.Cancel = performMe.Cancelled;
            }
            if (!e.Cancel)
            {
                if (base.Visible)
                {
                    this.SaveSettings();
                }
                if (this.floaters != null)
                {
                    FloatingToolForm[] floaters = this.floaters;
                    for (int i = 0; i < floaters.Length; i++)
                    {
                        floaters[i].Hide();
                    }
                }
                base.Hide();
                if (this.queuedInstanceMessages != null)
                {
                    this.queuedInstanceMessages.Clear();
                }
                PaintDotNet.SystemLayer.SingleInstanceManager singleInstanceManager = this.singleInstanceManager;
                this.SingleInstanceManager = null;
                if (singleInstanceManager != null)
                {
                    singleInstanceManager.Dispose();
                    singleInstanceManager = null;
                }
            }
            base.OnClosing(e);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.Activate();
            if ((base.IsCurrentModalForm && base.Enabled) && drgevent.Data.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop))
            {
                string[] data = (string[]) drgevent.Data.GetData(System.Windows.Forms.DataFormats.FileDrop);
                if (data == null)
                {
                    return;
                }
                string[] fileNames = this.PruneDirectories(data);
                bool flag = true;
                if (fileNames.Length == 0)
                {
                    return;
                }
                if ((fileNames.Length == 1) && (this.appWorkspace.DocumentWorkspaces.Length == 0))
                {
                    flag = false;
                }
                else
                {
                    string str4;
                    string str = (fileNames.Length > 1) ? "Plural" : "Singular";
                    Icon icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.DragDrop.OpenOrImport.FormIcon.png").Reference);
                    string str2 = PdnResources.GetString2("DragDrop.OpenOrImport.Title");
                    string str3 = PdnResources.GetString2("DragDrop.OpenOrImport.InfoText." + str);
                    TaskButton button = new TaskButton(PdnResources.GetImageResource2("Icons.MenuFileOpenIcon.png").Reference, PdnResources.GetString2("DragDrop.OpenOrImport.OpenButton.ActionText"), PdnResources.GetString2("DragDrop.OpenOrImport.OpenButton.ExplanationText." + str));
                    if (this.appWorkspace.DocumentWorkspaces.Length == 0)
                    {
                        str4 = PdnResources.GetString2("DragDrop.OpenOrImport.ImportLayers.ExplanationText.NoImagesYet.Plural");
                    }
                    else
                    {
                        str4 = PdnResources.GetString2("DragDrop.OpenOrImport.ImportLayers.ExplanationText." + str);
                    }
                    TaskButton button2 = new TaskButton(PdnResources.GetImageResource2("Icons.MenuLayersAddNewLayerIcon.png").Reference, PdnResources.GetString2("DragDrop.OpenOrImport.ImportLayers.ActionText." + str), str4);
                    TaskButton button3 = new TaskButton(PdnResources.GetImageResource2("Icons.CancelIcon.png").Reference, PdnResources.GetString2("TaskButton.Cancel.ActionText"), PdnResources.GetString2("TaskButton.Cancel.ExplanationText"));
                    TaskDialog dialog2 = new TaskDialog {
                        Icon = icon,
                        Title = str2,
                        ScaleTaskImageWithDpi = false,
                        IntroText = str3,
                        TaskButtons = new TaskButton[] { 
                            button,
                            button2,
                            button3
                        },
                        CancelButton = button3
                    };
                    TaskButton button4 = dialog2.Show(this);
                    if (button4 == button)
                    {
                        flag = false;
                    }
                    else if (button4 == button2)
                    {
                        flag = true;
                    }
                    else
                    {
                        return;
                    }
                }
                if (!flag)
                {
                    this.appWorkspace.OpenFilesInNewWorkspace(fileNames);
                }
                else
                {
                    if (this.appWorkspace.ActiveDocumentWorkspace == null)
                    {
                        Int32Size newDocumentSize = this.appWorkspace.GetNewDocumentSize();
                        this.appWorkspace.CreateBlankDocumentInNewWorkspace(newDocumentSize, Document.DefaultDpuUnit, Document.GetDefaultDpu(Document.DefaultDpuUnit), false);
                    }
                    HistoryMemento memento = new ImportFromFileAction().ImportMultipleFiles(this.appWorkspace.ActiveDocumentWorkspace, fileNames);
                    if (memento != null)
                    {
                        this.appWorkspace.ActiveDocumentWorkspace.History.PushNewMemento(memento);
                    }
                }
            }
            base.OnDragDrop(drgevent);
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            if (base.Enabled && drgevent.Data.GetDataPresent(System.Windows.Forms.DataFormats.FileDrop))
            {
                string[] data = (string[]) drgevent.Data.GetData(System.Windows.Forms.DataFormats.FileDrop);
                if (data != null)
                {
                    foreach (string str in data)
                    {
                        try
                        {
                            if ((File.GetAttributes(str) & FileAttributes.Directory) == 0)
                            {
                                drgevent.Effect = DragDropEffects.Copy;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            base.OnDragEnter(drgevent);
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            hevent.Handled = true;
            base.OnHelpRequested(hevent);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (((this.appWorkspace != null) && this.appWorkspace.IsGlassDesired) && UI.IsCompositionEnabled)
            {
                base.IsGlassDesired = true;
                base.GlassInset = this.appWorkspace.GlassInset;
                IMessageFilter glassWndProcFilter = base.GetGlassWndProcFilter();
                this.appWorkspace.SetGlassWndProcFilter(glassWndProcFilter);
            }
            else
            {
                base.IsGlassDesired = false;
                base.GlassInset = new Padding(0);
                this.appWorkspace.SetGlassWndProcFilter(null);
            }
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.EnsureFormIsOnScreen();
            if (this.killAfterInit)
            {
                Application.Exit();
            }
            this.floaters = new FloatingToolForm[] { this.appWorkspace.Widgets.ToolsForm, this.appWorkspace.Widgets.ColorsForm, this.appWorkspace.Widgets.HistoryForm, this.appWorkspace.Widgets.LayerForm };
            foreach (FloatingToolForm form in this.floaters)
            {
                form.Closing += new CancelEventHandler(this.HideInsteadOfCloseHandler);
            }
            this.PositionFloatingForms();
            base.OnLoad(e);
        }

        private void OnMenuDropDownClosed(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem) sender;
            foreach (ToolStripItem item2 in item.DropDownItems)
            {
                item2.Enabled = true;
            }
        }

        protected override void OnQueryEndSession(CancelEventArgs e)
        {
            if (base.IsCurrentModalForm)
            {
                this.OnClosing(e);
            }
            else
            {
                foreach (Form form in Application.OpenForms)
                {
                    PdnBaseForm form2 = form as PdnBaseForm;
                    if (form2 != null)
                    {
                        form2.Flash();
                    }
                }
                e.Cancel = true;
            }
            base.OnQueryEndSession(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.floaterOpacityTimer != null)
            {
                if (base.WindowState == FormWindowState.Minimized)
                {
                    if (this.floaterOpacityTimer.Enabled)
                    {
                        this.floaterOpacityTimer.Enabled = false;
                    }
                }
                else
                {
                    if (!this.floaterOpacityTimer.Enabled)
                    {
                        this.floaterOpacityTimer.Enabled = true;
                    }
                    this.FloaterOpacityTimer_Tick(this, EventArgs.Empty);
                }
            }
            base.OnResize(e);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Taskbar.MainWindow = this;
            if (PdnInfo.IsExpired)
            {
                foreach (Form form in Application.OpenForms)
                {
                    form.Enabled = false;
                }
                TaskButton button = new TaskButton(PdnResources.GetImageResource2("Icons.MenuUtilitiesCheckForUpdatesIcon.png").Reference, PdnResources.GetString2("ExpiredTaskDialog.CheckForUpdatesTB.ActionText"), PdnResources.GetString2("ExpiredTaskDialog.CheckForUpdatesTB.ExplanationText"));
                TaskButton button2 = new TaskButton(PdnResources.GetImageResource2("Icons.MenuHelpPdnWebsiteIcon.png").Reference, PdnResources.GetString2("ExpiredTaskDialog.GoToWebSiteTB.ActionText"), PdnResources.GetString2("ExpiredTaskDialog.GoToWebSiteTB.ExplanationText"));
                TaskButton button3 = new TaskButton(PdnResources.GetImageResource2("Icons.CancelIcon.png").Reference, PdnResources.GetString2("ExpiredTaskDialog.DoNotCheckForUpdatesTB.ActionText"), PdnResources.GetString2("ExpiredTaskDialog.DoNotCheckForUpdatesTB.ExplanationText"));
                TaskButton[] buttonArray = new TaskButton[] { button, button2, button3 };
                TaskDialog dialog2 = new TaskDialog {
                    Icon = base.Icon,
                    Title = PdnInfo.FullAppName,
                    TaskImage = PdnResources.GetImageResource2("Icons.WarningIcon.png").Reference,
                    ScaleTaskImageWithDpi = true,
                    IntroText = PdnResources.GetString2("ExpiredTaskDialog.InfoText"),
                    TaskButtons = buttonArray,
                    AcceptButton = button,
                    CancelButton = button3,
                    PixelWidth96Dpi = 450
                };
                TaskButton button4 = dialog2.Show(this);
                if (button4 == button)
                {
                    this.appWorkspace.CheckForUpdates();
                }
                else if (button4 == button2)
                {
                    PdnInfo.LaunchWebSite2(this, "redirect/pdnexpired.html");
                }
                base.Close();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.SetTitleText();
        }

        private void PositionFloatingForms()
        {
            this.appWorkspace.ResetFloatingForms();
            try
            {
                base.SnapManager.Load(Settings.CurrentUser);
            }
            catch
            {
                this.appWorkspace.ResetFloatingForms();
            }
            foreach (FloatingToolForm form in this.floaters)
            {
                base.AddOwnedForm(form);
            }
            if (Settings.CurrentUser.GetBoolean("ToolsForm.Visible", true))
            {
                this.appWorkspace.Widgets.ToolsForm.Show();
            }
            if (Settings.CurrentUser.GetBoolean("ColorsForm.Visible", true))
            {
                this.appWorkspace.Widgets.ColorsForm.Show();
            }
            if (Settings.CurrentUser.GetBoolean("HistoryForm.Visible", true))
            {
                this.appWorkspace.Widgets.HistoryForm.Show();
            }
            if (Settings.CurrentUser.GetBoolean("LayersForm.Visible", true))
            {
                this.appWorkspace.Widgets.LayerForm.Show();
            }
            Screen[] allScreens = Screen.AllScreens;
            foreach (FloatingToolForm form2 in this.floaters)
            {
                if (form2.Visible)
                {
                    bool flag = false;
                    try
                    {
                        bool flag2 = false;
                        foreach (Screen screen in allScreens)
                        {
                            Rectangle rectangle = Rectangle.Intersect(screen.Bounds, form2.Bounds);
                            if ((rectangle.Width > 0) && (rectangle.Height > 0))
                            {
                                flag2 = true;
                                break;
                            }
                        }
                        if (!flag2)
                        {
                            flag = true;
                        }
                    }
                    catch (Exception)
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        this.appWorkspace.ResetFloatingForm(form2);
                    }
                }
            }
            this.floaterOpacityTimer.Enabled = true;
        }

        private bool ProcessMessage(string message)
        {
            ArgumentAction action;
            string str;
            WaitCallback callBack = null;
            if (base.IsDisposed)
            {
                return false;
            }
            bool flag = this.SplitMessage(message, out action, out str);
            if (!flag)
            {
                return true;
            }
            switch (action)
            {
                case ArgumentAction.Open:
                    if (!this.processingOpen)
                    {
                        base.Activate();
                        if (!base.IsCurrentModalForm || !base.Enabled)
                        {
                            return flag;
                        }
                        this.processingOpen = true;
                        try
                        {
                            return this.appWorkspace.OpenFileInNewWorkspace(str);
                        }
                        finally
                        {
                            this.processingOpen = false;
                        }
                        break;
                    }
                    if (callBack == null)
                    {
                        callBack = delegate (object _) {
                            Thread.Sleep(150);
                            this.BeginInvoke(() => this.singleInstanceManager.SendInstanceMessage(message));
                        };
                    }
                    ThreadPool.QueueUserWorkItem(callBack);
                    return true;

                case ArgumentAction.OpenUntitled:
                    break;

                case ArgumentAction.Print:
                    base.Activate();
                    if ((!string.IsNullOrEmpty(str) && base.IsCurrentModalForm) && base.Enabled)
                    {
                        flag = this.appWorkspace.OpenFileInNewWorkspace(str);
                        if (!flag)
                        {
                            return flag;
                        }
                        DocumentWorkspace activeDocumentWorkspace = this.appWorkspace.ActiveDocumentWorkspace;
                        PrintAction action2 = new PrintAction();
                        activeDocumentWorkspace.PerformAction(action2);
                        CloseWorkspaceAction performMe = new CloseWorkspaceAction(activeDocumentWorkspace);
                        this.appWorkspace.PerformAction(performMe);
                        if (this.appWorkspace.DocumentWorkspaces.Length == 0)
                        {
                            Startup.CloseApplication();
                        }
                    }
                    return flag;

                case ArgumentAction.NoOp:
                    return true;

                default:
                    throw new InvalidEnumArgumentException();
            }
            base.Activate();
            if ((!string.IsNullOrEmpty(str) && base.IsCurrentModalForm) && base.Enabled)
            {
                DocumentWorkspace workspace;
                flag = this.appWorkspace.OpenFileInNewWorkspace(str, false, out workspace);
                if (flag)
                {
                    workspace.SetDocumentSaveOptions(null, null, null);
                    workspace.Document.Dirty = true;
                }
            }
            return flag;
        }

        private void ProcessQueuedInstanceMessages()
        {
            if (!base.IsDisposed && ((base.IsHandleCreated && !PdnInfo.IsExpired) && (this.singleInstanceManager != null)))
            {
                string[] pendingInstanceMessages = this.singleInstanceManager.GetPendingInstanceMessages();
                string[] strArray2 = this.queuedInstanceMessages.ToArrayEx<string>();
                this.queuedInstanceMessages.Clear();
                string[] strArray3 = new string[pendingInstanceMessages.Length + strArray2.Length];
                for (int i = 0; i < pendingInstanceMessages.Length; i++)
                {
                    strArray3[i] = pendingInstanceMessages[i];
                }
                for (int j = 0; j < strArray2.Length; j++)
                {
                    strArray3[j + pendingInstanceMessages.Length] = strArray2[j];
                }
                foreach (string str in strArray3)
                {
                    if (!this.ProcessMessage(str))
                    {
                        return;
                    }
                }
            }
        }

        private string[] PruneDirectories(string[] fileNames)
        {
            List<string> items = new List<string>();
            foreach (string str in fileNames)
            {
                try
                {
                    if ((File.GetAttributes(str) & FileAttributes.Directory) == 0)
                    {
                        items.Add(str);
                    }
                }
                catch (Exception)
                {
                }
            }
            return items.ToArrayEx<string>();
        }

        private void SaveSettings()
        {
            Settings.CurrentUser.SetInt32("Width", base.Width);
            Settings.CurrentUser.SetInt32("Height", base.Height);
            Settings.CurrentUser.SetInt32("Top", base.Top);
            Settings.CurrentUser.SetInt32("Left", base.Left);
            Settings.CurrentUser.SetString("WindowState", base.WindowState.ToString());
            Settings.CurrentUser.SetBoolean("TranslucentWindows", PdnBaseForm.EnableOpacity);
            Settings.CurrentUser.SetBoolean("GlassDialogButtons", PdnBaseForm.EnableAutoGlass);
            if (base.WindowState != FormWindowState.Minimized)
            {
                Settings.CurrentUser.SetBoolean("ToolsForm.Visible", this.appWorkspace.Widgets.ToolsForm.Visible);
                Settings.CurrentUser.SetBoolean("ColorsForm.Visible", this.appWorkspace.Widgets.ColorsForm.Visible);
                Settings.CurrentUser.SetBoolean("HistoryForm.Visible", this.appWorkspace.Widgets.HistoryForm.Visible);
                Settings.CurrentUser.SetBoolean("LayersForm.Visible", this.appWorkspace.Widgets.LayerForm.Visible);
            }
            base.SnapManager.Save(Settings.CurrentUser);
            this.appWorkspace.SaveSettings();
        }

        private void SetTitleText()
        {
            if (this.appWorkspace != null)
            {
                if (this.appWorkspace.ActiveDocumentWorkspace == null)
                {
                    this.Text = PdnInfo.AppName;
                }
                else
                {
                    string str4;
                    string appName = PdnInfo.AppName;
                    string str2 = string.Empty;
                    string friendlyName = this.appWorkspace.ActiveDocumentWorkspace.GetFriendlyName();
                    if (base.WindowState != FormWindowState.Minimized)
                    {
                        str4 = string.Format(PdnResources.GetString2("MainForm.Title.Format.Normal"), friendlyName, this.appWorkspace.ActiveDocumentWorkspace.ScaleFactor, appName);
                    }
                    else
                    {
                        str4 = string.Format(PdnResources.GetString2("MainForm.Title.Format.Minimized"), friendlyName, appName);
                    }
                    if (this.appWorkspace.ActiveDocumentWorkspace.Document != null)
                    {
                        str2 = str4;
                    }
                    this.Text = str2;
                }
            }
        }

        private void SingleInstanceManager_InstanceMessageReceived(object sender, EventArgs e)
        {
            base.BeginInvoke(new Action(this.ProcessQueuedInstanceMessages), null);
        }

        private bool SplitMessage(string message, out ArgumentAction action, out string actionParm)
        {
            if (message.Length == 0)
            {
                action = ArgumentAction.NoOp;
                actionParm = null;
                return false;
            }
            if (message.IndexOf("print:") == 0)
            {
                action = ArgumentAction.Print;
                actionParm = message.Substring("print:".Length);
                return true;
            }
            if (message.IndexOf("untitled:") == 0)
            {
                action = ArgumentAction.OpenUntitled;
                actionParm = message.Substring("untitled:".Length);
                return true;
            }
            action = ArgumentAction.Open;
            actionParm = message;
            return true;
        }

        protected override void WndProc(ref Message m)
        {
            if (this.singleInstanceManager != null)
            {
                this.singleInstanceManager.FilterMessage(ref m);
            }
            base.WndProc(ref m);
        }

        public PaintDotNet.SystemLayer.SingleInstanceManager SingleInstanceManager
        {
            get => 
                this.singleInstanceManager;
            set
            {
                if (this.singleInstanceManager != null)
                {
                    this.singleInstanceManager.InstanceMessageReceived -= new EventHandler(this.SingleInstanceManager_InstanceMessageReceived);
                    this.singleInstanceManager.SetWindow(null);
                }
                this.singleInstanceManager = value;
                if (this.singleInstanceManager != null)
                {
                    this.singleInstanceManager.SetWindow(this);
                    this.singleInstanceManager.InstanceMessageReceived += new EventHandler(this.SingleInstanceManager_InstanceMessageReceived);
                }
            }
        }

        private enum ArgumentAction
        {
            Open,
            OpenUntitled,
            Print,
            NoOp
        }
    }
}

