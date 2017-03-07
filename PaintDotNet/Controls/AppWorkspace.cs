namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Collections;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal class AppWorkspace : UserControl, IDispatcherObject, ISnapObstacleHost, IGlassyControl
    {
        private DocumentWorkspace activeDocumentWorkspace;
        private bool addedToSnapManager;
        private PaintDotNet.AppEnvironment appEnvironment;
        private ThreadDispatcher backgroundThread;
        private ColorsForm colorsForm;
        private readonly string cursorInfoStatusBarFormat = PdnResources.GetString2("StatusBar.CursorInfo.Format");
        private const int defaultMostRecentFilesMax = 8;
        private System.Type defaultToolTypeChoice;
        private IDispatcher dispatcher;
        private List<DocumentWorkspace> documentWorkspaces = new List<DocumentWorkspace>();
        private HashSet<Triple<Assembly, System.Type, Exception>> effectLoadErrors = new HashSet<Triple<Assembly, System.Type, Exception>>();
        private IMessageFilter glassWndProcFilter;
        private bool globalRulersChoice;
        private System.Type globalToolTypeChoice;
        private HistoryForm historyForm;
        private int ignoreUpdateSnapObstacle;
        private readonly string imageInfoStatusBarFormat = PdnResources.GetString2("StatusBar.Size.Format");
        private DocumentWorkspace initialWorkspace;
        private LayerForm layerForm;
        private ToolsForm mainToolBarForm;
        private PaintDotNet.MostRecentFiles mostRecentFiles;
        private SnapObstacleController snapObstacle;
        private PdnStatusBar statusBar;
        private int suspendThumbnailUpdates;
        private PaintDotNet.Threading.TaskManager taskManager;
        private PdnToolBar toolBar;
        private WorkspaceWidgets widgets;
        private Panel workspacePanel;

        public event EventHandler ActiveDocumentWorkspaceChanged;

        public event EventHandler ActiveDocumentWorkspaceChanging;

        public event CmdKeysEventHandler ProcessCmdKeyEvent;

        public event EventHandler RulersEnabledChanged;

        public event EventHandler StatusChanged;

        public event EventHandler UnitsChanged;

        public AppWorkspace()
        {
            this.dispatcher = new ControlDispatcher(this);
            this.backgroundThread = new ThreadDispatcher();
            this.taskManager = new PaintDotNet.Threading.TaskManager();
            base.SuspendLayout();
            this.InitializeComponent();
            this.InitializeFloatingForms();
            this.toolBar.SuspendLayout();
            this.toolBar.ViewConfigStrip.SuspendLayout();
            this.toolBar.CommonActionsStrip.SuspendLayout();
            this.toolBar.ToolConfigStrip.SuspendLayout();
            this.toolBar.ToolChooserStrip.SuspendLayout();
            this.toolBar.DocumentStrip.SuspendLayout();
            this.mainToolBarForm.ToolsControl.SetTools(DocumentWorkspace.ToolInfos);
            this.mainToolBarForm.ToolsControl.ToolClicked += new ToolClickedEventHandler(this.MainToolBar_ToolClicked);
            this.toolBar.ToolChooserStrip.SetTools(DocumentWorkspace.ToolInfos);
            this.toolBar.ToolChooserStrip.ToolClicked += new ToolClickedEventHandler(this.MainToolBar_ToolClicked);
            this.toolBar.AppWorkspace = this;
            this.widgets = new WorkspaceWidgets(this);
            this.widgets.ViewConfigStrip = this.toolBar.ViewConfigStrip;
            this.widgets.CommonActionsStrip = this.toolBar.CommonActionsStrip;
            this.widgets.ToolConfigStrip = this.toolBar.ToolConfigStrip;
            this.widgets.ToolsForm = this.mainToolBarForm;
            this.widgets.LayerForm = this.layerForm;
            this.widgets.HistoryForm = this.historyForm;
            this.widgets.ColorsForm = this.colorsForm;
            this.widgets.StatusBarProgress = this.statusBar;
            this.widgets.DocumentStrip = this.toolBar.DocumentStrip;
            this.LoadSettings();
            this.AppEnvironment.PrimaryColorChanged += new EventHandler(this.PrimaryColorChangedHandler);
            this.AppEnvironment.SecondaryColorChanged += new EventHandler(this.SecondaryColorChangedHandler);
            this.AppEnvironment.ShapeDrawTypeChanged += new EventHandler(this.ShapeDrawTypeChangedHandler);
            this.AppEnvironment.GradientInfoChanged += new EventHandler(this.GradientInfoChangedHandler);
            this.AppEnvironment.ToleranceChanged += new EventHandler(this.OnEnvironmentToleranceChanged);
            this.AppEnvironment.AlphaBlendingChanged += new EventHandler(this.AlphaBlendingChangedHandler);
            this.AppEnvironment.FontInfo = this.toolBar.ToolConfigStrip.FontInfo;
            this.AppEnvironment.TextAlignment = this.toolBar.ToolConfigStrip.FontAlignment;
            this.AppEnvironment.AntiAliasingChanged += new EventHandler(this.Environment_AntiAliasingChanged);
            this.AppEnvironment.FontInfoChanged += new EventHandler(this.Environment_FontInfoChanged);
            this.AppEnvironment.FontSmoothingChanged += new EventHandler(this.Environment_FontSmoothingChanged);
            this.AppEnvironment.TextAlignmentChanged += new EventHandler(this.Environment_TextAlignmentChanged);
            this.AppEnvironment.PenInfoChanged += new EventHandler(this.Environment_PenInfoChanged);
            this.AppEnvironment.BrushInfoChanged += new EventHandler(this.Environment_BrushInfoChanged);
            this.AppEnvironment.ColorPickerClickBehaviorChanged += new EventHandler(this.Environment_ColorPickerClickBehaviorChanged);
            this.AppEnvironment.ResamplingAlgorithmChanged += new EventHandler(this.Environment_ResamplingAlgorithmChanged);
            this.AppEnvironment.SelectionCombineModeChanged += new EventHandler(this.Environment_SelectionCombineModeChanged);
            this.AppEnvironment.FloodModeChanged += new EventHandler(this.Environment_FloodModeChanged);
            this.AppEnvironment.SelectionDrawModeInfoChanged += new EventHandler(this.Environment_SelectionDrawModeInfoChanged);
            this.toolBar.DocumentStrip.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.toolBar.ToolConfigStrip.ToleranceChanged += new EventHandler(this.OnToolBarToleranceChanged);
            this.toolBar.ToolConfigStrip.FontAlignmentChanged += new EventHandler(this.ToolConfigStrip_TextAlignmentChanged);
            this.toolBar.ToolConfigStrip.FontInfoChanged += new EventHandler(this.ToolConfigStrip_FontTextChanged);
            this.toolBar.ToolConfigStrip.FontSmoothingChanged += new EventHandler(this.ToolConfigStrip_FontSmoothingChanged);
            this.toolBar.ToolConfigStrip.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler2);
            this.toolBar.CommonActionsStrip.RelinquishFocus += new EventHandler(this.OnToolStripRelinquishFocus);
            this.toolBar.CommonActionsStrip.MouseWheel += new MouseEventHandler(this.OnToolStripMouseWheel);
            this.toolBar.CommonActionsStrip.ButtonClick += new EventHandler<EventArgs<CommonAction>>(this.CommonActionsStrip_ButtonClick);
            this.toolBar.ViewConfigStrip.DrawGridChanged += new EventHandler(this.ViewConfigStrip_DrawGridChanged);
            this.toolBar.ViewConfigStrip.RulersEnabledChanged += new EventHandler(this.ViewConfigStrip_RulersEnabledChanged);
            this.toolBar.ViewConfigStrip.ZoomBasisChanged += new EventHandler(this.ViewConfigStrip_ZoomBasisChanged);
            this.toolBar.ViewConfigStrip.ZoomScaleChanged += new EventHandler(this.ViewConfigStrip_ZoomScaleChanged);
            this.toolBar.ViewConfigStrip.ZoomIn += new EventHandler(this.ViewConfigStrip_ZoomIn);
            this.toolBar.ViewConfigStrip.ZoomOut += new EventHandler(this.ViewConfigStrip_ZoomOut);
            this.toolBar.ViewConfigStrip.UnitsChanged += new EventHandler(this.ViewConfigStrip_UnitsChanged);
            this.toolBar.ViewConfigStrip.RelinquishFocus += new EventHandler(this.OnToolStripRelinquishFocus);
            this.toolBar.ViewConfigStrip.MouseWheel += new MouseEventHandler(this.OnToolStripMouseWheel);
            this.toolBar.ToolConfigStrip.BrushInfoChanged += new EventHandler(this.DrawConfigStrip_BrushChanged);
            this.toolBar.ToolConfigStrip.ShapeDrawTypeChanged += new EventHandler(this.DrawConfigStrip_ShapeDrawTypeChanged);
            this.toolBar.ToolConfigStrip.PenInfoChanged += new EventHandler(this.DrawConfigStrip_PenChanged);
            this.toolBar.ToolConfigStrip.GradientInfoChanged += new EventHandler(this.ToolConfigStrip_GradientInfoChanged);
            this.toolBar.ToolConfigStrip.AlphaBlendingChanged += new EventHandler(this.OnDrawConfigStripAlphaBlendingChanged);
            this.toolBar.ToolConfigStrip.AntiAliasingChanged += new EventHandler(this.DrawConfigStrip_AntiAliasingChanged);
            this.toolBar.ToolConfigStrip.RelinquishFocus += new EventHandler(this.OnToolStripRelinquishFocus);
            this.toolBar.ToolConfigStrip.ColorPickerClickBehaviorChanged += new EventHandler(this.ToolConfigStrip_ColorPickerClickBehaviorChanged);
            this.toolBar.ToolConfigStrip.ResamplingAlgorithmChanged += new EventHandler(this.ToolConfigStrip_ResamplingAlgorithmChanged);
            this.toolBar.ToolConfigStrip.SelectionCombineModeChanged += new EventHandler(this.ToolConfigStrip_SelectionCombineModeChanged);
            this.toolBar.ToolConfigStrip.FloodModeChanged += new EventHandler(this.ToolConfigStrip_FloodModeChanged);
            this.toolBar.ToolConfigStrip.SelectionDrawModeInfoChanged += new EventHandler(this.ToolConfigStrip_SelectionDrawModeInfoChanged);
            this.toolBar.ToolConfigStrip.SelectionDrawModeUnitsChanging += new EventHandler(this.ToolConfigStrip_SelectionDrawModeUnitsChanging);
            this.toolBar.ToolConfigStrip.MouseWheel += new MouseEventHandler(this.OnToolStripMouseWheel);
            this.toolBar.DocumentStrip.RelinquishFocus += new EventHandler(this.OnToolStripRelinquishFocus);
            this.toolBar.DocumentStrip.DocumentClicked += new EventHandler<EventArgs<Pair<DocumentWorkspace, DocumentClickAction>>>(this.DocumentStrip_DocumentTabClicked);
            this.toolBar.DocumentStrip.DocumentListChanged += new EventHandler(this.DocumentStrip_DocumentListChanged);
            this.AppEnvironment.PerformAllChanged();
            this.globalToolTypeChoice = this.defaultToolTypeChoice;
            this.toolBar.ToolConfigStrip.ToolBarConfigItems = ToolBarConfigItems.None;
            this.layerForm.LayerControl.AppWorkspace = this;
            this.statusBar.Renderer = new PdnToolStripRenderer();
            this.toolBar.ViewConfigStrip.ResumeLayout(false);
            this.toolBar.CommonActionsStrip.ResumeLayout(false);
            this.toolBar.ToolConfigStrip.ResumeLayout(false);
            this.toolBar.ToolChooserStrip.ResumeLayout(false);
            this.toolBar.DocumentStrip.ResumeLayout(false);
            this.toolBar.ResumeLayout(false);
            base.ResumeLayout();
            base.PerformLayout();
        }

        private void ActiveDocumentWorkspace_FirstInputAfterGotFocus(object sender, EventArgs e)
        {
            this.toolBar.DocumentStrip.EnsureItemFullyVisible(this.toolBar.DocumentStrip.SelectedDocumentIndex);
        }

        public DocumentWorkspace AddNewDocumentWorkspace()
        {
            DocumentWorkspace workspace2;
            bool flag = false;
            try
            {
                if (this.initialWorkspace != null)
                {
                    bool flag2;
                    if (this.initialWorkspace.Document == null)
                    {
                        flag2 = true;
                    }
                    else if ((!this.initialWorkspace.Document.Dirty && (this.initialWorkspace.History.UndoStack.Count == 1)) && ((this.initialWorkspace.History.RedoStack.Count == 0) && (this.initialWorkspace.History.UndoStack[0] is NullHistoryMemento)))
                    {
                        flag2 = true;
                    }
                    else if ((!this.initialWorkspace.Document.Dirty && (this.initialWorkspace.History.UndoStack.Count == 0)) && (this.initialWorkspace.History.RedoStack.Count == 0))
                    {
                        flag2 = true;
                    }
                    else
                    {
                        flag2 = false;
                    }
                    if (flag2)
                    {
                        this.globalToolTypeChoice = this.initialWorkspace.GetToolType();
                        flag = true;
                        UI.SuspendControlPainting(this);
                        this.RemoveDocumentWorkspace(this.initialWorkspace);
                        this.initialWorkspace = null;
                    }
                }
                DocumentWorkspace item = new DocumentWorkspace {
                    AppWorkspace = this
                };
                item.PushCacheStandby();
                this.documentWorkspaces.Add(item);
                this.toolBar.DocumentStrip.AddDocumentWorkspace(item);
                workspace2 = item;
            }
            finally
            {
                if (flag)
                {
                    UI.ResumeControlPainting(this);
                    base.Invalidate(true);
                }
            }
            return workspace2;
        }

        private void AlphaBlendingChangedHandler(object sender, EventArgs e)
        {
            if (this.widgets.ToolConfigStrip.AlphaBlending != this.AppEnvironment.AlphaBlending)
            {
                this.widgets.ToolConfigStrip.AlphaBlending = this.AppEnvironment.AlphaBlending;
            }
        }

        private void AppWorkspace_Shown(object sender, EventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        public void CheckForUpdates()
        {
            this.toolBar.MainMenu.CheckForUpdates();
        }

        private void ColorDisplay_UserPrimaryAndSecondaryColorsChanged(object sender, EventArgs e)
        {
            if (this.widgets.ColorsForm.WhichUserColor == WhichUserColor.Primary)
            {
                this.widgets.ColorsForm.SetColorControlsRedraw(false);
                this.SecondaryColorChangedHandler(sender, e);
                this.PrimaryColorChangedHandler(sender, e);
                this.widgets.ColorsForm.SetColorControlsRedraw(true);
                this.widgets.ColorsForm.WhichUserColor = WhichUserColor.Primary;
            }
            else
            {
                this.widgets.ColorsForm.SetColorControlsRedraw(false);
                this.PrimaryColorChangedHandler(sender, e);
                this.SecondaryColorChangedHandler(sender, e);
                this.widgets.ColorsForm.SetColorControlsRedraw(true);
                this.widgets.ColorsForm.WhichUserColor = WhichUserColor.Secondary;
            }
        }

        private void ColorsForm_UserPrimaryColorChanged(object sender, ColorEventArgs e)
        {
            ColorsForm form1 = (ColorsForm) sender;
            this.AppEnvironment.PrimaryColor = e.Color;
        }

        private void ColorsForm_UserSecondaryColorChanged(object sender, ColorEventArgs e)
        {
            ColorsForm form1 = (ColorsForm) sender;
            this.AppEnvironment.SecondaryColor = e.Color;
        }

        private void CommonActionsStrip_ButtonClick(object sender, EventArgs<CommonAction> e)
        {
            switch (e.Data)
            {
                case CommonAction.New:
                    this.PerformAction(new NewImageAction());
                    break;

                case CommonAction.Open:
                    this.PerformAction(new OpenFileAction());
                    break;

                case CommonAction.Save:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.DoSave();
                    }
                    break;

                case CommonAction.Print:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        PrintAction action = new PrintAction();
                        this.ActiveDocumentWorkspace.PerformAction(action);
                    }
                    break;

                case CommonAction.Cut:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        new CutAction().PerformAction(this.ActiveDocumentWorkspace);
                    }
                    break;

                case CommonAction.Copy:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        new CopyToClipboardAction(this.ActiveDocumentWorkspace).PerformAction();
                    }
                    break;

                case CommonAction.Paste:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        new PasteAction(this.ActiveDocumentWorkspace).PerformAction();
                    }
                    break;

                case CommonAction.CropToSelection:
                {
                    if (this.ActiveDocumentWorkspace == null)
                    {
                        break;
                    }
                    using (new PushNullToolMode(this.ActiveDocumentWorkspace))
                    {
                        this.ActiveDocumentWorkspace.ExecuteFunction(new CropToSelectionFunction());
                        break;
                    }
                }
                case CommonAction.Deselect:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.ExecuteFunction(new DeselectFunction());
                    }
                    break;

                case CommonAction.Undo:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.PerformAction(new HistoryUndoAction());
                    }
                    break;

                case CommonAction.Redo:
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.PerformAction(new HistoryRedoAction());
                    }
                    break;

                default:
                    throw new InvalidEnumArgumentException("e.Data");
            }
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Focus();
            }
        }

        private void CoordinatesToStrings(int x, int y, out string xString, out string yString, out string unitsString)
        {
            this.activeDocumentWorkspace.Document.CoordinatesToStrings(this.Units, x, y, out xString, out yString, out unitsString);
        }

        public bool CreateBlankDocumentInNewWorkspace(Int32Size size, MeasurementUnit dpuUnit, double dpu, bool isInitial)
        {
            DocumentWorkspace activeDocumentWorkspace = this.activeDocumentWorkspace;
            if (activeDocumentWorkspace != null)
            {
                activeDocumentWorkspace.SuspendRefresh();
            }
            try
            {
                BitmapLayer layer;
                Document document = new Document(size.Width, size.Height) {
                    DpuUnit = dpuUnit,
                    DpuX = dpu,
                    DpuY = dpu
                };
                try
                {
                    using (new WaitCursorChanger(this))
                    {
                        layer = Layer.CreateBackgroundLayer(size.Width, size.Height);
                    }
                }
                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(this, PdnResources.GetString2("NewImageAction.Error.OutOfMemory"));
                    return false;
                }
                using (new WaitCursorChanger(this))
                {
                    bool flag = false;
                    if ((this.ActiveDocumentWorkspace != null) && this.ActiveDocumentWorkspace.Focused)
                    {
                        flag = true;
                    }
                    document.Layers.Add(layer);
                    DocumentWorkspace lockMe = this.AddNewDocumentWorkspace();
                    this.Widgets.DocumentStrip.LockDocumentWorkspaceDirtyValue(lockMe, false);
                    lockMe.SuspendRefresh();
                    try
                    {
                        lockMe.Document = document;
                    }
                    catch (OutOfMemoryException)
                    {
                        Utility.ErrorBox(this, PdnResources.GetString2("NewImageAction.Error.OutOfMemory"));
                        this.RemoveDocumentWorkspace(lockMe);
                        document.Dispose();
                        return false;
                    }
                    lockMe.ActiveLayer = (Layer) lockMe.Document.Layers[0];
                    this.ActiveDocumentWorkspace = lockMe;
                    lockMe.SetDocumentSaveOptions(null, null, null);
                    lockMe.History.ClearAll();
                    lockMe.History.PushNewMemento(new NullHistoryMemento(PdnResources.GetString2("NewImageAction.Name"), this.FileNewIcon));
                    lockMe.Document.Dirty = false;
                    lockMe.ResumeRefresh();
                    if (isInitial)
                    {
                        this.initialWorkspace = lockMe;
                    }
                    if (flag)
                    {
                        this.ActiveDocumentWorkspace.Focus();
                    }
                    this.Widgets.DocumentStrip.UnlockDocumentWorkspaceDirtyValue(lockMe);
                }
            }
            finally
            {
                if (activeDocumentWorkspace != null)
                {
                    activeDocumentWorkspace.ResumeRefresh();
                }
            }
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.taskManager != null)
                {
                    this.taskManager.Dispose();
                    this.taskManager = null;
                }
                if (this.backgroundThread != null)
                {
                    this.backgroundThread.Dispose();
                    this.backgroundThread = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DocumenKeyUp(object sender, KeyEventArgs e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.ActiveDocumentWorkspace.Tool.PerformKeyUp(e);
            }
        }

        private void DocumentClick(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.ActiveDocumentWorkspace.Tool.PerformClick();
            }
        }

        private void DocumentKeyDown(object sender, KeyEventArgs e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.ActiveDocumentWorkspace.Tool.PerformKeyDown(e);
            }
        }

        private void DocumentKeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.ActiveDocumentWorkspace.Tool.PerformKeyPress(e);
            }
        }

        private void DocumentMouseDownHandler(object sender, MouseEventArgsF e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseDown(e);
            }
        }

        private void DocumentMouseEnterHandler(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseEnter();
            }
        }

        private void DocumentMouseLeaveHandler(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseLeave();
            }
        }

        private void DocumentMouseMoveHandler(object sender, MouseEventArgsF e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseMove(e);
            }
            this.UpdateCursorInfoInStatusBar(e.X, e.Y);
        }

        private void DocumentMouseUpHandler(object sender, MouseEventArgsF e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.ActiveDocumentWorkspace.Tool.PerformMouseUp(e);
            }
        }

        private void DocumentStrip_DocumentListChanged(object sender, EventArgs e)
        {
            bool enabled = this.widgets.DocumentStrip.DocumentCount != 0;
            this.widgets.ToolsForm.Enabled = enabled;
            this.widgets.HistoryForm.Enabled = enabled;
            this.widgets.LayerForm.Enabled = enabled;
            this.widgets.ColorsForm.Enabled = enabled;
            this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Paste, enabled);
            this.UpdateHistoryButtons();
            this.UpdateDocInfoInStatusBar();
            this.UpdateCursorInfoInStatusBar(0, 0);
        }

        private void DocumentStrip_DocumentTabClicked(object sender, EventArgs<Pair<DocumentWorkspace, DocumentClickAction>> e)
        {
            switch (e.Data.Second)
            {
                case DocumentClickAction.Select:
                    this.ActiveDocumentWorkspace = e.Data.First;
                    break;

                case DocumentClickAction.Close:
                {
                    CloseWorkspaceAction performMe = new CloseWorkspaceAction(e.Data.First);
                    this.PerformAction(performMe);
                    break;
                }
                default:
                    throw new NotImplementedException("Code for DocumentClickAction." + e.Data.Second.ToString() + " not implemented");
            }
            base.Update();
        }

        private void DocumentWorkspace_DocumentChanged(object sender, EventArgs e)
        {
            this.UpdateDocInfoInStatusBar();
            UI.ResumeControlPainting(this);
            base.Invalidate(true);
        }

        private void DocumentWorkspace_DocumentChanging(object sender, EventArgs<Document> e)
        {
            UI.SuspendControlPainting(this);
        }

        private void DocumentWorkspace_DrawGridChanged(object sender, EventArgs e)
        {
            this.DrawGrid = this.activeDocumentWorkspace.DrawGrid;
        }

        private void DocumentWorkspace_Layout(object sender, LayoutEventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void DocumentWorkspace_RulersEnabledChanged(object sender, EventArgs e)
        {
            this.toolBar.ViewConfigStrip.RulersEnabled = this.activeDocumentWorkspace.RulersEnabled;
            this.globalRulersChoice = this.activeDocumentWorkspace.RulersEnabled;
            base.PerformLayout();
            this.ActiveDocumentWorkspace.UpdateRulerSelectionTinting();
            Settings.CurrentUser.SetBoolean("Rulers", this.activeDocumentWorkspace.RulersEnabled);
        }

        private void DocumentWorkspace_Scroll(object sender, ScrollEventArgs e)
        {
            this.OnScroll(e);
        }

        private void DocumentWorkspace_ZoomBasisChanged(object sender, EventArgs e)
        {
            if (this.toolBar.ViewConfigStrip.ZoomBasis != this.ActiveDocumentWorkspace.ZoomBasis)
            {
                this.toolBar.ViewConfigStrip.ZoomBasis = this.ActiveDocumentWorkspace.ZoomBasis;
            }
        }

        private void DrawConfigStrip_AntiAliasingChanged(object sender, EventArgs e)
        {
            this.AppEnvironment.AntiAliasing = ((ToolConfigStrip) sender).AntiAliasing;
        }

        private void DrawConfigStrip_BrushChanged(object sender, EventArgs e)
        {
            this.AppEnvironment.BrushInfo = this.toolBar.ToolConfigStrip.BrushInfo;
        }

        private void DrawConfigStrip_PenChanged(object sender, EventArgs e)
        {
            this.AppEnvironment.PenInfo = this.toolBar.ToolConfigStrip.PenInfo;
        }

        private void DrawConfigStrip_ShapeDrawTypeChanged(object sender, EventArgs e)
        {
            if (this.AppEnvironment.ShapeDrawType != this.widgets.ToolConfigStrip.ShapeDrawType)
            {
                this.AppEnvironment.ShapeDrawType = this.widgets.ToolConfigStrip.ShapeDrawType;
            }
        }

        private void Environment_AntiAliasingChanged(object sender, EventArgs e)
        {
            this.toolBar.ToolConfigStrip.AntiAliasing = this.AppEnvironment.AntiAliasing;
        }

        private void Environment_BrushInfoChanged(object sender, EventArgs e)
        {
        }

        private void Environment_ColorPickerClickBehaviorChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.ColorPickerClickBehavior = this.appEnvironment.ColorPickerClickBehavior;
        }

        private void Environment_FloodModeChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.FloodMode = this.appEnvironment.FloodMode;
        }

        private void Environment_FontInfoChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.FontInfo = this.AppEnvironment.FontInfo;
        }

        private void Environment_FontSmoothingChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.FontSmoothing = this.AppEnvironment.FontSmoothing;
        }

        private void Environment_PenInfoChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.PenInfo = this.AppEnvironment.PenInfo;
        }

        private void Environment_ResamplingAlgorithmChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.ResamplingAlgorithm = this.appEnvironment.ResamplingAlgorithm;
        }

        private void Environment_SelectionCombineModeChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.SelectionCombineMode = this.appEnvironment.SelectionCombineMode;
        }

        private void Environment_SelectionDrawModeInfoChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.SelectionDrawModeInfo = this.appEnvironment.SelectionDrawModeInfo;
        }

        private void Environment_TextAlignmentChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.FontAlignment = this.AppEnvironment.TextAlignment;
        }

        public IList<Triple<Assembly, System.Type, Exception>> GetEffectLoadErrors() => 
            this.effectLoadErrors.ToArrayEx<Triple<Assembly, System.Type, Exception>>();

        public static string GetLocalizedEffectErrorMessage(Assembly assembly, string typeName, Exception exception)
        {
            IPluginSupportInfo pluginSupportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
            return GetLocalizedEffectErrorMessage(assembly, typeName, pluginSupportInfo, exception);
        }

        public static string GetLocalizedEffectErrorMessage(Assembly assembly, System.Type type, Exception exception)
        {
            IPluginSupportInfo pluginSupportInfo;
            string fullName;
            if (type != null)
            {
                fullName = type.FullName;
                pluginSupportInfo = PluginSupportInfo.GetPluginSupportInfo(type);
            }
            else if (exception is TypeLoadException)
            {
                TypeLoadException exception2 = exception as TypeLoadException;
                fullName = exception2.TypeName;
                pluginSupportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
            }
            else
            {
                pluginSupportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
                fullName = null;
            }
            return GetLocalizedEffectErrorMessage(assembly, fullName, pluginSupportInfo, exception);
        }

        private static string GetLocalizedEffectErrorMessage(Assembly assembly, string typeName, IPluginSupportInfo supportInfo, Exception exception)
        {
            string pluginBlockReasonString;
            string location = assembly.Location;
            string format = PdnResources.GetString2("EffectErrorMessage.ShortFormat");
            string str3 = PdnResources.GetString2("EffectErrorMessage.FullFormat");
            string str4 = PdnResources.GetString2("EffectErrorMessage.InfoNotSupplied");
            if (exception is BlockedPluginException)
            {
                pluginBlockReasonString = GetPluginBlockReasonString(((BlockedPluginException) exception).Reason);
            }
            else
            {
                pluginBlockReasonString = exception.ToString();
            }
            if (supportInfo == null)
            {
                return string.Format(format, location ?? str4, typeName ?? str4, pluginBlockReasonString);
            }
            return string.Format(str3, new object[] { location ?? str4, typeName ?? (supportInfo.DisplayName ?? str4), (supportInfo.Version ?? new Version()).ToString(), supportInfo.Author ?? str4, supportInfo.Copyright ?? str4, (supportInfo.WebsiteUri == null) ? str4 : supportInfo.WebsiteUri.ToString(), pluginBlockReasonString });
        }

        public Int32Size GetNewDocumentSize()
        {
            PdnBaseForm form = base.FindForm() as PdnBaseForm;
            if ((form != null) && (form.ScreenAspect < 1.0))
            {
                return new Int32Size(600, 800);
            }
            return new Int32Size(800, 600);
        }

        private static string GetPluginBlockReasonString(PluginBlockReason reason)
        {
            new StringBuilder();
            IEnumerable<PluginBlockReason> enumerable2 = from v in Enum.GetValues(typeof(PluginBlockReason)).Cast<PluginBlockReason>()
                where (reason & v) != PluginBlockReason.NotBlocked
                select v;
            EnumLocalizer localizer = EnumLocalizer.Create(typeof(PluginBlockReason));
            return (from r in enumerable2 select localizer.GetLocalizedEnumValue(r).LocalizedName).Aggregate<string, string>(string.Empty, (sa, s) => (((sa.Length > 0) ? Environment.NewLine : string.Empty) + s));
        }

        private void GradientInfoChangedHandler(object sender, EventArgs e)
        {
            if (this.widgets.ToolConfigStrip.GradientInfo != this.AppEnvironment.GradientInfo)
            {
                this.widgets.ToolConfigStrip.GradientInfo = this.AppEnvironment.GradientInfo;
            }
        }

        private void HistoryChangedHandler(object sender, EventArgs e)
        {
            this.UpdateHistoryButtons();
            this.UpdateDocInfoInStatusBar();
        }

        private void HistoryForm_FastForwardButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new HistoryFastForwardAction());
            }
        }

        private void HistoryForm_RedoButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new HistoryRedoAction());
            }
        }

        private void HistoryForm_RewindButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new HistoryRewindAction());
            }
        }

        private void HistoryForm_UndoButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new HistoryUndoAction());
            }
        }

        private void InitializeComponent()
        {
            this.toolBar = new PdnToolBar();
            this.toolBar.SuspendLayout();
            this.statusBar = new PdnStatusBar();
            this.statusBar.SuspendLayout();
            this.workspacePanel = new Panel();
            this.workspacePanel.SuspendLayout();
            base.SuspendLayout();
            this.toolBar.Name = "toolBar";
            this.toolBar.Dock = DockStyle.Top;
            this.statusBar.Name = "statusBar";
            this.workspacePanel.Name = "workspacePanel";
            this.workspacePanel.Dock = DockStyle.Fill;
            base.Controls.Add(this.workspacePanel);
            base.Controls.Add(this.statusBar);
            base.Controls.Add(this.toolBar);
            base.Name = "AppWorkspace";
            base.Size = new System.Drawing.Size(0x368, 640);
            base.ResumeLayout(false);
            this.workspacePanel.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.toolBar.ResumeLayout(false);
        }

        private void InitializeFloatingForms()
        {
            this.mainToolBarForm = new ToolsForm();
            this.mainToolBarForm.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.mainToolBarForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(this.OnToolFormProcessCmdKeyEvent);
            this.layerForm = new LayerForm();
            this.layerForm.LayerControl.AppWorkspace = this;
            this.layerForm.LayerControl.ClickedOnLayer += new EventHandler<EventArgs<Layer>>(this.LayerControl_ClickedOnLayer);
            this.layerForm.NewLayerButtonClick += new EventHandler(this.LayerForm_NewLayerButtonClicked);
            this.layerForm.DeleteLayerButtonClick += new EventHandler(this.LayerForm_DeleteLayerButtonClicked);
            this.layerForm.DuplicateLayerButtonClick += new EventHandler(this.LayerForm_DuplicateLayerButtonClick);
            this.layerForm.MergeLayerDownClick += new EventHandler(this.LayerForm_MergeLayerDownClick);
            this.layerForm.MoveLayerUpButtonClick += new EventHandler(this.LayerForm_MoveLayerUpButtonClicked);
            this.layerForm.MoveLayerDownButtonClick += new EventHandler(this.LayerForm_MoveLayerDownButtonClicked);
            this.layerForm.PropertiesButtonClick += new EventHandler(this.LayerForm_PropertiesButtonClick);
            this.layerForm.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.layerForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(this.OnToolFormProcessCmdKeyEvent);
            this.historyForm = new HistoryForm();
            this.historyForm.RewindButtonClicked += new EventHandler(this.HistoryForm_RewindButtonClicked);
            this.historyForm.UndoButtonClicked += new EventHandler(this.HistoryForm_UndoButtonClicked);
            this.historyForm.RedoButtonClicked += new EventHandler(this.HistoryForm_RedoButtonClicked);
            this.historyForm.FastForwardButtonClicked += new EventHandler(this.HistoryForm_FastForwardButtonClicked);
            this.historyForm.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.historyForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(this.OnToolFormProcessCmdKeyEvent);
            this.colorsForm = new ColorsForm();
            this.colorsForm.PaletteCollection = new PaletteCollection();
            this.colorsForm.WhichUserColor = WhichUserColor.Primary;
            this.colorsForm.UserPrimaryColorChanged += new ColorEventHandler(this.ColorsForm_UserPrimaryColorChanged);
            this.colorsForm.UserSecondaryColorChanged += new ColorEventHandler(this.ColorsForm_UserSecondaryColorChanged);
            this.colorsForm.RelinquishFocus += new EventHandler(this.RelinquishFocusHandler);
            this.colorsForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(this.OnToolFormProcessCmdKeyEvent);
        }

        private void LayerControl_ClickedOnLayer(object sender, EventArgs<Layer> ce)
        {
            if ((this.ActiveDocumentWorkspace != null) && (ce.Data != this.ActiveDocumentWorkspace.ActiveLayer))
            {
                this.ActiveDocumentWorkspace.ActiveLayer = ce.Data;
            }
            this.RelinquishFocusHandler(sender, EventArgs.Empty);
        }

        private void LayerForm_DeleteLayerButtonClicked(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document.Layers.Count > 1))
            {
                this.ActiveDocumentWorkspace.ExecuteFunction(new DeleteLayerFunction(this.ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void LayerForm_DuplicateLayerButtonClick(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ExecuteFunction(new DuplicateLayerFunction(this.ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void LayerForm_MergeLayerDownClick(object sender, EventArgs e)
        {
            if (((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace != null)) && (this.ActiveDocumentWorkspace.ActiveLayerIndex > 0))
            {
                int num = (this.ActiveDocumentWorkspace.ActiveLayerIndex - 1).Clamp(0, this.ActiveDocumentWorkspace.Document.Layers.Count - 1);
                this.ActiveDocumentWorkspace.ExecuteFunction(new MergeLayerDownFunction(this.ActiveDocumentWorkspace.ActiveLayerIndex));
                this.ActiveDocumentWorkspace.ActiveLayerIndex = num;
            }
        }

        private void LayerForm_MoveLayerDownButtonClicked(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document.Layers.Count >= 2))
            {
                this.ActiveDocumentWorkspace.PerformAction(new MoveActiveLayerDownAction());
            }
        }

        private void LayerForm_MoveLayerUpButtonClicked(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document.Layers.Count >= 2))
            {
                this.ActiveDocumentWorkspace.PerformAction(new MoveActiveLayerUpAction());
            }
        }

        private void LayerForm_NewLayerButtonClicked(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ExecuteFunction(new AddNewBlankLayerFunction());
            }
        }

        private void LayerForm_PropertiesButtonClick(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.PerformAction(new OpenActiveLayerPropertiesAction());
            }
        }

        private void LoadDefaultToolType()
        {
            string defaultToolTypeName = Settings.CurrentUser.GetString("DefaultToolTypeName", PaintDotNet.Tools.Tool.DefaultToolType.Name);
            ToolInfo info = Array.Find<ToolInfo>(DocumentWorkspace.ToolInfos, check => string.Compare(defaultToolTypeName, check.ToolType.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (info == null)
            {
                this.defaultToolTypeChoice = PaintDotNet.Tools.Tool.DefaultToolType;
            }
            else
            {
                this.defaultToolTypeChoice = info.ToolType;
            }
        }

        public void LoadSettings()
        {
            try
            {
                this.LoadDefaultToolType();
                this.globalToolTypeChoice = this.defaultToolTypeChoice;
                this.globalRulersChoice = Settings.CurrentUser.GetBoolean("Rulers", false);
                this.DrawGrid = Settings.CurrentUser.GetBoolean("DrawGrid", false);
                this.appEnvironment = PaintDotNet.AppEnvironment.GetDefaultAppEnvironment();
                this.widgets.ViewConfigStrip.Units = (MeasurementUnit) Enum.Parse(typeof(MeasurementUnit), Settings.CurrentUser.GetString("Units", MeasurementUnit.Pixel.ToString()), true);
            }
            catch (Exception)
            {
                this.appEnvironment = new PaintDotNet.AppEnvironment();
                this.appEnvironment.SetToDefaults();
                try
                {
                    Settings.CurrentUser.Delete(new string[] { "Rulers", "DrawGrid", "Units", "DefaultAppEnvironment", "DefaultToolTypeName" });
                }
                catch (Exception)
                {
                }
            }
            try
            {
                this.toolBar.ToolConfigStrip.LoadFromAppEnvironment(this.appEnvironment);
            }
            catch (Exception)
            {
                this.appEnvironment = new PaintDotNet.AppEnvironment();
                this.appEnvironment.SetToDefaults();
                this.toolBar.ToolConfigStrip.LoadFromAppEnvironment(this.appEnvironment);
            }
        }

        private void MainToolBar_ToolClicked(object sender, ToolClickedEventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Focus();
                this.ActiveDocumentWorkspace.SetToolFromType(e.ToolType);
            }
        }

        private static bool NullGetThumbnailImageAbort() => 
            false;

        protected virtual void OnActiveDocumentWorkspaceChanged()
        {
            this.SuspendUpdateSnapObstacle();
            if (this.activeDocumentWorkspace == null)
            {
                this.toolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Print, false);
                this.toolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Save, false);
                this.UpdateSelectionToolbarButtons();
            }
            else
            {
                this.activeDocumentWorkspace.PopCacheStandby();
                this.activeDocumentWorkspace.SuspendLayout();
                this.toolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Print, true);
                this.toolBar.CommonActionsStrip.SetButtonEnabled(CommonAction.Save, true);
                this.activeDocumentWorkspace.BackColor = SystemColors.ControlDark;
                this.activeDocumentWorkspace.Dock = DockStyle.Fill;
                this.activeDocumentWorkspace.DrawGrid = this.DrawGrid;
                this.activeDocumentWorkspace.PanelAutoScroll = true;
                this.activeDocumentWorkspace.RulersEnabled = this.globalRulersChoice;
                this.activeDocumentWorkspace.TabIndex = 0;
                this.activeDocumentWorkspace.TabStop = false;
                this.activeDocumentWorkspace.RulersEnabledChanged += new EventHandler(this.DocumentWorkspace_RulersEnabledChanged);
                this.activeDocumentWorkspace.DocumentMouseEnter += new EventHandler(this.DocumentMouseEnterHandler);
                this.activeDocumentWorkspace.DocumentMouseLeave += new EventHandler(this.DocumentMouseLeaveHandler);
                this.activeDocumentWorkspace.DocumentMouseMove += new EventHandler<MouseEventArgsF>(this.DocumentMouseMoveHandler);
                this.activeDocumentWorkspace.DocumentMouseDown += new EventHandler<MouseEventArgsF>(this.DocumentMouseDownHandler);
                this.activeDocumentWorkspace.Scroll += new ScrollEventHandler(this.DocumentWorkspace_Scroll);
                this.activeDocumentWorkspace.DrawGridChanged += new EventHandler(this.DocumentWorkspace_DrawGridChanged);
                this.activeDocumentWorkspace.DocumentClick += new EventHandler(this.DocumentClick);
                this.activeDocumentWorkspace.DocumentMouseUp += new EventHandler<MouseEventArgsF>(this.DocumentMouseUpHandler);
                this.activeDocumentWorkspace.DocumentKeyPress += new KeyPressEventHandler(this.DocumentKeyPress);
                this.activeDocumentWorkspace.DocumentKeyUp += new KeyEventHandler(this.DocumenKeyUp);
                this.activeDocumentWorkspace.DocumentKeyDown += new KeyEventHandler(this.DocumentKeyDown);
                if (this.workspacePanel.Controls.Contains(this.activeDocumentWorkspace))
                {
                    this.activeDocumentWorkspace.Visible = true;
                }
                else
                {
                    this.activeDocumentWorkspace.Dock = DockStyle.Fill;
                    this.workspacePanel.Controls.Add(this.activeDocumentWorkspace);
                }
                this.activeDocumentWorkspace.Layout += new LayoutEventHandler(this.DocumentWorkspace_Layout);
                this.toolBar.ViewConfigStrip.ScaleFactor = this.activeDocumentWorkspace.ScaleFactor;
                this.toolBar.ViewConfigStrip.ZoomBasis = this.activeDocumentWorkspace.ZoomBasis;
                this.activeDocumentWorkspace.AppWorkspace = this;
                this.activeDocumentWorkspace.History.Changed += new EventHandler(this.HistoryChangedHandler);
                this.activeDocumentWorkspace.StatusChanged += new EventHandler(this.OnDocumentWorkspaceStatusChanged);
                this.activeDocumentWorkspace.DocumentChanging += new EventHandler<EventArgs<Document>>(this.DocumentWorkspace_DocumentChanging);
                this.activeDocumentWorkspace.DocumentChanged += new EventHandler(this.DocumentWorkspace_DocumentChanged);
                this.activeDocumentWorkspace.Selection.Changing += new EventHandler(this.SelectedPathChangingHandler);
                this.activeDocumentWorkspace.Selection.Changed += new EventHandler(this.SelectedPathChangedHandler);
                this.activeDocumentWorkspace.ScaleFactorChanged += new EventHandler(this.ZoomChangedHandler);
                this.activeDocumentWorkspace.ZoomBasisChanged += new EventHandler(this.DocumentWorkspace_ZoomBasisChanged);
                this.activeDocumentWorkspace.Units = this.widgets.ViewConfigStrip.Units;
                this.historyForm.HistoryControl.HistoryStack = this.ActiveDocumentWorkspace.History;
                this.activeDocumentWorkspace.ToolChanging += new EventHandler(this.ToolChangingHandler);
                this.activeDocumentWorkspace.ToolChanged += new EventHandler(this.ToolChangedHandler);
                this.toolBar.ViewConfigStrip.RulersEnabled = this.activeDocumentWorkspace.RulersEnabled;
                this.toolBar.DocumentStrip.SelectDocumentWorkspace(this.activeDocumentWorkspace);
                this.activeDocumentWorkspace.SetToolFromType(this.globalToolTypeChoice);
                this.UpdateSelectionToolbarButtons();
                this.UpdateHistoryButtons();
                this.UpdateDocInfoInStatusBar();
                this.activeDocumentWorkspace.ResumeLayout();
                this.activeDocumentWorkspace.PerformLayout();
                this.activeDocumentWorkspace.FirstInputAfterGotFocus += new EventHandler(this.ActiveDocumentWorkspace_FirstInputAfterGotFocus);
            }
            if (this.ActiveDocumentWorkspaceChanged != null)
            {
                this.ActiveDocumentWorkspaceChanged(this, EventArgs.Empty);
            }
            this.UpdateStatusBarContextStatus();
            this.ResumeUpdateSnapObstacle();
            this.UpdateSnapObstacle();
        }

        protected virtual void OnActiveDocumentWorkspaceChanging()
        {
            this.SuspendUpdateSnapObstacle();
            if (this.ActiveDocumentWorkspaceChanging != null)
            {
                this.ActiveDocumentWorkspaceChanging(this, EventArgs.Empty);
            }
            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.FirstInputAfterGotFocus += new EventHandler(this.ActiveDocumentWorkspace_FirstInputAfterGotFocus);
                this.activeDocumentWorkspace.RulersEnabledChanged -= new EventHandler(this.DocumentWorkspace_RulersEnabledChanged);
                this.activeDocumentWorkspace.DocumentMouseEnter -= new EventHandler(this.DocumentMouseEnterHandler);
                this.activeDocumentWorkspace.DocumentMouseLeave -= new EventHandler(this.DocumentMouseLeaveHandler);
                this.activeDocumentWorkspace.DocumentMouseMove -= new EventHandler<MouseEventArgsF>(this.DocumentMouseMoveHandler);
                this.activeDocumentWorkspace.DocumentMouseDown -= new EventHandler<MouseEventArgsF>(this.DocumentMouseDownHandler);
                this.activeDocumentWorkspace.Scroll -= new ScrollEventHandler(this.DocumentWorkspace_Scroll);
                this.activeDocumentWorkspace.Layout -= new LayoutEventHandler(this.DocumentWorkspace_Layout);
                this.activeDocumentWorkspace.DrawGridChanged -= new EventHandler(this.DocumentWorkspace_DrawGridChanged);
                this.activeDocumentWorkspace.DocumentClick -= new EventHandler(this.DocumentClick);
                this.activeDocumentWorkspace.DocumentMouseUp -= new EventHandler<MouseEventArgsF>(this.DocumentMouseUpHandler);
                this.activeDocumentWorkspace.DocumentKeyPress -= new KeyPressEventHandler(this.DocumentKeyPress);
                this.activeDocumentWorkspace.DocumentKeyUp -= new KeyEventHandler(this.DocumenKeyUp);
                this.activeDocumentWorkspace.DocumentKeyDown -= new KeyEventHandler(this.DocumentKeyDown);
                this.activeDocumentWorkspace.History.Changed -= new EventHandler(this.HistoryChangedHandler);
                this.activeDocumentWorkspace.StatusChanged -= new EventHandler(this.OnDocumentWorkspaceStatusChanged);
                this.activeDocumentWorkspace.DocumentChanging -= new EventHandler<EventArgs<Document>>(this.DocumentWorkspace_DocumentChanging);
                this.activeDocumentWorkspace.DocumentChanged -= new EventHandler(this.DocumentWorkspace_DocumentChanged);
                this.activeDocumentWorkspace.Selection.Changing -= new EventHandler(this.SelectedPathChangingHandler);
                this.activeDocumentWorkspace.Selection.Changed -= new EventHandler(this.SelectedPathChangedHandler);
                this.activeDocumentWorkspace.ScaleFactorChanged -= new EventHandler(this.ZoomChangedHandler);
                this.activeDocumentWorkspace.ZoomBasisChanged -= new EventHandler(this.DocumentWorkspace_ZoomBasisChanged);
                this.activeDocumentWorkspace.Visible = false;
                this.historyForm.HistoryControl.HistoryStack = null;
                this.activeDocumentWorkspace.ToolChanging -= new EventHandler(this.ToolChangingHandler);
                this.activeDocumentWorkspace.ToolChanged -= new EventHandler(this.ToolChangedHandler);
                if (this.activeDocumentWorkspace.Tool != null)
                {
                    while (this.activeDocumentWorkspace.Tool.IsMouseEntered)
                    {
                        this.activeDocumentWorkspace.Tool.PerformMouseLeave();
                    }
                }
                if (this.activeDocumentWorkspace.GetToolType() != null)
                {
                    this.globalToolTypeChoice = this.activeDocumentWorkspace.GetToolType();
                }
                this.activeDocumentWorkspace.PushCacheStandby();
            }
            this.ResumeUpdateSnapObstacle();
            this.UpdateSnapObstacle();
        }

        private void OnDocumentWorkspaceStatusChanged(object sender, EventArgs e)
        {
            this.OnStatusChanged();
            this.UpdateStatusBarContextStatus();
        }

        private void OnDrawConfigStripAlphaBlendingChanged(object sender, EventArgs e)
        {
            if (this.AppEnvironment.AlphaBlending != this.widgets.ToolConfigStrip.AlphaBlending)
            {
                this.AppEnvironment.AlphaBlending = this.widgets.ToolConfigStrip.AlphaBlending;
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnEnabledChanged(e);
        }

        private void OnEnvironmentToleranceChanged(object sender, EventArgs e)
        {
            this.widgets.ToolConfigStrip.Tolerance = this.AppEnvironment.Tolerance;
            base.Focus();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.UpdateSnapObstacle();
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Select();
            }
            this.UpdateSnapObstacle();
            base.OnLoad(e);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnLocationChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnResize(e);
            if ((base.ParentForm != null) && (this.ActiveDocumentWorkspace != null))
            {
                if (base.ParentForm.WindowState == FormWindowState.Minimized)
                {
                    this.ActiveDocumentWorkspace.EnableToolPulse = false;
                }
                else
                {
                    this.ActiveDocumentWorkspace.EnableToolPulse = true;
                }
            }
        }

        protected virtual void OnRulersEnabledChanged()
        {
            if (this.RulersEnabledChanged != null)
            {
                this.RulersEnabledChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnSizeChanged(e);
        }

        private void OnStatusChanged()
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(this, EventArgs.Empty);
            }
        }

        private void OnToolBarToleranceChanged(object sender, EventArgs e)
        {
            this.AppEnvironment.Tolerance = this.widgets.ToolConfigStrip.Tolerance;
            base.Focus();
        }

        private bool OnToolFormProcessCmdKeyEvent(object sender, ref Message msg, Keys keyData) => 
            ((this.ProcessCmdKeyEvent != null) && this.ProcessCmdKeyEvent(sender, ref msg, keyData));

        private void OnToolStripMouseWheel(object sender, MouseEventArgs e)
        {
            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.PerformMouseWheel((Control) sender, e);
            }
        }

        private void OnToolStripRelinquishFocus(object sender, EventArgs e)
        {
            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.Focus();
            }
        }

        protected virtual void OnUnitsChanged()
        {
            if (this.UnitsChanged != null)
            {
                this.UnitsChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            this.UpdateSnapObstacle();
            base.OnVisibleChanged(e);
        }

        public bool OpenFileInNewWorkspace(string fileName)
        {
            DocumentWorkspace workspace;
            return this.OpenFileInNewWorkspace(fileName, true, out workspace);
        }

        public bool OpenFileInNewWorkspace(string fileName, bool addToMruList, out DocumentWorkspace dwResult)
        {
            FileType type;
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (fileName.Length == 0)
            {
                throw new ArgumentOutOfRangeException("fileName.Length == 0");
            }
            PdnBaseForm.UpdateAllForms();
            this.widgets.StatusBarProgress.ResetProgressStatusBar();
            ProgressEventHandler progressCallback = delegate (object s, ProgressEventArgs e) {
                this.widgets.StatusBarProgress.SetProgressStatusBar(e.Percent);
            };
            Document document = DocumentWorkspace.LoadDocument(this, fileName, out type, progressCallback);
            this.widgets.StatusBarProgress.EraseProgressStatusBar();
            if (document == null)
            {
                this.Cursor = Cursors.Default;
                dwResult = null;
            }
            else
            {
                using (new WaitCursorChanger(this))
                {
                    DocumentWorkspace lockMe = this.AddNewDocumentWorkspace();
                    this.Widgets.DocumentStrip.LockDocumentWorkspaceDirtyValue(lockMe, false);
                    try
                    {
                        lockMe.Document = document;
                    }
                    catch (OutOfMemoryException)
                    {
                        Utility.ErrorBox(this, PdnResources.GetString2("LoadImage.Error.OutOfMemoryException"));
                        this.RemoveDocumentWorkspace(lockMe);
                        document.Dispose();
                        dwResult = null;
                        return false;
                    }
                    lockMe.ActiveLayer = (Layer) document.Layers[0];
                    lockMe.SetDocumentSaveOptions(fileName, type, null);
                    this.ActiveDocumentWorkspace = lockMe;
                    lockMe.History.ClearAll();
                    lockMe.History.PushNewMemento(new NullHistoryMemento(PdnResources.GetString2("OpenImageAction.Name"), this.ImageFromDiskIcon));
                    document.Dirty = false;
                    this.Widgets.DocumentStrip.UnlockDocumentWorkspaceDirtyValue(lockMe);
                }
                dwResult = this.ActiveDocumentWorkspace;
                this.ActiveDocumentWorkspace.ZoomBasis = ZoomBasis.FitToWindow;
                if (addToMruList)
                {
                    Task task = this.ActiveDocumentWorkspace.AddToMruList();
                    UI.BeginFrame(this, true, delegate (UI.IFrame frame) {
                        task.ResultAsync().Receive(_ => frame.Close());
                    });
                }
            }
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.Focus();
            }
            this.widgets.StatusBarProgress.EraseProgressStatusBar();
            return (document != null);
        }

        public bool OpenFilesInNewWorkspace(string[] fileNames)
        {
            if (base.IsDisposed)
            {
                return false;
            }
            bool flag = true;
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < fileNames.Length; i++)
            {
                DocumentWorkspace workspace;
                flag &= this.OpenFileInNewWorkspace(fileNames[i], false, out workspace);
                if (flag && (i >= (fileNames.Length - this.MostRecentFiles.MaxCount)))
                {
                    Task item = workspace.AddToMruList();
                    taskList.Add(item);
                }
                if (!flag)
                {
                    break;
                }
            }
            if (taskList.Count > 0)
            {
                <>c__DisplayClassb classb;
                int mruDoneCount = 0;
                UI.BeginFrame(this, true, delegate (UI.IFrame frame) {
                    Action<Result> handler = null;
                    <>c__DisplayClassb classb1 = classb;
                    foreach (Task task in taskList)
                    {
                        if (handler == null)
                        {
                            handler = delegate {
                                if (Interlocked.Increment(ref mruDoneCount) == classb1.taskList.Count)
                                {
                                    frame.Close();
                                }
                            };
                        }
                        task.ResultAsync().Receive(handler);
                    }
                });
            }
            return flag;
        }

        private void ParentForm_Layout(object sender, LayoutEventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void ParentForm_Move(object sender, EventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void ParentForm_Moving(object sender, MovingEventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void ParentForm_ResizeEnd(object sender, EventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        private void ParentForm_SizeChanged(object sender, EventArgs e)
        {
            this.UpdateSnapObstacle();
        }

        public void PerformAction(AppWorkspaceAction performMe)
        {
            base.Update();
            using (new WaitCursorChanger(this))
            {
                performMe.PerformAction(this);
            }
            base.Update();
        }

        public void PerformActionAsync(AppWorkspaceAction performMe)
        {
            base.BeginInvoke(new Action<AppWorkspaceAction>(this.PerformAction), new object[] { performMe });
        }

        private void PrimaryColorChangedHandler(object sender, EventArgs e)
        {
            if (sender == this.appEnvironment)
            {
                this.widgets.ColorsForm.UserPrimaryColor = this.AppEnvironment.PrimaryColor;
            }
        }

        public void RefreshTool()
        {
            System.Type toolType = this.activeDocumentWorkspace.GetToolType();
            this.Widgets.ToolsControl.SelectTool(toolType);
        }

        private void RelinquishFocusHandler(object sender, EventArgs e)
        {
            base.Focus();
        }

        private void RelinquishFocusHandler2(object sender, EventArgs e)
        {
            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.Focus();
            }
        }

        public void RemoveDocumentWorkspace(DocumentWorkspace documentWorkspace)
        {
            bool flag;
            int index = this.documentWorkspaces.IndexOf(documentWorkspace);
            if (index == -1)
            {
                throw new ArgumentException("DocumentWorkspace was not created with AddNewDocumentWorkspace");
            }
            if (this.ActiveDocumentWorkspace == documentWorkspace)
            {
                flag = true;
                this.globalToolTypeChoice = documentWorkspace.GetToolType();
            }
            else
            {
                flag = false;
            }
            documentWorkspace.SetTool(null);
            if (flag)
            {
                if (this.documentWorkspaces.Count == 1)
                {
                    this.ActiveDocumentWorkspace = null;
                }
                else if (index == 0)
                {
                    this.ActiveDocumentWorkspace = this.documentWorkspaces[1];
                }
                else
                {
                    this.ActiveDocumentWorkspace = this.documentWorkspaces[index - 1];
                }
            }
            this.documentWorkspaces.Remove(documentWorkspace);
            this.toolBar.DocumentStrip.RemoveDocumentWorkspace(documentWorkspace);
            if (this.initialWorkspace == documentWorkspace)
            {
                this.initialWorkspace = null;
            }
            Document document = documentWorkspace.Document;
            documentWorkspace.Document = null;
            documentWorkspace.Dispose();
            document.Dispose();
            documentWorkspace = null;
        }

        public void ReportEffectLoadError(Triple<Assembly, System.Type, Exception> error)
        {
            lock (this.effectLoadErrors)
            {
                if (!this.effectLoadErrors.Contains(error))
                {
                    this.effectLoadErrors.Add(error);
                }
            }
        }

        public void ResetFloatingForm(FloatingToolForm ftf)
        {
            SnapManager manager = SnapManager.FindMySnapManager(this);
            if (ftf == this.Widgets.ToolsForm)
            {
                manager.ParkObstacle(this.Widgets.ToolsForm, this, HorizontalSnapEdge.Top, VerticalSnapEdge.Left);
            }
            else if (ftf == this.Widgets.HistoryForm)
            {
                manager.ParkObstacle(this.Widgets.HistoryForm, this, HorizontalSnapEdge.Top, VerticalSnapEdge.Right);
            }
            else if (ftf == this.Widgets.LayerForm)
            {
                manager.ParkObstacle(this.Widgets.LayerForm, this, HorizontalSnapEdge.Bottom, VerticalSnapEdge.Right);
            }
            else
            {
                if (ftf != this.Widgets.ColorsForm)
                {
                    throw new ArgumentException();
                }
                manager.ParkObstacle(this.Widgets.ColorsForm, this, HorizontalSnapEdge.Bottom, VerticalSnapEdge.Left);
            }
        }

        public void ResetFloatingForms()
        {
            this.ResetFloatingForm(this.Widgets.ToolsForm);
            this.ResetFloatingForm(this.Widgets.HistoryForm);
            this.ResetFloatingForm(this.Widgets.LayerForm);
            this.ResetFloatingForm(this.Widgets.ColorsForm);
        }

        private void ResumeThumbnailUpdates()
        {
            this.suspendThumbnailUpdates--;
            if (this.suspendThumbnailUpdates == 0)
            {
                this.Widgets.DocumentStrip.ResumeThumbnailUpdates();
                this.Widgets.LayerControl.ResumeLayerPreviewUpdates();
            }
        }

        private void ResumeUpdateSnapObstacle()
        {
            this.ignoreUpdateSnapObstacle--;
        }

        public void RunEffect(System.Type effectType)
        {
            this.toolBar.MainMenu.RunEffect(effectType);
        }

        public void SaveSettings()
        {
            Settings.CurrentUser.SetBoolean("Rulers", this.globalRulersChoice);
            Settings.CurrentUser.SetBoolean("DrawGrid", this.DrawGrid);
            Settings.CurrentUser.SetString("DefaultToolTypeName", this.defaultToolTypeChoice.Name);
            this.MostRecentFiles.SaveMruList();
        }

        private void SecondaryColorChangedHandler(object sender, EventArgs e)
        {
            if (sender == this.appEnvironment)
            {
                this.widgets.ColorsForm.UserSecondaryColor = this.AppEnvironment.SecondaryColor;
            }
        }

        private void SelectedPathChangedHandler(object sender, EventArgs e)
        {
            this.UpdateSelectionToolbarButtons();
        }

        private void SelectedPathChangingHandler(object sender, EventArgs e)
        {
        }

        public void SetGlassWndProcFilter(IMessageFilter filter)
        {
            this.glassWndProcFilter = filter;
            this.toolBar.SetGlassWndProcFilter(filter);
        }

        private void ShapeDrawTypeChangedHandler(object sender, EventArgs e)
        {
            if (this.widgets.ToolConfigStrip.ShapeDrawType != this.AppEnvironment.ShapeDrawType)
            {
                this.widgets.ToolConfigStrip.ShapeDrawType = this.AppEnvironment.ShapeDrawType;
            }
        }

        public IDisposable SuspendThumbnailUpdates()
        {
            IDisposable disposable = Disposable.FromAction(new Action(this.ResumeThumbnailUpdates));
            this.suspendThumbnailUpdates++;
            if (this.suspendThumbnailUpdates == 1)
            {
                this.Widgets.DocumentStrip.SuspendThumbnailUpdates();
                this.Widgets.LayerControl.SuspendLayerPreviewUpdates();
            }
            return disposable;
        }

        private void SuspendUpdateSnapObstacle()
        {
            this.ignoreUpdateSnapObstacle++;
        }

        private void ToolChangedHandler(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace.Tool != null)
            {
                this.widgets.ToolsControl.SelectTool(this.ActiveDocumentWorkspace.GetToolType(), false);
                this.toolBar.ToolChooserStrip.SelectTool(this.ActiveDocumentWorkspace.GetToolType(), false);
                this.toolBar.ToolConfigStrip.Visible = true;
                this.toolBar.ToolConfigStrip.ToolBarConfigItems = this.ActiveDocumentWorkspace.Tool.ToolBarConfigItems;
                this.globalToolTypeChoice = this.ActiveDocumentWorkspace.GetToolType();
            }
            this.UpdateStatusBarContextStatus();
            UI.ResumeControlPainting(this.toolBar);
            this.toolBar.Refresh();
        }

        private void ToolChangingHandler(object sender, EventArgs e)
        {
            UI.SuspendControlPainting(this.toolBar);
            PaintDotNet.Tools.Tool tool = this.ActiveDocumentWorkspace.Tool;
        }

        private void ToolConfigStrip_ColorPickerClickBehaviorChanged(object sender, EventArgs e)
        {
            this.appEnvironment.ColorPickerClickBehavior = this.widgets.ToolConfigStrip.ColorPickerClickBehavior;
        }

        private void ToolConfigStrip_FloodModeChanged(object sender, EventArgs e)
        {
            this.appEnvironment.FloodMode = this.widgets.ToolConfigStrip.FloodMode;
        }

        private void ToolConfigStrip_FontSmoothingChanged(object sender, EventArgs e)
        {
            this.AppEnvironment.FontSmoothing = this.widgets.ToolConfigStrip.FontSmoothing;
        }

        private void ToolConfigStrip_FontTextChanged(object sender, EventArgs e)
        {
            this.AppEnvironment.FontInfo = this.widgets.ToolConfigStrip.FontInfo;
        }

        private void ToolConfigStrip_GradientInfoChanged(object sender, EventArgs e)
        {
            if (this.AppEnvironment.GradientInfo != this.widgets.ToolConfigStrip.GradientInfo)
            {
                this.AppEnvironment.GradientInfo = this.widgets.ToolConfigStrip.GradientInfo;
            }
        }

        private void ToolConfigStrip_ResamplingAlgorithmChanged(object sender, EventArgs e)
        {
            this.appEnvironment.ResamplingAlgorithm = this.widgets.ToolConfigStrip.ResamplingAlgorithm;
        }

        private void ToolConfigStrip_SelectionCombineModeChanged(object sender, EventArgs e)
        {
            this.appEnvironment.SelectionCombineMode = this.widgets.ToolConfigStrip.SelectionCombineMode;
        }

        private void ToolConfigStrip_SelectionDrawModeInfoChanged(object sender, EventArgs e)
        {
            this.appEnvironment.SelectionDrawModeInfo = this.widgets.ToolConfigStrip.SelectionDrawModeInfo;
        }

        private void ToolConfigStrip_SelectionDrawModeUnitsChanging(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.Document != null))
            {
                new ToolConfigStrip_SelectionDrawModeUnitsChangeHandler(this.toolBar.ToolConfigStrip, this.ActiveDocumentWorkspace.Document).Initialize();
            }
        }

        private void ToolConfigStrip_TextAlignmentChanged(object sender, EventArgs e)
        {
            this.AppEnvironment.TextAlignment = this.widgets.ToolConfigStrip.FontAlignment;
        }

        private void UpdateCursorInfoInStatusBar(int cursorX, int cursorY)
        {
            base.SuspendLayout();
            if ((this.activeDocumentWorkspace == null) || (this.activeDocumentWorkspace.Document == null))
            {
                this.statusBar.CursorInfoText = string.Empty;
            }
            else
            {
                string str;
                string str2;
                string str3;
                this.CoordinatesToStrings(cursorX, cursorY, out str, out str2, out str3);
                string str4 = string.Format(CultureInfo.InvariantCulture, this.cursorInfoStatusBarFormat, new object[] { str, str3, str2, str3 });
                this.statusBar.CursorInfoText = str4;
            }
            base.ResumeLayout(false);
        }

        private void UpdateDocInfoInStatusBar()
        {
            if ((this.activeDocumentWorkspace == null) || (this.activeDocumentWorkspace.Document == null))
            {
                this.statusBar.ImageInfoStatusText = string.Empty;
            }
            else if ((this.activeDocumentWorkspace != null) && (this.activeDocumentWorkspace.Document != null))
            {
                string str;
                string str2;
                string str3;
                this.CoordinatesToStrings(this.activeDocumentWorkspace.Document.Width, this.activeDocumentWorkspace.Document.Height, out str, out str2, out str3);
                string str4 = string.Format(CultureInfo.InvariantCulture, this.imageInfoStatusBarFormat, new object[] { str, str3, str2, str3 });
                this.statusBar.ImageInfoStatusText = str4;
            }
        }

        private void UpdateHistoryButtons()
        {
            if (this.ActiveDocumentWorkspace == null)
            {
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, false);
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, false);
            }
            else
            {
                if (this.ActiveDocumentWorkspace.History.UndoStack.Count > 1)
                {
                    this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, true);
                }
                else
                {
                    this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, false);
                }
                if (this.ActiveDocumentWorkspace.History.RedoStack.Count > 0)
                {
                    this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, true);
                }
                else
                {
                    this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, false);
                }
            }
        }

        private void UpdateSelectionToolbarButtons()
        {
            if ((this.ActiveDocumentWorkspace == null) || this.ActiveDocumentWorkspace.Selection.IsEmpty)
            {
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Cut, false);
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Copy, false);
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Deselect, false);
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CropToSelection, false);
            }
            else
            {
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Cut, true);
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Copy, true);
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Deselect, true);
                this.widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CropToSelection, true);
            }
        }

        private void UpdateSnapObstacle()
        {
            if ((this.ignoreUpdateSnapObstacle <= 0) && (this.snapObstacle != null))
            {
                if (!this.addedToSnapManager)
                {
                    SnapManager manager = SnapManager.FindMySnapManager(this);
                    if (manager != null)
                    {
                        PaintDotNet.SnapObstacle snapObstacle = this.SnapObstacle;
                        if (!this.addedToSnapManager)
                        {
                            manager.AddSnapObstacle(this.SnapObstacle);
                            this.addedToSnapManager = true;
                            base.FindForm().Shown += new EventHandler(this.AppWorkspace_Shown);
                        }
                    }
                }
                if (this.snapObstacle != null)
                {
                    Int32Rect visibleViewRect;
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        visibleViewRect = this.ActiveDocumentWorkspace.VisibleViewRect;
                    }
                    else
                    {
                        visibleViewRect = this.workspacePanel.ClientRectangle.ToInt32Rect();
                    }
                    Int32Rect bounds = this.workspacePanel.RectangleToScreen(visibleViewRect);
                    this.snapObstacle.SetBounds(bounds);
                    this.snapObstacle.Enabled = base.Visible && base.Enabled;
                }
            }
        }

        private void UpdateStatusBarContextStatus()
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.statusBar.ContextStatusText = this.activeDocumentWorkspace.StatusText;
                this.statusBar.ContextStatusImage = this.activeDocumentWorkspace.StatusIcon;
            }
            else
            {
                this.statusBar.ContextStatusText = string.Empty;
                this.statusBar.ContextStatusImage = null;
            }
        }

        private void ViewConfigStrip_DrawGridChanged(object sender, EventArgs e)
        {
            this.DrawGrid = ((ViewConfigStrip) sender).DrawGrid;
        }

        private void ViewConfigStrip_RulersEnabledChanged(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.RulersEnabled = this.toolBar.ViewConfigStrip.RulersEnabled;
            }
        }

        private void ViewConfigStrip_UnitsChanged(object sender, EventArgs e)
        {
            if (this.toolBar.ViewConfigStrip.Units != MeasurementUnit.Pixel)
            {
                Settings.CurrentUser.SetString("LastNonPixelUnits", this.toolBar.ViewConfigStrip.Units.ToString());
            }
            if (this.activeDocumentWorkspace != null)
            {
                this.activeDocumentWorkspace.Units = this.Units;
            }
            Settings.CurrentUser.SetString("Units", this.toolBar.ViewConfigStrip.Units.ToString());
            this.UpdateDocInfoInStatusBar();
            this.statusBar.CursorInfoText = string.Empty;
            this.OnUnitsChanged();
        }

        private void ViewConfigStrip_ZoomBasisChanged(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.ActiveDocumentWorkspace.ZoomBasis != this.toolBar.ViewConfigStrip.ZoomBasis))
            {
                this.ActiveDocumentWorkspace.ZoomBasis = this.toolBar.ViewConfigStrip.ZoomBasis;
            }
        }

        private void ViewConfigStrip_ZoomIn(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ZoomIn();
            }
        }

        private void ViewConfigStrip_ZoomOut(object sender, EventArgs e)
        {
            if (this.ActiveDocumentWorkspace != null)
            {
                this.ActiveDocumentWorkspace.ZoomOut();
            }
        }

        private void ViewConfigStrip_ZoomScaleChanged(object sender, EventArgs e)
        {
            if ((this.ActiveDocumentWorkspace != null) && (this.toolBar.ViewConfigStrip.ZoomBasis == ZoomBasis.ScaleFactor))
            {
                this.activeDocumentWorkspace.ScaleFactor = this.toolBar.ViewConfigStrip.ScaleFactor;
            }
        }

        protected override void WndProc(ref Message m)
        {
            bool flag = false;
            if (this.glassWndProcFilter != null)
            {
                flag = this.glassWndProcFilter.PreFilterMessage(ref m);
            }
            if (!flag)
            {
                base.WndProc(ref m);
            }
        }

        private void ZoomChangedHandler(object sender, EventArgs e)
        {
            ScaleFactor scaleFactor = this.activeDocumentWorkspace.ScaleFactor;
            this.toolBar.ViewConfigStrip.SuspendEvents();
            this.toolBar.ViewConfigStrip.ZoomBasis = this.activeDocumentWorkspace.ZoomBasis;
            this.toolBar.ViewConfigStrip.ScaleFactor = scaleFactor;
            this.toolBar.ViewConfigStrip.ResumeEvents();
        }

        [Browsable(false)]
        public DocumentWorkspace ActiveDocumentWorkspace
        {
            get => 
                this.activeDocumentWorkspace;
            set
            {
                if (value != this.activeDocumentWorkspace)
                {
                    if ((value != null) && (this.documentWorkspaces.IndexOf(value) == -1))
                    {
                        throw new ArgumentException("DocumentWorkspace was not created with AddNewDocumentWorkspace");
                    }
                    if (this.activeDocumentWorkspace != null)
                    {
                        bool focused = this.activeDocumentWorkspace.Focused;
                    }
                    UI.SuspendControlPainting(this);
                    this.OnActiveDocumentWorkspaceChanging();
                    this.activeDocumentWorkspace = value;
                    this.OnActiveDocumentWorkspaceChanged();
                    UI.ResumeControlPainting(this);
                    this.Refresh();
                    if (value != null)
                    {
                        value.Focus();
                    }
                }
            }
        }

        [Browsable(false)]
        public PaintDotNet.AppEnvironment AppEnvironment =>
            this.appEnvironment;

        public IDispatcher BackgroundThread =>
            this.backgroundThread;

        public System.Type DefaultToolType
        {
            get => 
                this.defaultToolTypeChoice;
            set
            {
                this.defaultToolTypeChoice = value;
                Settings.CurrentUser.SetString("DefaultToolTypeName", value.Name);
            }
        }

        public IDispatcher Dispatcher =>
            this.dispatcher;

        public DocumentWorkspace[] DocumentWorkspaces =>
            this.documentWorkspaces.ToArrayEx<DocumentWorkspace>();

        private bool DrawGrid
        {
            get => 
                this.Widgets.ViewConfigStrip.DrawGrid;
            set
            {
                if (this.Widgets.ViewConfigStrip.DrawGrid != value)
                {
                    this.Widgets.ViewConfigStrip.DrawGrid = value;
                }
                if ((this.activeDocumentWorkspace != null) && (this.activeDocumentWorkspace.DrawGrid != value))
                {
                    this.activeDocumentWorkspace.DrawGrid = value;
                }
                Settings.CurrentUser.SetBoolean("DrawGrid", this.DrawGrid);
            }
        }

        private ImageResource FileNewIcon =>
            PdnResources.GetImageResource2("Icons.MenuFileNewIcon.png");

        public Padding GlassInset =>
            this.toolBar.GlassInset;

        public System.Type GlobalToolTypeChoice
        {
            get => 
                this.globalToolTypeChoice;
            set
            {
                this.globalToolTypeChoice = value;
                if (this.ActiveDocumentWorkspace != null)
                {
                    this.ActiveDocumentWorkspace.SetToolFromType(value);
                }
            }
        }

        private ImageResource ImageFromDiskIcon =>
            PdnResources.GetImageResource2("Icons.ImageFromDiskIcon.png");

        public DocumentWorkspace InitialWorkspace
        {
            set
            {
                this.initialWorkspace = value;
            }
        }

        public bool IsGlassDesired =>
            this.toolBar.IsGlassDesired;

        public PaintDotNet.MostRecentFiles MostRecentFiles
        {
            get
            {
                if (this.mostRecentFiles == null)
                {
                    this.mostRecentFiles = new PaintDotNet.MostRecentFiles(8);
                }
                return this.mostRecentFiles;
            }
        }

        public bool RulersEnabled
        {
            get => 
                this.globalRulersChoice;
            set
            {
                if (this.globalRulersChoice != value)
                {
                    this.globalRulersChoice = value;
                    if (this.ActiveDocumentWorkspace != null)
                    {
                        this.ActiveDocumentWorkspace.RulersEnabled = value;
                    }
                    this.OnRulersEnabledChanged();
                }
            }
        }

        public PaintDotNet.SnapObstacle SnapObstacle
        {
            get
            {
                if (this.snapObstacle == null)
                {
                    this.snapObstacle = new SnapObstacleController(base.Name, Int32Rect.Empty, SnapRegion.Interior, true);
                    this.snapObstacle.EnableSave = false;
                    PdnBaseForm form = base.FindForm() as PdnBaseForm;
                    form.Moving += new MovingEventHandler(this.ParentForm_Moving);
                    form.Move += new EventHandler(this.ParentForm_Move);
                    form.ResizeEnd += new EventHandler(this.ParentForm_ResizeEnd);
                    form.Layout += new LayoutEventHandler(this.ParentForm_Layout);
                    form.SizeChanged += new EventHandler(this.ParentForm_SizeChanged);
                    this.UpdateSnapObstacle();
                }
                return this.snapObstacle;
            }
        }

        public PaintDotNet.Threading.TaskManager TaskManager =>
            this.taskManager;

        public PdnToolBar ToolBar =>
            this.toolBar;

        public MeasurementUnit Units
        {
            get => 
                this.widgets.ViewConfigStrip.Units;
            set
            {
                this.widgets.ViewConfigStrip.Units = value;
            }
        }

        [Browsable(false)]
        public WorkspaceWidgets Widgets =>
            this.widgets;

        private sealed class ToolConfigStrip_SelectionDrawModeUnitsChangeHandler
        {
            private Document activeDocument;
            private MeasurementUnit oldUnits;
            private ToolConfigStrip toolConfigStrip;

            public ToolConfigStrip_SelectionDrawModeUnitsChangeHandler(ToolConfigStrip toolConfigStrip, Document activeDocument)
            {
                this.toolConfigStrip = toolConfigStrip;
                this.activeDocument = activeDocument;
                this.oldUnits = toolConfigStrip.SelectionDrawModeInfo.Units;
            }

            public void Initialize()
            {
                this.toolConfigStrip.SelectionDrawModeUnitsChanged += new EventHandler(this.ToolConfigStrip_SelectionDrawModeUnitsChanged);
            }

            public void ToolConfigStrip_SelectionDrawModeUnitsChanged(object sender, EventArgs e)
            {
                try
                {
                    SelectionDrawModeInfo selectionDrawModeInfo = this.toolConfigStrip.SelectionDrawModeInfo;
                    MeasurementUnit units = selectionDrawModeInfo.Units;
                    double width = selectionDrawModeInfo.Width;
                    double height = selectionDrawModeInfo.Height;
                    double newWidth = Document.ConvertMeasurement(width, this.oldUnits, this.activeDocument.DpuUnit, this.activeDocument.DpuX, units);
                    double newHeight = Document.ConvertMeasurement(height, this.oldUnits, this.activeDocument.DpuUnit, this.activeDocument.DpuY, units);
                    SelectionDrawModeInfo info2 = selectionDrawModeInfo.CloneWithNewWidthAndHeight(newWidth, newHeight);
                    this.toolConfigStrip.SelectionDrawModeInfo = info2;
                }
                finally
                {
                    this.toolConfigStrip.SelectionDrawModeUnitsChanged -= new EventHandler(this.ToolConfigStrip_SelectionDrawModeUnitsChanged);
                }
            }
        }
    }
}

