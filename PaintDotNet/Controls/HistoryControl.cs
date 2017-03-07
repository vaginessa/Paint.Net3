namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class HistoryControl : Control
    {
        private PaintDotNet.HistoryStack historyStack;
        private int ignoreScrollOffsetSet;
        private int itemHeight = UI.ScaleHeight(0x12);
        private Point lastMouseClientPt = new Point(-1, -1);
        private bool managedFocus;
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;
        private int redoItemHighlight = -1;
        private int scrollOffset;
        private int undoItemHighlight = -1;
        private VScrollBar vScrollBar;

        public event EventHandler HistoryChanged;

        public event EventHandler RelinquishFocus;

        public event EventHandler ScrollOffsetChanged;

        public HistoryControl()
        {
            base.SetStyle(ControlStyles.StandardDoubleClick, false);
            this.InitializeComponent();
        }

        private Point ClientPointToViewPoint(Point pt) => 
            new Point(pt.X, pt.Y + this.ScrollOffset);

        public Rectangle ClientRectangleToViewRectangle(Rectangle clientRect) => 
            new Rectangle(this.ClientPointToViewPoint(clientRect.Location), clientRect.Size);

        private void EnsureItemIsFullyVisible(ItemType itemType, int itemIndex)
        {
            Point location = this.StackIndexToViewPoint(itemType, itemIndex);
            Rectangle rectangle = new Rectangle(location, new Size(this.ViewWidth, this.itemHeight));
            int num = rectangle.Bottom - base.ClientSize.Height;
            int top = rectangle.Top;
            this.ScrollOffset = Int32Util.ClampSafe(this.ScrollOffset, num, top);
        }

        private void EnsureLastUndoItemIsFullyVisible()
        {
            int itemIndex = this.historyStack.UndoStack.Count - 1;
            this.EnsureItemIsFullyVisible(ItemType.Undo, itemIndex);
        }

        private void History_Changed(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.PerformMouseMove();
                base.PerformLayout();
                this.Refresh();
                this.OnHistoryChanged();
            }
        }

        private void History_HistoryFlushed(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.EnsureLastUndoItemIsFullyVisible();
                this.PerformMouseMove();
                base.PerformLayout();
                this.Refresh();
            }
        }

        private void History_NewHistoryMemento(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.EnsureLastUndoItemIsFullyVisible();
                this.PerformMouseMove();
                base.PerformLayout();
                base.Invalidate();
            }
        }

        private void History_SteppedBackward(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.undoItemHighlight = -1;
                this.redoItemHighlight = -1;
                this.EnsureLastUndoItemIsFullyVisible();
                this.PerformMouseMove();
                base.PerformLayout();
                this.Refresh();
            }
        }

        private void History_SteppedForward(object sender, EventArgs e)
        {
            if (!base.IsDisposed)
            {
                this.undoItemHighlight = -1;
                this.redoItemHighlight = -1;
                this.EnsureLastUndoItemIsFullyVisible();
                this.PerformMouseMove();
                base.PerformLayout();
                this.Refresh();
            }
        }

        private void InitializeComponent()
        {
            this.vScrollBar = new VScrollBar();
            base.SuspendLayout();
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.ValueChanged += new EventHandler(this.VScrollBar_ValueChanged);
            base.Name = "HistoryControl";
            base.TabStop = false;
            base.Controls.Add(this.vScrollBar);
            base.ResizeRedraw = true;
            this.DoubleBuffered = true;
            base.ResumeLayout();
            base.PerformLayout();
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.historyStack != null)
            {
                ItemType type;
                int num;
                Point viewPt = this.ClientPointToViewPoint(this.lastMouseClientPt);
                this.ViewPointToStackIndex(viewPt, out type, out num);
                this.OnItemClicked(type, num);
            }
            base.OnClick(e);
        }

        private void OnHistoryChanged()
        {
            this.vScrollBar.Maximum = this.ViewHeight;
            if (this.HistoryChanged != null)
            {
                this.HistoryChanged(this, EventArgs.Empty);
            }
        }

        private void OnItemClicked(ItemType itemType, HistoryMemento hm)
        {
            int iD = hm.ID;
            if (itemType == ItemType.Undo)
            {
                if (iD == this.historyStack.UndoStack[this.historyStack.UndoStack.Count - 1].ID)
                {
                    if (this.historyStack.UndoStack.Count > 1)
                    {
                        this.historyStack.StepBackward(this);
                    }
                }
                else
                {
                    this.SuspendScrollOffsetSet();
                    this.historyStack.BeginStepGroup();
                    using (new WaitCursorChanger(this))
                    {
                        while (this.historyStack.UndoStack[this.historyStack.UndoStack.Count - 1].ID != iD)
                        {
                            this.historyStack.StepBackward(this);
                        }
                    }
                    this.historyStack.EndStepGroup();
                    this.ResumeScrollOffsetSet();
                }
            }
            else
            {
                this.SuspendScrollOffsetSet();
                this.historyStack.BeginStepGroup();
                using (new WaitCursorChanger(this))
                {
                    while (this.historyStack.UndoStack[this.historyStack.UndoStack.Count - 1].ID != iD)
                    {
                        this.historyStack.StepForward(this);
                    }
                }
                this.historyStack.EndStepGroup();
                this.ResumeScrollOffsetSet();
            }
            base.Focus();
        }

        private void OnItemClicked(ItemType itemType, int itemIndex)
        {
            HistoryMemento memento;
            if (itemType == ItemType.Undo)
            {
                if ((itemIndex >= 0) && (itemIndex < this.historyStack.UndoStack.Count))
                {
                    memento = this.historyStack.UndoStack[itemIndex];
                }
                else
                {
                    memento = null;
                }
            }
            else if ((itemIndex >= 0) && (itemIndex < this.historyStack.RedoStack.Count))
            {
                memento = this.historyStack.RedoStack[itemIndex];
            }
            else
            {
                memento = null;
            }
            if (memento != null)
            {
                this.EnsureItemIsFullyVisible(itemType, itemIndex);
                this.OnItemClicked(itemType, memento);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num;
            if (this.historyStack == null)
            {
                num = 0;
            }
            else
            {
                num = this.historyStack.UndoStack.Count + this.historyStack.RedoStack.Count;
            }
            int num2 = num * this.itemHeight;
            if (num2 > base.ClientSize.Height)
            {
                this.vScrollBar.Visible = true;
                this.vScrollBar.Location = new Point(base.ClientSize.Width - this.vScrollBar.Width, 0);
                this.vScrollBar.Height = base.ClientSize.Height;
                this.vScrollBar.Minimum = 0;
                this.vScrollBar.Maximum = num2;
                this.vScrollBar.LargeChange = base.ClientSize.Height;
                this.vScrollBar.SmallChange = this.itemHeight;
            }
            else
            {
                this.vScrollBar.Visible = false;
            }
            if (this.historyStack != null)
            {
                this.ScrollOffset = Int32Util.Clamp(this.ScrollOffset, this.MinScrollOffset, this.MaxScrollOffset);
            }
            base.OnLayout(levent);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (((this.historyStack != null) && this.managedFocus) && (!MenuStripEx.IsAnyMenuActive && UI.IsOurAppActive))
            {
                base.Focus();
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (this.historyStack != null)
            {
                this.undoItemHighlight = -1;
                this.redoItemHighlight = -1;
                this.Refresh();
                if (this.Focused && this.managedFocus)
                {
                    this.OnRelinquishFocus();
                }
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ItemType type;
            int num;
            if (this.historyStack == null)
            {
                goto Label_00B8;
            }
            Point pt = new Point(e.X, e.Y);
            Point viewPt = this.ClientPointToViewPoint(pt);
            this.ViewPointToStackIndex(viewPt, out type, out num);
            switch (type)
            {
                case ItemType.Undo:
                    if ((num < 0) || (num >= this.historyStack.UndoStack.Count))
                    {
                        this.undoItemHighlight = -1;
                        break;
                    }
                    this.undoItemHighlight = num;
                    break;

                case ItemType.Redo:
                    this.undoItemHighlight = -1;
                    if ((num < 0) || (num >= this.historyStack.RedoStack.Count))
                    {
                        this.redoItemHighlight = -1;
                    }
                    else
                    {
                        this.redoItemHighlight = num;
                    }
                    goto Label_00AB;

                default:
                    throw new InvalidEnumArgumentException();
            }
            this.redoItemHighlight = -1;
        Label_00AB:
            this.Refresh();
            this.lastMouseClientPt = pt;
        Label_00B8:
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (this.historyStack != null)
            {
                int num = (e.Delta * SystemInformation.MouseWheelScrollLines) / SystemInformation.MouseWheelScrollDelta;
                int num2 = num * this.itemHeight;
                this.ScrollOffset -= num2;
                this.PerformMouseMove();
            }
            base.OnMouseWheel(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.historyStack != null)
            {
                int num4;
                int num5;
                int num9;
                int num10;
                e.Graphics.FillRectangle(this.penBrushCache.GetSolidBrush(this.BackColor), e.ClipRectangle);
                e.Graphics.TranslateTransform(0f, (float) -this.scrollOffset);
                int x = UI.ScaleWidth(1);
                int num2 = UI.ScaleHeight(1);
                int num3 = UI.ScaleWidth(1);
                StringFormat format = (StringFormat) StringFormat.GenericTypographic.Clone();
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                TextFormatFlags flags = TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
                Rectangle a = this.ClientRectangleToViewRectangle(base.ClientRectangle);
                Rectangle undoViewRectangle = this.UndoViewRectangle;
                e.Graphics.FillRectangle(this.penBrushCache.GetSolidBrush(SystemColors.Window), undoViewRectangle);
                Rectangle rectangle3 = Rectangle.Intersect(a, undoViewRectangle);
                if ((rectangle3.Width > 0) && (rectangle3.Height > 0))
                {
                    ItemType type;
                    this.ViewPointToStackIndex(rectangle3.Location, out type, out num4);
                    this.ViewPointToStackIndex(new Point(rectangle3.Left, rectangle3.Bottom - 1), out type, out num5);
                }
                else
                {
                    num4 = 0;
                    num5 = -1;
                }
                for (int i = num4; i <= num5; i++)
                {
                    Image reference;
                    int num7;
                    HighlightState hover;
                    ImageResource resource = this.historyStack.UndoStack[i].Image;
                    if (resource != null)
                    {
                        reference = resource.Reference;
                    }
                    else
                    {
                        reference = null;
                    }
                    if (reference != null)
                    {
                        num7 = (reference.Width * (this.itemHeight - (2 * num2))) / reference.Height;
                    }
                    else
                    {
                        num7 = this.itemHeight - (2 * num2);
                    }
                    if (i == (this.historyStack.UndoStack.Count - 1))
                    {
                        hover = HighlightState.Checked;
                    }
                    else if (i == this.undoItemHighlight)
                    {
                        hover = HighlightState.Hover;
                    }
                    else
                    {
                        hover = HighlightState.Default;
                    }
                    Rectangle rect = new Rectangle(0, i * this.itemHeight, this.ViewWidth, this.itemHeight);
                    SelectionHighlight.DrawBackground(e.Graphics, this.penBrushCache, rect, hover);
                    Color selectionForeColor = SelectionHighlight.GetSelectionForeColor(hover);
                    if (reference != null)
                    {
                        e.Graphics.DrawImage(reference, new Rectangle(rect.X + x, rect.Y + num2, num7, this.itemHeight - (2 * num2)), new Rectangle(0, 0, reference.Width, reference.Height), GraphicsUnit.Pixel);
                    }
                    int num8 = (x + num3) + num7;
                    Rectangle bounds = new Rectangle(num8, i * this.itemHeight, this.ViewWidth - num8, this.itemHeight);
                    TextRenderer.DrawText(e.Graphics, this.historyStack.UndoStack[i].Name, this.Font, bounds, selectionForeColor, flags);
                }
                Rectangle redoViewRectangle = this.RedoViewRectangle;
                e.Graphics.FillRectangle(this.penBrushCache.GetSolidBrush(Color.SlateGray), redoViewRectangle);
                Font font = new Font(this.Font, this.Font.Style | FontStyle.Italic);
                Rectangle rectangle7 = Rectangle.Intersect(a, redoViewRectangle);
                if ((rectangle7.Width > 0) && (rectangle7.Height > 0))
                {
                    ItemType type2;
                    this.ViewPointToStackIndex(rectangle7.Location, out type2, out num9);
                    this.ViewPointToStackIndex(new Point(rectangle7.Left, rectangle7.Bottom - 1), out type2, out num10);
                }
                else
                {
                    num9 = 0;
                    num10 = -1;
                }
                for (int j = num9; j <= num10; j++)
                {
                    Image image2;
                    int num12;
                    Color inactiveCaptionText;
                    ImageResource image = this.historyStack.RedoStack[j].Image;
                    if (image != null)
                    {
                        image2 = image.Reference;
                    }
                    else
                    {
                        image2 = null;
                    }
                    if (image2 != null)
                    {
                        num12 = (image2.Width * (this.itemHeight - (2 * num2))) / image2.Height;
                    }
                    else
                    {
                        num12 = this.itemHeight - (2 * num2);
                    }
                    int y = redoViewRectangle.Top + (j * this.itemHeight);
                    if (j == this.redoItemHighlight)
                    {
                        Rectangle rectangle8 = new Rectangle(0, y, this.ViewWidth, this.itemHeight);
                        SelectionHighlight.DrawBackground(e.Graphics, this.penBrushCache, rectangle8, HighlightState.Hover);
                        inactiveCaptionText = SelectionHighlight.GetSelectionForeColor(HighlightState.Hover);
                    }
                    else
                    {
                        inactiveCaptionText = SystemColors.InactiveCaptionText;
                    }
                    if (image2 != null)
                    {
                        e.Graphics.DrawImage(image2, new Rectangle(x, y + num2, num12, this.itemHeight - (2 * num2)), new Rectangle(0, 0, image2.Width, image2.Height), GraphicsUnit.Pixel);
                    }
                    int num14 = (x + num3) + num12;
                    Rectangle rectangle9 = new Rectangle(num14, y, this.ViewWidth - num14, this.itemHeight);
                    TextRenderer.DrawText(e.Graphics, this.historyStack.RedoStack[j].Name, font, rectangle9, inactiveCaptionText, flags);
                }
                font.Dispose();
                font = null;
                format.Dispose();
                format = null;
                e.Graphics.TranslateTransform(0f, (float) this.scrollOffset);
            }
            base.OnPaint(e);
        }

        private void OnRelinquishFocus()
        {
            if (this.RelinquishFocus != null)
            {
                this.RelinquishFocus(this, EventArgs.Empty);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.PerformLayout();
            base.OnResize(e);
        }

        private void OnScrollOffsetChanged()
        {
            this.vScrollBar.Value = Int32Util.Clamp(this.scrollOffset, this.vScrollBar.Minimum, this.vScrollBar.Maximum);
            if (this.ScrollOffsetChanged != null)
            {
                this.ScrollOffsetChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.PerformLayout();
            base.OnSizeChanged(e);
        }

        private void PerformMouseMove()
        {
            Point pt = base.PointToClient(Control.MousePosition);
            if (base.ClientRectangle.Contains(pt))
            {
                MouseEventArgs e = new MouseEventArgs(MouseButtons.None, 0, pt.X, pt.Y, 0);
                this.OnMouseMove(e);
            }
        }

        private void ResumeScrollOffsetSet()
        {
            this.ignoreScrollOffsetSet--;
        }

        private Point StackIndexToViewPoint(ItemType itemType, int itemIndex)
        {
            Rectangle undoViewRectangle;
            if (itemType == ItemType.Undo)
            {
                undoViewRectangle = this.UndoViewRectangle;
            }
            else
            {
                undoViewRectangle = this.RedoViewRectangle;
            }
            return new Point(0, (itemIndex * this.itemHeight) + undoViewRectangle.Top);
        }

        private void SuspendScrollOffsetSet()
        {
            this.ignoreScrollOffsetSet++;
        }

        private void ViewPointToStackIndex(Point viewPt, out ItemType itemType, out int itemIndex)
        {
            Rectangle undoViewRectangle = this.UndoViewRectangle;
            if ((viewPt.Y >= undoViewRectangle.Top) && (viewPt.Y < undoViewRectangle.Bottom))
            {
                itemType = ItemType.Undo;
                itemIndex = (viewPt.Y - undoViewRectangle.Top) / this.itemHeight;
            }
            else
            {
                Rectangle redoViewRectangle = this.RedoViewRectangle;
                itemType = ItemType.Redo;
                itemIndex = (viewPt.Y - redoViewRectangle.Top) / this.itemHeight;
            }
        }

        private void VScrollBar_ValueChanged(object sender, EventArgs e)
        {
            this.ScrollOffset = this.vScrollBar.Value;
        }

        public PaintDotNet.HistoryStack HistoryStack
        {
            get => 
                this.historyStack;
            set
            {
                if (this.historyStack != null)
                {
                    this.historyStack.Changed -= new EventHandler(this.History_Changed);
                    this.historyStack.SteppedForward -= new EventHandler(this.History_SteppedForward);
                    this.historyStack.SteppedBackward -= new EventHandler(this.History_SteppedBackward);
                    this.historyStack.HistoryFlushed -= new EventHandler(this.History_HistoryFlushed);
                    this.historyStack.NewHistoryMemento -= new EventHandler(this.History_NewHistoryMemento);
                }
                this.historyStack = value;
                base.PerformLayout();
                if (this.historyStack != null)
                {
                    this.historyStack.Changed += new EventHandler(this.History_Changed);
                    this.historyStack.SteppedForward += new EventHandler(this.History_SteppedForward);
                    this.historyStack.SteppedBackward += new EventHandler(this.History_SteppedBackward);
                    this.historyStack.HistoryFlushed += new EventHandler(this.History_HistoryFlushed);
                    this.historyStack.NewHistoryMemento += new EventHandler(this.History_NewHistoryMemento);
                    this.EnsureLastUndoItemIsFullyVisible();
                }
                this.Refresh();
                this.OnHistoryChanged();
            }
        }

        private int ItemCount =>
            (this.historyStack?.UndoStack.Count + this.historyStack.RedoStack.Count);

        public bool ManagedFocus
        {
            get => 
                this.managedFocus;
            set
            {
                this.managedFocus = value;
            }
        }

        public int MaxScrollOffset =>
            Math.Max(0, this.ViewHeight - base.ClientSize.Height);

        public int MinScrollOffset =>
            0;

        private Rectangle RedoViewRectangle =>
            new Rectangle(0, this.itemHeight * this.historyStack.UndoStack.Count, this.ViewWidth, this.itemHeight * this.historyStack.RedoStack.Count);

        public int ScrollOffset
        {
            get => 
                this.scrollOffset;
            set
            {
                if (this.ignoreScrollOffsetSet <= 0)
                {
                    int num = Int32Util.Clamp(value, this.MinScrollOffset, this.MaxScrollOffset);
                    if (this.scrollOffset != num)
                    {
                        this.scrollOffset = num;
                        this.OnScrollOffsetChanged();
                        base.Invalidate(false);
                    }
                }
            }
        }

        private Rectangle UndoViewRectangle =>
            new Rectangle(0, 0, this.ViewWidth, this.itemHeight * this.historyStack.UndoStack.Count);

        private int ViewHeight =>
            (this.ItemCount * this.itemHeight);

        public Rectangle ViewRectangle =>
            new Rectangle(0, 0, this.ViewWidth, this.ViewHeight);

        public int ViewWidth
        {
            get
            {
                if (this.vScrollBar.Visible)
                {
                    return (base.ClientSize.Width - this.vScrollBar.Width);
                }
                return base.ClientSize.Width;
            }
        }

        private enum ItemType
        {
            Undo,
            Redo
        }
    }
}

