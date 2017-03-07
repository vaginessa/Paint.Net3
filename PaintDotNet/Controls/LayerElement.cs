namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Windows.Forms;

    internal class LayerElement : UserControl
    {
        private Container components;
        private PictureBox icon;
        private bool isMouseOver;
        private bool isSelected;
        private PaintDotNet.Layer layer;
        private Label layerDescription;
        private PropertyEventHandler layerPropertyChangedDelegate;
        private CheckBox layerVisible;
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;
        private int suspendPreviewUpdates;
        private PaintDotNet.ThumbnailManager thumbnailManager;
        private int thumbnailSize = 0x10;
        public static int ThumbSizePreScaling = 40;

        public LayerElement()
        {
            base.SuspendLayout();
            this.InitializeComponent();
            this.InitializeComponent2();
            base.ResumeLayout(false);
            this.IsSelected = false;
            this.layerPropertyChangedDelegate = new PropertyEventHandler(this.LayerPropertyChangedHandler);
            base.TabStop = false;
            base.MouseEnter += new EventHandler(this.MouseEnterHandler);
            base.MouseLeave += new EventHandler(this.MouseLeaveHandler);
            this.layerDescription.MouseEnter += new EventHandler(this.MouseEnterHandler);
            this.layerDescription.MouseLeave += new EventHandler(this.MouseLeaveHandler);
            this.icon.MouseEnter += new EventHandler(this.MouseEnterHandler);
            this.icon.MouseLeave += new EventHandler(this.MouseLeaveHandler);
            this.layerVisible.MouseEnter += new EventHandler(this.MouseEnterHandler);
            this.layerVisible.MouseLeave += new EventHandler(this.MouseLeaveHandler);
        }

        private void Control_Click(object sender, EventArgs e)
        {
            this.OnClick(e);
        }

        private void Control_DoubleClick(object sender, EventArgs e)
        {
            this.OnDoubleClick(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Layer = null;
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.layerDescription = new TransparentLabel();
            this.icon = new TransparentPictureBox();
            this.layerVisible = new TransparentCheckBox();
            base.SuspendLayout();
            this.layerDescription.BackColor = Color.Transparent;
            this.layerDescription.Dock = DockStyle.Fill;
            this.layerDescription.Name = "layerDescription";
            this.layerDescription.Size = new Size(150, 50);
            this.layerDescription.TabIndex = 9;
            this.layerDescription.TextAlign = ContentAlignment.MiddleLeft;
            this.layerDescription.Click += new EventHandler(this.Control_Click);
            this.layerDescription.DoubleClick += new EventHandler(this.Control_DoubleClick);
            this.layerDescription.UseMnemonic = false;
            this.icon.BackColor = Color.Transparent;
            this.icon.Dock = DockStyle.Left;
            this.icon.Location = new Point(0, 0);
            this.icon.Name = "icon";
            this.icon.TabStop = false;
            this.icon.Click += new EventHandler(this.Control_Click);
            this.icon.DoubleClick += new EventHandler(this.Control_DoubleClick);
            this.layerVisible.BackColor = Color.Transparent;
            this.layerVisible.Checked = true;
            this.layerVisible.CheckState = CheckState.Checked;
            this.layerVisible.Dock = DockStyle.Right;
            this.layerVisible.FlatStyle = FlatStyle.Standard;
            this.layerVisible.Location = new Point(0xb8, 0);
            this.layerVisible.Name = "layerVisible";
            this.layerVisible.TabIndex = 7;
            this.layerVisible.KeyPress += new KeyPressEventHandler(this.LayerVisible_KeyPress);
            this.layerVisible.CheckStateChanged += new EventHandler(this.LayerVisible_CheckStateChanged);
            this.layerVisible.KeyUp += new KeyEventHandler(this.LayerVisible_KeyUp);
            base.Controls.Add(this.layerDescription);
            base.Controls.Add(this.icon);
            base.Controls.Add(this.layerVisible);
            base.Name = "LayerElement";
            base.ResumeLayout(false);
        }

        private void InitializeComponent2()
        {
            base.Size = new Size(200, UI.ScaleWidth(ThumbSizePreScaling));
            this.icon.Size = new Size(12 + base.Height, base.Height);
            this.layerDescription.Location = new Point(this.icon.Right, 0);
            this.layerVisible.Size = new Size(0x10, base.Height);
        }

        private void Layer_Invalidated(object sender, InvalidateEventArgs e)
        {
            this.RefreshPreview();
        }

        private void LayerPropertyChangedHandler(object sender, PropertyEventArgs e)
        {
            this.layerDescription.Text = this.layer.Name;
            this.layerVisible.Checked = this.layer.Visible;
        }

        private void LayerVisible_CheckStateChanged(object sender, EventArgs e)
        {
            this.layer.Visible = this.layerVisible.Checked;
            base.Update();
        }

        private void LayerVisible_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        private void LayerVisible_KeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        private void MouseEnterHandler(object sender, EventArgs e)
        {
            this.isMouseOver = true;
            this.SetColors();
            base.Invalidate(true);
        }

        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            this.isMouseOver = false;
            this.SetColors();
            base.Invalidate(true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            this.RefreshPreview();
            base.OnHandleCreated(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            HighlightState hover;
            e.Graphics.FillRectangle(this.penBrushCache.GetSolidBrush(SystemColors.Window), e.ClipRectangle);
            if (this.IsSelected)
            {
                hover = HighlightState.Checked;
            }
            else if (this.isMouseOver)
            {
                hover = HighlightState.Hover;
            }
            else
            {
                hover = HighlightState.Default;
            }
            Rectangle clientRectangle = base.ClientRectangle;
            SelectionHighlight.DrawBackground(e.Graphics, this.penBrushCache, clientRectangle, hover);
            base.OnPaint(e);
        }

        private void OnThumbnailRendered(object sender, EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>> e)
        {
            if (!base.IsDisposed)
            {
                Bitmap image = e.Data.Second.CreateAliasedGdipBitmap();
                Bitmap bitmap2 = new Bitmap(this.icon.Width, this.icon.Height, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(bitmap2))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.Clear(Color.Transparent);
                    Rectangle destRect = new Rectangle((bitmap2.Width - image.Width) / 2, (bitmap2.Height - image.Height) / 2, image.Width, image.Height);
                    graphics.DrawImage(image, destRect, new Rectangle(new Point(0, 0), image.Size), GraphicsUnit.Pixel);
                    DropShadow.DrawOutside(graphics, destRect, DropShadow.GetRecommendedExtent(destRect.Size));
                }
                image.Dispose();
                this.Image = bitmap2;
            }
        }

        public void RefreshPreview()
        {
            if ((this.suspendPreviewUpdates <= 0) && base.IsHandleCreated)
            {
                this.thumbnailManager.QueueThumbnailUpdate(this.layer, this.thumbnailSize, new EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>(this.OnThumbnailRendered));
            }
        }

        public void ResumePreviewUpdates()
        {
            this.suspendPreviewUpdates--;
        }

        private void SetColors()
        {
            HighlightState hover;
            if (this.isSelected)
            {
                hover = HighlightState.Checked;
            }
            else if (this.isMouseOver)
            {
                hover = HighlightState.Hover;
            }
            else
            {
                hover = HighlightState.Default;
            }
            Color selectionForeColor = SelectionHighlight.GetSelectionForeColor(hover);
            SelectionHighlight.GetSelectionBackColor(hover);
            this.layerDescription.ForeColor = selectionForeColor;
        }

        public void SuspendPreviewUpdates()
        {
            this.suspendPreviewUpdates++;
        }

        public System.Drawing.Image Image
        {
            get => 
                this.icon.Image;
            set
            {
                if (this.icon.Image != null)
                {
                    this.icon.Image.Dispose();
                    this.icon.Image = null;
                }
                this.icon.Image = value;
                base.Invalidate(true);
                base.Update();
            }
        }

        public bool IsSelected
        {
            get => 
                this.isSelected;
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    this.SetColors();
                    base.Invalidate(true);
                    base.Update();
                }
            }
        }

        public PaintDotNet.Layer Layer
        {
            get => 
                this.layer;
            set
            {
                if (!object.ReferenceEquals(this.layer, value))
                {
                    if (this.layer != null)
                    {
                        this.layer.PropertyChanged -= this.layerPropertyChangedDelegate;
                        this.layer.Invalidated -= new InvalidateEventHandler(this.Layer_Invalidated);
                    }
                    this.layer = value;
                    if (this.layer != null)
                    {
                        this.layer.PropertyChanged += this.layerPropertyChangedDelegate;
                        this.layer.Invalidated += new InvalidateEventHandler(this.Layer_Invalidated);
                        this.layerPropertyChangedDelegate(this.layer, new PropertyEventArgs(""));
                        if (this.layer.IsBackground)
                        {
                            this.layerDescription.Font = new Font(this.layerDescription.Font.FontFamily, this.layerDescription.Font.Size, this.layerDescription.Font.Style | FontStyle.Italic);
                        }
                        this.RefreshPreview();
                    }
                    base.Update();
                }
            }
        }

        public CheckBox LayerVisible =>
            this.layerVisible;

        public PaintDotNet.ThumbnailManager ThumbnailManager
        {
            get => 
                this.thumbnailManager;
            set
            {
                this.thumbnailManager = value;
            }
        }

        public int ThumbnailSize
        {
            get => 
                this.thumbnailSize;
            set
            {
                if (this.thumbnailSize != value)
                {
                    this.thumbnailSize = value;
                    this.RefreshPreview();
                }
            }
        }
    }
}

