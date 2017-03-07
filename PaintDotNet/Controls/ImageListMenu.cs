namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class ImageListMenu : Control
    {
        private Bitmap backBuffer;
        private ComboBox comboBox;
        private int imageXInset;
        private int imageYInset;
        private Size itemSize = Size.Empty;
        private Size maxImageSize = Size.Empty;
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;
        private StringFormat stringFormat;
        private int textLeftMargin;
        private int textRightMargin;
        private int textVMargin;

        public event EventHandler Closed;

        public event EventHandler<EventArgs<Item>> ItemClicked;

        public ImageListMenu()
        {
            this.InitializeComponent();
            this.imageXInset = UI.ScaleWidth(5);
            this.imageYInset = UI.ScaleHeight(6);
            this.textLeftMargin = UI.ScaleWidth(4);
            this.textRightMargin = UI.ScaleWidth(0x10);
            this.textVMargin = UI.ScaleHeight(2);
            this.stringFormat = (StringFormat) StringFormat.GenericTypographic.Clone();
        }

        private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                if ((this.backBuffer != null) && ((this.backBuffer.Width != e.Bounds.Width) || (this.backBuffer.Height != e.Bounds.Height)))
                {
                    this.backBuffer.Dispose();
                    this.backBuffer = null;
                }
                if (this.backBuffer == null)
                {
                    this.backBuffer = new Bitmap(e.Bounds.Width, e.Bounds.Height, PixelFormat.Format24bppRgb);
                }
                Item item = (Item) this.comboBox.Items[e.Index];
                using (Graphics graphics = Graphics.FromImage(this.backBuffer))
                {
                    HighlightState hover;
                    if ((e.State & DrawItemState.Selected) != DrawItemState.None)
                    {
                        hover = HighlightState.Hover;
                    }
                    else
                    {
                        hover = HighlightState.Default;
                    }
                    Color selectionForeColor = SelectionHighlight.GetSelectionForeColor(hover);
                    Rectangle rect = new Rectangle(0, 0, this.backBuffer.Width, this.backBuffer.Height);
                    graphics.FillRectangle(this.penBrushCache.GetSolidBrush(SystemColors.Window), rect);
                    Rectangle rectangle2 = Rectangle.Inflate(rect, -1, -1);
                    SelectionHighlight.DrawBackground(graphics, this.penBrushCache, rectangle2, hover);
                    int extent = 0;
                    if ((item.Image != null) && (item.Image.PixelFormat != PixelFormat.Undefined))
                    {
                        extent = DropShadow.GetRecommendedExtent(item.Image.Size);
                        Rectangle destRect = new Rectangle((this.imageXInset + extent) + ((this.maxImageSize.Width - item.Image.Width) / 2), (this.imageYInset + extent) + ((this.maxImageSize.Height - item.Image.Height) / 2), item.Image.Width, item.Image.Height);
                        graphics.DrawImage(item.Image, destRect, new Rectangle(0, 0, item.Image.Width, item.Image.Height), GraphicsUnit.Pixel);
                        DropShadow.DrawOutside(graphics, this.penBrushCache, destRect, extent);
                    }
                    Size size = Size.Ceiling(e.Graphics.MeasureString(item.Name, this.Font, new PointF(0f, 0f), this.stringFormat));
                    SolidBrush solidBrush = this.penBrushCache.GetSolidBrush(selectionForeColor);
                    graphics.DrawString(item.Name, this.Font, solidBrush, (float) (((((this.imageXInset + extent) + this.maxImageSize.Width) + extent) + this.imageXInset) + this.textLeftMargin), (float) ((this.itemSize.Height - size.Height) / 2));
                }
                CompositingMode compositingMode = e.Graphics.CompositingMode;
                e.Graphics.CompositingMode = CompositingMode.SourceCopy;
                e.Graphics.DrawImage(this.backBuffer, e.Bounds, new Rectangle(0, 0, this.backBuffer.Width, this.backBuffer.Height), GraphicsUnit.Pixel);
                e.Graphics.CompositingMode = compositingMode;
            }
        }

        private void ComboBox_DropDown(object sender, EventArgs e)
        {
            MenuStripEx.PushMenuActivate();
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            MenuStripEx.PopMenuActivate();
            this.comboBox.Items.Clear();
            this.OnClosed();
        }

        private void ComboBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemWidth = this.itemSize.Width;
            e.ItemHeight = this.itemSize.Height;
        }

        private void ComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int selectedIndex = this.comboBox.SelectedIndex;
            if ((selectedIndex >= 0) && (selectedIndex < this.comboBox.Items.Count))
            {
                this.OnItemClicked((Item) this.comboBox.Items[selectedIndex]);
            }
        }

        private void DetermineMaxItemSize(Graphics g, Item[] items, out Size maxItemSizeResult, out Size maxImageSizeResult)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            foreach (Item item in items)
            {
                num = Math.Max(num, (item.Image == null) ? 0 : item.Image.Width);
                num2 = Math.Max(num2, (item.Image == null) ? 0 : item.Image.Height);
                Size size = Size.Ceiling(g.MeasureString(item.Name, this.Font, new PointF(0f, 0f), this.stringFormat));
                num3 = Math.Max(size.Width, num3);
                num4 = Math.Max(size.Height, num4);
            }
            int recommendedExtent = DropShadow.GetRecommendedExtent(new Size(num, num2));
            int width = ((((((recommendedExtent + this.imageXInset) + num) + this.imageXInset) + recommendedExtent) + this.textLeftMargin) + num3) + this.textRightMargin;
            int height = Math.Max((int) ((((this.imageYInset + recommendedExtent) + num2) + this.imageYInset) + recommendedExtent), (int) ((this.textVMargin + num4) + this.textVMargin));
            maxItemSizeResult = new Size(width, height);
            maxImageSizeResult = new Size(num, num2);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.stringFormat != null)
                {
                    this.stringFormat.Dispose();
                    this.stringFormat = null;
                }
                if (this.backBuffer != null)
                {
                    this.backBuffer.Dispose();
                    this.backBuffer = null;
                }
            }
            base.Dispose(disposing);
        }

        public void HideImageList()
        {
            UI.ShowComboBox(this.comboBox, false);
        }

        private void InitializeComponent()
        {
            this.comboBox = new ComboBox();
            this.comboBox.Name = "comboBox";
            this.comboBox.MeasureItem += new MeasureItemEventHandler(this.ComboBox_MeasureItem);
            this.comboBox.DrawItem += new DrawItemEventHandler(this.ComboBox_DrawItem);
            this.comboBox.DropDown += new EventHandler(this.ComboBox_DropDown);
            this.comboBox.DropDownClosed += new EventHandler(this.ComboBox_DropDownClosed);
            this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBox.SelectionChangeCommitted += new EventHandler(this.ComboBox_SelectionChangeCommitted);
            this.comboBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboBox.Visible = true;
            base.TabStop = false;
            base.Controls.Add(this.comboBox);
            base.Name = "ImageListMenu";
        }

        private void OnClosed()
        {
            if (this.Closed != null)
            {
                this.Closed(this, EventArgs.Empty);
            }
        }

        private void OnItemClicked(Item item)
        {
            if (this.ItemClicked != null)
            {
                this.ItemClicked(this, new EventArgs<Item>(item));
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.comboBox.Location = new Point(0, -this.comboBox.Height);
            base.OnLayout(levent);
        }

        public void ShowImageList(Item[] items)
        {
            this.HideImageList();
            this.comboBox.Items.AddRange(items);
            using (Graphics graphics = base.CreateGraphics())
            {
                this.DetermineMaxItemSize(graphics, items, out this.itemSize, out this.maxImageSize);
            }
            this.comboBox.ItemHeight = this.itemSize.Height;
            this.comboBox.DropDownWidth = (this.itemSize.Width + SystemInformation.VerticalScrollBarWidth) + UI.ScaleWidth(2);
            Screen screen = Screen.FromControl(this);
            Point point = base.PointToScreen(new Point(this.comboBox.Left, this.comboBox.Bottom));
            int num = screen.WorkingArea.Height - point.Y;
            num = this.itemSize.Height * (num / this.itemSize.Height);
            num += 2;
            int num2 = 2 + (this.itemSize.Height * 3);
            int num3 = Math.Max(num, num2);
            this.comboBox.DropDownHeight = num3;
            int num4 = Array.FindIndex<Item>(items, item => item.Selected);
            this.comboBox.SelectedIndex = num4;
            int x = base.PointToScreen(new Point(0, base.Height)).X;
            if ((x + this.comboBox.DropDownWidth) > screen.WorkingArea.Right)
            {
                x = screen.WorkingArea.Right - this.comboBox.DropDownWidth;
            }
            Point point2 = base.PointToClient(new Point(x, point.Y));
            base.SuspendLayout();
            this.comboBox.Left = point2.X;
            base.ResumeLayout(false);
            this.comboBox.Focus();
            UI.ShowComboBox(this.comboBox, true);
        }

        public bool IsImageListVisible =>
            this.comboBox.DroppedDown;

        public sealed class Item
        {
            private System.Drawing.Image image;
            private string name;
            private bool selected;
            private object tag;

            public Item(System.Drawing.Image image, string name, bool selected)
            {
                this.image = image;
                this.name = name;
                this.selected = selected;
            }

            public override string ToString() => 
                this.name;

            public System.Drawing.Image Image =>
                this.image;

            public string Name =>
                this.name;

            public bool Selected =>
                this.selected;

            public object Tag
            {
                get => 
                    this.tag;
                set
                {
                    this.tag = value;
                }
            }
        }
    }
}

