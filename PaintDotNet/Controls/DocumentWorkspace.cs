namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.IO;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Threading.IterativeTaskDirectives;
    using PaintDotNet.Tools;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal class DocumentWorkspace : DocumentView, IDispatcherObject, IHistoryWorkspace, IThumbnailProvider
    {
        private Layer activeLayer;
        private PaintDotNet.Tools.Tool activeTool;
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private string borrowScratchSurfaceReason = string.Empty;
        private readonly string contextStatusBarFormat = PdnResources.GetString2("StatusBar.Context.SelectedArea.Text.Format");
        private readonly string contextStatusBarWithAngleFormat = PdnResources.GetString2("StatusBar.Context.SelectedArea.Text.WithAngle.Format");
        private IDispatcher dispatcher;
        private string filePath;
        private PaintDotNet.FileType fileType;
        private HistoryStack history;
        private bool isScratchSurfaceBorrowed;
        private DateTime lastSaveTime = NeverSavedDateTime;
        public static readonly DateTime NeverSavedDateTime = DateTime.MinValue;
        private int nullToolCount;
        private System.Type preNullTool;
        private System.Type previousActiveToolType;
        private PaintDotNet.SaveConfigToken saveConfigToken;
        private int savedAli;
        private ScaleFactor savedSf;
        private PaintDotNet.ZoomBasis savedZb;
        private Surface scratchSurface;
        private PaintDotNet.Selection selection;
        private SelectionRenderer selectionRenderer;
        private Hashtable staticToolData = Hashtable.Synchronized(new Hashtable());
        private ImageResource statusIcon;
        private string statusText;
        private int suspendToolCursorChanges;
        private PaintDotNet.Threading.TaskManager taskManager;
        private static ToolInfo[] toolInfos;
        private System.Windows.Forms.Timer toolPulseTimer;
        private static System.Type[] tools;
        private bool useHQFluidZoom = (Environment.ProcessorCount >= 4);
        private PaintDotNet.ZoomBasis zoomBasis;
        private int zoomChangesCount;

        public event EventHandler ActiveLayerChanged;

        public event EventHandler ActiveLayerChanging;

        public event EventHandler FilePathChanged;

        public event EventHandler SaveOptionsChanged;

        public event EventHandler StatusChanged;

        public event EventHandler ToolChanged;

        public event EventHandler ToolChanging;

        public event EventHandler ZoomBasisChanged;

        public event EventHandler ZoomBasisChanging;

        static DocumentWorkspace()
        {
            InitializeTools();
            InitializeToolInfos();
        }

        public DocumentWorkspace()
        {
            try
            {
                if ((this.useHQFluidZoom && (Environment.ProcessorCount < 8)) && ((-1 != Processor.CpuName.IndexOf("Atom", StringComparison.InvariantCultureIgnoreCase)) && (-1 != Processor.CpuName.IndexOf("Intel", StringComparison.InvariantCultureIgnoreCase))))
                {
                    this.useHQFluidZoom = false;
                }
            }
            catch (Exception)
            {
            }
            this.dispatcher = new ControlDispatcher(this);
            this.taskManager = new PaintDotNet.Threading.TaskManager();
            this.selection = new PaintDotNet.Selection();
            this.activeLayer = null;
            this.history = new HistoryStack(this);
            this.InitializeComponent();
            this.selectionRenderer = new SelectionRenderer(base.CanvasRenderer, this.Selection, this);
            this.selectionRenderer.UseSystemTinting = true;
            base.CanvasRenderer.Add(this.selectionRenderer, true);
            this.selectionRenderer.EnableSelectionTinting = false;
            this.selectionRenderer.EnableSelectionOutline = true;
            this.selection.Changed += new EventHandler(this.Selection_Changed);
            this.zoomBasis = PaintDotNet.ZoomBasis.FitToWindow;
        }

        public Task AddToMruList()
        {
            string fullPath = Path.GetFullPath(this.FilePath);
            Shell.AddToRecentDocumentsList(fullPath);
            MostRecentFile mrf = new MostRecentFile(fullPath, new Bitmap(1, 1));
            if (this.AppWorkspace.MostRecentFiles.Contains(fullPath))
            {
                this.AppWorkspace.MostRecentFiles.Remove(fullPath);
            }
            this.AppWorkspace.MostRecentFiles.Add(mrf);
            this.AppWorkspace.MostRecentFiles.SaveMruList();
            IterativeTask task = this.TaskManager.CreateIterativeTask(_ => this.AddToMruListTask());
            task.Start(this.appWorkspace.Dispatcher);
            return task;
        }

        public IEnumerator<Directive> AddToMruListTask()
        {
            ISurface<ColorBgra> iteratorVariable6;
            string fullPath = Path.GetFullPath(this.FilePath);
            int iconSize = this.AppWorkspace.MostRecentFiles.IconSize;
            yield return Directive.DispatchTo(this.appWorkspace.BackgroundThread);
            IRenderer<ColorBgra> renderer = this.CreateThumbnailRenderer(iconSize);
            int recommendedExtent = DropShadow.GetRecommendedExtent(renderer.Size<ColorBgra>().ToGdipSize());
            ShadowDecorationRenderer iteratorVariable4 = new ShadowDecorationRenderer(renderer, recommendedExtent);
            IRenderer<ColorBgra> iteratorVariable5 = iteratorVariable4.Inset(new Int32Size(iconSize + (recommendedExtent * 2), iconSize + (recommendedExtent * 2)), new Int32Point(((iconSize + (recommendedExtent * 2)) - iteratorVariable4.Width) / 2, ((iconSize + (recommendedExtent * 2)) - iteratorVariable4.Height) / 2));
            try
            {
                iteratorVariable6 = iteratorVariable5.ToSurface();
            }
            catch (Exception)
            {
                goto Label_PostSwitchInIterator;
            }
            using (RenderArgs iteratorVariable7 = new RenderArgs(iteratorVariable6))
            {
                MostRecentFile mrf = new MostRecentFile(fullPath, Utility.FullCloneBitmap(iteratorVariable7.Bitmap));
                yield return Directive.DispatchTo(this.appWorkspace.Dispatcher);
                if (this.AppWorkspace.MostRecentFiles.Contains(fullPath))
                {
                    this.AppWorkspace.MostRecentFiles.Remove(fullPath);
                }
                this.AppWorkspace.MostRecentFiles.Add(mrf);
                this.AppWorkspace.MostRecentFiles.SaveMruList();
            }
            iteratorVariable6.Dispose();
        Label_PostSwitchInIterator:;
        }

        private void BeginZoomChanges()
        {
            this.zoomChangesCount++;
        }

        public Surface BorrowScratchSurface(string reason)
        {
            if (this.isScratchSurfaceBorrowed)
            {
                throw new InvalidOperationException("ScratchSurface already borrowed: '" + this.borrowScratchSurfaceReason + "' (trying to borrow for: '" + reason + "')");
            }
            this.isScratchSurfaceBorrowed = true;
            this.borrowScratchSurfaceReason = reason;
            return this.scratchSurface;
        }

        public static DialogResult ChooseFile(Control parent, out string fileName) => 
            ChooseFile(parent, out fileName, null);

        public static DialogResult ChooseFile(Control parent, out string fileName, string startingDir)
        {
            string[] strArray;
            DialogResult result = ChooseFiles(parent, out strArray, false, startingDir);
            if (result == DialogResult.OK)
            {
                fileName = strArray[0];
                return result;
            }
            fileName = null;
            return result;
        }

        public static DialogResult ChooseFiles(Control owner, out string[] fileNames, bool multiselect) => 
            ChooseFiles(owner, out fileNames, multiselect, null);

        public static DialogResult ChooseFiles(Control owner, out string[] fileNames, bool multiselect, string startingDir)
        {
            FileTypeCollection fileTypes = FileTypes.GetFileTypes();
            using (PaintDotNet.SystemLayer.IFileOpenDialog dialog = CommonDialogs.CreateFileOpenDialog())
            {
                if (startingDir != null)
                {
                    dialog.InitialDirectory = startingDir;
                }
                else
                {
                    dialog.InitialDirectory = GetDefaultSavePath();
                }
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.Multiselect = multiselect;
                dialog.Filter = fileTypes.ToString(true, PdnResources.GetString2("FileDialog.Types.AllImages"), false, true);
                dialog.FilterIndex = 0;
                DialogResult result = ShowFileDialog(owner, dialog, true);
                if (result == DialogResult.OK)
                {
                    fileNames = dialog.FileNames;
                }
                else
                {
                    fileNames = new string[0];
                }
                return result;
            }
        }

        public IRenderer<ColorBgra> CreateThumbnailRenderer(int maxEdgeLength)
        {
            if (base.Document == null)
            {
                return RendererBgra.Create(1, 1).Clear(ColorBgra.Red);
            }
            IRenderer<ColorBgra> source = base.Document.CreateRenderer();
            Int32Size size = Utility.ComputeThumbnailSize(base.Document.Size(), maxEdgeLength);
            IRenderer<ColorBgra> sourceLHS = RendererBgra.Checkers(size);
            IRenderer<ColorBgra> sourceRHS = source.ResizeSuperSampling(size);
            return sourceLHS.DrawBlend(UserBlendOps.NormalBlendOp.Static, sourceRHS);
        }

        public PaintDotNet.Tools.Tool CreateTool(System.Type toolType) => 
            CreateTool(toolType, this);

        private static PaintDotNet.Tools.Tool CreateTool(System.Type toolType, DocumentWorkspace dc) => 
            ((PaintDotNet.Tools.Tool) toolType.GetConstructor(new System.Type[] { typeof(DocumentWorkspace) }).Invoke(new object[] { dc }));

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.taskManager != null)
                {
                    this.taskManager.BeginShutdown();
                    this.taskManager.Dispose();
                    this.taskManager = null;
                }
                if (this.activeTool != null)
                {
                    this.activeTool.Dispose();
                    this.activeTool = null;
                }
                if (this.toolPulseTimer != null)
                {
                    this.toolPulseTimer.Dispose();
                    this.toolPulseTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        public bool DoSave() => 
            this.DoSave(false);

        protected bool DoSave(bool tryToFlatten)
        {
            using (new PushNullToolMode(this))
            {
                string str;
                <>c__DisplayClass18 class3;
                PaintDotNet.FileType newFileType;
                PaintDotNet.SaveConfigToken newSaveConfigToken;
                this.GetDocumentSaveOptions(out str, out newFileType, out newSaveConfigToken);
                if (str == null)
                {
                    return this.DoSaveAs();
                }
                if (newFileType == null)
                {
                    FileTypeCollection fileTypes = FileTypes.GetFileTypes();
                    string extension = Path.GetExtension(str);
                    int num = fileTypes.IndexOfExtension(extension);
                    newFileType = fileTypes[num];
                }
                if ((base.Document.Layers.Count > 1) && !newFileType.SupportsLayers)
                {
                    if (!tryToFlatten)
                    {
                        return this.DoSaveAs();
                    }
                    if (this.WarnAboutFlattening() != DialogResult.Yes)
                    {
                        return false;
                    }
                    this.ExecuteFunction(new FlattenFunction());
                }
                if (newSaveConfigToken == null)
                {
                    bool flag;
                    Surface saveScratchSurface = this.BorrowScratchSurface(base.GetType().Name + ".DoSave() calling GetSaveConfigToken()");
                    try
                    {
                        flag = this.GetSaveConfigToken(newFileType, newSaveConfigToken, out newSaveConfigToken, saveScratchSurface);
                    }
                    finally
                    {
                        this.ReturnScratchSurface(saveScratchSurface);
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
                if (newFileType.SupportsCustomHeaders)
                {
                    using (new WaitCursorChanger(this))
                    {
                        ISurface<ColorBgra> surface2;
                        byte[] buffer;
                        Utility.GCFullCollect();
                        if ((base.Document.Width > 0x100) || (base.Document.Height > 0x100))
                        {
                            Int32Size newSize = Utility.ComputeThumbnailSize(base.Document.Size(), 0x100);
                            surface2 = base.Document.CreateRenderer().ResizeSuperSampling(newSize).Parallelize(7).ToSurface();
                        }
                        else
                        {
                            surface2 = base.Document.CreateRenderer().Parallelize(7).ToSurface();
                        }
                        using (MemoryStream stream = new MemoryStream())
                        {
                            using (Bitmap bitmap = surface2.CreateAliasedGdipBitmap())
                            {
                                bitmap.Save(stream, ImageFormat.Png);
                                buffer = stream.GetBuffer();
                            }
                        }
                        surface2.Dispose();
                        string str3 = Convert.ToBase64String(buffer, Base64FormattingOptions.None);
                        string str4 = "<thumb png=\"" + str3 + "\" />";
                        base.Document.CustomHeaders = str4;
                    }
                }
                Result saveResult = null;
                Surface saveScratch = this.BorrowScratchSurface(base.GetType().Name + ".DoSave() for purposes of saving");
                try
                {
                    using (SaveTransaction saveTx = new SaveTransaction(str, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileOptions.None))
                    {
                        VirtualTask<Unit> saveTask = this.TaskManager.CreateVirtualTask();
                        TaskProgressDialog progressDialog = new TaskProgressDialog {
                            Task = saveTask,
                            CloseOnFinished = true,
                            ShowCancelButton = false,
                            Text = PdnResources.GetString2("SaveProgressDialog.Title")
                        };
                        string str5 = PdnResources.GetString2("TaskProgressDialog.Initializing.Text");
                        string savingText = PdnResources.GetString2("SaveProgressDialog.Description");
                        string savingWithPercentTextFormat = PdnResources.GetString2("SaveProgressDialog.DescriptionWithPercent.Format");
                        progressDialog.HeaderText = str5;
                        progressDialog.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuFileSaveIcon.png").Reference);
                        progressDialog.Shown += delegate {
                            WaitCallback callBack = null;
                            try
                            {
                                if (callBack == null)
                                {
                                    callBack = delegate {
                                        Action f = null;
                                        try
                                        {
                                            saveTask.SetState(TaskState.Running);
                                            progressDialog.SetHeaderTextAsync(savingText);
                                            if (f == null)
                                            {
                                                f = delegate {
                                                    ProgressEventHandler callback = null;
                                                    try
                                                    {
                                                        if (callback == null)
                                                        {
                                                            callback = delegate (object s2, ProgressEventArgs e2) {
                                                                saveTask.Progress = new double?(DoubleUtil.Clamp(e2.Percent / 100.0, 0.0, 1.0));
                                                                progressDialog.SetHeaderTextAsync(string.Format(savingWithPercentTextFormat, ((int) e2.Percent).Clamp(0, 100)));
                                                            };
                                                        }
                                                        newFileType.Save(this.Document, saveTx.Stream, newSaveConfigToken, saveScratch, callback, true);
                                                        saveTx.Stream.Flush();
                                                        Exception[] innerExceptions = saveTx.Stream.Exceptions.ToArrayEx<Exception>();
                                                        if (innerExceptions.Length != 0)
                                                        {
                                                            saveTx.Rollback();
                                                            throw new MultiException(innerExceptions);
                                                        }
                                                        saveTx.Commit();
                                                    }
                                                    catch (Exception)
                                                    {
                                                        switch (saveTx.State)
                                                        {
                                                            case SaveTransactionState.Initialized:
                                                            case SaveTransactionState.FailedCommit:
                                                                saveTx.Rollback();
                                                                break;
                                                        }
                                                        throw;
                                                    }
                                                };
                                            }
                                            saveResult = f.Try();
                                        }
                                        finally
                                        {
                                            saveTask.SetState(TaskState.Finished);
                                        }
                                    };
                                }
                                ThreadPool.QueueUserWorkItem(callBack);
                            }
                            catch (Exception exception)
                            {
                                saveResult = Result.NewError(exception);
                                saveTask.SetState(TaskState.Finished);
                            }
                        };
                        progressDialog.ShowDialog(this);
                        DisposableUtil.Free<TaskProgressDialog>(ref progressDialog);
                        this.lastSaveTime = DateTime.Now;
                    }
                }
                catch (Exception exception)
                {
                    if ((saveResult == null) || !saveResult.IsError)
                    {
                        saveResult = Result.NewError(exception);
                    }
                    else
                    {
                        saveResult.Observe();
                        saveResult = Result.NewError(new MultiException(new Exception[] { exception, saveResult.Error }));
                    }
                }
                this.ReturnScratchSurface(saveScratch);
                if (saveResult == null)
                {
                    Utility.ErrorBox(this, PdnResources.GetString2("SaveImage.Error.Exception"));
                    return false;
                }
                if (saveResult.IsError)
                {
                    if (saveResult.Error is UnauthorizedAccessException)
                    {
                        Utility.ErrorBox(this, PdnResources.GetString2("SaveImage.Error.UnauthorizedAccessException"));
                    }
                    else if (saveResult.Error is SecurityException)
                    {
                        Utility.ErrorBox(this, PdnResources.GetString2("SaveImage.Error.SecurityException"));
                    }
                    else if (saveResult.Error is DirectoryNotFoundException)
                    {
                        Utility.ErrorBox(this, PdnResources.GetString2("SaveImage.Error.DirectoryNotFoundException"));
                    }
                    else if (saveResult.Error is IOException)
                    {
                        Utility.ErrorBox(this, PdnResources.GetString2("SaveImage.Error.IOException"));
                    }
                    else if (saveResult.Error is OutOfMemoryException)
                    {
                        Utility.ErrorBox(this, PdnResources.GetString2("SaveImage.Error.OutOfMemoryException"));
                    }
                    else
                    {
                        Utility.ErrorBox(this, PdnResources.GetString2("SaveImage.Error.Exception"));
                    }
                    saveResult.Observe();
                    return false;
                }
                base.Document.Dirty = false;
                this.SetDocumentSaveOptions(str, newFileType, newSaveConfigToken);
                Task task = this.AddToMruList();
                UI.BeginFrame(this, true, delegate (UI.IFrame frame) {
                    <>c__DisplayClass18 class1 = class3;
                    task.ResultAsync().Receive(delegate (Result r) {
                        r.Observe();
                        frame.Close();
                    }).Observe();
                });
                return true;
            }
        }

        public bool DoSaveAs()
        {
            using (new PushNullToolMode(this))
            {
                string str;
                PaintDotNet.FileType type;
                PaintDotNet.SaveConfigToken token;
                bool flag;
                Surface saveScratchSurface = this.BorrowScratchSurface(base.GetType() + ".DoSaveAs() handing off scratch surface to DoSaveAsDialog()");
                try
                {
                    flag = this.DoSaveAsDialog(out str, out type, out token, saveScratchSurface);
                }
                finally
                {
                    this.ReturnScratchSurface(saveScratchSurface);
                }
                if (flag)
                {
                    string str2;
                    PaintDotNet.FileType type2;
                    PaintDotNet.SaveConfigToken token2;
                    this.GetDocumentSaveOptions(out str2, out type2, out token2);
                    this.SetDocumentSaveOptions(str, type, token);
                    bool flag2 = this.DoSave(true);
                    if (!flag2)
                    {
                        this.SetDocumentSaveOptions(str2, type2, token2);
                    }
                    return flag2;
                }
                return false;
            }
        }

        private bool DoSaveAsDialog(out string newFileName, out PaintDotNet.FileType newFileType, out PaintDotNet.SaveConfigToken newSaveConfigToken, Surface saveScratchSurface)
        {
            FileTypeCollection types = new FileTypeCollection(from ft in FileTypes.GetFileTypes().FileTypes
                where ft.SupportsSaving
                select ft);
            using (PaintDotNet.SystemLayer.IFileSaveDialog dialog = CommonDialogs.CreateFileSaveDialog())
            {
                string str2;
                PaintDotNet.FileType type;
                PaintDotNet.SaveConfigToken token;
                string defaultSavePath;
                bool flag;
                string defaultSaveName;
                string defaultExtension;
                PaintDotNet.FileType pdn;
                PaintDotNet.SaveConfigToken token2;
                string str9;
                bool flag2;
                string str10;
                PaintDotNet.FileType type3;
                PaintDotNet.SaveConfigToken token3;
                dialog.AddExtension = true;
                dialog.CheckPathExists = true;
                dialog.OverwritePrompt = true;
                string str = types.ToString(false, null, true, false);
                dialog.Filter = str;
                this.GetDocumentSaveOptions(out str2, out type, out token);
                if (((base.Document.Layers.Count > 1) && (type != null)) && !type.SupportsLayers)
                {
                    pdn = PdnFileTypes.Pdn;
                    token2 = null;
                }
                else if (type == null)
                {
                    if (base.Document.Layers.Count == 1)
                    {
                        pdn = PdnFileTypes.Png;
                    }
                    else
                    {
                        pdn = PdnFileTypes.Pdn;
                    }
                    token2 = null;
                }
                else
                {
                    pdn = type;
                    token2 = token;
                }
                if (str2 == null)
                {
                    defaultSaveName = GetDefaultSaveName();
                    if ((defaultSaveName.Length > 0) && (defaultSaveName[0] == '.'))
                    {
                        defaultSaveName = defaultSaveName.Substring(1);
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                    defaultSavePath = GetDefaultSavePath();
                    defaultExtension = pdn.DefaultExtension;
                }
                else
                {
                    string fullPath = Path.GetFullPath(str2);
                    defaultSavePath = Path.GetDirectoryName(fullPath);
                    string fileName = Path.GetFileName(fullPath);
                    if ((fileName == null) || (fileName.Length == 0))
                    {
                        defaultSaveName = GetDefaultSaveName();
                    }
                    if (fileName[0] == '.')
                    {
                        flag = true;
                        fileName = fileName.Substring(1);
                    }
                    else
                    {
                        flag = false;
                    }
                    string extension = Path.GetExtension(fileName);
                    if (pdn.SupportsExtension(extension))
                    {
                        defaultSaveName = Path.ChangeExtension(fileName, null);
                        defaultExtension = extension;
                    }
                    else if (types.IndexOfExtension(extension) != -1)
                    {
                        defaultSaveName = Path.ChangeExtension(fileName, null);
                        defaultExtension = pdn.DefaultExtension;
                    }
                    else
                    {
                        defaultSaveName = fileName;
                        defaultExtension = pdn.DefaultExtension;
                    }
                }
                if (pdn.SupportsExtension(defaultExtension))
                {
                    str9 = (flag ? "." : "") + defaultSaveName;
                }
                else
                {
                    str9 = (flag ? "." : "") + defaultSaveName + defaultExtension;
                }
                dialog.InitialDirectory = defaultSavePath;
                dialog.FileName = str9;
                dialog.FilterIndex = 1 + types.IndexOfFileType(pdn);
                dialog.Title = PdnResources.GetString2("SaveAsDialog.Title");
                if (ShowFileDialog(this, dialog, false) != DialogResult.OK)
                {
                    flag2 = false;
                    str10 = null;
                    type3 = null;
                    token3 = null;
                }
                else
                {
                    str10 = dialog.FileName;
                    type3 = types[dialog.FilterIndex - 1];
                    flag2 = this.GetSaveConfigToken(type3, token2, out token3, saveScratchSurface);
                }
                if (flag2)
                {
                    newFileName = str10;
                    newFileType = type3;
                    newSaveConfigToken = token3;
                }
                else
                {
                    newFileName = null;
                    newFileType = null;
                    newSaveConfigToken = null;
                }
                return flag2;
            }
        }

        private void EndZoomChanges()
        {
            this.zoomChangesCount--;
        }

        private static string GetDefaultSaveName() => 
            PdnResources.GetString2("Untitled.FriendlyName");

        private static string GetDefaultSavePath()
        {
            string virtualPath;
            try
            {
                virtualPath = Shell.GetVirtualPath(VirtualFolderName.UserPictures, false);
                new DirectoryInfo(virtualPath);
            }
            catch (Exception)
            {
                virtualPath = "";
            }
            string path = Settings.CurrentUser.GetString("LastFileDialogDirectory", null);
            if (path == null)
            {
                return virtualPath;
            }
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                if (!info.Exists)
                {
                    path = virtualPath;
                }
            }
            catch (Exception)
            {
                path = virtualPath;
            }
            return path;
        }

        public void GetDocumentSaveOptions(out string filePathResult, out PaintDotNet.FileType fileTypeResult, out PaintDotNet.SaveConfigToken saveConfigTokenResult)
        {
            filePathResult = this.filePath;
            fileTypeResult = this.fileType;
            if (this.saveConfigToken == null)
            {
                saveConfigTokenResult = null;
            }
            else
            {
                saveConfigTokenResult = (PaintDotNet.SaveConfigToken) this.saveConfigToken.Clone();
            }
        }

        public string GetFriendlyName()
        {
            if (this.filePath != null)
            {
                return Path.GetFileName(this.filePath);
            }
            return PdnResources.GetString2("Untitled.FriendlyName");
        }

        private bool GetSaveConfigToken(PaintDotNet.FileType currentFileType, PaintDotNet.SaveConfigToken currentSaveConfigToken, out PaintDotNet.SaveConfigToken newSaveConfigToken, Surface saveScratchSurface)
        {
            ProgressEventHandler handler2 = null;
            if (currentFileType.SupportsConfiguration)
            {
                using (SaveConfigDialog dialog = new SaveConfigDialog())
                {
                    dialog.ScratchSurface = saveScratchSurface;
                    if (handler2 == null)
                    {
                        handler2 = delegate (object sender, ProgressEventArgs e) {
                            if ((e.Percent < 1.0) || (e.Percent >= 100.0))
                            {
                                this.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                                this.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                            }
                            else
                            {
                                this.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(e.Percent);
                            }
                        };
                    }
                    ProgressEventHandler handler = handler2;
                    dialog.Progress += handler;
                    dialog.Document = base.Document;
                    dialog.FileType = currentFileType;
                    PaintDotNet.SaveConfigToken lastSaveConfigToken = currentFileType.GetLastSaveConfigToken();
                    if ((currentSaveConfigToken != null) && (lastSaveConfigToken.GetType() == currentSaveConfigToken.GetType()))
                    {
                        dialog.SaveConfigToken = currentSaveConfigToken;
                    }
                    dialog.EnableInstanceOpacity = false;
                    DialogResult result = dialog.ShowDialog(this);
                    dialog.Progress -= handler;
                    this.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                    this.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                    if (result == DialogResult.OK)
                    {
                        newSaveConfigToken = dialog.SaveConfigToken;
                        return true;
                    }
                    newSaveConfigToken = null;
                    return false;
                }
            }
            newSaveConfigToken = currentFileType.GetLastSaveConfigToken();
            return true;
        }

        public object GetStaticToolData(System.Type toolType) => 
            this.staticToolData[toolType];

        public System.Type GetToolType()
        {
            if (this.Tool != null)
            {
                return this.Tool.GetType();
            }
            return null;
        }

        protected override void HandleMouseWheel(Control sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                double num = ((double) e.Delta) / 120.0;
                Rect visibleDocumentBounds = base.VisibleDocumentBounds;
                System.Windows.Point point = base.MouseToDocument(sender, new System.Drawing.Point(e.X, e.Y));
                Rect visibleDocumentRect = base.VisibleDocumentRect;
                System.Windows.Point point2 = new System.Windows.Point((point.X - visibleDocumentRect.X) / visibleDocumentRect.Width, (point.Y - visibleDocumentRect.Y) / visibleDocumentRect.Height);
                double factor = Math.Pow(1.12, Math.Abs(num));
                if (e.Delta > 0)
                {
                    this.ZoomIn(factor);
                }
                else if (e.Delta < 0)
                {
                    this.ZoomOut(factor);
                }
                Rect rect2 = base.VisibleDocumentRect;
                System.Windows.Point point3 = new System.Windows.Point(point.X - (rect2.Width * point2.X), point.Y - (rect2.Height * point2.Y));
                base.DocumentScrollPosition = point3;
                Rect rect3 = base.VisibleDocumentBounds;
                bool highQualityZoomIn = base.DocumentBox.HighQualityZoomIn;
                bool highQualityZoomOut = base.DocumentBox.HighQualityZoomOut;
                if (!this.useHQFluidZoom)
                {
                    base.DocumentBox.HighQualityZoomIn = false;
                    base.DocumentBox.HighQualityZoomOut = false;
                }
                base.Update();
                if (!this.useHQFluidZoom)
                {
                    base.DocumentBox.HighQualityZoomIn = highQualityZoomIn;
                    base.DocumentBox.HighQualityZoomOut = highQualityZoomOut;
                }
            }
            base.HandleMouseWheel(sender, e);
        }

        private void InitializeComponent()
        {
            this.toolPulseTimer = new System.Windows.Forms.Timer();
            this.toolPulseTimer.Interval = 0x10;
            this.toolPulseTimer.Tick += new EventHandler(this.ToolPulseTimer_Tick);
        }

        private static void InitializeToolInfos()
        {
            int index = 0;
            toolInfos = new ToolInfo[tools.Length];
            foreach (System.Type type in tools)
            {
                using (PaintDotNet.Tools.Tool tool = CreateTool(type, null))
                {
                    toolInfos[index] = tool.Info;
                    index++;
                }
            }
        }

        private static void InitializeTools()
        {
            tools = new System.Type[] { 
                typeof(RectangleSelectTool), typeof(MoveTool), typeof(LassoSelectTool), typeof(MoveSelectionTool), typeof(EllipseSelectTool), typeof(ZoomTool), typeof(MagicWandTool), typeof(PanTool), typeof(PaintBucketTool), typeof(GradientTool), typeof(PaintBrushTool), typeof(EraserTool), typeof(PencilTool), typeof(ColorPickerTool), typeof(CloneStampTool), typeof(RecolorTool),
                typeof(TextTool), typeof(LineTool), typeof(RectangleTool), typeof(RoundedRectangleTool), typeof(EllipseTool), typeof(FreeformShapeTool)
            };
        }

        private void LayerInsertedHandler(object sender, IndexEventArgs e)
        {
            Layer layer = (Layer) base.Document.Layers[e.Index];
            this.ActiveLayer = layer;
            layer.PropertyChanging += new PropertyEventHandler(this.LayerPropertyChangingHandler);
            layer.PropertyChanged += new PropertyEventHandler(this.LayerPropertyChangedHandler);
        }

        private void LayerPropertyChangedHandler(object sender, PropertyEventArgs e)
        {
            Layer layer = (Layer) sender;
            if ((!layer.Visible && (layer == this.ActiveLayer)) && ((base.Document.Layers.Count > 1) && !this.History.IsExecutingMemento))
            {
                this.SelectClosestVisibleLayer(layer);
            }
        }

        private void LayerPropertyChangingHandler(object sender, PropertyEventArgs e)
        {
            LayerPropertyHistoryMemento memento = new LayerPropertyHistoryMemento(string.Format(PdnResources.GetString2("LayerPropertyChanging.HistoryMementoNameFormat"), e.PropertyName), PdnResources.GetImageResource2("Icons.MenuLayersLayerPropertiesIcon.png"), this, base.Document.Layers.IndexOf(sender));
            this.History.PushNewMemento(memento);
        }

        private void LayerRemovedHandler(object sender, IndexEventArgs e)
        {
        }

        private void LayerRemovingHandler(object sender, IndexEventArgs e)
        {
            int num;
            Layer layer = (Layer) base.Document.Layers[e.Index];
            layer.PropertyChanging -= new PropertyEventHandler(this.LayerPropertyChangingHandler);
            layer.PropertyChanged -= new PropertyEventHandler(this.LayerPropertyChangedHandler);
            if (e.Index == (base.Document.Layers.Count - 1))
            {
                num = e.Index - 1;
            }
            else
            {
                num = e.Index + 1;
            }
            if ((num >= 0) && (num < base.Document.Layers.Count))
            {
                this.ActiveLayer = (Layer) base.Document.Layers[num];
            }
            else if (base.Document.Layers.Count == 0)
            {
                this.ActiveLayer = null;
            }
            else
            {
                this.ActiveLayer = (Layer) base.Document.Layers[0];
            }
        }

        public static Document LoadDocument(Control owner, string fileName, out PaintDotNet.FileType fileTypeResult, ProgressEventHandler progressCallback)
        {
            Func<ArgumentException, string> f = null;
            PaintDotNet.FileType fileType;
            fileTypeResult = null;
            try
            {
                FileTypeCollection fileTypes = FileTypes.GetFileTypes();
                int num = fileTypes.IndexOfExtension(Path.GetExtension(fileName));
                if (num == -1)
                {
                    Utility.ErrorBox(owner, PdnResources.GetString2("LoadImage.Error.ImageTypeNotRecognized"));
                    return null;
                }
                fileType = fileTypes[num];
                fileTypeResult = fileType;
            }
            catch (ArgumentException)
            {
                string message = string.Format(PdnResources.GetString2("LoadImage.Error.InvalidFileName.Format"), fileName);
                Utility.ErrorBox(owner, message);
                return null;
            }
            Stream underlyingStream = null;
            Result<Document> docResult = Result.NewError<Document>(new Exception(), false);
            try
            {
                <>c__DisplayClass3f classf;
                underlyingStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                long totalBytes = 0L;
                SiphonStream siphonStream = new SiphonStream(underlyingStream);
                IOEventHandler handler = null;
                handler = delegate (object sender, IOEventArgs e) {
                    <>c__DisplayClass3f classf1 = classf;
                    ((Action) (() => classf1.owner.BeginInvoke(() => delegate {
                        if (classf1.progressCallback != null)
                        {
                            totalBytes += e.Count;
                            double percent = (100.0 * (((double) totalBytes) / ((double) siphonStream.Length))).Clamp(0.0, 100.0);
                            classf1.progressCallback(null, new ProgressEventArgs(percent));
                        }
                    }.Try().Observe()))).Try().Observe();
                };
                siphonStream.IOFinished += handler;
                UI.BeginFrame(owner, true, delegate (UI.IFrame ifc) {
                    WaitCallback callBack = null;
                    <>c__DisplayClass3f classf1 = classf;
                    try
                    {
                        if (callBack == null)
                        {
                            callBack = delegate {
                                try
                                {
                                    classf1.docResult = new Func<Stream, Document>(classf1.fileType.Load).Eval<Stream, Document>(siphonStream);
                                }
                                finally
                                {
                                    ifc.Close();
                                }
                            };
                        }
                        ThreadPool.QueueUserWorkItem(callBack);
                    }
                    catch (Exception exception)
                    {
                        docResult = Result.NewError<Document>(exception);
                        ifc.Close();
                    }
                });
                if (progressCallback != null)
                {
                    progressCallback(null, new ProgressEventArgs(100.0));
                }
                siphonStream.IOFinished -= handler;
                siphonStream.Close();
            }
            catch (Exception exception)
            {
                docResult = Result.NewError<Document>(exception);
            }
            if (docResult.IsValue)
            {
                Metadata metadata = docResult.Value.Metadata;
                metadata.RemoveExifValues(ExifTagID.JPEGInterchangeFormat);
                metadata.RemoveExifValues(ExifTagID.JPEGInterchangeFormatLength);
                metadata.RemoveExifValues(ExifTagID.ThumbnailData);
                metadata.RemoveExifValues(ExifTagID.Orientation);
                return docResult.Value;
            }
            if (f == null)
            {
                f = delegate (ArgumentException ex) {
                    if (fileName.Length == 0)
                    {
                        return "LoadImage.Error.BlankFileName";
                    }
                    return "LoadImage.Error.ArgumentException";
                };
            }
            string stringName = Result.NewError<string>(docResult.Error, false).Repair<string, ArgumentException>(f).Repair<string, UnauthorizedAccessException>(ex => "LoadImage.Error.UnauthorizedAccessException").Repair<string, SecurityException>(ex => "LoadImage.Error.SecurityException").Repair<string, FileNotFoundException>(ex => "LoadImage.Error.FileNotFoundException").Repair<string, DirectoryNotFoundException>(ex => "LoadImage.Error.DirectoryNotFoundException").Repair<string, PathTooLongException>(ex => "LoadImage.Error.PathTooLongException").Repair<string, IOException>(ex => "LoadImage.Error.IOException").Repair<string, SerializationException>(ex => "LoadImage.Error.SerializationException").Repair<string, OutOfMemoryException>(ex => "LoadImage.Error.OutOfMemoryException").Repair<string>(ex => "LoadImage.Error.Exception").Value;
            Utility.ErrorBox(owner, PdnResources.GetString2(stringName));
            if (underlyingStream != null)
            {
                underlyingStream.Close();
                underlyingStream = null;
            }
            return null;
        }

        protected override void OnDocumentChanged()
        {
            if (base.Document == null)
            {
                this.ActiveLayer = null;
            }
            else
            {
                if (this.activeTool != null)
                {
                    throw new InvalidOperationException("Tool was not deactivated while Document was being changed");
                }
                if (this.scratchSurface != null)
                {
                    if (this.isScratchSurfaceBorrowed)
                    {
                        throw new InvalidOperationException("scratchSurface is currently borrowed: " + this.borrowScratchSurfaceReason);
                    }
                    if ((base.Document == null) || (this.scratchSurface.Size != base.Document.Size))
                    {
                        this.scratchSurface.Dispose();
                        this.scratchSurface = null;
                    }
                }
                if (this.scratchSurface == null)
                {
                    this.scratchSurface = new Surface(base.Document.Size);
                }
                this.Selection.ClipRectangle = base.Document.Bounds();
                foreach (Layer layer in base.Document.Layers)
                {
                    layer.PropertyChanging += new PropertyEventHandler(this.LayerPropertyChangingHandler);
                    layer.PropertyChanged += new PropertyEventHandler(this.LayerPropertyChangedHandler);
                }
                base.Document.Layers.RemovingAt += new IndexEventHandler(this.LayerRemovingHandler);
                base.Document.Layers.RemovedAt += new IndexEventHandler(this.LayerRemovedHandler);
                base.Document.Layers.Inserted += new IndexEventHandler(this.LayerInsertedHandler);
                if (!base.Document.Layers.Contains(this.ActiveLayer))
                {
                    if (base.Document.Layers.Count > 0)
                    {
                        if ((this.savedAli >= 0) && (this.savedAli < base.Document.Layers.Count))
                        {
                            this.ActiveLayer = (Layer) base.Document.Layers[this.savedAli];
                        }
                        else
                        {
                            this.ActiveLayer = (Layer) base.Document.Layers[0];
                        }
                    }
                    else
                    {
                        this.ActiveLayer = null;
                    }
                }
                foreach (Layer layer2 in base.Document.Layers)
                {
                    layer2.Invalidate();
                }
                bool dirty = base.Document.Dirty;
                base.Document.Invalidate();
                base.Document.Dirty = dirty;
                this.ZoomBasis = this.savedZb;
                if (this.savedZb == PaintDotNet.ZoomBasis.ScaleFactor)
                {
                    base.ScaleFactor = this.savedSf;
                }
            }
            this.PopNullTool();
            base.AutoScrollPosition = new System.Drawing.Point(0, 0);
            base.OnDocumentChanged();
        }

        protected override void OnDocumentChanging(Document newDocument)
        {
            base.OnDocumentChanging(newDocument);
            this.savedZb = this.ZoomBasis;
            this.savedSf = base.ScaleFactor;
            if (this.ActiveLayer != null)
            {
                this.savedAli = this.ActiveLayerIndex;
            }
            else
            {
                this.savedAli = -1;
            }
            if (newDocument != null)
            {
                this.UpdateExifTags(newDocument);
            }
            if (base.Document != null)
            {
                foreach (Layer layer in base.Document.Layers)
                {
                    layer.PropertyChanging -= new PropertyEventHandler(this.LayerPropertyChangingHandler);
                    layer.PropertyChanged -= new PropertyEventHandler(this.LayerPropertyChangedHandler);
                }
                base.Document.Layers.RemovingAt -= new IndexEventHandler(this.LayerRemovingHandler);
                base.Document.Layers.RemovedAt -= new IndexEventHandler(this.LayerRemovedHandler);
                base.Document.Layers.Inserted -= new IndexEventHandler(this.LayerInsertedHandler);
            }
            this.staticToolData.Clear();
            this.PushNullTool();
            this.ActiveLayer = null;
            if (this.scratchSurface != null)
            {
                if (this.isScratchSurfaceBorrowed)
                {
                    throw new InvalidOperationException("scratchSurface is currently borrowed: " + this.borrowScratchSurfaceReason);
                }
                if ((newDocument == null) || (newDocument.Size != this.scratchSurface.Size))
                {
                    this.scratchSurface.Dispose();
                    this.scratchSurface = null;
                }
            }
            if (!this.Selection.IsEmpty)
            {
                this.Selection.Reset();
            }
        }

        protected virtual void OnFilePathChanged()
        {
            if (this.FilePathChanged != null)
            {
                this.FilePathChanged(this, EventArgs.Empty);
            }
        }

        protected void OnLayerChanged()
        {
            base.Focus();
            if (this.ActiveLayerChanged != null)
            {
                this.ActiveLayerChanged(this, EventArgs.Empty);
            }
        }

        protected void OnLayerChanging()
        {
            if (this.ActiveLayerChanging != null)
            {
                this.ActiveLayerChanging(this, EventArgs.Empty);
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (this.zoomBasis == PaintDotNet.ZoomBasis.FitToWindow)
            {
                base.ZoomToWindow();
                base.PanelAutoScroll = true;
                base.PanelAutoScroll = false;
            }
            base.OnLayout(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.appWorkspace == null)
            {
                throw new InvalidOperationException("Must set the Workspace property");
            }
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (this.zoomBasis == PaintDotNet.ZoomBasis.FitToWindow)
            {
                base.PerformLayout();
            }
            base.OnResize(e);
        }

        protected virtual void OnSaveOptionsChanged()
        {
            if (this.SaveOptionsChanged != null)
            {
                this.SaveOptionsChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.PerformLayout();
            base.OnSizeChanged(e);
        }

        protected virtual void OnStatusChanged()
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(this, EventArgs.Empty);
            }
        }

        protected void OnToolChanged()
        {
            if (this.ToolChanged != null)
            {
                this.ToolChanged(this, EventArgs.Empty);
            }
        }

        protected void OnToolChanging()
        {
            if (this.ToolChanging != null)
            {
                this.ToolChanging(this, EventArgs.Empty);
            }
        }

        protected override void OnUnitsChanged()
        {
            if (!this.Selection.IsEmpty)
            {
                this.UpdateSelectionInfoInStatusBar();
            }
            base.OnUnitsChanged();
        }

        protected virtual void OnZoomBasisChanged()
        {
            if (this.ZoomBasisChanged != null)
            {
                this.ZoomBasisChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnZoomBasisChanging()
        {
            if (this.ZoomBasisChanging != null)
            {
                this.ZoomBasisChanging(this, EventArgs.Empty);
            }
        }

        public void PerformAction(DocumentWorkspaceAction action)
        {
            bool flag = false;
            if ((action.ActionFlags & ActionFlags.KeepToolActive) != ActionFlags.KeepToolActive)
            {
                this.PushNullTool();
                base.Update();
                flag = true;
            }
            try
            {
                using (new WaitCursorChanger(this))
                {
                    HistoryMemento memento = action.PerformAction(this);
                    if (memento != null)
                    {
                        this.History.PushNewMemento(memento);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    this.PopNullTool();
                }
            }
        }

        public void PerformAction(System.Type actionType, string newName, ImageResource icon)
        {
            using (new WaitCursorChanger(this))
            {
                DocumentWorkspaceAction action = actionType.GetConstructor(new System.Type[] { typeof(DocumentWorkspace) }).Invoke(new object[] { this }) as DocumentWorkspaceAction;
                if (action != null)
                {
                    bool flag = false;
                    if ((action.ActionFlags & ActionFlags.KeepToolActive) != ActionFlags.KeepToolActive)
                    {
                        this.PushNullTool();
                        base.Update();
                        flag = true;
                    }
                    try
                    {
                        HistoryMemento memento = action.PerformAction(this);
                        if (memento != null)
                        {
                            memento.Name = newName;
                            memento.Image = icon;
                            this.History.PushNewMemento(memento);
                        }
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.PopNullTool();
                        }
                    }
                }
            }
        }

        public void PopNullTool()
        {
            this.nullToolCount--;
            if (this.nullToolCount == 0)
            {
                this.SetToolFromType(this.preNullTool);
                this.preNullTool = null;
            }
            else if (this.nullToolCount < 0)
            {
                throw new InvalidOperationException("PopNullTool() call was not matched with PushNullTool()");
            }
        }

        public void PushNullTool()
        {
            if (this.nullToolCount == 0)
            {
                this.preNullTool = this.GetToolType();
                this.SetTool(null);
                this.nullToolCount = 1;
            }
            else
            {
                this.nullToolCount++;
            }
        }

        public void ResumeToolCursorChanges()
        {
            this.suspendToolCursorChanges--;
            if ((this.suspendToolCursorChanges <= 0) && (this.activeTool != null))
            {
                this.Cursor = this.activeTool.Cursor;
            }
        }

        public void ReturnScratchSurface(Surface borrowedScratchSurface)
        {
            if (!this.isScratchSurfaceBorrowed)
            {
                throw new InvalidOperationException("ScratchSurface wasn't borrowed");
            }
            if (this.scratchSurface != borrowedScratchSurface)
            {
                throw new InvalidOperationException("returned ScratchSurface doesn't match the real one");
            }
            this.scratchSurface.Scan0.DiscardHint();
            this.isScratchSurfaceBorrowed = false;
            this.borrowScratchSurfaceReason = string.Empty;
        }

        public void SelectClosestVisibleLayer(Layer layer)
        {
            int index = base.Document.Layers.IndexOf(layer);
            int num2 = index;
            for (int i = 0; i < base.Document.Layers.Count; i++)
            {
                int num4 = index - i;
                int num5 = index + i;
                if (((num4 >= 0) && (num4 < base.Document.Layers.Count)) && ((Layer) base.Document.Layers[num4]).Visible)
                {
                    num2 = num4;
                    break;
                }
                if (((num5 >= 0) && (num5 < base.Document.Layers.Count)) && ((Layer) base.Document.Layers[num5]).Visible)
                {
                    num2 = num5;
                    break;
                }
            }
            if (num2 != index)
            {
                this.ActiveLayer = (Layer) base.Document.Layers[num2];
            }
        }

        private void Selection_Changed(object sender, EventArgs e)
        {
            this.UpdateRulerSelectionTinting();
            this.UpdateSelectionInfoInStatusBar();
        }

        public void SetDocumentSaveOptions(string newFilePath, PaintDotNet.FileType newFileType, PaintDotNet.SaveConfigToken newSaveConfigToken)
        {
            this.filePath = newFilePath;
            this.OnFilePathChanged();
            this.fileType = newFileType;
            if (newSaveConfigToken == null)
            {
                this.saveConfigToken = null;
            }
            else
            {
                this.saveConfigToken = (PaintDotNet.SaveConfigToken) newSaveConfigToken.Clone();
            }
            this.OnSaveOptionsChanged();
        }

        public void SetStaticToolData(System.Type toolType, object data)
        {
            this.staticToolData[toolType] = data;
        }

        public void SetStatus(string newStatusText, ImageResource newStatusIcon)
        {
            this.statusText = newStatusText;
            this.statusIcon = newStatusIcon;
            this.OnStatusChanged();
        }

        public void SetTool(PaintDotNet.Tools.Tool copyMe)
        {
            this.OnToolChanging();
            if (this.activeTool != null)
            {
                this.previousActiveToolType = this.activeTool.GetType();
                this.activeTool.CursorChanged -= new EventHandler(this.ToolCursorChangedHandler);
                this.activeTool.PerformDeactivate();
                this.activeTool.Dispose();
                this.activeTool = null;
            }
            if (copyMe == null)
            {
                this.EnableToolPulse = false;
            }
            else
            {
                this.activeTool = this.CreateTool(copyMe.GetType());
                this.activeTool.PerformActivate();
                this.activeTool.CursorChanged += new EventHandler(this.ToolCursorChangedHandler);
                if (this.suspendToolCursorChanges <= 0)
                {
                    this.Cursor = this.activeTool.Cursor;
                }
                this.EnableToolPulse = true;
            }
            this.OnToolChanged();
        }

        public void SetToolFromType(System.Type toolType)
        {
            if (toolType != this.GetToolType())
            {
                if (toolType == null)
                {
                    this.SetTool(null);
                }
                else
                {
                    PaintDotNet.Tools.Tool copyMe = this.CreateTool(toolType);
                    this.SetTool(copyMe);
                }
            }
        }

        public static DialogResult ShowFileDialog(Control owner, PaintDotNet.SystemLayer.IFileDialog fd, bool populateInitialDir)
        {
            if (populateInitialDir)
            {
                string path = Settings.CurrentUser.GetString("LastFileDialogDirectory", fd.InitialDirectory);
                try
                {
                    DirectoryInfo info = new DirectoryInfo(path);
                    using (new WaitCursorChanger(owner))
                    {
                        bool exists = info.Exists;
                        if (!info.Exists)
                        {
                            path = fd.InitialDirectory;
                        }
                    }
                }
                catch (Exception)
                {
                    path = fd.InitialDirectory;
                }
                fd.InitialDirectory = path;
            }
            DialogResult result = fd.ShowDialog(owner);
            if (result == DialogResult.OK)
            {
                string fileName;
                if (fd is PaintDotNet.SystemLayer.IFileOpenDialog)
                {
                    string[] fileNames = ((PaintDotNet.SystemLayer.IFileOpenDialog) fd).FileNames;
                    if (fileNames.Length > 0)
                    {
                        fileName = fileNames[0];
                    }
                    else
                    {
                        fileName = null;
                    }
                }
                else
                {
                    if (!(fd is PaintDotNet.SystemLayer.IFileSaveDialog))
                    {
                        throw new InvalidOperationException();
                    }
                    fileName = ((PaintDotNet.SystemLayer.IFileSaveDialog) fd).FileName;
                }
                if (fileName == null)
                {
                    throw new FileNotFoundException();
                }
                string directoryName = Path.GetDirectoryName(fileName);
                Settings.CurrentUser.SetString("LastFileDialogDirectory", directoryName);
            }
            return result;
        }

        public void SuspendToolCursorChanges()
        {
            this.suspendToolCursorChanges++;
        }

        private void ToolCursorChangedHandler(object sender, EventArgs e)
        {
            if (this.suspendToolCursorChanges <= 0)
            {
                this.Cursor = this.activeTool.Cursor;
            }
        }

        private void ToolPulseTimer_Tick(object sender, EventArgs e)
        {
            if (((base.FindForm() != null) && (base.FindForm().WindowState != FormWindowState.Minimized)) && ((this.Tool != null) && this.Tool.Active))
            {
                this.Tool.PerformPulse();
            }
        }

        private void UpdateExifTags(Document document)
        {
            System.Drawing.Imaging.PropertyItem item = Exif.CreateAscii(ExifTagID.Software, PdnInfo.GetInvariantProductName());
            document.Metadata.ReplaceExifValues(ExifTagID.Software, new System.Drawing.Imaging.PropertyItem[] { item });
        }

        public void UpdateRulerSelectionTinting()
        {
            if (base.RulersEnabled)
            {
                Rect boundsF = this.Selection.GetBoundsF();
                base.SetHighlightRectangle(boundsF);
            }
        }

        private void UpdateSelectionInfoInStatusBar()
        {
            if (this.Selection.IsEmpty)
            {
                this.UpdateStatusBarToToolHelpText();
            }
            else
            {
                string str;
                string str2;
                string str3;
                string str4;
                string str5;
                string str6;
                string str7;
                string str8;
                int num = 0;
                Int32Rect zero = Int32RectUtil.Zero;
                using (GeometryList list = this.Selection.CreateGeometryList())
                {
                    num = 0;
                    Int32Rect rect = base.Document.Bounds();
                    UnsafeList<Int32Rect> interiorScansUnsafeList = list.GetInteriorScansUnsafeList();
                    if (interiorScansUnsafeList.Count == 0)
                    {
                        Rect rect4 = Rect.Intersect(list.Bounds, rect.ToRect());
                        zero = new Int32Rect((int) Math.Floor(rect4.X), (int) Math.Floor(rect4.Y), 0, 0);
                    }
                    else
                    {
                        for (int i = 0; i < interiorScansUnsafeList.Count; i++)
                        {
                            if (i == 0)
                            {
                                zero = interiorScansUnsafeList[i];
                            }
                            else
                            {
                                zero = Int32RectUtil.Union(zero, interiorScansUnsafeList[i]);
                            }
                            Int32Rect rect5 = interiorScansUnsafeList[i].IntersectCopy(rect);
                            num += rect5.Area();
                        }
                    }
                }
                base.Document.CoordinatesToStrings(base.Units, zero.X, zero.Y, out str3, out str4, out str2);
                base.Document.CoordinatesToStrings(base.Units, zero.Width, zero.Height, out str6, out str7, out str5);
                NumberFormatInfo provider = (NumberFormatInfo) CultureInfo.CurrentCulture.NumberFormat.Clone();
                if (base.Units == MeasurementUnit.Pixel)
                {
                    provider.NumberDecimalDigits = 0;
                    str8 = num.ToString("N", provider);
                }
                else
                {
                    provider.NumberDecimalDigits = 2;
                    str8 = base.Document.PixelAreaToPhysicalArea((double) num, base.Units).ToString("N", provider);
                }
                string str9 = PdnResources.GetString2("MeasurementUnit." + base.Units.ToString() + ".Plural");
                MoveToolBase tool = this.Tool as MoveToolBase;
                if ((tool != null) && tool.HostShouldShowAngle)
                {
                    NumberFormatInfo info2 = (NumberFormatInfo) provider.Clone();
                    info2.NumberDecimalDigits = 2;
                    double hostAngle = tool.HostAngle;
                    while (hostAngle > 180.0)
                    {
                        hostAngle -= 360.0;
                    }
                    while (hostAngle < -180.0)
                    {
                        hostAngle += 360.0;
                    }
                    str = string.Format(this.contextStatusBarWithAngleFormat, new object[] { str3, str2, str4, str2, str6, str5, str7, str5, str8, str9.ToLower(), tool.HostAngle.ToString("N", info2) });
                }
                else
                {
                    str = string.Format(this.contextStatusBarFormat, new object[] { str3, str2, str4, str2, str6, str5, str7, str5, str8, str9.ToLower() });
                }
                this.SetStatus(str, PdnResources.GetImageResource2("Icons.SelectionIcon.png"));
            }
        }

        public void UpdateStatusBarToToolHelpText()
        {
            this.UpdateStatusBarToToolHelpText(this.activeTool);
        }

        public void UpdateStatusBarToToolHelpText(PaintDotNet.Tools.Tool tool)
        {
            if (tool == null)
            {
                this.SetStatus(string.Empty, null);
            }
            else
            {
                string name = tool.Name;
                string helpText = tool.HelpText;
                string newStatusText = string.Format(PdnResources.GetString2("StatusBar.Context.Help.Text.Format"), name, helpText);
                this.SetStatus(newStatusText, PdnResources.GetImageResource2("Icons.MenuHelpHelpTopicsIcon.png"));
            }
        }

        private DialogResult WarnAboutFlattening()
        {
            Icon icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuFileSaveIcon.png").Reference);
            string str = PdnResources.GetString2("WarnAboutFlattening.Title");
            string str2 = PdnResources.GetString2("WarnAboutFlattening.IntroText");
            Image image = null;
            TaskButton button = new TaskButton(PdnResources.GetImageResource2("Icons.MenuImageFlattenIcon.png").Reference, PdnResources.GetString2("WarnAboutFlattening.FlattenTB.ActionText"), PdnResources.GetString2("WarnAboutFlattening.FlattenTB.ExplanationText"));
            TaskButton button2 = new TaskButton(PdnResources.GetImageResource2("Icons.CancelIcon.png").Reference, PdnResources.GetString2("WarnAboutFlattening.CancelTB.ActionText"), PdnResources.GetString2("WarnAboutFlattening.CancelTB.ExplanationText"));
            TaskDialog dialog = new TaskDialog {
                Icon = icon,
                Title = str,
                TaskImage = image,
                IntroText = str2,
                TaskButtons = new TaskButton[] { 
                    button,
                    button2
                },
                AcceptButton = button,
                CancelButton = button2,
                PixelWidth96Dpi = (TaskDialog.DefaultPixelWidth96Dpi * 5) / 4
            };
            if (dialog.Show(this.AppWorkspace) == button)
            {
                return DialogResult.Yes;
            }
            return DialogResult.No;
        }

        public override void ZoomIn()
        {
            this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
            base.ZoomIn();
        }

        public override void ZoomIn(double factor)
        {
            this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
            base.ZoomIn(factor);
        }

        public override void ZoomOut()
        {
            this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
            base.ZoomOut();
        }

        public override void ZoomOut(double factor)
        {
            this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
            base.ZoomOut(factor);
        }

        public void ZoomToRectangle(Rectangle selectionBounds)
        {
            PointF tf = new PointF((float) (((selectionBounds.Left + selectionBounds.Right) + 1) / 2), (float) (((selectionBounds.Top + selectionBounds.Bottom) + 1) / 2));
            ScaleFactor factor = ScaleFactor.Min(base.ClientRectangleMin.Width, selectionBounds.Width + 2, base.ClientRectangleMin.Height, selectionBounds.Height + 2, ScaleFactor.MinValue);
            this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
            base.ScaleFactor = factor;
            System.Windows.Point point = new System.Windows.Point(tf.X - (base.VisibleDocumentRect.Width / 2.0), tf.Y - (base.VisibleDocumentRect.Height / 2.0));
            base.DocumentScrollPosition = point;
        }

        public void ZoomToRectangle(Int32Rect selectionBounds)
        {
            this.ZoomToRectangle(selectionBounds.ToGdipRectangle());
        }

        public void ZoomToSelection()
        {
            if (this.Selection.IsEmpty)
            {
                base.ZoomToWindow();
            }
            else
            {
                using (GeometryList list = this.Selection.CreateGeometryList())
                {
                    this.ZoomToRectangle(list.Bounds.Int32Bound());
                }
            }
        }

        public Layer ActiveLayer
        {
            get => 
                this.activeLayer;
            set
            {
                bool deactivateOnLayerChange;
                this.OnLayerChanging();
                if (this.Tool != null)
                {
                    deactivateOnLayerChange = this.Tool.DeactivateOnLayerChange;
                }
                else
                {
                    deactivateOnLayerChange = false;
                }
                if (deactivateOnLayerChange)
                {
                    this.PushNullTool();
                    this.EnableToolPulse = false;
                }
                try
                {
                    if (base.Document != null)
                    {
                        if ((value != null) && !base.Document.Layers.Contains(value))
                        {
                            throw new InvalidOperationException("ActiveLayer was changed to a layer that is not contained within the Document");
                        }
                    }
                    else if (value != null)
                    {
                        throw new InvalidOperationException("ActiveLayer was set to non-null while Document was null");
                    }
                    this.activeLayer = value;
                }
                finally
                {
                    if (deactivateOnLayerChange)
                    {
                        this.PopNullTool();
                        this.EnableToolPulse = true;
                    }
                }
                this.OnLayerChanged();
            }
        }

        public int ActiveLayerIndex
        {
            get => 
                base.Document.Layers.IndexOf(this.ActiveLayer);
            set
            {
                this.ActiveLayer = (Layer) base.Document.Layers[value];
            }
        }

        public PaintDotNet.Controls.AppWorkspace AppWorkspace
        {
            get => 
                this.appWorkspace;
            set
            {
                if (value != this.appWorkspace)
                {
                    if (this.appWorkspace != null)
                    {
                        throw new InvalidOperationException("Once a DocumentWorkspace is assigned to an AppWorkspace, it may not be reassigned");
                    }
                    this.appWorkspace = value;
                }
            }
        }

        public IDispatcher BackgroundThread =>
            this.appWorkspace.BackgroundThread;

        public IDispatcher Dispatcher =>
            this.dispatcher;

        public bool EnableSelectionOutline
        {
            get => 
                this.selectionRenderer.EnableSelectionOutline;
            set
            {
                this.selectionRenderer.EnableSelectionOutline = value;
            }
        }

        public bool EnableSelectionTinting
        {
            get => 
                this.selectionRenderer.EnableSelectionTinting;
            set
            {
                this.selectionRenderer.EnableSelectionTinting = value;
            }
        }

        public bool EnableToolPulse
        {
            get => 
                ((this.toolPulseTimer != null) && this.toolPulseTimer.Enabled);
            set
            {
                if (this.toolPulseTimer != null)
                {
                    this.toolPulseTimer.Enabled = value;
                }
            }
        }

        public string FilePath =>
            this.filePath;

        public PaintDotNet.FileType FileType =>
            this.fileType;

        public HistoryStack History =>
            this.history;

        public bool IsZoomChanging =>
            (this.zoomChangesCount > 0);

        public DateTime LastSaveTime =>
            this.lastSaveTime;

        public System.Type PreviousActiveToolType =>
            this.previousActiveToolType;

        public PaintDotNet.SaveConfigToken SaveConfigToken
        {
            get
            {
                if (this.saveConfigToken == null)
                {
                    return null;
                }
                return (PaintDotNet.SaveConfigToken) this.saveConfigToken.Clone();
            }
        }

        public PaintDotNet.Selection Selection =>
            this.selection;

        public ImageResource StatusIcon =>
            this.statusIcon;

        public string StatusText =>
            this.statusText;

        public PaintDotNet.Threading.TaskManager TaskManager =>
            this.taskManager;

        public PaintDotNet.Tools.Tool Tool =>
            this.activeTool;

        public static ToolInfo[] ToolInfos =>
            ((ToolInfo[]) toolInfos.Clone());

        public static System.Type[] Tools =>
            ((System.Type[]) tools.Clone());

        public PaintDotNet.ZoomBasis ZoomBasis
        {
            get => 
                this.zoomBasis;
            set
            {
                if (this.zoomBasis != value)
                {
                    this.OnZoomBasisChanging();
                    this.zoomBasis = value;
                    switch (this.zoomBasis)
                    {
                        case PaintDotNet.ZoomBasis.FitToWindow:
                            base.ZoomToWindow();
                            base.PanelAutoScroll = true;
                            base.PanelAutoScroll = false;
                            this.zoomBasis = PaintDotNet.ZoomBasis.FitToWindow;
                            break;

                        case PaintDotNet.ZoomBasis.ScaleFactor:
                            base.PanelAutoScroll = true;
                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }
                    this.OnZoomBasisChanged();
                }
            }
        }

    }
}

