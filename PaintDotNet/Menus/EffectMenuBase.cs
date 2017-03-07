namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.AppModel;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Effects;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal abstract class EffectMenuBase : PdnMenuItem
    {
        private Container components;
        private const int effectRefreshInterval = 0x10;
        private EffectsCollection effects;
        private Dictionary<System.Type, EffectConfigToken> effectTokens = new Dictionary<System.Type, EffectConfigToken>();
        private System.Windows.Forms.Timer invalidateTimer;
        private Image lastEffectImage;
        private string lastEffectName;
        private EffectConfigToken lastEffectToken;
        private System.Type lastEffectType;
        private bool menuPopulated;
        private PdnRegion[] progressRegions;
        private int progressRegionsStartIndex;
        private int renderingThreadCount = Math.Max(2, Processor.LogicalCpuCount);
        private PdnMenuItem sentinel;
        private bool showProgressInStatusBar;
        private const int tilesPerCpu = 0x4b;

        public EffectMenuBase()
        {
            this.InitializeComponent();
        }

        private void AddEffectsToMenu()
        {
            EffectsCollection effects = this.Effects;
            System.Type[] typeArray1 = effects.Effects;
            bool enableEffectShortcuts = this.EnableEffectShortcuts;
            List<Effect> list = new List<Effect>();
            foreach (System.Type type in effects.Effects)
            {
                try
                {
                    Effect effect = (Effect) type.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                    if (this.FilterEffects(effect))
                    {
                        list.Add(effect);
                    }
                }
                catch (Exception exception)
                {
                    base.AppWorkspace.ReportEffectLoadError(Triple.Create<Assembly, System.Type, Exception>(type.Assembly, type, exception));
                }
            }
            list.Sort((lhs, rhs) => string.Compare(lhs.Name, rhs.Name, true));
            List<string> list2 = new List<string>();
            foreach (Effect effect2 in list)
            {
                if (!string.IsNullOrEmpty(effect2.SubMenuName))
                {
                    list2.Add(effect2.SubMenuName);
                }
            }
            list2.Sort((lhs, rhs) => string.Compare(lhs, rhs, true));
            string str = null;
            foreach (string str2 in list2)
            {
                if (str2 != str)
                {
                    PdnMenuItem item = new PdnMenuItem(str2, null, null);
                    base.DropDownItems.Add(item);
                    str = str2;
                }
            }
            foreach (Effect effect3 in list)
            {
                this.AddEffectToMenu(effect3, enableEffectShortcuts);
                effect3.Dispose();
            }
        }

        private void AddEffectToMenu(Effect effect, bool withShortcut)
        {
            if (this.FilterEffects(effect))
            {
                Image image;
                string name = effect.Name;
                if (effect.CheckForEffectFlags(EffectFlags.Configurable))
                {
                    name = string.Format(PdnResources.GetString2("Effects.Name.Format.Configurable"), name);
                }
                if (effect.Image == null)
                {
                    image = null;
                }
                else
                {
                    try
                    {
                        image = effect.Image.CloneT<Image>();
                    }
                    catch (Exception)
                    {
                        image = null;
                    }
                }
                PdnMenuItem item = new PdnMenuItem(name, image, new EventHandler(this.EffectMenuItem_Click));
                if (withShortcut)
                {
                    item.ShortcutKeys = this.GetEffectShortcutKeys(effect);
                }
                else
                {
                    item.ShortcutKeys = Keys.None;
                }
                item.Tag = effect.GetType();
                item.Name = "Effect(" + effect.GetType().FullName + ")";
                PdnMenuItem item2 = this;
                if (effect.SubMenuName != null)
                {
                    PdnMenuItem item3 = null;
                    foreach (ToolStripItem item4 in base.DropDownItems)
                    {
                        PdnMenuItem item5 = item4 as PdnMenuItem;
                        if ((item5 != null) && (item5.Text == effect.SubMenuName))
                        {
                            item3 = item5;
                            break;
                        }
                    }
                    if (item3 == null)
                    {
                        item3 = new PdnMenuItem(effect.SubMenuName, null, null);
                        base.DropDownItems.Add(item3);
                    }
                    item2 = item3;
                }
                item2.DropDownItems.Add(item);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.lastEffectImage != null)
                {
                    this.lastEffectImage.Dispose();
                    this.lastEffectImage = null;
                }
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
            }
            base.Dispose(disposing);
        }

        private bool DoEffect(Effect effect, EffectConfigToken token, PdnRegion selectedRegion, PdnRegion regionToRender, Surface originalSurface, out Exception exception)
        {
            exception = null;
            DocumentWorkspace activeDocumentWorkspace = base.AppWorkspace.ActiveDocumentWorkspace;
            bool dirty = activeDocumentWorkspace.Document.Dirty;
            bool flag2 = false;
            bool flag3 = false;
            try
            {
                EventHandler handler = null;
                VirtualTask<Unit> renderTask = activeDocumentWorkspace.TaskManager.CreateVirtualTask();
                using (TaskProgressDialog progressDialog = new TaskProgressDialog())
                {
                    if (effect.Image != null)
                    {
                        progressDialog.Icon = Utility.ImageToIcon(effect.Image, false);
                    }
                    progressDialog.Text = effect.Name;
                    PdnResources.GetString2("Effects.ApplyingDialog.Description");
                    string renderintTextPercentFormat = PdnResources.GetString2("Effects.ApplyingDialog.Description.WithPercent.Format");
                    string cancelingText = PdnResources.GetString2("TaskProgressDialog.Canceling.Text");
                    string str = PdnResources.GetString2("TaskProgressDialog.Initializing.Text");
                    progressDialog.HeaderText = str;
                    this.showProgressInStatusBar = false;
                    this.invalidateTimer.Enabled = true;
                    using (new WaitCursorChanger(base.AppWorkspace))
                    {
                        HistoryMemento memento = null;
                        DialogResult none = DialogResult.None;
                        base.AppWorkspace.Widgets.LayerControl.SuspendLayerPreviewUpdates();
                        try
                        {
                            <>c__DisplayClass34 class3;
                            <>c__DisplayClass32 class4;
                            <>c__DisplayClass30 class5;
                            <>c__DisplayClass2d classd;
                            ManualResetEvent saveEvent = new ManualResetEvent(false);
                            BitmapHistoryMemento bha = null;
                            GeometryList selectedGeometry = GeometryList.FromNonOverlappingScans(selectedRegion.GetRegionScansReadOnlyInt().ToInt32RectArray());
                            PrivateThreadPool.Global.QueueUserWorkItem(delegate (object context) {
                                try
                                {
                                    ImageResource resource;
                                    if (effect.Image == null)
                                    {
                                        resource = null;
                                    }
                                    else
                                    {
                                        resource = ImageResource.FromImage(effect.Image);
                                    }
                                    bha = new BitmapHistoryMemento(effect.Name, resource, this.AppWorkspace.ActiveDocumentWorkspace, this.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex, selectedGeometry, originalSurface);
                                }
                                finally
                                {
                                    saveEvent.Set();
                                    DisposableUtil.Free<GeometryList>(ref selectedGeometry);
                                }
                            });
                            BackgroundEffectRenderer ber = new BackgroundEffectRenderer(effect, token, new RenderArgs(((BitmapLayer) base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer).Surface), new RenderArgs(originalSurface), regionToRender, 0x4b * this.renderingThreadCount, this.renderingThreadCount);
                            int tileCount = 0;
                            ber.RenderedTile += delegate (object s, RenderedTileEventArgs e) {
                                <>c__DisplayClass34 class1 = class3;
                                <>c__DisplayClass32 class2 = class4;
                                <>c__DisplayClass30 class3 = class5;
                                <>c__DisplayClass2d classd1 = classd;
                                progressDialog.Dispatcher.BeginTry(delegate {
                                    if (!class3.renderTask.IsCancelRequested)
                                    {
                                        tileCount++;
                                        double num = ((double) (tileCount + 1)) / ((double) e.TileCount);
                                        class3.renderTask.Progress = new double?(num.Clamp(0.0, 1.0));
                                        int num2 = (int) (100.0 * num);
                                        string str = string.Format(class1.renderintTextPercentFormat, num2);
                                        class2.progressDialog.HeaderText = str;
                                    }
                                }).Observe();
                            };
                            ber.RenderedTile += new RenderedTileEventHandler(this.RenderedTileHandler);
                            ber.StartingRendering += new EventHandler(this.StartingRenderingHandler);
                            if (handler == null)
                            {
                                handler = delegate (object s, EventArgs e) {
                                    renderTask.SetState(TaskState.Finished);
                                };
                            }
                            ber.FinishedRendering += handler;
                            ber.FinishedRendering += new EventHandler(this.FinishedRenderingHandler);
                            renderTask.CancelRequested += delegate (object sender, EventArgs e) {
                                ber.AbortAsync();
                                progressDialog.Dispatcher.BeginTry((Action) (() => (progressDialog.HeaderText = cancelingText))).Observe();
                            };
                            renderTask.SetState(TaskState.Running);
                            progressDialog.Shown += delegate (object s, EventArgs e) {
                                ber.Start();
                            };
                            progressDialog.Task = renderTask;
                            progressDialog.CloseOnFinished = true;
                            progressDialog.ShowDialog(base.AppWorkspace);
                            if (!renderTask.IsCancelRequested)
                            {
                                none = DialogResult.OK;
                            }
                            else
                            {
                                none = DialogResult.Cancel;
                                flag2 = true;
                                using (new WaitCursorChanger(base.AppWorkspace))
                                {
                                    try
                                    {
                                        ber.Abort();
                                        ber.Join();
                                    }
                                    catch (Exception exception2)
                                    {
                                        exception = exception2;
                                    }
                                    try
                                    {
                                        if (originalSurface.Scan0.MaySetAllowWrites)
                                        {
                                            originalSurface.Scan0.AllowWrites = true;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                                originalSurface.Parallelize(7).Render(((BitmapLayer) base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer).Surface, Int32Point.Zero);
                            }
                            this.invalidateTimer.Enabled = false;
                            try
                            {
                                ber.Join();
                            }
                            catch (Exception exception3)
                            {
                                exception = exception3;
                            }
                            ber.Dispose();
                            this.WaitWithUI(base.AppWorkspace, effect, WaitWithUIType.Finishing, saveEvent);
                            saveEvent.Close();
                            saveEvent = null;
                            memento = bha;
                        }
                        catch (Exception)
                        {
                            using (new WaitCursorChanger(base.AppWorkspace))
                            {
                                Surface dst = ((BitmapLayer) base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer).Surface;
                                originalSurface.Parallelize(7).Render<ColorBgra>(dst);
                                memento = null;
                            }
                        }
                        finally
                        {
                            base.AppWorkspace.Widgets.LayerControl.ResumeLayerPreviewUpdates();
                        }
                        using (PdnRegion region = Utility.SimplifyAndInflateRegion(selectedRegion))
                        {
                            using (new WaitCursorChanger(base.AppWorkspace))
                            {
                                base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer.Invalidate(region);
                            }
                        }
                        using (new WaitCursorChanger(base.AppWorkspace))
                        {
                            if (none == DialogResult.OK)
                            {
                                if (memento != null)
                                {
                                    base.AppWorkspace.ActiveDocumentWorkspace.History.PushNewMemento(memento);
                                }
                                base.AppWorkspace.Update();
                                return true;
                            }
                            Utility.GCFullCollect();
                            return flag3;
                        }
                    }
                }
            }
            finally
            {
                if (flag2)
                {
                    base.AppWorkspace.ActiveDocumentWorkspace.Document.Dirty = dirty;
                }
                this.invalidateTimer.Enabled = false;
            }
            return flag3;
        }

        private void EffectMenuItem_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                PdnMenuItem item = (PdnMenuItem) sender;
                System.Type tag = (System.Type) item.Tag;
                this.RunEffect(tag);
            }
        }

        protected abstract bool FilterEffects(Effect effect);
        private void FinishedRenderingHandler(object sender, EventArgs e)
        {
            if (base.AppWorkspace.InvokeRequired)
            {
                base.AppWorkspace.BeginInvoke(new EventHandler(this.FinishedRenderingHandler), new object[] { sender, e });
            }
        }

        private static EffectsCollection GatherEffects()
        {
            bool flag;
            List<Assembly> assemblies = new List<Assembly> {
                Assembly.GetAssembly(typeof(Effect))
            };
            string path = Path.Combine(PdnInfo.ApplicationDir2, "Effects");
            try
            {
                flag = Directory.Exists(path);
            }
            catch (Exception)
            {
                flag = false;
            }
            if (flag)
            {
                string searchPattern = "*.dll";
                foreach (string str4 in Directory.GetFiles(path, searchPattern))
                {
                    Assembly item = null;
                    try
                    {
                        item = Assembly.LoadFrom(str4);
                        assemblies.Add(item);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return new EffectsCollection(assemblies);
        }

        protected virtual Keys GetEffectShortcutKeys(Effect effect) => 
            Keys.None;

        private void HandleEffectException(AppWorkspace appWorkspace, Effect effect, Exception ex)
        {
            Action action2 = null;
            try
            {
                base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                base.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
            }
            catch (Exception)
            {
            }
            if (this.IsBuiltInEffect(effect))
            {
                throw new ApplicationException("Effect threw an exception", ex);
            }
            Icon icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.BugWarning.png").Reference);
            string str = PdnResources.GetString2("Effect.PluginErrorDialog.Title");
            Image image = null;
            string str2 = PdnResources.GetString2("Effect.PluginErrorDialog.IntroText");
            TaskButton button = new TaskButton(PdnResources.GetImageResource2("Icons.RightArrowBlue.png").Reference, PdnResources.GetString2("Effect.PluginErrorDialog.RestartTB.ActionText"), PdnResources.GetString2("Effect.PluginErrorDialog.RestartTB.ExplanationText"));
            TaskButton button2 = new TaskButton(PdnResources.GetImageResource2("Icons.WarningIcon.png").Reference, PdnResources.GetString2("Effect.PluginErrorDialog.DoNotRestartTB.ActionText"), PdnResources.GetString2("Effect.PluginErrorDialog.DoNotRestartTB.ExplanationText"));
            string str3 = PdnResources.GetString2("Effect.PluginErrorDialog.AuxButton1.Text");
            if (action2 == null)
            {
                action2 = delegate {
                    using (PdnBaseForm form = new PdnBaseForm())
                    {
                        form.Name = "EffectCrash";
                        TextBox box = new TextBox();
                        form.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.WarningIcon.png").Reference);
                        form.Text = PdnResources.GetString2("Effect.PluginErrorDialog.Title");
                        box.Dock = DockStyle.Fill;
                        box.ReadOnly = true;
                        box.Multiline = true;
                        string str = AppWorkspace.GetLocalizedEffectErrorMessage(effect.GetType().Assembly, effect.GetType(), ex);
                        box.Font = new Font(FontFamily.GenericMonospace, box.Font.Size);
                        box.Text = str;
                        box.ScrollBars = ScrollBars.Vertical;
                        form.StartPosition = FormStartPosition.CenterParent;
                        form.ShowInTaskbar = false;
                        form.MinimizeBox = false;
                        form.Controls.Add(box);
                        form.Width = UI.ScaleWidth(700);
                        form.ShowDialog();
                    }
                };
            }
            Action auxButtonClickHandler = action2;
            TaskAuxButton button3 = new TaskAuxButton {
                Text = str3
            };
            button3.Clicked += (s, e) => auxButtonClickHandler();
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
                PixelWidth96Dpi = TaskDialog.DefaultPixelWidth96Dpi * 2,
                AuxControls = new TaskAuxControl[] { button3 }
            };
            if (dialog.Show(appWorkspace) == button)
            {
                if (Shell.IsActivityQueuedForRestart)
                {
                    Utility.ErrorBox(appWorkspace, PdnResources.GetString2("Effect.PluginErrorDialog.CantQueue2ndRestart"));
                }
                else
                {
                    CloseAllWorkspacesAction action = new CloseAllWorkspacesAction();
                    action.PerformAction(appWorkspace);
                    if (!action.Cancelled)
                    {
                        Shell.RestartApplication();
                        Startup.CloseApplication();
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.sentinel = new PdnMenuItem();
            this.sentinel.Name = null;
            this.components = new Container();
            this.invalidateTimer = new System.Windows.Forms.Timer(this.components);
            this.invalidateTimer.Enabled = false;
            this.invalidateTimer.Tick += new EventHandler(this.InvalidateTimer_Tick);
            this.invalidateTimer.Interval = 0x10;
            base.DropDownItems.Add(this.sentinel);
        }

        private void InvalidateTimer_Tick(object sender, EventArgs e)
        {
            if ((base.AppWorkspace.FindForm().WindowState != FormWindowState.Minimized) && (this.progressRegions != null))
            {
                lock (this.progressRegions)
                {
                    int progressRegionsStartIndex = this.progressRegionsStartIndex;
                    int index = progressRegionsStartIndex;
                    while (index < this.progressRegions.Length)
                    {
                        if (this.progressRegions[index] == null)
                        {
                            break;
                        }
                        index++;
                    }
                    if (progressRegionsStartIndex != index)
                    {
                        using (PdnRegion region = PdnRegion.CreateEmpty())
                        {
                            for (int i = progressRegionsStartIndex; i < index; i++)
                            {
                                region.Union(this.progressRegions[i]);
                            }
                            using (PdnRegion region2 = Utility.SimplifyAndInflateRegion(region))
                            {
                                base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer.Invalidate(region2);
                            }
                            this.progressRegionsStartIndex = index;
                        }
                    }
                    if (this.showProgressInStatusBar)
                    {
                        double percent = (100.0 * index) / ((double) this.progressRegions.Length);
                        base.AppWorkspace.Widgets.StatusBarProgress.SetProgressStatusBar(percent);
                    }
                    else
                    {
                        base.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                    }
                }
            }
        }

        private bool IsBuiltInEffect(Effect effect)
        {
            if (effect == null)
            {
                return true;
            }
            System.Type type = effect.GetType();
            System.Type type2 = typeof(Effect);
            return (type.Assembly == type2.Assembly);
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            if (!this.menuPopulated)
            {
                this.PopulateMenu();
            }
            bool flag = base.AppWorkspace.ActiveDocumentWorkspace != null;
            foreach (ToolStripItem item in base.DropDownItems)
            {
                item.Enabled = flag;
            }
            base.OnDropDownOpening(e);
        }

        public void PopulateEffects()
        {
            this.PopulateMenu(false);
        }

        private void PopulateMenu()
        {
            base.DropDownItems.Clear();
            if (this.EnableRepeatEffectMenuItem && (this.lastEffectType != null))
            {
                PdnMenuItem item = new PdnMenuItem(string.Format(PdnResources.GetString2("Effects.RepeatMenuItem.Format"), this.lastEffectName), this.lastEffectImage, new EventHandler(this.RepeatEffectMenuItem_Click)) {
                    Name = "RepeatEffect(" + this.lastEffectType.FullName + ")",
                    ShortcutKeys = Keys.Control | Keys.F
                };
                base.DropDownItems.Add(item);
                ToolStripSeparator separator = new ToolStripSeparator();
                base.DropDownItems.Add(separator);
            }
            this.AddEffectsToMenu();
            Triple<Assembly, System.Type, Exception>[] loaderExceptions = this.Effects.GetLoaderExceptions();
            for (int i = 0; i < loaderExceptions.Length; i++)
            {
                base.AppWorkspace.ReportEffectLoadError(loaderExceptions[i]);
            }
        }

        private void PopulateMenu(bool forceRepopulate)
        {
            if (forceRepopulate)
            {
                this.menuPopulated = false;
            }
            this.PopulateMenu();
        }

        private void RenderedTileHandler(object sender, RenderedTileEventArgs e)
        {
            if (base.AppWorkspace.InvokeRequired)
            {
                base.AppWorkspace.BeginInvoke(new RenderedTileEventHandler(this.RenderedTileHandler), new object[] { sender, e });
            }
            else
            {
                lock (this.progressRegions)
                {
                    if (this.progressRegions[e.TileNumber] == null)
                    {
                        this.progressRegions[e.TileNumber] = e.RenderedRegion;
                    }
                }
            }
        }

        private void RepeatEffectMenuItem_Click(object sender, EventArgs e)
        {
            Exception exception = null;
            Effect effect = null;
            DocumentWorkspace activeDocumentWorkspace = base.AppWorkspace.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace != null)
            {
                using (new PushNullToolMode(activeDocumentWorkspace))
                {
                    Surface dst = activeDocumentWorkspace.BorrowScratchSurface(base.GetType() + ".RepeatEffectMenuItem_Click() utilizing scratch for rendering");
                    ServiceProviderForEffects effects = new ServiceProviderForEffects();
                    try
                    {
                        EffectConfigToken token;
                        using (new WaitCursorChanger(base.AppWorkspace))
                        {
                            ((BitmapLayer) activeDocumentWorkspace.ActiveLayer).Surface.Parallelize(7).Render<ColorBgra>(dst);
                        }
                        PdnRegion selection = activeDocumentWorkspace.Selection.CreateRegion();
                        EffectEnvironmentParameters parameters = new EffectEnvironmentParameters(base.AppWorkspace.AppEnvironment.PrimaryColor, base.AppWorkspace.AppEnvironment.SecondaryColor, base.AppWorkspace.AppEnvironment.PenInfo.Width, selection, dst);
                        effect = (Effect) Activator.CreateInstance(this.lastEffectType);
                        effect.EnvironmentParameters = parameters;
                        effect.Services = effects;
                        if (this.lastEffectToken == null)
                        {
                            token = null;
                        }
                        else
                        {
                            token = (EffectConfigToken) this.lastEffectToken.Clone();
                        }
                        this.DoEffect(effect, token, selection, selection, dst, out exception);
                    }
                    finally
                    {
                        activeDocumentWorkspace.ReturnScratchSurface(dst);
                    }
                }
            }
            if (exception != null)
            {
                this.HandleEffectException(base.AppWorkspace, effect, exception);
            }
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
        }

        public void RunEffect(System.Type effectType)
        {
            ThreadPriority priority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            try
            {
                this.RunEffectImpl(effectType);
            }
            finally
            {
                Thread.CurrentThread.Priority = priority;
            }
        }

        private void RunEffectImpl(System.Type effectType)
        {
            PdnRegion region;
            Action f = null;
            bool dirty = base.AppWorkspace.ActiveDocumentWorkspace.Document.Dirty;
            bool flag2 = false;
            base.AppWorkspace.Update();
            base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
            DocumentWorkspace activeDocumentWorkspace = base.AppWorkspace.ActiveDocumentWorkspace;
            if (activeDocumentWorkspace.Selection.IsEmpty)
            {
                region = new PdnRegion(activeDocumentWorkspace.Document.Bounds);
            }
            else
            {
                region = activeDocumentWorkspace.Selection.CreateRegion();
            }
            Exception exception = null;
            Effect effect = null;
            BitmapLayer activeLayer = (BitmapLayer) activeDocumentWorkspace.ActiveLayer;
            ThreadDispatcher backThread = new ThreadDispatcher();
            using (new PushNullToolMode(activeDocumentWorkspace))
            {
                try
                {
                    effect = (Effect) Activator.CreateInstance(effectType);
                    effect.Services = new ServiceProviderForEffects();
                    string name = effect.Name;
                    EffectConfigToken token = null;
                    if (!effect.CheckForEffectFlags(EffectFlags.Configurable))
                    {
                        Surface dst = activeDocumentWorkspace.BorrowScratchSurface(base.GetType() + ".RunEffect() using scratch surface for non-configurable rendering");
                        try
                        {
                            using (new WaitCursorChanger(base.AppWorkspace))
                            {
                                activeLayer.Surface.Parallelize(7).Render<ColorBgra>(dst);
                            }
                            EffectEnvironmentParameters parameters = new EffectEnvironmentParameters(base.AppWorkspace.AppEnvironment.PrimaryColor, base.AppWorkspace.AppEnvironment.SecondaryColor, base.AppWorkspace.AppEnvironment.PenInfo.Width, region, dst);
                            effect.EnvironmentParameters = parameters;
                            this.DoEffect(effect, null, region, region, dst, out exception);
                        }
                        finally
                        {
                            activeDocumentWorkspace.ReturnScratchSurface(dst);
                        }
                    }
                    else
                    {
                        PdnRegion renderRegion = region.Clone();
                        renderRegion.Intersect(Rect.Inflate(activeDocumentWorkspace.VisibleDocumentRect, 1.0, 1.0).ToGdipRectangleF());
                        Surface surface2 = activeDocumentWorkspace.BorrowScratchSurface(base.GetType() + ".RunEffect() using scratch surface for rendering during configuration");
                        try
                        {
                            using (new WaitCursorChanger(base.AppWorkspace))
                            {
                                activeLayer.Surface.Parallelize(7).Render<ColorBgra>(surface2);
                            }
                            EffectEnvironmentParameters parameters2 = new EffectEnvironmentParameters(base.AppWorkspace.AppEnvironment.PrimaryColor, base.AppWorkspace.AppEnvironment.SecondaryColor, base.AppWorkspace.AppEnvironment.PenInfo.Width, region, surface2);
                            effect.EnvironmentParameters = parameters2;
                            IDisposable disposable = base.AppWorkspace.SuspendThumbnailUpdates();
                            long asyncVersion = 0L;
                            using (EffectConfigDialog configDialog = effect.CreateConfigDialog())
                            {
                                DialogResult none;
                                <>c__DisplayClass1a classa;
                                <>c__DisplayClass18 class3;
                                <>c__DisplayClass16 class4;
                                EventHandler handler2 = null;
                                configDialog.Effect = effect;
                                configDialog.EffectSourceSurface = surface2;
                                configDialog.Selection = region;
                                BackgroundEffectRenderer ber = null;
                                EventHandler handler = delegate (object sender, EventArgs e) {
                                    EffectConfigDialog ecf = (EffectConfigDialog) sender;
                                    if (ber != null)
                                    {
                                        <>c__DisplayClass1a classa1 = classa;
                                        <>c__DisplayClass18 class1 = class3;
                                        <>c__DisplayClass16 class2 = class4;
                                        this.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBarAsync();
                                        asyncVersion += 1L;
                                        long ourRunVersion = asyncVersion;
                                        backThread.BeginTry(delegate {
                                            try
                                            {
                                                if (ourRunVersion == class1.asyncVersion)
                                                {
                                                    ber.Start();
                                                }
                                            }
                                            catch (Exception exception1)
                                            {
                                                class2.exception = exception1;
                                                classa1.configDialog.BeginInvoke(new Action(ecf.Close));
                                            }
                                        });
                                    }
                                };
                                configDialog.EffectTokenChanged += handler;
                                if (this.effectTokens.ContainsKey(effectType))
                                {
                                    EffectConfigToken token2 = (EffectConfigToken) this.effectTokens[effectType].Clone();
                                    configDialog.EffectToken = token2;
                                }
                                ber = new BackgroundEffectRenderer(effect, configDialog.EffectToken, new RenderArgs(activeLayer.Surface), new RenderArgs(surface2), renderRegion, 0x4b * this.renderingThreadCount, this.renderingThreadCount);
                                ber.RenderedTile += new RenderedTileEventHandler(this.RenderedTileHandler);
                                ber.StartingRendering += new EventHandler(this.StartingRenderingHandler);
                                ber.FinishedRendering += new EventHandler(this.FinishedRenderingHandler);
                                this.showProgressInStatusBar = true;
                                this.invalidateTimer.Enabled = true;
                                try
                                {
                                    if (handler2 == null)
                                    {
                                        handler2 = (EventHandler) ((s, e) => backThread.BeginTry(() => ber.Start()));
                                    }
                                    configDialog.Shown += handler2;
                                    try
                                    {
                                        none = configDialog.ShowDialog(base.AppWorkspace);
                                    }
                                    catch (Exception exception)
                                    {
                                        none = DialogResult.None;
                                        exception = exception;
                                    }
                                    configDialog.EffectTokenChanged -= handler;
                                    asyncVersion += 1L;
                                }
                                finally
                                {
                                    this.invalidateTimer.Enabled = false;
                                }
                                this.InvalidateTimer_Tick(this.invalidateTimer, EventArgs.Empty);
                                if (none == DialogResult.OK)
                                {
                                    this.effectTokens[effectType] = (EffectConfigToken) configDialog.EffectToken.Clone();
                                }
                                using (new WaitCursorChanger(base.AppWorkspace))
                                {
                                    Action action = null;
                                    using (ManualResetEvent stopDone = new ManualResetEvent(false))
                                    {
                                        WaitWithUIType cancelling;
                                        if (action == null)
                                        {
                                            action = delegate {
                                                try
                                                {
                                                    ber.Abort();
                                                    ber.Join();
                                                    ber.Dispose();
                                                    ber = null;
                                                }
                                                catch (Exception exception1)
                                                {
                                                    exception = exception1;
                                                }
                                                finally
                                                {
                                                    stopDone.Set();
                                                }
                                            };
                                        }
                                        backThread.BeginTry(action);
                                        if (none == DialogResult.Cancel)
                                        {
                                            cancelling = WaitWithUIType.Cancelling;
                                        }
                                        else
                                        {
                                            cancelling = WaitWithUIType.Finishing;
                                        }
                                        this.WaitWithUI(base.AppWorkspace, effect, cancelling, stopDone);
                                    }
                                    try
                                    {
                                        if (surface2.Scan0.MaySetAllowWrites)
                                        {
                                            surface2.Scan0.AllowWrites = true;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    if (none != DialogResult.OK)
                                    {
                                        Layer layer2 = activeDocumentWorkspace.ActiveLayer;
                                        BitmapLayer layer3 = (BitmapLayer) layer2;
                                        Surface surface = layer3.Surface;
                                        surface2.Parallelize(7).Render<ColorBgra>(surface);
                                        layer2.Invalidate();
                                    }
                                    configDialog.EffectTokenChanged -= handler;
                                    configDialog.Hide();
                                    base.AppWorkspace.Update();
                                    renderRegion.Dispose();
                                }
                                disposable.Dispose();
                                disposable = null;
                                if (none == DialogResult.OK)
                                {
                                    PdnRegion regionToRender = region.Clone();
                                    PdnRegion roi = PdnRegion.CreateEmpty();
                                    for (int i = 0; i < this.progressRegions.Length; i++)
                                    {
                                        if (this.progressRegions[i] == null)
                                        {
                                            break;
                                        }
                                        regionToRender.Exclude(this.progressRegions[i]);
                                        roi.Union(this.progressRegions[i]);
                                    }
                                    activeDocumentWorkspace.ActiveLayer.Invalidate(roi);
                                    token = (EffectConfigToken) configDialog.EffectToken.Clone();
                                    base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                                    this.DoEffect(effect, token, region, regionToRender, surface2, out exception);
                                }
                                else
                                {
                                    using (new WaitCursorChanger(base.AppWorkspace))
                                    {
                                        activeDocumentWorkspace.ActiveLayer.Invalidate();
                                        Utility.GCFullCollect();
                                    }
                                    flag2 = true;
                                    return;
                                }
                            }
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                        }
                        finally
                        {
                            activeDocumentWorkspace.ReturnScratchSurface(surface2);
                        }
                    }
                    if (effect.Category == EffectCategory.Effect)
                    {
                        if (this.lastEffectImage != null)
                        {
                            this.lastEffectImage.Dispose();
                            this.lastEffectImage = null;
                        }
                        this.lastEffectType = effect.GetType();
                        this.lastEffectName = effect.Name;
                        this.lastEffectImage = (effect.Image == null) ? null : effect.Image.CloneT<Image>();
                        if (token == null)
                        {
                            this.lastEffectToken = null;
                        }
                        else
                        {
                            this.lastEffectToken = (EffectConfigToken) token.Clone();
                        }
                        this.PopulateMenu(true);
                    }
                }
                catch (Exception exception3)
                {
                    exception = exception3;
                }
                finally
                {
                    region.Dispose();
                    base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBar();
                    base.AppWorkspace.Widgets.StatusBarProgress.EraseProgressStatusBar();
                    if (this.progressRegions != null)
                    {
                        for (int j = 0; j < this.progressRegions.Length; j++)
                        {
                            if (this.progressRegions[j] != null)
                            {
                                this.progressRegions[j].Dispose();
                                this.progressRegions[j] = null;
                            }
                        }
                    }
                    if (flag2)
                    {
                        base.AppWorkspace.ActiveDocumentWorkspace.Document.Dirty = dirty;
                    }
                    if (exception != null)
                    {
                        this.HandleEffectException(base.AppWorkspace, effect, exception);
                    }
                    if (f == null)
                    {
                        f = () => DisposableUtil.Free<ThreadDispatcher>(ref backThread);
                    }
                    backThread.BeginTry(f).Observe();
                    if (effect != null)
                    {
                        effect.Dispose();
                        effect = null;
                    }
                }
            }
        }

        private void StartingRenderingHandler(object sender, EventArgs e)
        {
            if (base.AppWorkspace.InvokeRequired)
            {
                base.AppWorkspace.BeginInvoke(new EventHandler(this.StartingRenderingHandler), new object[] { sender, e });
            }
            else
            {
                base.AppWorkspace.Widgets.StatusBarProgress.ResetProgressStatusBarAsync();
                if (this.progressRegions == null)
                {
                    this.progressRegions = new PdnRegion[0x4b * this.renderingThreadCount];
                }
                lock (this.progressRegions)
                {
                    for (int i = 0; i < this.progressRegions.Length; i++)
                    {
                        this.progressRegions[i] = null;
                    }
                    this.progressRegionsStartIndex = 0;
                }
            }
        }

        private void WaitWithUI(IWin32Window owner, Effect effect, WaitWithUIType waitType, WaitHandle doneSignal)
        {
            VirtualTask<Unit> vTask;
            if (!doneSignal.WaitOne(0, false))
            {
                TaskProgressDialog dialog = new TaskProgressDialog {
                    CloseOnFinished = true,
                    ShowCancelButton = false
                };
                switch (waitType)
                {
                    case WaitWithUIType.Cancelling:
                        dialog.HeaderText = PdnResources.GetString2("TaskProgressDialog.Canceling.Text");
                        break;

                    default:
                        dialog.HeaderText = PdnResources.GetString2("SaveConfigDialog.Finishing.Text");
                        break;
                }
                dialog.Text = effect.Name;
                dialog.Icon = null;
                if (effect.Image != null)
                {
                    Icon icon;
                    try
                    {
                        icon = Utility.ImageToIcon(effect.Image, false);
                    }
                    catch (Exception)
                    {
                        icon = null;
                    }
                    if (icon != null)
                    {
                        dialog.Icon = icon;
                    }
                }
                vTask = TaskManager.Global.CreateVirtualTask();
                dialog.Task = vTask;
                vTask.SetState(TaskState.Running);
                ThreadPool.QueueUserWorkItem(delegate {
                    try
                    {
                        doneSignal.WaitOne();
                    }
                    finally
                    {
                        vTask.SetState(TaskState.Finished);
                    }
                });
                dialog.ShowDialog(owner);
            }
        }

        public EffectsCollection Effects
        {
            get
            {
                if (this.effects == null)
                {
                    this.effects = GatherEffects();
                }
                return this.effects;
            }
        }

        protected abstract bool EnableEffectShortcuts { get; }

        protected abstract bool EnableRepeatEffectMenuItem { get; }

        private enum WaitWithUIType
        {
            Cancelling,
            Finishing
        }
    }
}

