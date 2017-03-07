namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Menus;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.VisualStyling;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class PdnToolBar : Control, IPaintBackground, IGlassyControl
    {
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private PaintDotNet.Controls.CommonActionsStrip commonActionsStrip;
        private bool computedMaxRowHeight;
        private ArrowButton documentListButton;
        private PdnToolBarDocumentStrip documentStrip;
        private IMessageFilter glassWndProcFilter;
        private DateTime ignoreShowDocumentListUntil = DateTime.MinValue;
        private ImageListMenu imageListMenu;
        private PdnMainMenu mainMenu;
        private int maxRowHeight = -1;
        private PdnToolBarStripRenderer otsr = new PdnToolBarStripRenderer();
        private GraphicsPath outlinePath;
        private int outlinePathClientWidth;
        private int outlinePathHInset;
        private int outlinePathOpaqueWidth;
        private Region outlineRegion;
        private PenBrushCache penBrushCache = PenBrushCache.ThreadInstance;
        private PaintDotNet.Controls.ToolChooserStrip toolChooserStrip;
        private PaintDotNet.Controls.ToolConfigStrip toolConfigStrip;
        private ToolStripPanel toolStripPanel;
        private const ToolStripGripStyle toolStripsGripStyle = ToolStripGripStyle.Hidden;
        private PaintDotNet.Controls.ViewConfigStrip viewConfigStrip;

        public PdnToolBar()
        {
            this.DoubleBuffered = true;
            base.SuspendLayout();
            this.InitializeComponent();
            this.toolChooserStrip.SetTools(DocumentWorkspace.ToolInfos);
            this.otsr = new PdnToolBarStripRenderer();
            this.commonActionsStrip.Renderer = this.otsr;
            this.viewConfigStrip.Renderer = this.otsr;
            this.toolStripPanel.Renderer = this.otsr;
            this.toolChooserStrip.Renderer = this.otsr;
            this.toolConfigStrip.Renderer = this.otsr;
            this.mainMenu.Renderer = this.otsr;
            this.documentListButton.ArrowImage = PdnResources.GetImageResource2("Images.ToolBar.ImageListMenu.OpenButton.png").Reference;
            base.ResumeLayout(false);
        }

        private void AsyncPerformLayout()
        {
            Action method = null;
            if (base.IsHandleCreated)
            {
                try
                {
                    if (method == null)
                    {
                        method = delegate {
                            try
                            {
                                base.PerformLayout();
                            }
                            catch (Exception)
                            {
                            }
                        };
                    }
                    base.BeginInvoke(method);
                }
                catch (Exception)
                {
                }
            }
        }

        private void AsyncPerformLayout(object sender, EventArgs e)
        {
            this.AsyncPerformLayout();
        }

        private void DocumentListButton_Click(object sender, EventArgs e)
        {
            if (this.imageListMenu.IsImageListVisible)
            {
                this.HideDocumentList();
            }
            else
            {
                this.ShowDocumentList();
            }
        }

        private void DocumentStrip_DocumentClicked(object sender, EventArgs<Pair<DocumentWorkspace, DocumentClickAction>> e)
        {
            if (((DocumentClickAction) e.Data.Second) == DocumentClickAction.Select)
            {
                base.PerformLayout();
            }
        }

        private void DocumentStrip_DocumentListChanged(object sender, EventArgs e)
        {
            base.PerformLayout();
            if (this.documentStrip.DocumentCount == 0)
            {
                this.viewConfigStrip.Enabled = false;
                this.toolChooserStrip.Enabled = false;
                this.toolConfigStrip.Enabled = false;
            }
            else
            {
                this.viewConfigStrip.Enabled = true;
                this.toolChooserStrip.Enabled = true;
                this.toolConfigStrip.Enabled = true;
            }
        }

        public void HideDocumentList()
        {
            this.imageListMenu.HideImageList();
        }

        private void ImageListMenu_Closed(object sender, EventArgs e)
        {
            this.documentListButton.ForcedPushedAppearance = false;
            this.ignoreShowDocumentListUntil = DateTime.Now + new TimeSpan(0, 0, 0, 0, 250);
        }

        private void ImageListMenu_ItemClicked(object sender, EventArgs<ImageListMenu.Item> e)
        {
            DocumentWorkspace tag = (DocumentWorkspace) e.Data.Tag;
            if (!tag.IsDisposed)
            {
                this.documentStrip.SelectedDocument = tag;
            }
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            this.mainMenu = new PdnMainMenu();
            this.toolStripPanel = new ToolStripPanel();
            this.commonActionsStrip = new PaintDotNet.Controls.CommonActionsStrip();
            this.viewConfigStrip = new PaintDotNet.Controls.ViewConfigStrip();
            this.toolChooserStrip = new PaintDotNet.Controls.ToolChooserStrip();
            this.toolConfigStrip = new PaintDotNet.Controls.ToolConfigStrip();
            this.documentStrip = new PdnToolBarDocumentStrip();
            this.documentListButton = new ArrowButton();
            this.imageListMenu = new ImageListMenu();
            this.toolStripPanel.BeginInit();
            this.toolStripPanel.SuspendLayout();
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Dock = DockStyle.None;
            this.toolStripPanel.AutoSize = false;
            this.toolStripPanel.Name = "toolStripPanel";
            this.toolStripPanel.TabIndex = 0;
            this.toolStripPanel.TabStop = false;
            this.toolStripPanel.Join(this.viewConfigStrip);
            this.toolStripPanel.Join(this.commonActionsStrip);
            this.toolStripPanel.Join(this.toolConfigStrip);
            this.toolStripPanel.Join(this.toolChooserStrip);
            this.commonActionsStrip.Name = "commonActionsStrip";
            this.commonActionsStrip.AutoSize = false;
            this.commonActionsStrip.TabIndex = 0;
            this.commonActionsStrip.Dock = DockStyle.None;
            this.commonActionsStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.viewConfigStrip.Name = "viewConfigStrip";
            this.viewConfigStrip.AutoSize = false;
            this.viewConfigStrip.ZoomBasis = ZoomBasis.FitToWindow;
            this.viewConfigStrip.TabStop = false;
            this.viewConfigStrip.DrawGrid = false;
            this.viewConfigStrip.TabIndex = 1;
            this.viewConfigStrip.Dock = DockStyle.None;
            this.viewConfigStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolChooserStrip.Name = "toolChooserStrip";
            this.toolChooserStrip.AutoSize = false;
            this.toolChooserStrip.TabIndex = 2;
            this.toolChooserStrip.Dock = DockStyle.None;
            this.toolChooserStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolChooserStrip.ChooseDefaultsClicked += new EventHandler(this.ToolChooserStrip_ChooseDefaultsClicked);
            this.toolConfigStrip.Name = "drawConfigStrip";
            this.toolConfigStrip.AutoSize = false;
            this.toolConfigStrip.ShapeDrawType = ShapeDrawType.Outline;
            this.toolConfigStrip.TabIndex = 3;
            this.toolConfigStrip.Dock = DockStyle.None;
            this.toolConfigStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.documentStrip.AutoSize = false;
            this.documentStrip.Name = "documentStrip";
            this.documentStrip.TabIndex = 5;
            this.documentStrip.ShowScrollButtons = true;
            this.documentStrip.DocumentListChanged += new EventHandler(this.DocumentStrip_DocumentListChanged);
            this.documentStrip.DocumentClicked += new EventHandler<EventArgs<Pair<DocumentWorkspace, DocumentClickAction>>>(this.DocumentStrip_DocumentClicked);
            this.documentStrip.ManagedFocus = true;
            this.documentStrip.DocumentListChanged += (s, e) => base.Invalidate();
            this.documentListButton.Name = "documentListButton";
            this.documentListButton.ArrowDirection = ArrowDirection.Down;
            this.documentListButton.ReverseArrowColors = true;
            this.documentListButton.Click += new EventHandler(this.DocumentListButton_Click);
            this.imageListMenu.Name = "imageListMenu";
            this.imageListMenu.Closed += new EventHandler(this.ImageListMenu_Closed);
            this.imageListMenu.ItemClicked += new EventHandler<EventArgs<ImageListMenu.Item>>(this.ImageListMenu_ItemClicked);
            base.Controls.Add(this.documentListButton);
            base.Controls.Add(this.documentStrip);
            base.Controls.Add(this.toolStripPanel);
            base.Controls.Add(this.mainMenu);
            base.Controls.Add(this.imageListMenu);
            this.toolStripPanel.EndInit();
            this.toolStripPanel.ResumeLayout(true);
            base.ResumeLayout(false);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            PdnBaseForm form = base.FindForm() as PdnBaseForm;
            if (((form != null) && form.MaximizeBox) && ((form.WindowState != FormWindowState.Minimized) && form.IsGlassEffectivelyEnabled))
            {
                using (PdnRegion region = form.CreateGlassInsetRegion())
                {
                    Point mousePosition = Control.MousePosition;
                    Point location = base.PointToClient(mousePosition);
                    if (region.IsVisible(new Rectangle(location, new Size(1, 1))))
                    {
                        FormWindowState maximized;
                        switch (form.WindowState)
                        {
                            case FormWindowState.Normal:
                                maximized = FormWindowState.Maximized;
                                break;

                            case FormWindowState.Maximized:
                                maximized = FormWindowState.Normal;
                                break;

                            default:
                                throw new InternalErrorException(new InvalidEnumArgumentException("form.WindowState", (int) form.WindowState, typeof(FormWindowState)));
                        }
                        form.WindowState = maximized;
                    }
                }
            }
            base.OnDoubleClick(e);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (base.Visible)
            {
                bool flag = ThemeConfig.EffectiveTheme == PdnTheme.Aero;
                bool flag2 = (((this.mainMenu.Width >= this.mainMenu.PreferredSize.Width) && (this.commonActionsStrip.Width >= this.commonActionsStrip.PreferredSize.Width)) && ((this.viewConfigStrip.Width >= this.viewConfigStrip.PreferredSize.Width) && (this.toolChooserStrip.Width >= this.toolChooserStrip.PreferredSize.Width))) && (this.toolConfigStrip.Width >= this.toolConfigStrip.PreferredSize.Width);
                Rectangle bounds = this.documentStrip.Bounds;
                if (!flag2)
                {
                    UI.SuspendControlPainting(this);
                }
                else
                {
                    UI.SuspendControlPainting(this.documentStrip);
                }
                this.mainMenu.Location = new Point(0, 0);
                this.mainMenu.Height = this.mainMenu.PreferredSize.Height + (flag ? 3 : 0);
                this.toolStripPanel.Location = new Point(0, this.mainMenu.Bottom);
                this.mainMenu.Padding = new Padding(flag ? 4 : 0, this.mainMenu.Padding.Top, 0, this.mainMenu.Padding.Bottom - (flag ? 1 : 0));
                this.commonActionsStrip.Width = this.commonActionsStrip.PreferredSize.Width;
                this.viewConfigStrip.Width = this.viewConfigStrip.PreferredSize.Width;
                this.toolChooserStrip.Width = this.toolChooserStrip.PreferredSize.Width;
                this.toolConfigStrip.Width = this.toolConfigStrip.PreferredSize.Width;
                if (!this.computedMaxRowHeight)
                {
                    ToolBarConfigItems toolBarConfigItems = this.toolConfigStrip.ToolBarConfigItems;
                    this.toolConfigStrip.ToolBarConfigItems = ~(ToolBarConfigItems.None | ToolBarConfigItems.Text);
                    this.toolConfigStrip.PerformLayout();
                    this.maxRowHeight = Math.Max(this.commonActionsStrip.PreferredSize.Height, Math.Max(this.viewConfigStrip.PreferredSize.Height, Math.Max(this.toolChooserStrip.PreferredSize.Height, this.toolConfigStrip.PreferredSize.Height)));
                    this.toolConfigStrip.ToolBarConfigItems = toolBarConfigItems;
                    this.toolConfigStrip.PerformLayout();
                    this.computedMaxRowHeight = true;
                }
                this.commonActionsStrip.Height = this.maxRowHeight;
                this.viewConfigStrip.Height = this.maxRowHeight;
                this.toolChooserStrip.Height = this.maxRowHeight;
                this.toolConfigStrip.Height = this.maxRowHeight;
                this.commonActionsStrip.Location = new Point(this.toolStripPanel.RowMargin.Left, this.toolStripPanel.RowMargin.Top);
                this.viewConfigStrip.Location = new Point(this.commonActionsStrip.Right, this.commonActionsStrip.Top);
                this.toolChooserStrip.Location = new Point(this.toolStripPanel.RowMargin.Left, this.viewConfigStrip.Bottom);
                this.toolConfigStrip.Location = new Point(this.toolChooserStrip.Right, this.toolChooserStrip.Top);
                this.toolStripPanel.Height = Math.Max(this.commonActionsStrip.Bottom, Math.Max(this.viewConfigStrip.Bottom, Math.Max(this.toolChooserStrip.Bottom, this.toolConfigStrip.Visible ? this.toolConfigStrip.Bottom : this.toolChooserStrip.Bottom)));
                int num = (((this.commonActionsStrip.Left + this.commonActionsStrip.PreferredSize.Width) + this.commonActionsStrip.Margin.Horizontal) + this.viewConfigStrip.PreferredSize.Width) + this.viewConfigStrip.Margin.Horizontal;
                int num2 = (((this.toolChooserStrip.Left + this.toolChooserStrip.PreferredSize.Width) + this.toolChooserStrip.Margin.Horizontal) + this.toolConfigStrip.PreferredSize.Width) + this.toolConfigStrip.Margin.Horizontal;
                int num3 = Math.Max(num, num2);
                bool flag3 = this.documentStrip.DocumentCount > 0;
                this.documentListButton.Visible = flag3;
                this.documentListButton.Enabled = flag3;
                if (flag3)
                {
                    int num4 = UI.ScaleWidth(15);
                    this.documentListButton.Width = num4;
                }
                else
                {
                    this.documentListButton.Width = 0;
                }
                if (this.documentStrip.DocumentCount == 0)
                {
                    this.documentStrip.Width = 0;
                }
                else
                {
                    this.documentStrip.Width = Math.Max(this.documentStrip.PreferredMinClientWidth, Math.Min(this.documentStrip.PreferredSize.Width, (base.ClientSize.Width - num3) - this.documentListButton.Width));
                }
                this.documentListButton.Location = new Point(base.ClientSize.Width - this.documentListButton.Width, 0);
                this.documentStrip.Location = new Point(this.documentListButton.Left - this.documentStrip.Width, 0);
                this.imageListMenu.Location = new Point(this.documentListButton.Left, this.documentListButton.Bottom - 1);
                this.imageListMenu.Width = this.documentListButton.Width;
                this.imageListMenu.Height = 0;
                this.documentListButton.Visible = flag3;
                this.documentListButton.Enabled = flag3;
                int height = this.documentStrip.Height;
                this.documentStrip.Height = this.toolStripPanel.Bottom;
                this.documentListButton.Height = this.documentStrip.Height;
                int num6 = base.ClientSize.Width - (this.documentStrip.Width + this.documentListButton.Width);
                this.mainMenu.Width = Math.Min(num6, this.mainMenu.PreferredSize.Width);
                this.toolStripPanel.Width = Math.Min(num3, num6);
                int num7 = UI.ScaleHeight(1);
                base.Height = this.toolStripPanel.Bottom + num7;
                this.documentStrip.PerformLayout();
                if (!flag2)
                {
                    UI.ResumeControlPainting(this);
                    base.Invalidate(true);
                }
                else
                {
                    UI.ResumeControlPainting(this.documentStrip);
                    this.documentStrip.Invalidate(true);
                }
                Rectangle rectangle2 = this.documentStrip.Bounds;
                if ((rectangle2 != bounds) && (rectangle2.Left > bounds.Left))
                {
                    base.Invalidate(new Rectangle(bounds.Left, bounds.Top, rectangle2.Left - bounds.Left, base.ClientSize.Height));
                }
                if (this.documentStrip.Width == 0)
                {
                    this.mainMenu.Invalidate();
                }
                if (height != this.documentStrip.Height)
                {
                    this.documentStrip.RefreshAllThumbnails();
                }
                base.OnLayout(e);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.PaintBackground(e.Graphics, e.ClipRectangle);
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.PerformLayout();
            base.OnResize(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            this.appWorkspace.AppEnvironment.PropertyChanged += new PropertyChangedEventHandler(this.AsyncPerformLayout);
            this.toolConfigStrip.ToolBarConfigItemsChanged += new EventHandler(this.AsyncPerformLayout);
            base.OnVisibleChanged(e);
        }

        public void PaintBackground(Graphics g, Rectangle clipRect)
        {
            if (clipRect.HasPositiveArea())
            {
                SmoothingMode smoothingMode = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.None;
                PixelOffsetMode pixelOffsetMode = g.PixelOffsetMode;
                g.PixelOffsetMode = PixelOffsetMode.None;
                InterpolationMode interpolationMode = g.InterpolationMode;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                Region clip = g.Clip;
                g.SetClip(clipRect, CombineMode.Replace);
                if (ThemeConfig.EffectiveTheme != PdnTheme.Aero)
                {
                    Color menuStripGradientEnd = ProfessionalColors.MenuStripGradientEnd;
                    g.FillRectangle(this.penBrushCache.GetSolidBrush(menuStripGradientEnd), clipRect);
                }
                else
                {
                    Color transparent;
                    Rectangle rectangle = new Rectangle(Point.Empty, base.ClientSize);
                    bool isCompositionEnabled = UI.IsCompositionEnabled;
                    int width = 8;
                    int x = this.mainMenu.Padding.Left - 2;
                    int num3 = 3 + Math.Min(this.documentStrip.Left, this.mainMenu.Left + this.mainMenu.PreferredSize.Width);
                    int num4 = isCompositionEnabled ? -1 : 0;
                    if (((rectangle.Width != this.outlinePathClientWidth) || (num3 != this.outlinePathOpaqueWidth)) || (num4 != this.outlinePathHInset))
                    {
                        if (this.outlinePath != null)
                        {
                            this.outlinePath.Dispose();
                            this.outlinePath = null;
                        }
                        if (this.outlineRegion != null)
                        {
                            this.outlineRegion.Dispose();
                            this.outlineRegion = null;
                        }
                    }
                    if (this.outlinePath == null)
                    {
                        this.outlinePathClientWidth = rectangle.Width;
                        this.outlinePathOpaqueWidth = num3;
                        this.outlinePathHInset = num4;
                        this.outlinePath = new GraphicsPath();
                        int y = this.GlassInset.Top - 1;
                        int num1 = rectangle.Height;
                        int num6 = (rectangle.Right - 1) - num4;
                        this.outlinePath.AddLine(new Point(num4, rectangle.Bottom - 1), new Point(num4, y + 1));
                        this.outlinePath.AddLine(new Point(num4, y + 1), new Point(num4 + 1, y));
                        this.outlinePath.AddLine(new Point(num4 + 1, y), new Point(x - 1, y));
                        this.outlinePath.AddLine(new Point(x - 1, y), new Point(x, y - 1));
                        this.outlinePath.AddArc(new Rectangle(x, 0, width, width), 180f, 90f);
                        this.outlinePath.AddLine(new Point(width + x, 0), new Point(((num3 - x) - width) - 1, 0));
                        this.outlinePath.AddArc(new Rectangle(((num3 - x) - width) - 1, 0, width, width + 1), -90f, 90f);
                        this.outlinePath.AddLine(new Point((num3 - x) - 1, width + 1), new Point((num3 - x) - 1, y - 1));
                        this.outlinePath.AddLine(new Point((num3 - x) - 1, y - 1), new Point(num3 - x, y));
                        this.outlinePath.AddLine(new Point(num3 - x, y), new Point(num6 - 1, y));
                        this.outlinePath.AddLine(new Point(num6 - 1, y), new Point(num6, y + 1));
                        this.outlinePath.AddLine(new Point(num6, y + 1), new Point(num6, rectangle.Bottom - 1));
                        this.outlinePath.AddLine(new Point(num6, rectangle.Bottom - 1), new Point(num4, rectangle.Bottom - 1));
                        this.outlineRegion = new Region(this.outlinePath);
                    }
                    if (isCompositionEnabled)
                    {
                        transparent = Color.Transparent;
                    }
                    else
                    {
                        bool flag3;
                        Form form = base.FindForm();
                        if (form == null)
                        {
                            flag3 = false;
                        }
                        else if ((Form.ActiveForm == form) || (form.OwnedForms.IndexOf<Form>(Form.ActiveForm) != -1))
                        {
                            flag3 = true;
                        }
                        else
                        {
                            flag3 = false;
                        }
                        transparent = flag3 ? SystemColors.GradientActiveCaption : SystemColors.GradientInactiveCaption;
                    }
                    Brush solidBrush = this.penBrushCache.GetSolidBrush(transparent);
                    CompositingMode compositingMode = g.CompositingMode;
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.FillRectangle(solidBrush, new Rectangle(0, 0, width + x, this.GlassInset.Top));
                    Rectangle b = new Rectangle(num3 - width, 0, (rectangle.Right - num3) + width, this.GlassInset.Top);
                    Rectangle rect = Rectangle.Intersect(clipRect, b);
                    if (rect.HasPositiveArea())
                    {
                        g.FillRectangle(solidBrush, rect);
                        g.CompositingMode = compositingMode;
                    }
                    solidBrush = null;
                    g.CompositingMode = compositingMode;
                    Rectangle rectangle4 = new Rectangle(0, 0, num3, this.mainMenu.Bottom);
                    if (Rectangle.Intersect(clipRect, rectangle4).HasPositiveArea())
                    {
                        Color color3 = Color.FromArgb(0xff, 0xfb, 0xfd, 0xff);
                        using (Region region2 = this.outlineRegion.Clone())
                        {
                            region2.Intersect(rectangle4);
                            g.FillRegion(this.penBrushCache.GetSolidBrush(color3), region2);
                        }
                    }
                    int height = 40;
                    Rectangle rectangle6 = new Rectangle(0, rectangle4.Bottom, rectangle.Width, height);
                    if (Rectangle.Intersect(clipRect, rectangle6).HasPositiveArea())
                    {
                        Color color4 = Color.FromArgb(0xff, 0xfb, 0xfd, 0xff);
                        Color color5 = Color.FromArgb(0xff, 220, 0xe7, 0xf5);
                        Rectangle rectangle8 = rectangle6;
                        rectangle8.Y--;
                        rectangle8.Height++;
                        using (LinearGradientBrush brush2 = new LinearGradientBrush(rectangle8, color4, color5, LinearGradientMode.Vertical))
                        {
                            using (Region region3 = this.outlineRegion.Clone())
                            {
                                region3.Intersect(rectangle6);
                                g.FillRegion(brush2, region3);
                            }
                        }
                    }
                    Rectangle rectangle9 = new Rectangle(rectangle6.Left, rectangle6.Bottom, rectangle6.Width, base.ClientSize.Height - rectangle6.Bottom);
                    if (Rectangle.Intersect(clipRect, rectangle9).HasPositiveArea())
                    {
                        Color color6 = Color.FromArgb(0xff, 220, 0xe7, 0xf5);
                        using (Region region4 = this.outlineRegion.Clone())
                        {
                            region4.Intersect(rectangle9);
                            g.FillRegion(this.penBrushCache.GetSolidBrush(color6), region4);
                        }
                    }
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    Color color = Color.FromArgb(0xff, 0x36, 0x5d, 0x90);
                    g.DrawPath(this.penBrushCache.GetPen(color), this.outlinePath);
                }
                g.Clip = clip;
                clip.Dispose();
                g.InterpolationMode = interpolationMode;
                g.PixelOffsetMode = pixelOffsetMode;
                g.SmoothingMode = smoothingMode;
            }
        }

        public void SetGlassWndProcFilter(IMessageFilter filter)
        {
            this.glassWndProcFilter = filter;
        }

        public void ShowDocumentList()
        {
            if (((this.documentStrip.DocumentCount >= 1) && (DateTime.Now >= this.ignoreShowDocumentListUntil)) && !this.imageListMenu.IsImageListVisible)
            {
                DocumentWorkspace[] documentList = this.documentStrip.DocumentList;
                Image[] documentThumbnails = this.documentStrip.DocumentThumbnails;
                ImageListMenu.Item[] items = new ImageListMenu.Item[this.documentStrip.DocumentCount];
                for (int i = 0; i < items.Length; i++)
                {
                    bool selected = documentList[i] == this.documentStrip.SelectedDocument;
                    items[i] = new ImageListMenu.Item(documentThumbnails[i], documentList[i].GetFriendlyName(), selected);
                    items[i].Tag = documentList[i];
                }
                Cursor.Current = Cursors.Default;
                this.documentListButton.ForcedPushedAppearance = true;
                this.imageListMenu.ShowImageList(items);
            }
        }

        private void ToolChooserStrip_ChooseDefaultsClicked(object sender, EventArgs e)
        {
            PdnBaseForm.UpdateAllForms();
            WaitCursorChanger wcc = new WaitCursorChanger(this);
            using (ChooseToolDefaultsDialog dialog = new ChooseToolDefaultsDialog())
            {
                EventHandler shownDelegate = null;
                shownDelegate = delegate (object sender2, EventArgs e2) {
                    wcc.Dispose();
                    wcc = null;
                    dialog.Shown -= shownDelegate;
                };
                dialog.Shown += shownDelegate;
                dialog.SetToolBarSettings(this.appWorkspace.GlobalToolTypeChoice, this.appWorkspace.AppEnvironment);
                AppEnvironment defaultAppEnvironment = AppEnvironment.GetDefaultAppEnvironment();
                try
                {
                    dialog.LoadUIFromAppEnvironment(defaultAppEnvironment);
                }
                catch (Exception)
                {
                    defaultAppEnvironment = new AppEnvironment();
                    defaultAppEnvironment.SetToDefaults();
                    dialog.LoadUIFromAppEnvironment(defaultAppEnvironment);
                }
                dialog.ToolType = this.appWorkspace.DefaultToolType;
                if (dialog.ShowDialog(this) != DialogResult.Cancel)
                {
                    AppEnvironment appEnvironment = dialog.CreateAppEnvironmentFromUI();
                    appEnvironment.SaveAsDefaultAppEnvironment();
                    this.appWorkspace.AppEnvironment.LoadFrom(appEnvironment);
                    this.appWorkspace.DefaultToolType = dialog.ToolType;
                    this.appWorkspace.GlobalToolTypeChoice = dialog.ToolType;
                }
            }
            if (wcc != null)
            {
                wcc.Dispose();
                wcc = null;
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

        public PaintDotNet.Controls.AppWorkspace AppWorkspace
        {
            get => 
                this.appWorkspace;
            set
            {
                this.appWorkspace = value;
                this.mainMenu.AppWorkspace = value;
            }
        }

        public PaintDotNet.Controls.CommonActionsStrip CommonActionsStrip =>
            this.commonActionsStrip;

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                if (OS.IsVistaOrLater)
                {
                    UI.AddCompositedToCP(createParams);
                }
                return createParams;
            }
        }

        public PaintDotNet.Controls.DocumentStrip DocumentStrip =>
            this.documentStrip;

        public Padding GlassInset =>
            new Padding(0, this.mainMenu.Height, 0, 0);

        public bool IsGlassDesired =>
            ((ThemeConfig.EffectiveTheme == PdnTheme.Aero) && UI.IsCompositionEnabled);

        public PdnMainMenu MainMenu =>
            this.mainMenu;

        public PaintDotNet.Controls.ToolChooserStrip ToolChooserStrip =>
            this.toolChooserStrip;

        public PaintDotNet.Controls.ToolConfigStrip ToolConfigStrip =>
            this.toolConfigStrip;

        public ToolStripPanel ToolStripContainer =>
            this.toolStripPanel;

        public PaintDotNet.Controls.ViewConfigStrip ViewConfigStrip =>
            this.viewConfigStrip;

        private class PdnToolBarDocumentStrip : DocumentStrip, IPaintBackground
        {
            protected override void DrawItemBackground(Graphics g, ImageStrip.Item item, Rectangle itemRect)
            {
                this.PaintBackground(g, itemRect);
            }

            protected override unsafe void DrawItemForeground(Graphics g, ImageStrip.Item item, Rectangle itemRect, Rectangle imageRect, Rectangle imageInsetRect, Rectangle closeButtonRect, Rectangle dirtyOverlayRect)
            {
                bool flag = base.LeftScrollButton.Visible && itemRect.IntersectsWith(base.LeftScrollButton.Bounds);
                bool flag2 = base.RightScrollButton.Visible && itemRect.IntersectsWith(base.RightScrollButton.Bounds);
                if (flag || flag2)
                {
                    using (Surface surface = new Surface(itemRect.Size))
                    {
                        surface.Clear(ColorBgra.Transparent);
                        using (RenderArgs args = new RenderArgs(surface))
                        {
                            int dx = -itemRect.Left;
                            int dy = -itemRect.Top;
                            base.DrawItemForeground(args.Graphics, item, RectangleUtil.Offset(itemRect, dx, dy), RectangleUtil.Offset(imageRect, dx, dy), RectangleUtil.Offset(imageInsetRect, dx, dy), RectangleUtil.Offset(closeButtonRect, dx, dy), RectangleUtil.Offset(dirtyOverlayRect, dx, dy));
                            Control control = flag ? base.LeftScrollButton : base.RightScrollButton;
                            Rectangle bounds = control.Bounds;
                            int num3 = bounds.Left + dx;
                            int num4 = bounds.Right + dx;
                            int num5 = num3;
                            int num6 = num4;
                            if (flag)
                            {
                                num5 += (num6 - num5) / 2;
                            }
                            else
                            {
                                num6 -= (num6 - num5) / 2;
                            }
                            for (int i = num3; i < num4; i++)
                            {
                                if (surface.IsColumnVisible(i))
                                {
                                    byte num8;
                                    if ((i >= num5) && (i < num6))
                                    {
                                        num8 = (byte) (((i - num5) * 0xff) / (num6 - num5));
                                        num8 = (byte) (0xff - ByteUtil.FastScale((byte) (0xff - num8), (byte) (0xff - num8)));
                                        if (flag2)
                                        {
                                            num8 = (byte) (0xff - num8);
                                        }
                                    }
                                    else
                                    {
                                        num8 = 0;
                                    }
                                    for (int j = 0; j < surface.Height; j++)
                                    {
                                        ColorBgra* pointAddress = surface.GetPointAddress(i, j);
                                        pointAddress->A = ByteUtil.FastScale(pointAddress->A, num8);
                                    }
                                }
                            }
                            CompositingMode compositingMode = g.CompositingMode;
                            g.CompositingMode = CompositingMode.SourceOver;
                            g.DrawImage(args.Bitmap, itemRect, new Rectangle(Point.Empty, surface.Size), GraphicsUnit.Pixel);
                            g.CompositingMode = compositingMode;
                        }
                        return;
                    }
                }
                base.DrawItemForeground(g, item, itemRect, imageRect, imageInsetRect, closeButtonRect, dirtyOverlayRect);
            }

            public void PaintBackground(Graphics g, Rectangle clipRect)
            {
                IPaintBackground parent = base.Parent as IPaintBackground;
                if (parent != null)
                {
                    Rectangle rectangle = new Rectangle(clipRect.Left + base.Left, clipRect.Top + base.Top, clipRect.Width, clipRect.Height);
                    g.TranslateTransform((float) -base.Left, (float) -base.Top, MatrixOrder.Append);
                    parent.PaintBackground(g, rectangle);
                    g.TranslateTransform((float) base.Left, (float) base.Top, MatrixOrder.Append);
                }
            }
        }

        private sealed class PdnToolBarStripRenderer : PdnToolStripRenderer
        {
            public PdnToolBarStripRenderer()
            {
                base.RoundedEdges = false;
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                if ((((e.Item is PdnMenuItem) && !e.Item.IsOnDropDown) && ((e.TextDirection == ToolStripTextDirection.Horizontal) && (ThemeConfig.EffectiveTheme == PdnTheme.Aero))) && UI.IsCompositionEnabled)
                {
                    PdnMenuItem item = (PdnMenuItem) e.Item;
                    Color c = Color.FromArgb(0xff, 0xfb, 0xfd, 0xff);
                    Color black = Color.Black;
                    using (Surface surface = new Surface(e.TextRectangle.Size))
                    {
                        surface.Clear(ColorBgra.FromColor(c));
                        using (RenderArgs args = new RenderArgs(surface))
                        {
                            TextRenderer.DrawText(args.Graphics, e.Text, e.TextFont, new Rectangle(Point.Empty, surface.Size), black, c, e.TextFormat);
                            if (item.Selected || item.Pressed)
                            {
                                SelectionHighlight.DrawBackground(args.Graphics, new Rectangle(-e.TextRectangle.X, -e.TextRectangle.Y, e.Item.Width, e.Item.Height), HighlightState.Hover);
                            }
                            CompositingMode compositingMode = e.Graphics.CompositingMode;
                            e.Graphics.CompositingMode = CompositingMode.SourceCopy;
                            e.Graphics.DrawImage(args.Bitmap, e.TextRectangle, new Rectangle(Point.Empty, surface.Size), GraphicsUnit.Pixel);
                            e.Graphics.CompositingMode = compositingMode;
                        }
                        return;
                    }
                }
                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                if (((ThemeConfig.EffectiveTheme == PdnTheme.Aero) || (e.ToolStrip.GetType() == typeof(ToolStrip))) || ((e.ToolStrip.GetType() == typeof(ToolStripEx)) || (e.ToolStrip.GetType() == typeof(PdnMainMenu))))
                {
                    this.PaintBackground(e.Graphics, e.ToolStrip, e.AffectedBounds);
                }
                else
                {
                    base.OnRenderToolStripBackground(e);
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                if ((ThemeConfig.EffectiveTheme != PdnTheme.Aero) || (e.ToolStrip is ToolStripDropDown))
                {
                    base.OnRenderToolStripBorder(e);
                }
            }

            protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
            {
                this.PaintBackground(e.Graphics, e.ToolStripPanel, new Rectangle(new Point(0, 0), e.ToolStripPanel.Size));
                e.Handled = true;
            }

            private void PaintBackground(Graphics g, Control control, Rectangle clipRect)
            {
                Control parent = control;
                IPaintBackground background = null;
                do
                {
                    parent = parent.Parent;
                    if (parent == null)
                    {
                        break;
                    }
                    background = parent as IPaintBackground;
                }
                while (background == null);
                if (background != null)
                {
                    Rectangle r = control.RectangleToScreen(clipRect);
                    Rectangle rectangle2 = parent.RectangleToClient(r);
                    int num = rectangle2.Left - clipRect.Left;
                    int num2 = rectangle2.Top - clipRect.Top;
                    g.TranslateTransform((float) -num, (float) -num2, MatrixOrder.Append);
                    background.PaintBackground(g, rectangle2);
                    g.TranslateTransform((float) num, (float) num2, MatrixOrder.Append);
                }
            }
        }
    }
}

