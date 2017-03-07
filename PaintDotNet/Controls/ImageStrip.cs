namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal class ImageStrip : Control
    {
        private int animationFrame;
        private ulong animationHz = 50L;
        private ulong animationStartTick;
        private System.Windows.Forms.Timer animationTimer;
        private Image[] busyAnimationFrames;
        private const int closeButtonLength = 0x11;
        private int closeButtonPadding;
        private int ctorThreadID = Thread.CurrentThread.ManagedThreadId;
        private int dirtyOverlayLength = 11;
        private int dirtyOverlayPaddingLeft;
        private int dirtyOverlayPaddingTop = 2;
        private bool drawDirtyOverlay = true;
        private bool drawShadow = true;
        private int imagePadding = 4;
        private Dictionary<Item, int> itemIndices = new Dictionary<Item, int>();
        private List<Item> items = new List<Item>();
        private Point lastMouseMovePt = new Point(-32000, -32000);
        private PaintDotNet.Controls.ArrowButton leftScrollButton;
        private bool managedFocus;
        private bool mouseDownApplyRendering;
        private MouseButtons mouseDownButton;
        private int mouseDownIndex = -1;
        private ItemPart mouseDownItemPart;
        private bool mouseOverApplyRendering;
        private int mouseOverIndex = -1;
        private ItemPart mouseOverItemPart;
        private PaintDotNet.Controls.ArrowButton rightScrollButton;
        private int scrollOffset;
        private bool showCloseButtons;
        private bool showScrollButtons;
        private Timing timing = new Timing();

        private event Action<Item> itemChangedHook;

        public event EventHandler<EventArgs<Triple<Item, ItemPart, MouseButtons>>> ItemClicked;

        public event EventHandler RelinquishFocus;

        public event EventHandler<EventArgs<ArrowDirection>> ScrollArrowClicked;

        public event EventHandler ScrollOffsetChanged;

        public ImageStrip()
        {
            base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            base.SetStyle(ControlStyles.Selectable, false);
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            this.InitializeComponent();
            this.leftScrollButton.ArrowImage = PdnResources.GetImageResource2("Images.ImageStrip.ScrollLeftArrow.png").Reference;
            this.rightScrollButton.ArrowImage = PdnResources.GetImageResource2("Images.ImageStrip.ScrollRightArrow.png").Reference;
            this.animationTimer = new System.Windows.Forms.Timer();
            this.animationTimer.Interval = ((int) this.animationHz) / 2;
            this.animationTimer.Enabled = true;
            this.animationTimer.Tick += new EventHandler(this.AnimationTimer_Tick);
        }

        public void AddItem(Item newItem)
        {
            if (this.items.Contains(newItem))
            {
                throw new ArgumentException("newItem was already added to this control");
            }
            newItem.Changed += new EventHandler(this.OnItemChanged);
            this.items.Add(newItem);
            this.itemIndices.Add(newItem, this.items.Count - 1);
            if (newItem.Image == null)
            {
                this.animationTimer.Enabled = true;
            }
            base.PerformLayout();
            base.Invalidate();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if ((from doc in this.items
                where doc.Image == null
                select doc).Count<Item>() == 0)
            {
                this.animationTimer.Enabled = false;
            }
            else
            {
                int num2 = (int) (this.timing.GetTickCount() / this.animationHz);
                if (num2 != this.animationFrame)
                {
                    this.animationFrame = num2;
                    for (int i = 0; i < this.items.Count; i++)
                    {
                        Item item = this.items[i];
                        if ((item.Image == null) && this.IsItemVisible(i))
                        {
                            item.Update();
                        }
                    }
                    base.Update();
                }
            }
        }

        private void CalculateVisibleScrollOffsets(int itemIndex, out int minOffset, out int maxOffset, out int minFullyShownOffset, out int maxFullyShownOffset)
        {
            Rectangle rectangle = this.ItemIndexToViewRect(itemIndex);
            minOffset = (rectangle.Left + 1) - base.ClientSize.Width;
            maxOffset = rectangle.Right - 1;
            minFullyShownOffset = rectangle.Right - base.ClientSize.Width;
            maxFullyShownOffset = rectangle.Left;
            if (this.leftScrollButton.Visible)
            {
                maxOffset -= this.leftScrollButton.Width;
                maxFullyShownOffset -= this.leftScrollButton.Width;
            }
            if (this.rightScrollButton.Visible)
            {
                minOffset += this.rightScrollButton.Width;
                minFullyShownOffset += this.rightScrollButton.Width;
            }
        }

        public void ClearItems()
        {
            base.SuspendLayout();
            UI.SuspendControlPainting(this);
            while (this.items.Count > 0)
            {
                this.RemoveItem(this.items[this.items.Count - 1]);
            }
            UI.ResumeControlPainting(this);
            base.ResumeLayout(true);
            base.Invalidate();
        }

        public Point ClientPointToViewPoint(Point clientPt) => 
            new Point(clientPt.X + this.scrollOffset, clientPt.Y);

        public Rectangle ClientRectToViewRect(Rectangle clientRect) => 
            new Rectangle(this.ClientPointToViewPoint(clientRect.Location), clientRect.Size);

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.animationTimer != null))
            {
                this.animationTimer.Dispose();
                this.animationTimer = null;
            }
            base.Dispose(disposing);
        }

        private void DrawItem(Graphics g, Item item, Point offset)
        {
            Rectangle rectangle;
            Rectangle rectangle2;
            Rectangle rectangle3;
            Rectangle rectangle4;
            Rectangle rectangle5;
            this.MeasureItemPartRectangles(item, out rectangle, out rectangle2, out rectangle3, out rectangle4, out rectangle5);
            rectangle.X += offset.X;
            rectangle.Y += offset.Y;
            rectangle2.X += offset.X;
            rectangle2.Y += offset.Y;
            rectangle3.X += offset.X;
            rectangle3.Y += offset.Y;
            rectangle4.X += offset.X;
            rectangle4.Y += offset.Y;
            rectangle5.X += offset.X;
            rectangle5.Y += offset.Y;
            this.DrawItemBackground(g, item, rectangle);
            this.DrawItemForeground(g, item, rectangle, rectangle2, rectangle3, rectangle4, rectangle5);
        }

        protected virtual void DrawItemBackground(Graphics g, Item item, Rectangle itemRect)
        {
        }

        protected virtual void DrawItemCloseButton(Graphics g, Item item, Rectangle itemRect, Rectangle closeButtonRect)
        {
            if (item.Checked && item.Selected)
            {
                string str;
                switch (UI.VisualStyleClass)
                {
                    case VisualStyleClass.Luna:
                        str = "Luna";
                        break;

                    case VisualStyleClass.Aero:
                        str = "Aero";
                        break;

                    default:
                        if (OS.IsVistaOrLater)
                        {
                            str = "Aero";
                        }
                        else
                        {
                            str = "Classic";
                        }
                        break;
                }
                string str2 = "." + str + ".png";
                string str3 = item.CloseRenderState.ToString();
                Image reference = PdnResources.GetImageResource2("Images.ImageStrip.CloseButton." + str3 + str2).Reference;
                SmoothingMode smoothingMode = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                InterpolationMode interpolationMode = g.InterpolationMode;
                if (reference.Size != closeButtonRect.Size)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                }
                g.DrawImage(reference, closeButtonRect, new Rectangle(0, 0, reference.Width, reference.Width), GraphicsUnit.Pixel);
                g.InterpolationMode = interpolationMode;
                g.SmoothingMode = smoothingMode;
            }
        }

        protected virtual void DrawItemDirtyOverlay(Graphics g, Item item, Rectangle itemRect, Rectangle dirtyOverlayRect)
        {
            int num;
            if (dirtyOverlayRect.Width <= 11)
            {
                num = 11;
            }
            else
            {
                num = 0x12;
            }
            string format = "Images.ImageStrip.DirtyOverlay.{0}.png";
            Image reference = PdnResources.GetImageResource2(string.Format(format, num.ToString())).Reference;
            InterpolationMode interpolationMode = g.InterpolationMode;
            if (reference.Size != dirtyOverlayRect.Size)
            {
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            }
            g.DrawImage(reference, dirtyOverlayRect, new Rectangle(Point.Empty, reference.Size), GraphicsUnit.Pixel);
            g.InterpolationMode = interpolationMode;
        }

        protected virtual void DrawItemForeground(Graphics g, Item item, Rectangle itemRect, Rectangle imageRect, Rectangle imageInsetRect, Rectangle closeButtonRect, Rectangle dirtyOverlayRect)
        {
            Rectangle highlightRect = itemRect;
            this.DrawItemHighlight(g, item, itemRect, highlightRect);
            if (this.drawShadow)
            {
                this.DrawItemImageShadow(g, item, itemRect, imageRect, imageInsetRect);
            }
            this.DrawItemImage(g, item, itemRect, imageRect, imageInsetRect);
            if (this.showCloseButtons)
            {
                this.DrawItemCloseButton(g, item, itemRect, closeButtonRect);
            }
            if (this.drawDirtyOverlay && item.Dirty)
            {
                this.DrawItemDirtyOverlay(g, item, itemRect, dirtyOverlayRect);
            }
        }

        protected virtual void DrawItemHighlight(Graphics g, Item item, Rectangle itemRect, Rectangle highlightRect)
        {
            HighlightState hover;
            if (item.Checked)
            {
                hover = HighlightState.Checked;
            }
            else if (item.Selected)
            {
                hover = HighlightState.Hover;
            }
            else
            {
                hover = HighlightState.Default;
            }
            itemRect.Inflate(-1, -1);
            SelectionHighlight.DrawBackground(g, itemRect, hover);
        }

        protected virtual void DrawItemImage(Graphics g, Item item, Rectangle itemRect, Rectangle imageRect, Rectangle imageInsetRect)
        {
            if (item.Image == null)
            {
                int index = this.animationFrame % this.BusyAnimationFrames.Length;
                Image image = this.BusyAnimationFrames[index];
                Rectangle srcRect = new Rectangle(0, 0, image.Width, image.Height);
                Rectangle destRect = new Rectangle(itemRect.X + ((imageRect.Width - image.Width) / 2), itemRect.Y + ((imageRect.Height - image.Height) / 2), image.Width, image.Height);
                g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.DrawImage(item.Image, imageInsetRect, new Rectangle(0, 0, item.Image.Width, item.Image.Height), GraphicsUnit.Pixel);
            }
        }

        protected virtual void DrawItemImageShadow(Graphics g, Item item, Rectangle itemRect, Rectangle imageRect, Rectangle imageInsetRect)
        {
            if (item.Image != null)
            {
                DropShadow.DrawOutside(g, imageInsetRect, DropShadow.GetRecommendedExtent(imageInsetRect.Size));
            }
        }

        public void EnsureItemFullyVisible(Item item)
        {
            int index = this.items.IndexOf(item);
            this.EnsureItemFullyVisible(index);
        }

        public void EnsureItemFullyVisible(int index)
        {
            if (!this.IsItemFullyVisible(index))
            {
                int num;
                int num2;
                int num3;
                int num4;
                this.CalculateVisibleScrollOffsets(index, out num, out num2, out num3, out num4);
                int scrollOffset = this.scrollOffset;
                int num6 = Math.Abs((int) (scrollOffset - num3));
                int num7 = Math.Abs((int) (scrollOffset - num4));
                if (num6 <= num7)
                {
                    this.ScrollOffset = num3;
                }
                else
                {
                    this.ScrollOffset = num4;
                }
            }
        }

        private void ForceMouseMove()
        {
            Point point = base.PointToClient(Control.MousePosition);
            this.lastMouseMovePt = new Point(this.lastMouseMovePt.X + 1, this.lastMouseMovePt.Y + 1);
            MouseEventArgs e = new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0);
            this.OnMouseMove(e);
        }

        private void GetFocus()
        {
            if ((this.managedFocus && !MenuStripEx.IsAnyMenuActive) && UI.IsOurAppActive)
            {
                base.Focus();
            }
        }

        private void InitializeComponent()
        {
            this.leftScrollButton = new PaintDotNet.Controls.ArrowButton();
            this.rightScrollButton = new PaintDotNet.Controls.ArrowButton();
            base.SuspendLayout();
            this.leftScrollButton.Name = "leftScrollButton";
            this.leftScrollButton.ArrowDirection = ArrowDirection.Left;
            this.leftScrollButton.ArrowOutlineWidth = 1f;
            this.leftScrollButton.Click += new EventHandler(this.LeftScrollButton_Click);
            this.leftScrollButton.DrawWithGradient = true;
            this.rightScrollButton.Name = "rightScrollButton";
            this.rightScrollButton.ArrowDirection = ArrowDirection.Right;
            this.rightScrollButton.ArrowOutlineWidth = 1f;
            this.rightScrollButton.Click += new EventHandler(this.RightScrollButton_Click);
            this.rightScrollButton.DrawWithGradient = true;
            base.Name = "ImageStrip";
            base.TabStop = false;
            base.Controls.Add(this.leftScrollButton);
            base.Controls.Add(this.rightScrollButton);
            base.ResumeLayout();
            base.PerformLayout();
        }

        public bool IsItemFullyVisible(int index)
        {
            Rectangle a = this.ItemIndexToViewRect(index);
            Rectangle scrolledViewRect = this.ScrolledViewRect;
            if (this.leftScrollButton.Visible)
            {
                scrolledViewRect.X += this.leftScrollButton.Width;
                scrolledViewRect.Width -= this.leftScrollButton.Width;
            }
            if (this.rightScrollButton.Visible)
            {
                scrolledViewRect.Width -= this.rightScrollButton.Width;
            }
            return (Rectangle.Intersect(a, scrolledViewRect) == a);
        }

        public bool IsItemVisible(int index)
        {
            Rectangle rectangle2 = Rectangle.Intersect(this.ItemIndexToViewRect(index), this.ScrolledViewRect);
            if (rectangle2.Width <= 0)
            {
                return (rectangle2.Height > 0);
            }
            return true;
        }

        private Rectangle ItemIndexToClientRect(int itemIndex)
        {
            Rectangle viewRect = this.ItemIndexToViewRect(itemIndex);
            return this.ViewRectToClientRect(viewRect);
        }

        public Item ItemIndexToItem(int index) => 
            this.items[index];

        private Rectangle ItemIndexToViewRect(int itemIndex)
        {
            Size itemSize = this.ItemSize;
            return new Rectangle(itemIndex * itemSize.Width, 0, itemSize.Width, itemSize.Height);
        }

        private ItemPart ItemPointToItemPart(Item item, Point pt)
        {
            Rectangle rectangle;
            Rectangle rectangle2;
            Rectangle rectangle3;
            Rectangle rectangle4;
            Rectangle rectangle5;
            this.MeasureItemPartRectangles(item, out rectangle, out rectangle2, out rectangle3, out rectangle4, out rectangle5);
            if (rectangle4.Contains(pt))
            {
                return ItemPart.CloseButton;
            }
            if (rectangle2.Contains(pt))
            {
                return ItemPart.Image;
            }
            return ItemPart.None;
        }

        public int ItemToItemIndex(Item item) => 
            this.itemIndices[item];

        private void LeftScrollButton_Click(object sender, EventArgs e)
        {
            base.Focus();
            this.OnScrollArrowClicked(ArrowDirection.Left);
        }

        private void MeasureItemPartRectangles(out Rectangle itemRect, out Rectangle imageRect)
        {
            itemRect = new Rectangle(0, 0, base.ClientSize.Height, base.ClientSize.Height);
            imageRect = new Rectangle(itemRect.Left, itemRect.Top, itemRect.Width, itemRect.Width);
        }

        private void MeasureItemPartRectangles(Item item, out Rectangle itemRect, out Rectangle imageRect, out Rectangle imageInsetRect, out Rectangle closeButtonRect, out Rectangle dirtyOverlayRect)
        {
            Int32Size size;
            this.MeasureItemPartRectangles(out itemRect, out imageRect);
            Rectangle rectangle = new Rectangle(imageRect.Left + this.imagePadding, imageRect.Top + this.imagePadding, imageRect.Width - (this.imagePadding * 2), imageRect.Height - (this.imagePadding * 2));
            if (item.Image == null)
            {
                size = imageRect.Size.ToInt32Size();
            }
            else
            {
                size = Utility.ComputeThumbnailSize(item.Image.Size.ToInt32Size(), rectangle.Width);
            }
            imageInsetRect = new Rectangle(rectangle.Left + ((rectangle.Width - size.Width) / 2), (rectangle.Bottom - size.Height) - 1, size.Width, size.Height);
            int width = UI.ScaleWidth(0x11);
            int num2 = UI.ScaleWidth(this.closeButtonPadding);
            closeButtonRect = new Rectangle((rectangle.Right - width) - num2, rectangle.Top + num2, width, width);
            int num3 = UI.ScaleWidth(this.dirtyOverlayLength);
            int num4 = UI.ScaleWidth(this.dirtyOverlayPaddingTop);
            int num5 = UI.ScaleWidth(this.dirtyOverlayPaddingLeft);
            dirtyOverlayRect = new Rectangle(rectangle.Left + num5, rectangle.Top + num4, num3, num3);
        }

        private void MouseStatesToItemStates()
        {
            UI.SuspendControlPainting(this);
            try
            {
                int? nullable = null;
                ItemPart none = ItemPart.None;
                PushButtonState renderState = PushButtonState.Default;
                if (this.mouseDownApplyRendering)
                {
                    if ((this.mouseDownIndex < 0) || (this.mouseDownIndex >= this.items.Count))
                    {
                        this.mouseDownApplyRendering = false;
                    }
                    else
                    {
                        nullable = new int?(this.mouseDownIndex);
                        none = this.mouseDownItemPart;
                        renderState = PushButtonState.Pressed;
                    }
                }
                else if (this.mouseOverApplyRendering)
                {
                    if ((this.mouseOverIndex < 0) || (this.mouseOverIndex >= this.items.Count))
                    {
                        this.mouseOverApplyRendering = false;
                    }
                    else
                    {
                        nullable = new int?(this.mouseOverIndex);
                        none = this.mouseOverItemPart;
                        renderState = PushButtonState.Hot;
                    }
                }
                for (int i = 0; i < this.items.Count; i++)
                {
                    this.items[i].CheckRenderState = PushButtonState.Normal;
                    this.items[i].CloseRenderState = PushButtonState.Normal;
                    this.items[i].ImageRenderState = PushButtonState.Normal;
                    this.items[i].Selected = false;
                    if (nullable.HasValue && (i == nullable.Value))
                    {
                        this.items[i].Selected = true;
                        this.items[i].SetPartRenderState(none, renderState);
                    }
                }
            }
            finally
            {
                UI.ResumeControlPainting(this);
                base.Invalidate(true);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (Thread.CurrentThread.ManagedThreadId != this.ctorThreadID)
            {
                throw new InvalidOperationException("Control handle was created on a thread other than the one this object was constructed on");
            }
            base.OnHandleCreated(e);
        }

        private void OnItemChanged(object sender, EventArgs e)
        {
            Item item = (Item) sender;
            if (item.Image == null)
            {
                this.animationTimer.Enabled = true;
            }
            int itemIndex = this.ItemToItemIndex(item);
            Rectangle rc = this.ItemIndexToClientRect(itemIndex);
            base.Invalidate(rc);
            if (this.itemChangedHook != null)
            {
                this.itemChangedHook(item);
            }
        }

        protected virtual void OnItemClicked(Item item, ItemPart itemPart, MouseButtons mouseButtons)
        {
            if (this.ItemClicked != null)
            {
                this.ItemClicked(this, new EventArgs<Triple<Item, ItemPart, MouseButtons>>(Triple.Create<Item, ItemPart, MouseButtons>(item, itemPart, mouseButtons)));
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int width = UI.ScaleWidth(0x10);
            this.ScrollOffset = this.scrollOffset.Clamp(this.MinScrollOffset, this.MaxScrollOffset);
            this.leftScrollButton.Size = new Size(width, base.ClientSize.Height);
            this.leftScrollButton.Location = new Point(0, 0);
            this.rightScrollButton.Size = new Size(width, base.ClientSize.Height);
            this.rightScrollButton.Location = new Point(base.ClientSize.Width - this.rightScrollButton.Width, 0);
            bool flag = this.showScrollButtons && (this.ViewRectangle.Width > base.ClientRectangle.Width);
            bool flag2 = (this.scrollOffset < this.MaxScrollOffset) && flag;
            bool flag3 = (this.scrollOffset > this.MinScrollOffset) && flag;
            this.rightScrollButton.Enabled = flag2;
            this.rightScrollButton.Visible = flag2;
            this.leftScrollButton.Enabled = flag3;
            this.leftScrollButton.Visible = flag3;
            base.OnLayout(levent);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (this.mouseDownButton == MouseButtons.None)
            {
                Point clientPt = new Point(e.X, e.Y);
                Point viewPt = this.ClientPointToViewPoint(clientPt);
                int itemIndex = this.ViewPointToItemIndex(viewPt);
                if ((itemIndex >= 0) && (itemIndex < this.items.Count))
                {
                    Item item = this.items[itemIndex];
                    Point pt = this.ViewPointToItemPoint(itemIndex, viewPt);
                    ItemPart itemPart = this.ItemPointToItemPart(item, pt);
                    if (itemPart == ItemPart.Image)
                    {
                        this.OnItemClicked(item, itemPart, e.Button);
                        this.mouseDownApplyRendering = false;
                        this.mouseOverIndex = itemIndex;
                        this.mouseOverItemPart = itemPart;
                        this.mouseOverApplyRendering = true;
                    }
                    else
                    {
                        this.mouseDownIndex = itemIndex;
                        this.mouseDownItemPart = itemPart;
                        this.mouseDownButton = e.Button;
                        this.mouseDownApplyRendering = true;
                        this.mouseOverApplyRendering = false;
                    }
                    this.MouseStatesToItemStates();
                    base.Update();
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.GetFocus();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.mouseDownApplyRendering = false;
            this.mouseOverApplyRendering = false;
            this.MouseStatesToItemStates();
            base.Update();
            if ((this.managedFocus && !MenuStripEx.IsAnyMenuActive) && UI.IsOurAppActive)
            {
                this.OnRelinquishFocus();
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.GetFocus();
            Point clientPt = new Point(e.X, e.Y);
            if (clientPt != this.lastMouseMovePt)
            {
                Point viewPt = this.ClientPointToViewPoint(clientPt);
                int itemIndex = this.ViewPointToItemIndex(viewPt);
                if (this.mouseDownButton == MouseButtons.None)
                {
                    if ((itemIndex >= 0) && (itemIndex < this.items.Count))
                    {
                        Item item = this.items[itemIndex];
                        Point pt = this.ViewPointToItemPoint(itemIndex, viewPt);
                        ItemPart part = this.ItemPointToItemPart(item, pt);
                        this.mouseOverIndex = itemIndex;
                        this.mouseOverItemPart = part;
                        this.mouseOverApplyRendering = true;
                    }
                    else
                    {
                        this.mouseOverApplyRendering = false;
                    }
                }
                else
                {
                    this.mouseOverApplyRendering = false;
                    if (itemIndex != this.mouseDownIndex)
                    {
                        this.mouseDownApplyRendering = false;
                    }
                    else if ((itemIndex < 0) || (itemIndex >= this.items.Count))
                    {
                        this.mouseDownApplyRendering = false;
                    }
                    else
                    {
                        Item item2 = this.Items[itemIndex];
                        Point point4 = this.ViewPointToItemPoint(itemIndex, viewPt);
                        if (this.ItemPointToItemPart(item2, point4) != this.mouseDownItemPart)
                        {
                            this.mouseDownApplyRendering = false;
                        }
                    }
                }
                this.MouseStatesToItemStates();
                base.Update();
            }
            this.lastMouseMovePt = clientPt;
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            bool flag = false;
            if (this.mouseDownButton == e.Button)
            {
                Point clientPt = new Point(e.X, e.Y);
                Point viewPt = this.ClientPointToViewPoint(clientPt);
                int itemIndex = this.ViewPointToItemIndex(viewPt);
                if ((itemIndex >= 0) && (itemIndex < this.items.Count))
                {
                    Item item = this.items[itemIndex];
                    Point pt = this.ViewPointToItemPoint(itemIndex, viewPt);
                    ItemPart itemPart = this.ItemPointToItemPart(item, pt);
                    if ((itemIndex == this.mouseDownIndex) && (itemPart == this.mouseDownItemPart))
                    {
                        if ((itemPart == ItemPart.CloseButton) && !item.Checked)
                        {
                            itemPart = ItemPart.Image;
                        }
                        this.OnItemClicked(item, itemPart, this.mouseDownButton);
                        flag = true;
                    }
                    this.mouseOverApplyRendering = true;
                    this.mouseOverItemPart = itemPart;
                    this.mouseOverIndex = itemIndex;
                }
                this.mouseDownApplyRendering = false;
                this.mouseDownButton = MouseButtons.None;
                this.MouseStatesToItemStates();
                base.Update();
            }
            if (flag)
            {
                this.ForceMouseMove();
            }
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            float num = ((float) e.Delta) / ((float) SystemInformation.MouseWheelScrollDelta);
            int num2 = (int) (num * this.ItemSize.Width);
            int num3 = this.ScrollOffset - num2;
            this.ScrollOffset = num3;
            this.ForceMouseMove();
            base.Invalidate();
            base.OnMouseWheel(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (UI.IsControlPaintingEnabled(this))
            {
                Size itemSize = this.ItemSize;
                for (int i = e.ClipRectangle.Left; i <= (e.ClipRectangle.Right + itemSize.Width); i += itemSize.Width)
                {
                    Point clientPt = new Point(i, 0);
                    Point viewPt = this.ClientPointToViewPoint(clientPt);
                    int itemIndex = this.ViewPointToItemIndex(viewPt);
                    Rectangle rectangle = this.ItemIndexToClientRect(itemIndex);
                    if (itemIndex >= 0)
                    {
                        this.DrawItem(e.Graphics, this.items[itemIndex], rectangle.Location);
                    }
                }
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

        protected virtual void OnScrollArrowClicked(ArrowDirection arrowDirection)
        {
            if (this.ScrollArrowClicked != null)
            {
                this.ScrollArrowClicked(this, new EventArgs<ArrowDirection>(arrowDirection));
            }
        }

        protected virtual void OnScrollOffsetChanged()
        {
            base.PerformLayout();
            if (this.ScrollOffsetChanged != null)
            {
                this.ScrollOffsetChanged(this, EventArgs.Empty);
            }
        }

        public void PerformItemClick(Item item, ItemPart itemPart, MouseButtons mouseButtons)
        {
            this.OnItemClicked(item, itemPart, mouseButtons);
        }

        public void PerformItemClick(int itemIndex, ItemPart itemPart, MouseButtons mouseButtons)
        {
            this.PerformItemClick(this.items[itemIndex], itemPart, mouseButtons);
        }

        public void RemoveItem(Item item)
        {
            if (!this.items.Contains(item))
            {
                throw new ArgumentException("item was never added to this control");
            }
            item.Changed -= new EventHandler(this.OnItemChanged);
            this.items.Remove(item);
            this.itemIndices.Remove(item);
            for (int i = 0; i < this.items.Count; i++)
            {
                this.itemIndices[this.items[i]] = i;
            }
            base.PerformLayout();
            base.Invalidate();
        }

        private void RightScrollButton_Click(object sender, EventArgs e)
        {
            base.Focus();
            this.OnScrollArrowClicked(ArrowDirection.Right);
        }

        public Point ViewPointToClientPoint(Point viewPt) => 
            new Point(viewPt.X - this.scrollOffset, viewPt.Y);

        public int ViewPointToItemIndex(Point viewPt)
        {
            if (!this.ViewRectangle.Contains(viewPt))
            {
                return -1;
            }
            Size itemSize = this.ItemSize;
            return (viewPt.X / itemSize.Width);
        }

        private Point ViewPointToItemPoint(int itemIndex, Point viewPt)
        {
            Rectangle rectangle = this.ItemIndexToViewRect(itemIndex);
            return new Point(viewPt.X - rectangle.X, viewPt.Y);
        }

        public Rectangle ViewRectToClientRect(Rectangle viewRect) => 
            new Rectangle(this.ViewPointToClientPoint(viewRect.Location), viewRect.Size);

        private Image[] BusyAnimationFrames
        {
            get
            {
                if (this.busyAnimationFrames == null)
                {
                    this.busyAnimationFrames = AnimationResources.Working;
                    this.animationStartTick = this.timing.GetTickCount();
                }
                return this.busyAnimationFrames;
            }
        }

        public bool DrawDirtyOverlay
        {
            get => 
                this.drawDirtyOverlay;
            set
            {
                if (this.drawDirtyOverlay != value)
                {
                    this.drawDirtyOverlay = value;
                    this.Refresh();
                }
            }
        }

        public bool DrawShadow
        {
            get => 
                this.drawShadow;
            set
            {
                if (this.drawShadow != value)
                {
                    this.drawShadow = value;
                    this.Refresh();
                }
            }
        }

        public int ItemCount =>
            this.items.Count;

        public Item[] Items =>
            this.items.ToArrayEx<Item>();

        public Size ItemSize
        {
            get
            {
                Rectangle rectangle;
                Rectangle rectangle2;
                this.MeasureItemPartRectangles(out rectangle, out rectangle2);
                return rectangle.Size;
            }
        }

        public PaintDotNet.Controls.ArrowButton LeftScrollButton =>
            this.leftScrollButton;

        public bool ManagedFocus
        {
            get => 
                this.managedFocus;
            set
            {
                this.managedFocus = value;
            }
        }

        public int MaxScrollOffset
        {
            get
            {
                int num = this.ItemSize.Width * this.items.Count;
                int num2 = num - base.ClientSize.Width;
                return Math.Max(0, num2);
            }
        }

        public int MinScrollOffset =>
            0;

        public Size PreferredImageSize
        {
            get
            {
                Rectangle rectangle;
                Rectangle rectangle2;
                this.MeasureItemPartRectangles(out rectangle, out rectangle2);
                return new Size(rectangle2.Width - (this.imagePadding * 2), rectangle2.Height - (this.imagePadding * 2));
            }
        }

        public int PreferredMinClientWidth
        {
            get
            {
                if (this.items.Count == 0)
                {
                    return 0;
                }
                int width = this.ItemSize.Width;
                if (this.leftScrollButton.Visible || this.rightScrollButton.Visible)
                {
                    width += this.leftScrollButton.Width;
                    width += this.rightScrollButton.Width;
                }
                return Math.Min(width, this.ViewRectangle.Width);
            }
        }

        protected PaintDotNet.Controls.ArrowButton RightScrollButton =>
            this.rightScrollButton;

        public Rectangle ScrolledViewRect =>
            new Rectangle(this.scrollOffset, 0, base.ClientSize.Width, base.ClientSize.Height);

        public int ScrollOffset
        {
            get => 
                this.scrollOffset;
            set
            {
                int num = value.Clamp(this.MinScrollOffset, this.MaxScrollOffset);
                if (this.scrollOffset != num)
                {
                    this.scrollOffset = num;
                    this.OnScrollOffsetChanged();
                    base.Invalidate(true);
                }
            }
        }

        public bool ShowCloseButtons
        {
            get => 
                this.showCloseButtons;
            set
            {
                if (this.showCloseButtons != value)
                {
                    this.showCloseButtons = value;
                    base.PerformLayout();
                    base.Invalidate();
                }
            }
        }

        public bool ShowScrollButtons
        {
            get => 
                this.showScrollButtons;
            set
            {
                if (this.showScrollButtons != value)
                {
                    this.showScrollButtons = value;
                    base.PerformLayout();
                    base.Invalidate(true);
                }
            }
        }

        public Rectangle ViewRectangle
        {
            get
            {
                Size itemSize = this.ItemSize;
                return new Rectangle(0, 0, itemSize.Width * this.ItemCount, itemSize.Height);
            }
        }

        public sealed class Item
        {
            private PushButtonState checkRenderState;
            private System.Windows.Forms.CheckState checkState;
            private PushButtonState closeRenderState;
            private bool dirty;
            private int dirtyValueLockCount;
            private System.Drawing.Image image;
            private PushButtonState imageRenderState;
            private bool lockedDirtyValue;
            private bool selected;
            private object tag;

            public event EventHandler Changed;

            public Item()
            {
            }

            public Item(System.Drawing.Image image)
            {
                this.image = image;
            }

            public PushButtonState GetPartRenderState(ImageStrip.ItemPart itemPart)
            {
                switch (itemPart)
                {
                    case ImageStrip.ItemPart.None:
                        return PushButtonState.Default;

                    case ImageStrip.ItemPart.Image:
                        return this.ImageRenderState;

                    case ImageStrip.ItemPart.CloseButton:
                        return this.CloseRenderState;
                }
                throw new InvalidEnumArgumentException();
            }

            public void LockDirtyValue(bool forceValue)
            {
                this.dirtyValueLockCount++;
                if (this.dirtyValueLockCount == 1)
                {
                    this.lockedDirtyValue = forceValue;
                }
            }

            private void OnChanged()
            {
                if (this.Changed != null)
                {
                    this.Changed(this, EventArgs.Empty);
                }
            }

            public void SetPartRenderState(ImageStrip.ItemPart itemPart, PushButtonState renderState)
            {
                switch (itemPart)
                {
                    case ImageStrip.ItemPart.None:
                        return;

                    case ImageStrip.ItemPart.Image:
                        this.ImageRenderState = renderState;
                        return;

                    case ImageStrip.ItemPart.CloseButton:
                        this.CloseRenderState = renderState;
                        return;
                }
                throw new InvalidEnumArgumentException();
            }

            public void UnlockDirtyValue()
            {
                this.dirtyValueLockCount--;
                if (this.dirtyValueLockCount == 0)
                {
                    this.OnChanged();
                }
                else if (this.dirtyValueLockCount < 0)
                {
                    throw new InvalidOperationException("Calls to UnlockDirtyValue() must be matched by a preceding call to LockDirtyValue()");
                }
            }

            public void Update()
            {
                this.OnChanged();
            }

            public bool Checked
            {
                get => 
                    (this.CheckState == System.Windows.Forms.CheckState.Checked);
                set
                {
                    if (value)
                    {
                        this.CheckState = System.Windows.Forms.CheckState.Checked;
                    }
                    else
                    {
                        this.CheckState = System.Windows.Forms.CheckState.Unchecked;
                    }
                }
            }

            public PushButtonState CheckRenderState
            {
                get => 
                    this.checkRenderState;
                set
                {
                    if (this.checkRenderState != value)
                    {
                        this.checkRenderState = value;
                        this.OnChanged();
                    }
                }
            }

            public System.Windows.Forms.CheckState CheckState
            {
                get => 
                    this.checkState;
                set
                {
                    if (this.checkState != value)
                    {
                        this.checkState = value;
                        this.OnChanged();
                    }
                }
            }

            public PushButtonState CloseRenderState
            {
                get => 
                    this.closeRenderState;
                set
                {
                    if (this.closeRenderState != value)
                    {
                        this.closeRenderState = value;
                        this.OnChanged();
                    }
                }
            }

            public bool Dirty
            {
                get
                {
                    if (this.dirtyValueLockCount > 0)
                    {
                        return this.lockedDirtyValue;
                    }
                    return this.dirty;
                }
                set
                {
                    if (this.dirty != value)
                    {
                        this.dirty = value;
                        if (this.dirtyValueLockCount <= 0)
                        {
                            this.OnChanged();
                        }
                    }
                }
            }

            public System.Drawing.Image Image
            {
                get => 
                    this.image;
                set
                {
                    this.image = value;
                    this.OnChanged();
                }
            }

            public PushButtonState ImageRenderState
            {
                get => 
                    this.imageRenderState;
                set
                {
                    if (this.imageRenderState != value)
                    {
                        this.imageRenderState = value;
                        this.OnChanged();
                    }
                }
            }

            public bool Selected
            {
                get => 
                    this.selected;
                set
                {
                    if (this.selected != value)
                    {
                        this.selected = value;
                        this.OnChanged();
                    }
                }
            }

            public object Tag
            {
                get => 
                    this.tag;
                set
                {
                    this.tag = value;
                    this.OnChanged();
                }
            }
        }

        public enum ItemPart
        {
            None,
            Image,
            CloseButton
        }
    }
}

