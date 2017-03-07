namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal abstract class Tool : IDispatcherObject, IDisposable, IHotKeyTarget, IDisposedEvent, IFormAssociate
    {
        private bool active;
        protected bool autoScroll = true;
        private System.Windows.Forms.Cursor cursor;
        private const char decPenSizeBy5Shortcut = '\x001b';
        private const char decPenSizeShortcut = '[';
        public static readonly System.Type DefaultToolType = typeof(PaintBrushTool);
        private PaintDotNet.Controls.DocumentWorkspace documentWorkspace;
        protected System.Windows.Forms.Cursor handCursor;
        protected System.Windows.Forms.Cursor handCursorInvalid;
        protected System.Windows.Forms.Cursor handCursorMouseDown;
        private int ignoreMouseMove;
        private const char incPenSizeBy5Shortcut = '\x001d';
        private const char incPenSizeShortcut = ']';
        private int keyboardMoveRepeats;
        private int keyboardMoveSpeed = 1;
        private Dictionary<Keys, KeyTimeInfo> keysThatAreDown = new Dictionary<Keys, KeyTimeInfo>();
        private MouseButtons lastButton;
        private Keys lastKey;
        private DateTime lastKeyboardMove = DateTime.MinValue;
        private Int32Point lastMouseXY;
        private Int32Point lastPanMouseXY;
        private static DateTime lastToolSwitch = DateTime.MinValue;
        private int mouseDown;
        private int mouseEnter;
        private bool panMode;
        private System.Windows.Forms.Cursor panOldCursor;
        private bool panTracking;
        private int pulseCounter;
        private BitVector2D savedTiles;
        private GeometryList saveRegion;
        private const int saveTileGranularity = 0x20;
        private Surface scratchSurface;
        private const char swapColorsShortcut = 'x';
        private const char swapPrimarySecondaryChoice = 'c';
        private ImageResource toolBarImage;
        private ToolInfo toolInfo;
        private static readonly TimeSpan toolSwitchReset = new TimeSpan(0, 0, 0, 2, 0);
        private char[] wildShortcuts = new char[] { ',', '.', '/' };

        public event EventHandler CursorChanged;

        public event EventHandler CursorChanging;

        public event EventHandler Disposed;

        public Tool(PaintDotNet.Controls.DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, bool skipIfActiveOnHotKey, PaintDotNet.ToolBarConfigItems toolBarConfigItems)
        {
            this.documentWorkspace = documentWorkspace;
            this.toolBarImage = toolBarImage;
            this.toolInfo = new ToolInfo(name, helpText, toolBarImage, hotKey, skipIfActiveOnHotKey, toolBarConfigItems, base.GetType());
            if (this.documentWorkspace != null)
            {
                this.documentWorkspace.UpdateStatusBarToToolHelpText(this);
            }
        }

        private void Activate()
        {
            this.active = true;
            this.handCursor = PdnResources.GetCursor2("Cursors.PanToolCursor.cur");
            this.handCursorMouseDown = PdnResources.GetCursor2("Cursors.PanToolCursorMouseDown.cur");
            this.handCursorInvalid = PdnResources.GetCursor2("Cursors.PanToolCursorInvalid.cur");
            this.panTracking = false;
            this.panMode = false;
            this.mouseDown = 0;
            this.savedTiles = null;
            this.saveRegion = null;
            this.scratchSurface = this.DocumentWorkspace.BorrowScratchSurface(base.GetType().Name + ": Tool.Activate()");
            this.Selection.Changing += new EventHandler(this.SelectionChangingHandler);
            this.Selection.Changed += new EventHandler(this.SelectionChangedHandler);
            this.HistoryStack.ExecutingHistoryMemento += new ExecutingHistoryMementoEventHandler(this.ExecutingHistoryMemento);
            this.HistoryStack.ExecutedHistoryMemento += new ExecutedHistoryMementoEventHandler(this.ExecutedHistoryMemento);
            this.HistoryStack.FinishedStepGroup += new EventHandler(this.FinishedHistoryStepGroup);
            this.OnActivate();
        }

        private bool CanPan()
        {
            if (this.DocumentWorkspace.VisibleDocumentRect.Int32Bound().IntersectCopy(this.Document.Bounds()) == this.Document.Bounds())
            {
                return false;
            }
            return true;
        }

        public void ClearSavedMemory()
        {
            this.savedTiles = null;
        }

        public void ClearSavedRegion()
        {
            DisposableUtil.Free<GeometryList>(ref this.saveRegion);
        }

        private void Click()
        {
            this.OnClick();
        }

        private void Deactivate()
        {
            this.active = false;
            this.Selection.Changing -= new EventHandler(this.SelectionChangingHandler);
            this.Selection.Changed -= new EventHandler(this.SelectionChangedHandler);
            this.HistoryStack.ExecutingHistoryMemento -= new ExecutingHistoryMementoEventHandler(this.ExecutingHistoryMemento);
            this.HistoryStack.ExecutedHistoryMemento -= new ExecutedHistoryMementoEventHandler(this.ExecutedHistoryMemento);
            this.HistoryStack.FinishedStepGroup -= new EventHandler(this.FinishedHistoryStepGroup);
            this.OnDeactivate();
            this.DocumentWorkspace.ReturnScratchSurface(this.scratchSurface);
            this.scratchSurface = null;
            if (this.saveRegion != null)
            {
                this.saveRegion.Dispose();
                this.saveRegion = null;
            }
            this.savedTiles = null;
            if (this.handCursor != null)
            {
                this.handCursor.Dispose();
                this.handCursor = null;
            }
            if (this.handCursorMouseDown != null)
            {
                this.handCursorMouseDown.Dispose();
                this.handCursorMouseDown = null;
            }
            if (this.handCursorInvalid != null)
            {
                this.handCursorInvalid.Dispose();
                this.handCursorInvalid = null;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.saveRegion != null)
                {
                    this.saveRegion.Dispose();
                    this.saveRegion = null;
                }
                this.OnDisposed();
            }
        }

        private void ExecutedHistoryMemento(object sender, ExecutedHistoryMementoEventArgs e)
        {
            this.OnExecutedHistoryMemento(e);
        }

        private void ExecutingHistoryMemento(object sender, ExecutingHistoryMementoEventArgs e)
        {
            this.OnExecutingHistoryMemento(e);
        }

        ~Tool()
        {
            this.Dispose(false);
        }

        private void FinishedHistoryStepGroup(object sender, EventArgs e)
        {
            this.OnFinishedHistoryStepGroup();
        }

        protected object GetStaticData() => 
            this.DocumentWorkspace.GetStaticToolData(base.GetType());

        private bool IsOverflow(MouseEventArgsF e)
        {
            System.Windows.Point point = this.DocumentWorkspace.DocumentToClient(new System.Windows.Point((double) e.X, (double) e.Y));
            if (point.X >= -16384.0)
            {
                return (point.Y < -16384.0);
            }
            return true;
        }

        private void KeyDown(KeyEventArgs e)
        {
            this.OnKeyDown(e);
        }

        private void KeyPress(KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        private void KeyPress(Keys key)
        {
            this.OnKeyPress(key);
        }

        private void KeyUp(KeyEventArgs e)
        {
            if (this.panMode)
            {
                this.panMode = false;
                this.panTracking = false;
                this.Cursor = this.panOldCursor;
                this.panOldCursor = null;
                e.Handled = true;
            }
            this.OnKeyUp(e);
        }

        private void MouseDown(MouseEventArgsF e)
        {
            this.mouseDown++;
            if (this.panMode)
            {
                this.panTracking = true;
                this.lastPanMouseXY = new System.Drawing.Point(e.X, e.Y);
                if (this.CanPan())
                {
                    this.Cursor = this.handCursorMouseDown;
                }
            }
            else
            {
                this.OnMouseDown(e);
            }
            this.lastMouseXY = new System.Drawing.Point(e.X, e.Y);
        }

        private void MouseEnter()
        {
            this.mouseEnter++;
            if (this.mouseEnter == 1)
            {
                this.OnMouseEnter();
            }
        }

        private void MouseLeave()
        {
            if (this.mouseEnter == 1)
            {
                this.mouseEnter = 0;
                this.OnMouseLeave();
            }
            else
            {
                this.mouseEnter = Math.Max(0, this.mouseEnter - 1);
            }
        }

        private void MouseMove(MouseEventArgsF e)
        {
            if (this.ignoreMouseMove > 0)
            {
                this.ignoreMouseMove--;
            }
            else if (this.panTracking && (e.Button == MouseButtons.Left))
            {
                new Int32Point(e.X, e.Y);
                this.DocumentWorkspace.VisibleDocumentRect.Center();
                System.Windows.Point point = new System.Windows.Point((double) (e.X - this.lastPanMouseXY.X), (double) (e.Y - this.lastPanMouseXY.Y));
                System.Windows.Point documentScrollPosition = this.DocumentWorkspace.DocumentScrollPosition;
                if ((point.X != 0.0) || (point.Y != 0.0))
                {
                    documentScrollPosition.X -= point.X;
                    documentScrollPosition.Y -= point.Y;
                    this.lastPanMouseXY = new Int32Point(e.X, e.Y);
                    this.lastPanMouseXY.X -= (int) Math.Truncate(point.X);
                    this.lastPanMouseXY.Y -= (int) Math.Truncate(point.Y);
                    this.ignoreMouseMove++;
                    this.DocumentWorkspace.DocumentScrollPosition = documentScrollPosition;
                    this.Update();
                }
            }
            else if (!this.panMode)
            {
                this.OnMouseMove(e);
            }
            this.lastMouseXY = new System.Drawing.Point(e.X, e.Y);
            this.lastButton = e.Button;
        }

        private void MouseUp(MouseEventArgsF e)
        {
            this.mouseDown--;
            if (!this.panMode)
            {
                this.OnMouseUp(e);
            }
            this.lastMouseXY = new Int32Point(e.X, e.Y);
        }

        protected virtual void OnActivate()
        {
        }

        protected virtual void OnClick()
        {
        }

        protected virtual void OnCursorChanged()
        {
            if (this.CursorChanged != null)
            {
                this.CursorChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnCursorChanging()
        {
            if (this.CursorChanging != null)
            {
                this.CursorChanging(this, EventArgs.Empty);
            }
        }

        protected virtual void OnDeactivate()
        {
        }

        private void OnDisposed()
        {
            if (this.Disposed != null)
            {
                this.Disposed(this, EventArgs.Empty);
            }
        }

        protected virtual void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
        }

        protected virtual void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
        }

        protected virtual void OnFinishedHistoryStepGroup()
        {
        }

        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (!this.keysThatAreDown.ContainsKey(e.KeyData))
                {
                    this.keysThatAreDown.Add(e.KeyData, new KeyTimeInfo());
                }
                if ((!this.IsMouseDown && !this.panMode) && (e.KeyCode == Keys.Space))
                {
                    this.panMode = true;
                    this.panOldCursor = this.Cursor;
                    if (this.CanPan())
                    {
                        this.Cursor = this.handCursor;
                    }
                    else
                    {
                        this.Cursor = this.handCursorInvalid;
                    }
                }
                this.OnKeyPress(e.KeyData);
            }
        }

        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            if (!e.Handled && this.DocumentWorkspace.Focused)
            {
                int index = Array.IndexOf<char>(this.wildShortcuts, e.KeyChar);
                if (index != -1)
                {
                    e.Handled = this.OnWildShortcutKey(index);
                }
                else if (e.KeyChar == 'x')
                {
                    this.AppWorkspace.Widgets.ColorsForm.SwapUserColors();
                    e.Handled = true;
                }
                else if (e.KeyChar == 'c')
                {
                    this.AppWorkspace.Widgets.ColorsForm.ToggleWhichUserColor();
                    e.Handled = true;
                }
                else if (e.KeyChar == '[')
                {
                    this.AppWorkspace.Widgets.ToolConfigStrip.AddToPenSize(-1f);
                    e.Handled = true;
                }
                else if ((e.KeyChar == '\x001b') && ((this.ModifierKeys & Keys.Control) != Keys.None))
                {
                    this.AppWorkspace.Widgets.ToolConfigStrip.AddToPenSize(-5f);
                    e.Handled = true;
                }
                else if (e.KeyChar == ']')
                {
                    this.AppWorkspace.Widgets.ToolConfigStrip.AddToPenSize(1f);
                    e.Handled = true;
                }
                else if ((e.KeyChar == '\x001d') && ((this.ModifierKeys & Keys.Control) != Keys.None))
                {
                    this.AppWorkspace.Widgets.ToolConfigStrip.AddToPenSize(5f);
                    e.Handled = true;
                }
                else
                {
                    ToolInfo[] toolInfos = PaintDotNet.Controls.DocumentWorkspace.ToolInfos;
                    System.Type toolType = this.DocumentWorkspace.GetToolType();
                    int num2 = 0;
                    if ((this.ModifierKeys & Keys.Shift) != Keys.None)
                    {
                        Array.Reverse(toolInfos);
                    }
                    if ((char.ToLower(this.HotKey) != char.ToLower(e.KeyChar)) || ((DateTime.Now - lastToolSwitch) > toolSwitchReset))
                    {
                        num2 = -1;
                    }
                    else
                    {
                        for (int j = 0; j < toolInfos.Length; j++)
                        {
                            if (toolInfos[j].ToolType == toolType)
                            {
                                num2 = j;
                                break;
                            }
                        }
                    }
                    for (int i = 0; i < toolInfos.Length; i++)
                    {
                        int num5 = ((i + num2) + 1) % toolInfos.Length;
                        ToolInfo info = toolInfos[num5];
                        if (((info.ToolType != this.DocumentWorkspace.GetToolType()) || !info.SkipIfActiveOnHotKey) && (char.ToLower(info.HotKey) == char.ToLower(e.KeyChar)))
                        {
                            if (!this.IsMouseDown)
                            {
                                this.AppWorkspace.Widgets.ToolsControl.SelectTool(info.ToolType);
                            }
                            e.Handled = true;
                            lastToolSwitch = DateTime.Now;
                            break;
                        }
                    }
                    if (!e.Handled)
                    {
                        char keyChar = e.KeyChar;
                        if (((keyChar == '\r') || (keyChar == '\x001b')) && ((this.mouseDown == 0) && !this.Selection.IsEmpty))
                        {
                            e.Handled = true;
                            this.DocumentWorkspace.ExecuteFunction(new DeselectFunction());
                        }
                    }
                }
            }
        }

        protected virtual void OnKeyPress(Keys key)
        {
            System.Drawing.Point empty = System.Drawing.Point.Empty;
            if (key != this.lastKey)
            {
                this.lastKeyboardMove = DateTime.MinValue;
            }
            this.lastKey = key;
            switch (key)
            {
                case Keys.Left:
                    empty.X--;
                    break;

                case Keys.Up:
                    empty.Y--;
                    break;

                case Keys.Right:
                    empty.X++;
                    break;

                case Keys.Down:
                    empty.Y++;
                    break;
            }
            if (!empty.Equals(System.Drawing.Point.Empty))
            {
                long num = DateTime.Now.Ticks - this.lastKeyboardMove.Ticks;
                if ((num * 4L) > 0x989680L)
                {
                    this.keyboardMoveRepeats = 0;
                    this.keyboardMoveSpeed = 1;
                }
                else
                {
                    this.keyboardMoveRepeats++;
                    if ((this.keyboardMoveRepeats > 15) && ((this.keyboardMoveRepeats % 4) == 0))
                    {
                        this.keyboardMoveSpeed++;
                    }
                }
                this.lastKeyboardMove = DateTime.Now;
                int num2 = (int) (Math.Ceiling(this.DocumentWorkspace.ScaleFactor.Ratio) * this.keyboardMoveSpeed);
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X + (num2 * empty.X), System.Windows.Forms.Cursor.Position.Y + (num2 * empty.Y));
                System.Windows.Point point2 = this.DocumentWorkspace.PointToScreen(this.DocumentWorkspace.DocumentToClient(new System.Windows.Point(0.0, 0.0)));
                System.Windows.Point p = new System.Windows.Point(System.Windows.Forms.Cursor.Position.X - point2.X, System.Windows.Forms.Cursor.Position.Y - point2.Y);
                Int32Point point4 = Int32Point.Truncate(this.DocumentWorkspace.ScaleFactor.Unscale(p));
                this.DocumentWorkspace.PerformDocumentMouseMove(new MouseEventArgsF(this.lastButton, 1, (double) point4.X, (double) point4.Y, 0));
            }
        }

        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            this.keysThatAreDown.Clear();
        }

        protected virtual void OnMouseDown(MouseEventArgsF e)
        {
            this.lastButton = e.Button;
        }

        protected virtual void OnMouseEnter()
        {
        }

        protected virtual void OnMouseLeave()
        {
        }

        protected virtual void OnMouseMove(MouseEventArgsF e)
        {
            if (this.panMode || (this.mouseDown > 0))
            {
                this.ScrollIfNecessary(new System.Windows.Point((double) e.X, (double) e.Y));
            }
        }

        protected virtual void OnMouseUp(MouseEventArgsF e)
        {
            this.lastButton = e.Button;
        }

        protected virtual void OnPaste(IDataObject data, out bool handled)
        {
            handled = false;
        }

        protected virtual void OnPasteQuery(IDataObject data, out bool canHandle)
        {
            canHandle = false;
        }

        protected virtual void OnPulse()
        {
        }

        protected virtual void OnSelectionChanged()
        {
        }

        protected virtual void OnSelectionChanging()
        {
        }

        protected virtual bool OnWildShortcutKey(int ordinal) => 
            false;

        private void Paste(IDataObject data, out bool handled)
        {
            this.OnPaste(data, out handled);
        }

        private void PasteQuery(IDataObject data, out bool canHandle)
        {
            this.OnPasteQuery(data, out canHandle);
        }

        public void PerformActivate()
        {
            this.Activate();
        }

        public void PerformClick()
        {
            this.Click();
        }

        public void PerformDeactivate()
        {
            this.Deactivate();
        }

        public void PerformKeyDown(KeyEventArgs e)
        {
            this.KeyDown(e);
        }

        public void PerformKeyPress(KeyPressEventArgs e)
        {
            this.KeyPress(e);
        }

        public void PerformKeyPress(Keys key)
        {
            this.KeyPress(key);
        }

        public void PerformKeyUp(KeyEventArgs e)
        {
            this.KeyUp(e);
        }

        public void PerformMouseDown(MouseEventArgsF e)
        {
            if (!this.IsOverflow(e))
            {
                this.DocumentWorkspace.Focus();
                this.MouseDown(e);
            }
        }

        public void PerformMouseEnter()
        {
            this.MouseEnter();
        }

        public void PerformMouseLeave()
        {
            this.MouseLeave();
        }

        public void PerformMouseMove(MouseEventArgsF e)
        {
            if (!this.IsOverflow(e))
            {
                this.MouseMove(e);
            }
        }

        public void PerformMouseUp(MouseEventArgsF e)
        {
            if (!this.IsOverflow(e))
            {
                this.MouseUp(e);
            }
        }

        public void PerformPaste(IDataObject data, out bool handled)
        {
            this.Paste(data, out handled);
        }

        public void PerformPasteQuery(IDataObject data, out bool canHandle)
        {
            this.PasteQuery(data, out canHandle);
        }

        public void PerformPulse()
        {
            this.Pulse();
        }

        private void Pulse()
        {
            this.pulseCounter++;
            if (this.IsFormActive)
            {
                this.OnPulse();
            }
            else if ((this.pulseCounter % 4) == 0)
            {
                this.OnPulse();
            }
        }

        public void RestoreRegion(GeometryList region)
        {
            if (region != null)
            {
                BitmapLayer activeLayer = (BitmapLayer) this.ActiveLayer;
                Int32Rect[] interiorScans = region.GetInteriorScans();
                activeLayer.Surface.CopySurface(this.scratchSurface, interiorScans);
                activeLayer.Invalidate(region);
            }
        }

        public void RestoreSavedRegion()
        {
            if (this.saveRegion != null)
            {
                BitmapLayer activeLayer = (BitmapLayer) this.ActiveLayer;
                Int32Rect[] interiorScans = this.saveRegion.GetInteriorScans();
                activeLayer.Surface.CopySurface(this.ScratchSurface, interiorScans);
                activeLayer.Invalidate(this.saveRegion);
                DisposableUtil.Free<GeometryList>(ref this.saveRegion);
            }
        }

        public void SaveRegion(GeometryList saveMeGeometry, Int32Rect saveMeBounds)
        {
            Int32Rect rect;
            BitmapLayer activeLayer = (BitmapLayer) this.ActiveLayer;
            if (this.savedTiles == null)
            {
                this.savedTiles = new BitVector2D(((activeLayer.Width + 0x20) - 1) / 0x20, ((activeLayer.Height + 0x20) - 1) / 0x20);
                this.savedTiles.Clear(false);
            }
            if (saveMeGeometry == null)
            {
                rect = saveMeBounds;
            }
            else
            {
                rect = saveMeGeometry.Bounds.Int32Bound();
            }
            Int32Rect rect2 = activeLayer.Bounds();
            Int32Rect rect6 = rect.UnionCopy(saveMeBounds).IntersectCopy(rect2).CoalesceCopy();
            int num = rect6.Left() / 0x20;
            int num2 = rect6.Top() / 0x20;
            int num3 = (rect6.Right() - 1) / 0x20;
            int num4 = (rect6.Bottom() - 1) / 0x20;
            for (int i = num2; i <= num4; i++)
            {
                Int32Rect? nullable = null;
                for (int j = num; j <= num3; j++)
                {
                    if (!this.savedTiles.Get(j, i))
                    {
                        Int32Rect rect7 = new Int32Rect(j * 0x20, i * 0x20, 0x20, 0x20);
                        rect7 = Int32RectUtil.Intersect(rect7, activeLayer.Bounds());
                        if (nullable.HasValue)
                        {
                            nullable = new Int32Rect?(Int32RectUtil.Union(nullable.Value, rect7));
                        }
                        else
                        {
                            nullable = new Int32Rect?(rect7);
                        }
                        this.savedTiles.Set(j, i, true);
                    }
                    else if (nullable.HasValue)
                    {
                        using (ISurface<ColorBgra> surface = this.ScratchSurface.CreateWindow<ColorBgra>(nullable.Value))
                        {
                            using (ISurface<ColorBgra> surface2 = activeLayer.Surface.CreateWindow<ColorBgra>(nullable.Value))
                            {
                                surface2.Render<ColorBgra>(surface);
                            }
                        }
                        nullable = null;
                    }
                }
                if (nullable.HasValue)
                {
                    using (ISurface<ColorBgra> surface3 = this.ScratchSurface.CreateWindow<ColorBgra>(nullable.Value))
                    {
                        using (ISurface<ColorBgra> surface4 = activeLayer.Surface.CreateWindow<ColorBgra>(nullable.Value))
                        {
                            surface4.Render<ColorBgra>(surface3);
                        }
                    }
                    nullable = null;
                }
            }
            if (this.saveRegion != null)
            {
                this.saveRegion.Dispose();
                this.saveRegion = null;
            }
            if (saveMeGeometry != null)
            {
                this.saveRegion = saveMeGeometry.Clone();
            }
        }

        protected bool ScrollIfNecessary(System.Windows.Point position)
        {
            if (this.autoScroll && this.CanPan())
            {
                Rect visibleDocumentRect = this.DocumentWorkspace.VisibleDocumentRect;
                System.Windows.Point documentScrollPosition = this.DocumentWorkspace.DocumentScrollPosition;
                System.Windows.Point pt = new System.Windows.Point(0.0, 0.0);
                System.Windows.Point point3 = new System.Windows.Point(0.0, 0.0) {
                    X = DoubleUtil.Lerp((visibleDocumentRect.Left + visibleDocumentRect.Right) / 2.0, position.X, 1.02),
                    Y = DoubleUtil.Lerp((visibleDocumentRect.Top + visibleDocumentRect.Bottom) / 2.0, position.Y, 1.02)
                };
                if (point3.X < visibleDocumentRect.Left)
                {
                    pt.X = point3.X - visibleDocumentRect.Left;
                }
                else if (point3.X > visibleDocumentRect.Right)
                {
                    pt.X = point3.X - visibleDocumentRect.Right;
                }
                if (point3.Y < visibleDocumentRect.Top)
                {
                    pt.Y = point3.Y - visibleDocumentRect.Top;
                }
                else if (point3.Y > visibleDocumentRect.Bottom)
                {
                    pt.Y = point3.Y - visibleDocumentRect.Bottom;
                }
                if (!pt.IsCloseToZero())
                {
                    System.Windows.Point point4 = new System.Windows.Point(documentScrollPosition.X + pt.X, documentScrollPosition.Y + pt.Y);
                    this.DocumentWorkspace.DocumentScrollPosition = point4;
                    this.Update();
                    return true;
                }
            }
            return false;
        }

        private void SelectionChanged()
        {
            this.OnSelectionChanged();
        }

        private void SelectionChangedHandler(object sender, EventArgs e)
        {
            this.OnSelectionChanged();
        }

        private void SelectionChanging()
        {
            this.OnSelectionChanging();
        }

        private void SelectionChangingHandler(object sender, EventArgs e)
        {
            this.OnSelectionChanging();
        }

        protected void SetStaticData(object data)
        {
            this.DocumentWorkspace.SetStaticToolData(base.GetType(), data);
        }

        protected void SetStatus(ImageResource statusIcon, string statusText)
        {
            if ((statusIcon == null) && (statusText != null))
            {
                statusIcon = PdnResources.GetImageResource2("Icons.MenuHelpHelpTopicsIcon.png");
            }
            this.DocumentWorkspace.SetStatus(statusText, statusIcon);
        }

        protected System.Windows.Point SnapPoint(System.Windows.Point canvasPoint) => 
            ((System.Windows.Point) Int32Point.Truncate(canvasPoint));

        protected System.Windows.Point[] SnapPoints(IList<System.Windows.Point> canvasPoints)
        {
            System.Windows.Point[] pointArray = new System.Windows.Point[canvasPoints.Count];
            for (int i = 0; i < pointArray.Length; i++)
            {
                pointArray[i] = this.SnapPoint(canvasPoints[i]);
            }
            return pointArray;
        }

        protected void Update()
        {
            this.DocumentWorkspace.Update();
        }

        public bool Active =>
            this.active;

        protected Layer ActiveLayer =>
            this.DocumentWorkspace.ActiveLayer;

        protected int ActiveLayerIndex
        {
            get => 
                this.DocumentWorkspace.ActiveLayerIndex;
            set
            {
                this.DocumentWorkspace.ActiveLayerIndex = value;
            }
        }

        protected PaintDotNet.AppEnvironment AppEnvironment =>
            this.documentWorkspace.AppWorkspace.AppEnvironment;

        public PaintDotNet.Controls.AppWorkspace AppWorkspace =>
            this.DocumentWorkspace.AppWorkspace;

        public Form AssociatedForm =>
            this.AppWorkspace.FindForm();

        protected PaintDotNet.Canvas.CanvasRenderer CanvasRenderer =>
            this.DocumentWorkspace.CanvasRenderer;

        public System.Windows.Forms.Cursor Cursor
        {
            get => 
                this.cursor;
            set
            {
                this.OnCursorChanging();
                this.cursor = value;
                this.OnCursorChanged();
            }
        }

        public virtual bool DeactivateOnLayerChange =>
            true;

        public IDispatcher Dispatcher =>
            this.documentWorkspace.Dispatcher;

        protected PaintDotNet.Document Document =>
            this.DocumentWorkspace.Document;

        public PaintDotNet.Controls.DocumentWorkspace DocumentWorkspace =>
            this.documentWorkspace;

        public bool Focused =>
            this.DocumentWorkspace.Focused;

        public string HelpText =>
            this.toolInfo.HelpText;

        protected PaintDotNet.HistoryStack HistoryStack =>
            this.DocumentWorkspace.History;

        public char HotKey =>
            this.toolInfo.HotKey;

        public ImageResource Image =>
            this.toolBarImage;

        public ToolInfo Info =>
            this.toolInfo;

        protected bool IsFormActive =>
            object.ReferenceEquals(Form.ActiveForm, this.DocumentWorkspace.FindForm());

        public bool IsMouseDown =>
            (this.mouseDown > 0);

        public bool IsMouseEntered =>
            (this.mouseEnter > 0);

        public Keys ModifierKeys =>
            Control.ModifierKeys;

        public string Name =>
            this.toolInfo.Name;

        protected Surface ScratchSurface =>
            this.scratchSurface;

        protected PaintDotNet.Selection Selection =>
            this.DocumentWorkspace.Selection;

        public PaintDotNet.ToolBarConfigItems ToolBarConfigItems =>
            this.toolInfo.ToolBarConfigItems;

        private sealed class KeyTimeInfo
        {
            public DateTime KeyDownTime = DateTime.Now;
            public DateTime LastKeyPressPulse;
            private int repeats;

            public KeyTimeInfo()
            {
                this.LastKeyPressPulse = this.KeyDownTime;
            }

            public int Repeats
            {
                get => 
                    this.repeats;
                set
                {
                    this.repeats = value;
                }
            }
        }
    }
}

