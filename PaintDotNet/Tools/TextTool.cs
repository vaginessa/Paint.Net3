namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Threading;
    using PaintDotNet.Typography;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class TextTool : PaintDotNet.Tools.Tool
    {
        private TextAlignment alignment;
        private EventHandler alignmentChangedDelegate;
        private EventHandler antiAliasChangedDelegate;
        private EventHandler brushChangedDelegate;
        private Int32Point clickPoint;
        private bool controlKeyDown;
        private readonly TimeSpan controlKeyDownThreshold;
        private DateTime controlKeyDownTime;
        private CompoundHistoryMemento currentHA;
        private const int cursorInterval = 300;
        private bool enableNub;
        private PaintDotNet.Typography.Font font;
        private EventHandler fontChangedDelegate;
        private IFontRenderer fontRenderer;
        private EventHandler fontSmoothingChangedDelegate;
        private EventHandler foreColorChangedDelegate;
        private int ignoreRedraw;
        private bool lastPulseCursorState;
        private int linePos;
        private List<string> lines;
        private int managedThreadId;
        private EditingMode mode;
        private MoveNubRenderer moveNub;
        private bool pulseEnabled;
        private GeometryList savedRegion;
        private Int32Point startClickPoint;
        private Int32Point startMouseXY;
        private DateTime startTime;
        private string statusBarTextFormat;
        private Int32Point[] textPoints;
        private int textPos;
        private TextSize[] textSizes;
        private Cursor textToolCursor;
        private PrivateThreadPool threadPool;
        private bool tracking;
        private ITypographyDriver typographyDriver;
        private ITypographyService typographyService;

        public TextTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.TextToolIcon.png"), PdnResources.GetString2("TextTool.Name"), PdnResources.GetString2("TextTool.HelpText"), 't', false, ToolBarConfigItems.AlphaBlending | ToolBarConfigItems.Antialiasing | ToolBarConfigItems.Brush | ToolBarConfigItems.Text)
        {
            this.statusBarTextFormat = PdnResources.GetString2("TextTool.StatusText.TextInfo.Format");
            this.enableNub = true;
            this.controlKeyDownTime = DateTime.MinValue;
            this.controlKeyDownThreshold = new TimeSpan(0, 0, 0, 0, 400);
            this.managedThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private void AlignmentChangedHandler(object sender, EventArgs a)
        {
            this.alignment = base.AppEnvironment.TextAlignment;
            if (this.mode != EditingMode.NotEditing)
            {
                this.textSizes = null;
                this.RedrawText(true);
            }
        }

        private void AlphaBlendingChangedHandler(object sender, EventArgs e)
        {
            if (this.mode != EditingMode.NotEditing)
            {
                this.RedrawText(true);
            }
        }

        private void AntiAliasChangedHandler(object sender, EventArgs a)
        {
            this.RefreshFontRenderer();
            if (this.mode != EditingMode.NotEditing)
            {
                this.textSizes = null;
                this.RedrawText(true);
            }
        }

        private void BackColorChangedHandler(object sender, EventArgs e)
        {
            if (this.mode != EditingMode.NotEditing)
            {
                this.RedrawText(true);
            }
        }

        private void BrushChangedHandler(object sender, EventArgs a)
        {
            if (this.mode != EditingMode.NotEditing)
            {
                this.RedrawText(true);
            }
        }

        private IFontRenderer CreateFontRenderer() => 
            this.typographyDriver.CreateFontRenderer(this.font, this.typographyDriver.CreateFontRendererSettings(TextDisplayIntent.Other, base.AppEnvironment.AntiAliasing ? TextRenderMode.Antialiased : TextRenderMode.Aliased));

        private int FindOffsetPosition(double offset, string line, int lno)
        {
            for (int i = 0; i < line.Length; i++)
            {
                double num2 = this.TextPositionToPoint(new Position(lno, i)).X - this.clickPoint.X;
                if (num2 >= offset)
                {
                    return i;
                }
            }
            return line.Length;
        }

        private void FontChangedHandler(object sender, EventArgs a)
        {
            this.font = base.AppEnvironment.FontInfo.CreateFont(this.typographyService);
            this.RefreshFontRenderer();
            if (this.mode != EditingMode.NotEditing)
            {
                this.textSizes = null;
                this.RedrawText(true);
            }
        }

        private void FontSmoothingChangedHandler(object sender, EventArgs e)
        {
            this.RefreshFontRenderer();
            if (this.mode != EditingMode.NotEditing)
            {
                this.textSizes = null;
                this.RedrawText(true);
            }
        }

        private void ForeColorChangedHandler(object sender, EventArgs e)
        {
            if (this.mode != EditingMode.NotEditing)
            {
                this.RedrawText(true);
            }
        }

        private string GetStatusBarXYText()
        {
            string str;
            string str2;
            string str3;
            base.Document.CoordinatesToStrings(base.AppWorkspace.Units, this.textPoints[0].X, this.textPoints[0].Y, out str2, out str3, out str);
            return string.Format(this.statusBarTextFormat, new object[] { str2, str, str3, str });
        }

        private void InsertCharIntoString(char c)
        {
            this.lines[this.linePos] = this.lines[this.linePos].Insert(this.textPos, c.ToString());
            this.textSizes = null;
        }

        protected override void OnActivate()
        {
            this.typographyService = new TypographyService();
            this.typographyDriver = this.typographyService.DefaultDriver;
            PdnBaseForm.RegisterFormHotKey(Keys.Back, new Func<Keys, bool>(this.OnBackspaceTyped));
            base.OnActivate();
            this.textToolCursor = PdnResources.GetCursor2("Cursors.TextToolCursor.cur");
            base.Cursor = this.textToolCursor;
            this.fontChangedDelegate = new EventHandler(this.FontChangedHandler);
            this.fontSmoothingChangedDelegate = new EventHandler(this.FontSmoothingChangedHandler);
            this.alignmentChangedDelegate = new EventHandler(this.AlignmentChangedHandler);
            this.brushChangedDelegate = new EventHandler(this.BrushChangedHandler);
            this.antiAliasChangedDelegate = new EventHandler(this.AntiAliasChangedHandler);
            this.foreColorChangedDelegate = new EventHandler(this.ForeColorChangedHandler);
            this.mode = EditingMode.NotEditing;
            if (this.fontRenderer != null)
            {
                this.fontRenderer.Dispose();
                this.fontRenderer = null;
            }
            this.font = base.AppEnvironment.FontInfo.CreateFont(this.typographyService);
            this.alignment = base.AppEnvironment.TextAlignment;
            base.AppEnvironment.BrushInfoChanged += this.brushChangedDelegate;
            base.AppEnvironment.FontInfoChanged += this.fontChangedDelegate;
            base.AppEnvironment.FontSmoothingChanged += this.fontSmoothingChangedDelegate;
            base.AppEnvironment.TextAlignmentChanged += this.alignmentChangedDelegate;
            base.AppEnvironment.AntiAliasingChanged += this.antiAliasChangedDelegate;
            base.AppEnvironment.PrimaryColorChanged += this.foreColorChangedDelegate;
            base.AppEnvironment.SecondaryColorChanged += new EventHandler(this.BackColorChangedHandler);
            base.AppEnvironment.AlphaBlendingChanged += new EventHandler(this.AlphaBlendingChangedHandler);
            this.threadPool = new PrivateThreadPool();
            this.moveNub = new MoveNubRenderer(base.CanvasRenderer);
            this.moveNub.Shape = MoveNubShape.Compass;
            this.moveNub.Size = new System.Windows.Size(10.0, 10.0);
            this.moveNub.Visible = false;
            base.CanvasRenderer.Add(this.moveNub, false);
        }

        private bool OnBackspaceTyped(Keys keys)
        {
            if (base.DocumentWorkspace.Visible && (this.mode != EditingMode.NotEditing))
            {
                this.OnKeyPress(Keys.Back);
                return true;
            }
            return false;
        }

        protected override void OnDeactivate()
        {
            PdnBaseForm.UnregisterFormHotKey(Keys.Back, new Func<Keys, bool>(this.OnBackspaceTyped));
            base.OnDeactivate();
            switch (this.mode)
            {
                case EditingMode.NotEditing:
                    break;

                case EditingMode.EmptyEdit:
                    this.RedrawText(false);
                    break;

                case EditingMode.Editing:
                    this.SaveHistoryMemento();
                    break;

                default:
                    throw new InvalidEnumArgumentException("Invalid Editing Mode");
            }
            if (this.savedRegion != null)
            {
                this.savedRegion.Dispose();
                this.savedRegion = null;
            }
            base.AppEnvironment.BrushInfoChanged -= this.brushChangedDelegate;
            base.AppEnvironment.FontInfoChanged -= this.fontChangedDelegate;
            base.AppEnvironment.FontSmoothingChanged -= this.fontSmoothingChangedDelegate;
            base.AppEnvironment.TextAlignmentChanged -= this.alignmentChangedDelegate;
            base.AppEnvironment.AntiAliasingChanged -= this.antiAliasChangedDelegate;
            base.AppEnvironment.PrimaryColorChanged -= this.foreColorChangedDelegate;
            base.AppEnvironment.SecondaryColorChanged -= new EventHandler(this.BackColorChangedHandler);
            base.AppEnvironment.AlphaBlendingChanged -= new EventHandler(this.AlphaBlendingChangedHandler);
            this.StopEditing();
            this.threadPool = null;
            base.CanvasRenderer.Remove(this.moveNub);
            this.moveNub.Dispose();
            this.moveNub = null;
            if (this.fontRenderer != null)
            {
                this.fontRenderer.Dispose();
                this.fontRenderer = null;
            }
            this.font = null;
            if (this.textToolCursor != null)
            {
                this.textToolCursor.Dispose();
                this.textToolCursor = null;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Back:
                case Keys.Delete:
                    if (this.mode != EditingMode.NotEditing)
                    {
                        this.OnKeyPress(e.KeyCode);
                        e.Handled = true;
                    }
                    break;

                case Keys.Tab:
                    if (((e.Modifiers & Keys.Control) == Keys.None) && (this.mode != EditingMode.NotEditing))
                    {
                        this.OnKeyPress(e.KeyCode);
                        e.Handled = true;
                    }
                    break;

                case Keys.ControlKey:
                    if (!this.controlKeyDown)
                    {
                        this.controlKeyDown = true;
                        this.controlKeyDownTime = DateTime.Now;
                    }
                    break;

                case Keys.Space:
                    if (this.mode != EditingMode.NotEditing)
                    {
                        e.Handled = true;
                    }
                    break;

                case Keys.PageUp:
                case Keys.Next:
                case Keys.End:
                case Keys.Home:
                case (Keys.Shift | Keys.PageUp):
                case (Keys.Shift | Keys.Next):
                case (Keys.Shift | Keys.End):
                case (Keys.Shift | Keys.Home):
                    if (this.mode != EditingMode.NotEditing)
                    {
                        this.OnKeyPress(e.KeyCode);
                        e.Handled = true;
                    }
                    break;
            }
            if (this.mode != EditingMode.NotEditing)
            {
                Int32Point location = Int32Point.Truncate(this.TextPositionToPoint(new Position(this.linePos, this.textPos)));
                int width = (int) Math.Round(this.textSizes[this.linePos].Height);
                Int32Rect rect = Int32RectUtil.From(location, new Int32Size(width, width));
                Int32Rect rect2 = base.DocumentWorkspace.VisibleDocumentRect.Int32Bound();
                if (rect2.IntersectCopy(rect) != rect)
                {
                    System.Windows.Point point2 = rect2.ToRect().Center();
                    if ((location.X > rect2.Right()) || (location.X < rect2.Left()))
                    {
                        point2.X = location.X;
                    }
                    if ((location.Y > rect2.Bottom()) || (location.Y < rect2.Top()))
                    {
                        point2.Y = location.Y;
                    }
                    base.DocumentWorkspace.DocumentCenterPoint = point2;
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '\r':
                    if (this.tracking)
                    {
                        e.Handled = true;
                    }
                    break;

                case '\x001b':
                    if (this.tracking)
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        if (this.mode == EditingMode.Editing)
                        {
                            this.SaveHistoryMemento();
                        }
                        else if (this.mode == EditingMode.EmptyEdit)
                        {
                            this.RedrawText(false);
                        }
                        if (this.mode != EditingMode.NotEditing)
                        {
                            e.Handled = true;
                            this.StopEditing();
                        }
                    }
                    break;
            }
            if ((!e.Handled && (this.mode != EditingMode.NotEditing)) && !this.tracking)
            {
                e.Handled = true;
                if (this.mode == EditingMode.EmptyEdit)
                {
                    this.mode = EditingMode.Editing;
                    CompoundHistoryMemento memento = new CompoundHistoryMemento(base.Name, base.Image, new List<HistoryMemento>());
                    this.currentHA = memento;
                    base.HistoryStack.PushNewMemento(memento);
                }
                if (!char.IsControl(e.KeyChar))
                {
                    this.InsertCharIntoString(e.KeyChar);
                    this.textPos++;
                    this.RedrawText(true);
                }
            }
            base.OnKeyPress(e);
        }

        protected override void OnKeyPress(Keys keyData)
        {
            bool flag = true;
            Keys keys = keyData & Keys.KeyCode;
            Keys keys2 = keyData & ~Keys.KeyCode;
            if (this.tracking)
            {
                flag = false;
                goto Label_017A;
            }
            if ((keys2 == Keys.Alt) || (this.mode == EditingMode.NotEditing))
            {
                goto Label_017A;
            }
            Keys keys3 = keys;
            if (keys3 != Keys.Back)
            {
                switch (keys3)
                {
                    case Keys.End:
                        if (keys2 == Keys.Control)
                        {
                            this.linePos = this.lines.Count - 1;
                        }
                        this.textPos = this.lines[this.linePos].Length;
                        goto Label_015D;

                    case Keys.Home:
                        if (keys2 == Keys.Control)
                        {
                            this.linePos = 0;
                        }
                        this.textPos = 0;
                        goto Label_015D;

                    case Keys.Left:
                        if (keys2 != Keys.Control)
                        {
                            this.PerformLeft();
                        }
                        else
                        {
                            this.PerformControlLeft();
                        }
                        goto Label_015D;

                    case Keys.Up:
                        this.PerformUp();
                        goto Label_015D;

                    case Keys.Right:
                        if (keys2 != Keys.Control)
                        {
                            this.PerformRight();
                        }
                        else
                        {
                            this.PerformControlRight();
                        }
                        goto Label_015D;

                    case Keys.Down:
                        this.PerformDown();
                        goto Label_015D;

                    case Keys.Delete:
                        if (keys2 != Keys.Control)
                        {
                            this.PerformDelete();
                        }
                        else
                        {
                            this.PerformControlDelete();
                        }
                        goto Label_015D;

                    case Keys.Enter:
                        this.PerformEnter();
                        goto Label_015D;
                }
            }
            else
            {
                if (keys2 == Keys.Control)
                {
                    this.PerformControlBackspace();
                }
                else
                {
                    this.PerformBackspace();
                }
                goto Label_015D;
            }
            flag = false;
        Label_015D:
            this.startTime = DateTime.Now;
            if ((this.mode != EditingMode.NotEditing) && flag)
            {
                this.RedrawText(true);
            }
        Label_017A:
            if (!flag)
            {
                base.OnKeyPress(keyData);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                TimeSpan span = (TimeSpan) (DateTime.Now - this.controlKeyDownTime);
                if (span < this.controlKeyDownThreshold)
                {
                    this.enableNub = !this.enableNub;
                }
                this.controlKeyDown = false;
            }
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            System.Windows.Point ptF = e.Point();
            bool flag = this.moveNub.IsPointTouching(ptF, false);
            if ((this.mode != EditingMode.NotEditing) && ((e.Button == MouseButtons.Right) || flag))
            {
                this.tracking = true;
                this.startMouseXY = new System.Drawing.Point(e.X, e.Y);
                this.startClickPoint = this.clickPoint;
                base.Cursor = base.handCursorMouseDown;
                this.UpdateStatusText();
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (this.savedRegion != null)
                {
                    Int32Rect rect = this.savedRegion.Bounds.Int32Bound();
                    int w = (int) Math.Ceiling(this.FontRenderer.Height);
                    rect = rect.InflateCopy(w, w);
                    if ((this.lines != null) && rect.Contains(e.X, e.Y))
                    {
                        Position position = this.PointToTextPosition(new System.Windows.Point((double) e.X, e.Y + (this.FontRenderer.Height / 2.0)));
                        this.linePos = position.Line;
                        this.textPos = position.Offset;
                        this.RedrawText(true);
                        return;
                    }
                }
                switch (this.mode)
                {
                    case EditingMode.EmptyEdit:
                        this.RedrawText(false);
                        this.StopEditing();
                        break;

                    case EditingMode.Editing:
                        this.SaveHistoryMemento();
                        this.StopEditing();
                        break;
                }
                this.clickPoint = new System.Drawing.Point(e.X, e.Y);
                this.StartEditing();
                this.RedrawText(true);
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            if (this.tracking)
            {
                System.Drawing.Point point = new System.Drawing.Point(e.X, e.Y);
                System.Drawing.Size size = new System.Drawing.Size(point.X - this.startMouseXY.X, point.Y - this.startMouseXY.Y);
                this.clickPoint = new System.Drawing.Point(this.startClickPoint.X + size.Width, this.startClickPoint.Y + size.Height);
                this.RedrawText(false);
                this.UpdateStatusText();
            }
            else
            {
                System.Windows.Point ptF = e.Point();
                if (this.moveNub.IsPointTouching(ptF, false) && this.moveNub.Visible)
                {
                    base.Cursor = base.handCursor;
                }
                else
                {
                    base.Cursor = this.textToolCursor;
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            if (this.tracking)
            {
                this.OnMouseMove(e);
                this.tracking = false;
                this.UpdateStatusText();
            }
            base.OnMouseUp(e);
        }

        protected override void OnPaste(IDataObject data, out bool handled)
        {
            base.OnPaste(data, out handled);
            if ((data.GetDataPresent(DataFormats.StringFormat, true) && base.Active) && (this.mode != EditingMode.NotEditing))
            {
                string str = (string) data.GetData(DataFormats.StringFormat, true);
                if (str.Length > 0)
                {
                    this.ignoreRedraw++;
                    foreach (char ch in str)
                    {
                        if (ch == '\n')
                        {
                            this.PerformEnter();
                        }
                        else
                        {
                            base.PerformKeyPress(new KeyPressEventArgs(ch));
                        }
                    }
                    handled = true;
                    this.ignoreRedraw--;
                    this.moveNub.Visible = true;
                    this.RedrawText(false);
                }
            }
        }

        protected override void OnPasteQuery(IDataObject data, out bool canHandle)
        {
            base.OnPasteQuery(data, out canHandle);
            if ((data.GetDataPresent(DataFormats.StringFormat, true) && base.Active) && (this.mode != EditingMode.NotEditing))
            {
                canHandle = true;
            }
        }

        protected override void OnPulse()
        {
            base.OnPulse();
            if (this.pulseEnabled)
            {
                bool flag;
                TimeSpan span = (TimeSpan) (DateTime.Now - this.startTime);
                long totalMilliseconds = (long) span.TotalMilliseconds;
                if (0L == ((totalMilliseconds / 300L) % 2L))
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                }
                flag &= base.Focused;
                if (base.IsFormActive)
                {
                    flag &= (base.ModifierKeys & Keys.Control) == Keys.None;
                }
                if (flag != this.lastPulseCursorState)
                {
                    this.RedrawText(flag);
                    this.lastPulseCursorState = flag;
                }
                if (base.IsFormActive && ((base.ModifierKeys & Keys.Control) != Keys.None))
                {
                    this.moveNub.Visible = false;
                }
                else
                {
                    this.moveNub.Visible = true;
                }
                this.moveNub.Visible &= !this.tracking;
                this.moveNub.Visible &= this.enableNub;
                long num2 = span.Ticks % 0x1312d00L;
                double num3 = Math.Sin((((double) num2) / 20000000.0) * 6.2831853071795862);
                num3 = Math.Min(0.5, num3) + 1.0;
                num3 /= 2.0;
                num3 += 0.25;
                if (this.moveNub != null)
                {
                    int num5 = ((int) (num3 * 255.0)).Clamp(0, 0xff);
                    this.moveNub.Alpha = num5;
                }
                this.PlaceMoveNub();
            }
        }

        private void PerformBackspace()
        {
            if ((this.textPos == 0) && (this.linePos > 0))
            {
                int length = this.lines[this.linePos - 1].Length;
                this.lines[this.linePos - 1] = this.lines[this.linePos - 1] + this.lines[this.linePos];
                this.lines.RemoveAt(this.linePos);
                this.linePos--;
                this.textPos = length;
                this.textSizes = null;
            }
            else if (this.textPos > 0)
            {
                string str = this.lines[this.linePos];
                if (this.textPos == str.Length)
                {
                    this.lines[this.linePos] = str.Substring(0, str.Length - 1);
                }
                else
                {
                    this.lines[this.linePos] = str.Substring(0, this.textPos - 1) + str.Substring(this.textPos);
                }
                this.textPos--;
                this.textSizes = null;
            }
        }

        private void PerformControlBackspace()
        {
            if ((this.textPos == 0) && (this.linePos > 0))
            {
                this.PerformBackspace();
            }
            else if (this.textPos > 0)
            {
                string str = this.lines[this.linePos];
                int textPos = this.textPos;
                if (char.IsLetterOrDigit(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsLetterOrDigit(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else if (char.IsWhiteSpace(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsWhiteSpace(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else if (char.IsPunctuation(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsPunctuation(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else
                {
                    textPos--;
                }
                this.lines[this.linePos] = str.Substring(0, textPos) + str.Substring(this.textPos);
                this.textPos = textPos;
                this.textSizes = null;
            }
        }

        private void PerformControlDelete()
        {
            if ((this.linePos != (this.lines.Count - 1)) || (this.textPos != this.lines[this.lines.Count - 1].Length))
            {
                if (this.textPos == this.lines[this.linePos].Length)
                {
                    this.lines[this.linePos] = this.lines[this.linePos] + this.lines[this.linePos + 1];
                    this.lines.RemoveAt(this.linePos + 1);
                }
                else
                {
                    int textPos = this.textPos;
                    string str = this.lines[this.linePos];
                    if (char.IsLetterOrDigit(str[textPos]))
                    {
                        while ((textPos < str.Length) && char.IsLetterOrDigit(str[textPos]))
                        {
                            str = str.Remove(textPos, 1);
                        }
                    }
                    else if (char.IsWhiteSpace(str[textPos]))
                    {
                        while ((textPos < str.Length) && char.IsWhiteSpace(str[textPos]))
                        {
                            str = str.Remove(textPos, 1);
                        }
                    }
                    else if (char.IsPunctuation(str[textPos]))
                    {
                        while ((textPos < str.Length) && char.IsPunctuation(str[textPos]))
                        {
                            str = str.Remove(textPos, 1);
                        }
                    }
                    else
                    {
                        textPos--;
                    }
                    this.lines[this.linePos] = str;
                }
                if ((this.lines.Count == 1) && (this.lines[0] == ""))
                {
                    this.mode = EditingMode.EmptyEdit;
                }
                this.textSizes = null;
            }
        }

        private void PerformControlLeft()
        {
            if (this.textPos > 0)
            {
                int textPos = this.textPos;
                string str = this.lines[this.linePos];
                if (char.IsLetterOrDigit(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsLetterOrDigit(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else if (char.IsWhiteSpace(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsWhiteSpace(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else if ((textPos > 0) && char.IsPunctuation(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsPunctuation(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else
                {
                    textPos--;
                }
                this.textPos = textPos;
            }
            else if ((this.textPos == 0) && (this.linePos > 0))
            {
                this.linePos--;
                this.textPos = this.lines[this.linePos].Length;
            }
        }

        private void PerformControlRight()
        {
            if (this.textPos < this.lines[this.linePos].Length)
            {
                int textPos = this.textPos;
                string str = this.lines[this.linePos];
                if (char.IsLetterOrDigit(str[textPos]))
                {
                    while ((textPos < str.Length) && char.IsLetterOrDigit(str[textPos]))
                    {
                        textPos++;
                    }
                }
                else if (char.IsWhiteSpace(str[textPos]))
                {
                    while ((textPos < str.Length) && char.IsWhiteSpace(str[textPos]))
                    {
                        textPos++;
                    }
                }
                else if ((textPos > 0) && char.IsPunctuation(str[textPos]))
                {
                    while ((textPos < str.Length) && char.IsPunctuation(str[textPos]))
                    {
                        textPos++;
                    }
                }
                else
                {
                    textPos++;
                }
                this.textPos = textPos;
            }
            else if ((this.textPos == this.lines[this.linePos].Length) && (this.linePos < (this.lines.Count - 1)))
            {
                this.linePos++;
                this.textPos = 0;
            }
        }

        private void PerformDelete()
        {
            if ((this.linePos != (this.lines.Count - 1)) || (this.textPos != this.lines[this.lines.Count - 1].Length))
            {
                if (this.textPos == this.lines[this.linePos].Length)
                {
                    this.lines[this.linePos] = this.lines[this.linePos] + this.lines[this.linePos + 1];
                    this.lines.RemoveAt(this.linePos + 1);
                }
                else
                {
                    this.lines[this.linePos] = this.lines[this.linePos].Substring(0, this.textPos) + this.lines[this.linePos].Substring(this.textPos + 1);
                }
                if ((this.lines.Count == 1) && (this.lines[0] == ""))
                {
                    this.mode = EditingMode.EmptyEdit;
                }
                this.textSizes = null;
            }
        }

        private void PerformDown()
        {
            if (this.linePos != (this.lines.Count - 1))
            {
                System.Windows.Point pf = this.TextPositionToPoint(new Position(this.linePos, this.textPos));
                pf.Y += this.textSizes[0].Height;
                Position position = this.PointToTextPosition(pf);
                this.linePos = position.Line;
                this.textPos = position.Offset;
            }
        }

        private void PerformEnter()
        {
            if (this.lines != null)
            {
                string str = this.lines[this.linePos];
                if (this.textPos == str.Length)
                {
                    this.lines.Insert(this.linePos + 1, string.Empty);
                }
                else
                {
                    this.lines.Insert(this.linePos + 1, str.Substring(this.textPos, str.Length - this.textPos));
                    this.lines[this.linePos] = this.lines[this.linePos].Substring(0, this.textPos);
                }
                this.linePos++;
                this.textPos = 0;
                this.textSizes = null;
            }
        }

        private void PerformLeft()
        {
            if (this.textPos > 0)
            {
                this.textPos--;
            }
            else if ((this.textPos == 0) && (this.linePos > 0))
            {
                this.linePos--;
                this.textPos = this.lines[this.linePos].Length;
            }
        }

        private void PerformRight()
        {
            if (this.textPos < this.lines[this.linePos].Length)
            {
                this.textPos++;
            }
            else if ((this.textPos == this.lines[this.linePos].Length) && (this.linePos < (this.lines.Count - 1)))
            {
                this.linePos++;
                this.textPos = 0;
            }
        }

        private void PerformUp()
        {
            System.Windows.Point pf = this.TextPositionToPoint(new Position(this.linePos, this.textPos));
            pf.Y -= this.textSizes[0].Height;
            Position position = this.PointToTextPosition(pf);
            this.linePos = position.Line;
            this.textPos = position.Offset;
        }

        private void PlaceMoveNub()
        {
            if ((this.textPoints != null) && (this.textPoints.Length > 0))
            {
                Int32Point point = this.textPoints[this.textPoints.Length - 1];
                point.X += (int) Math.Ceiling(this.textSizes[this.textPoints.Length - 1].Width);
                point.Y += (int) Math.Ceiling(this.textSizes[this.textPoints.Length - 1].Height);
                point.X += (int) (10.0 / base.DocumentWorkspace.ScaleFactor.Ratio);
                point.Y += (int) (10.0 / base.DocumentWorkspace.ScaleFactor.Ratio);
                point.X = (int) Math.Round(Math.Min(base.ActiveLayer.Width - this.moveNub.Size.Width, (double) point.X));
                point.X = (int) Math.Round(Math.Max(this.moveNub.Size.Width, (double) point.X));
                point.Y = (int) Math.Round(Math.Min(base.ActiveLayer.Height - this.moveNub.Size.Height, (double) point.Y));
                point.Y = (int) Math.Round(Math.Max(this.moveNub.Size.Height, (double) point.Y));
                this.moveNub.Location = (System.Windows.Point) point;
            }
        }

        private Position PointToTextPosition(System.Windows.Point pf)
        {
            double offset = pf.X - this.clickPoint.X;
            double num2 = pf.Y - this.clickPoint.Y;
            int lno = (int) Math.Floor((double) (num2 / this.textSizes[0].Height));
            if (lno < 0)
            {
                lno = 0;
            }
            else if (lno >= this.lines.Count)
            {
                lno = this.lines.Count - 1;
            }
            int num4 = this.FindOffsetPosition(offset, this.lines[lno], lno);
            Position position = new Position(lno, num4);
            if (position.Offset >= this.lines[position.Line].Length)
            {
                position.Offset = this.lines[position.Line].Length;
            }
            return position;
        }

        private unsafe void RedrawText(bool cursorOn)
        {
            RenderArgs raLayer;
            RenderArgs raScratch;
            Int32Rect[] textClipRects;
            int threads;
            Int32Rect[] textClipScans;
            Int32Rect[] userClipScans;
            Surface brushStencil;
            bool blending;
            object drawSync;
            int linesLeft;
            bool[] lineRendered;
            Int32Rect?[] threadRects;
            if (this.ignoreRedraw <= 0)
            {
                GeometryList savedRegion;
                raLayer = new RenderArgs(((BitmapLayer) base.ActiveLayer).Surface);
                raScratch = new RenderArgs(base.ScratchSurface);
                if (this.savedRegion == null)
                {
                    savedRegion = new GeometryList();
                }
                else
                {
                    savedRegion = this.savedRegion;
                    this.savedRegion = null;
                    base.RestoreRegion(savedRegion);
                }
                double num = Math.Floor((double) (this.clickPoint.Y - (this.FontRenderer.Height / 2.0)));
                double num2 = 0.0;
                if (this.textSizes == null)
                {
                    this.textSizes = new TextSize[this.lines.Count];
                    for (int num3 = 0; num3 < this.lines.Count; num3++)
                    {
                        int iP = num3;
                        this.threadPool.QueueUserWorkItem((WaitCallback) (_ => (this.textSizes[iP] = this.StringSize(this.lines[iP]))));
                    }
                    this.threadPool.Drain();
                }
                this.textPoints = new Int32Point[this.lines.Count];
                for (int num4 = 0; num4 < this.lines.Count; num4++)
                {
                    double num5;
                    this.textSizes[num4] = this.FontRenderer.MeasureText(this.lines[num4]);
                    switch (this.alignment)
                    {
                        case TextAlignment.Left:
                            num5 = 0.0;
                            break;

                        case TextAlignment.Center:
                            num5 = -this.textSizes[num4].Width / 2.0;
                            break;

                        case TextAlignment.Right:
                            num5 = -this.textSizes[num4].Width;
                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }
                    double num6 = this.clickPoint.X + num5;
                    this.textPoints[num4] = new System.Windows.Point(num6, num + num2).RoundCopy();
                    num2 += this.textSizes[num4].Height;
                }
                string text = this.lines[this.linePos].Substring(0, this.textPos);
                TextSize size = this.StringSize(text);
                Rect rect2 = new Rect(this.textPoints[this.linePos].X + size.Width, (double) this.textPoints[this.linePos].Y, 2.0, this.textSizes[this.linePos].Height);
                Int32Rect rect = Int32RectUtil.Truncate(rect2);
                textClipRects = new Int32Rect[this.lines.Count + 1];
                for (int num7 = 0; num7 < this.lines.Count; num7++)
                {
                    textClipRects[num7] = Rect.Offset(this.textSizes[num7].SafeClipBounds, (double) this.textPoints[num7].X, (double) this.textPoints[num7].Y).Int32Bound();
                }
                textClipRects[textClipRects.Length - 1] = rect;
                threads = this.threadPool.Threads;
                Int32Rect rect5 = textClipRects.Bounds().IntersectCopy(raLayer.Surface.Bounds<ColorBgra>()).CoalesceCopy();
                textClipScans = new Int32Rect[threads];
                Int32RectUtil.Split(rect5, textClipScans);
                GeometryList list2 = base.Selection.CreateGeometryListClippingMask();
                Int32Rect[] interiorScans = list2.GetInteriorScans();
                if (interiorScans.Length == 1)
                {
                    userClipScans = new Int32Rect[threads];
                    Int32RectUtil.Split(interiorScans[0], userClipScans);
                }
                else
                {
                    userClipScans = interiorScans;
                }
                this.savedRegion = new GeometryList(rect5);
                base.SaveRegion(null, rect5);
                brushStencil = new Surface(8, 8);
                using (RenderArgs args = new RenderArgs(brushStencil))
                {
                    args.Graphics.CompositingMode = CompositingMode.SourceCopy;
                    using (Brush brush = base.AppEnvironment.BrushInfo.CreateBrush((Color) base.AppEnvironment.PrimaryColor, (Color) base.AppEnvironment.SecondaryColor))
                    {
                        args.Graphics.FillRectangle(brush, brushStencil.Bounds);
                    }
                }
                blending = base.AppEnvironment.AlphaBlending;
                for (int num8 = 0; num8 < textClipScans.Length; num8++)
                {
                    int iP = num8;
                    this.threadPool.QueueUserWorkItem(_ => raLayer.Surface.Clear(textClipScans[iP], ColorBgra.White));
                }
                this.threadPool.Drain();
                drawSync = new object();
                linesLeft = this.lines.Count;
                lineRendered = new bool[this.lines.Count];
                threadRects = new Int32Rect?[threads];
                for (int num9 = 0; num9 < threads; num9++)
                {
                    threadRects[num9] = null;
                    int tP = num9;
                    this.threadPool.QueueUserWorkItem(delegate {
                        using (IBitmapTextRenderTarget target = this.typographyDriver.CreateBitmapRenderTarget(raLayer.Surface))
                        {
                            target.Opaque = false;
                            using (IFontRenderer renderer = this.CreateFontRenderer())
                            {
                                int num;
                            Label_003E:
                                num = 0;
                                lock (drawSync)
                                {
                                    num = 0;
                                    while (num < this.lines.Count)
                                    {
                                        if (!lineRendered[num])
                                        {
                                            int index = 0;
                                            index = 0;
                                            while (index < threads)
                                            {
                                                if (threadRects[index].HasValue && textClipRects[num].IntersectsWith(threadRects[index].Value))
                                                {
                                                    break;
                                                }
                                                index++;
                                            }
                                            if (index == threads)
                                            {
                                                break;
                                            }
                                        }
                                        num++;
                                    }
                                    if (num != this.lines.Count)
                                    {
                                        threadRects[tP] = new Int32Rect?(textClipRects[num]);
                                    }
                                }
                                if (num == this.lines.Count)
                                {
                                    lock (drawSync)
                                    {
                                        if (linesLeft == 0)
                                        {
                                            return;
                                        }
                                        Monitor.Wait(drawSync);
                                        goto Label_003E;
                                    }
                                }
                                bool flag = false;
                                try
                                {
                                    renderer.DrawText(target, this.lines[num], *((System.Windows.Point*) &(this.textPoints[num])), textClipRects[num]);
                                }
                                finally
                                {
                                    lock (drawSync)
                                    {
                                        lineRendered[num] = true;
                                        threadRects[tP] = null;
                                        int num3 = Interlocked.Decrement(ref linesLeft);
                                        Monitor.PulseAll(drawSync);
                                        if (num3 == 0)
                                        {
                                            flag = true;
                                        }
                                    }
                                }
                                if (!flag)
                                {
                                    goto Label_003E;
                                }
                            }
                        }
                    });
                }
                this.threadPool.Drain();
                if (cursorOn)
                {
                    Int32Rect rect6 = Int32RectUtil.Intersect(rect, raLayer.Surface.Bounds<ColorBgra>()).CoalesceCopy();
                    for (int num10 = rect6.Y; num10 < (rect6.Y + rect6.Height); num10++)
                    {
                        ColorBgra* bgraPtr = raLayer.Surface.GetPointAddress(rect6.X, num10);
                        for (int num11 = rect6.X; num11 < (rect6.X + rect6.Width); num11++)
                        {
                            bgraPtr->Bgra ^= 0xffffff;
                            bgraPtr++;
                        }
                    }
                }
                for (int num12 = 0; num12 < threads; num12++)
                {
                    int tP = num12;
                    this.threadPool.QueueUserWorkItem(delegate {
                        Int32Rect rect = textClipScans[tP];
                        for (int i = rect.Y; i < (rect.Y + rect.Height); i++)
                        {
                            ColorBgra* pointAddress = raLayer.Surface.GetPointAddress(rect.X, i);
                            for (int m = rect.X; m < (rect.X + rect.Width); m++)
                            {
                                pointAddress->A = 0xff;
                                pointAddress++;
                            }
                        }
                        for (int j = 0; j < userClipScans.Length; j++)
                        {
                            Int32Rect rect2 = userClipScans[j].IntersectCopy(rect);
                            if (!rect2.HasZeroArea())
                            {
                                for (int n = rect2.Y; n < (rect2.Y + rect2.Height); n++)
                                {
                                    ColorBgra* bgraPtr2 = raLayer.Surface.GetPointAddress(rect2.X, n);
                                    for (int num5 = rect2.X; num5 < (rect2.X + rect2.Width); num5++)
                                    {
                                        bgraPtr2->A = 0;
                                        bgraPtr2++;
                                    }
                                }
                            }
                        }
                        for (int k = rect.Y; k < (rect.Y + rect.Height); k++)
                        {
                            ColorBgra* bgraPtr3 = raScratch.Surface.GetPointAddress(rect.X, k);
                            ColorBgra* bgraPtr4 = raLayer.Surface.GetPointAddress(rect.X, k);
                            int y = k & 7;
                            ColorBgra* rowAddress = brushStencil.GetRowAddress(y);
                            for (int num8 = rect.X; num8 < (rect.X + rect.Width); num8++)
                            {
                                ColorBgra bgra = bgraPtr3[0];
                                ColorBgra bgra2 = bgraPtr4[0];
                                ColorBgra rhs = rowAddress[num8 & 7];
                                byte x = (byte) (0xff - bgra2.GetIntensityByte());
                                int num10 = ByteUtil.FastScale(x, rhs.A);
                                rhs.A = (byte) num10;
                                if ((x == 0) || (bgra2.A == 0xff))
                                {
                                    bgraPtr4->Bgra = bgra.Bgra;
                                }
                                else if ((num10 == 0xff) || !blending)
                                {
                                    bgraPtr4->Bgra = rhs.Bgra;
                                }
                                else
                                {
                                    bgraPtr4->Bgra = UserBlendOps.NormalBlendOp.ApplyStatic(bgraPtr3[0], rhs).Bgra;
                                }
                                bgraPtr4++;
                                bgraPtr3++;
                            }
                        }
                    });
                }
                this.threadPool.Drain();
                list2.Dispose();
                brushStencil.Dispose();
                raLayer.Dispose();
                raScratch.Dispose();
                this.PlaceMoveNub();
                this.UpdateStatusText();
                base.ActiveLayer.Invalidate(savedRegion);
                base.ActiveLayer.Invalidate(this.savedRegion);
                savedRegion.Dispose();
                savedRegion = null;
                base.Update();
            }
        }

        private void RefreshFontRenderer()
        {
            if (Thread.CurrentThread.ManagedThreadId != this.managedThreadId)
            {
                throw new InvalidOperationException("This method can only be called on the UI thread");
            }
            if (this.fontRenderer != null)
            {
                this.fontRenderer.Dispose();
                this.fontRenderer = null;
            }
            this.fontRenderer = this.CreateFontRenderer();
        }

        private void SaveHistoryMemento()
        {
            this.pulseEnabled = false;
            this.RedrawText(false);
            if (this.savedRegion != null)
            {
                GeometryList lhs = base.Selection.CreateGeometryListClippingMask();
                GeometryList changedRegion = GeometryList.Combine(lhs, GeometryCombineMode.Intersect, this.savedRegion);
                if (!changedRegion.IsEmpty)
                {
                    BitmapHistoryMemento memento = new BitmapHistoryMemento(base.Name, base.Image, base.DocumentWorkspace, base.ActiveLayerIndex, changedRegion, base.ScratchSurface);
                    if (this.currentHA == null)
                    {
                        base.HistoryStack.PushNewMemento(memento);
                    }
                    else
                    {
                        this.currentHA.PushNewAction(memento);
                        this.currentHA = null;
                    }
                }
                changedRegion.Dispose();
                lhs.Dispose();
                base.ClearSavedMemory();
                base.ClearSavedRegion();
                this.savedRegion.Dispose();
                this.savedRegion = null;
            }
        }

        private void StartEditing()
        {
            this.linePos = 0;
            this.textPos = 0;
            this.lines = new List<string>();
            this.textSizes = null;
            this.lines.Add(string.Empty);
            this.startTime = DateTime.Now;
            this.mode = EditingMode.EmptyEdit;
            this.pulseEnabled = true;
            this.UpdateStatusText();
        }

        private void StopEditing()
        {
            this.mode = EditingMode.NotEditing;
            this.pulseEnabled = false;
            this.lines = null;
            this.moveNub.Visible = false;
        }

        private TextSize StringSize(string text)
        {
            if (text.Length == 0)
            {
                TextSize size = this.StringSize(" ");
                return new TextSize(0.0, size.Height, 0.0, 0.0, 0.0, 0.0);
            }
            if ((this.fontRenderer != null) && this.fontRenderer.CheckAccess())
            {
                return this.fontRenderer.MeasureText(text);
            }
            using (IFontRenderer renderer = this.CreateFontRenderer())
            {
                return renderer.MeasureText(text);
            }
        }

        private System.Windows.Point TextPositionToPoint(Position p)
        {
            System.Windows.Point point = new System.Windows.Point(0.0, 0.0);
            TextSize size = this.StringSize(this.lines[p.Line].Substring(0, p.Offset));
            TextSize size2 = this.StringSize(this.lines[p.Line]);
            switch (this.alignment)
            {
                case TextAlignment.Left:
                    return new System.Windows.Point(this.clickPoint.X + size.Width, this.clickPoint.Y + (size.Height * p.Line));

                case TextAlignment.Center:
                    return new System.Windows.Point(this.clickPoint.X + (size.Width - (size2.Width / 2.0)), this.clickPoint.Y + (size.Height * p.Line));

                case TextAlignment.Right:
                    return new System.Windows.Point(this.clickPoint.X + (size.Width - size2.Width), this.clickPoint.Y + (size.Height * p.Line));
            }
            throw new InvalidEnumArgumentException("Invalid Alignment");
        }

        private void UpdateStatusText()
        {
            string statusBarXYText;
            ImageResource image;
            if (this.tracking)
            {
                statusBarXYText = this.GetStatusBarXYText();
                image = base.Image;
            }
            else
            {
                statusBarXYText = PdnResources.GetString2("TextTool.StatusText.StartTyping");
                image = null;
            }
            base.SetStatus(image, statusBarXYText);
        }

        private IFontRenderer FontRenderer
        {
            get
            {
                if (this.fontRenderer == null)
                {
                    this.RefreshFontRenderer();
                }
                return this.fontRenderer;
            }
        }

        private enum EditingMode
        {
            NotEditing,
            EmptyEdit,
            Editing
        }

        private sealed class Position
        {
            private int line;
            private int offset;

            public Position(int line, int offset)
            {
                this.line = line;
                this.offset = offset;
            }

            public int Line
            {
                get => 
                    this.line;
                set
                {
                    if (value >= 0)
                    {
                        this.line = value;
                    }
                    else
                    {
                        this.line = 0;
                    }
                }
            }

            public int Offset
            {
                get => 
                    this.offset;
                set
                {
                    if (value >= 0)
                    {
                        this.offset = value;
                    }
                    else
                    {
                        this.offset = 0;
                    }
                }
            }
        }
    }
}

