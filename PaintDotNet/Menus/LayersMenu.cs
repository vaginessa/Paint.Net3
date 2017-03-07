namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Controls;
    using PaintDotNet.Effects;
    using PaintDotNet.HistoryFunctions;
    using System;
    using System.Windows.Forms;

    internal sealed class LayersMenu : PdnMenuItem
    {
        private PdnMenuItem menuLayersAddNewLayer;
        private PdnMenuItem menuLayersDeleteLayer;
        private PdnMenuItem menuLayersDuplicateLayer;
        private PdnMenuItem menuLayersFlipHorizontal;
        private PdnMenuItem menuLayersFlipVertical;
        private PdnMenuItem menuLayersImportFromFile;
        private PdnMenuItem menuLayersLayerProperties;
        private PdnMenuItem menuLayersMergeLayerDown;
        private PdnMenuItem menuLayersRotateZoom;
        private ToolStripSeparator menuLayersSeparator1;
        private ToolStripSeparator menuLayersSeparator2;

        public LayersMenu()
        {
            this.InitializeComponent();
            string staticName = RotateZoomEffect.StaticName;
            Keys staticShortcutKeys = RotateZoomEffect.StaticShortcutKeys;
            ImageResource staticImage = RotateZoomEffect.StaticImage;
            string str3 = string.Format(PdnResources.GetString2("Effects.Name.Format.Configurable"), staticName);
            this.menuLayersRotateZoom.Text = str3;
            this.menuLayersRotateZoom.SetIcon(staticImage);
            this.menuLayersRotateZoom.ShortcutKeys = staticShortcutKeys;
        }

        private void InitializeComponent()
        {
            this.menuLayersAddNewLayer = new PdnMenuItem();
            this.menuLayersDeleteLayer = new PdnMenuItem();
            this.menuLayersDuplicateLayer = new PdnMenuItem();
            this.menuLayersMergeLayerDown = new PdnMenuItem();
            this.menuLayersImportFromFile = new PdnMenuItem();
            this.menuLayersSeparator1 = new ToolStripSeparator();
            this.menuLayersFlipHorizontal = new PdnMenuItem();
            this.menuLayersFlipVertical = new PdnMenuItem();
            this.menuLayersRotateZoom = new PdnMenuItem();
            this.menuLayersSeparator2 = new ToolStripSeparator();
            this.menuLayersLayerProperties = new PdnMenuItem();
            base.DropDownItems.AddRange(new ToolStripItem[] { this.menuLayersAddNewLayer, this.menuLayersDeleteLayer, this.menuLayersDuplicateLayer, this.menuLayersMergeLayerDown, this.menuLayersImportFromFile, this.menuLayersSeparator1, this.menuLayersFlipHorizontal, this.menuLayersFlipVertical, this.menuLayersRotateZoom, this.menuLayersSeparator2, this.menuLayersLayerProperties });
            base.Name = "Menu.Layers";
            this.Text = PdnResources.GetString2("Menu.Layers.Text");
            this.menuLayersAddNewLayer.Name = "AddNewLayer";
            this.menuLayersAddNewLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;
            this.menuLayersAddNewLayer.Click += new EventHandler(this.MenuLayersAddNewLayer_Click);
            this.menuLayersDeleteLayer.Name = "DeleteLayer";
            this.menuLayersDeleteLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Delete;
            this.menuLayersDeleteLayer.Click += new EventHandler(this.MenuLayersDeleteLayer_Click);
            this.menuLayersDuplicateLayer.Name = "DuplicateLayer";
            this.menuLayersDuplicateLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D;
            this.menuLayersDuplicateLayer.Click += new EventHandler(this.MenuLayersDuplicateLayer_Click);
            this.menuLayersMergeLayerDown.Name = "MergeLayerDown";
            this.menuLayersMergeLayerDown.ShortcutKeys = Keys.Control | Keys.M;
            this.menuLayersMergeLayerDown.Click += new EventHandler(this.MenuLayersMergeDown_Click);
            this.menuLayersImportFromFile.Name = "ImportFromFile";
            this.menuLayersImportFromFile.Click += new EventHandler(this.MenuLayersImportFromFile_Click);
            this.menuLayersFlipHorizontal.Name = "FlipHorizontal";
            this.menuLayersFlipHorizontal.Click += new EventHandler(this.MenuLayersFlipHorizontal_Click);
            this.menuLayersFlipVertical.Name = "FlipVertical";
            this.menuLayersFlipVertical.Click += new EventHandler(this.MenuLayersFlipVertical_Click);
            this.menuLayersRotateZoom.Name = "RotateZoom";
            this.menuLayersRotateZoom.Click += new EventHandler(this.MenuLayersRotateZoom_Click);
            this.menuLayersLayerProperties.Name = "LayerProperties";
            this.menuLayersLayerProperties.ShortcutKeys = Keys.F4;
            this.menuLayersLayerProperties.Click += new EventHandler(this.MenuLayersLayerProperties_Click);
        }

        private void MenuLayersAddNewLayer_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new AddNewBlankLayerFunction());
            }
        }

        private void MenuLayersDeleteLayer_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.Widgets.LayerForm.PerformDeleteLayerClick();
        }

        private void MenuLayersDuplicateLayer_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.Widgets.LayerForm.PerformDuplicateLayerClick();
        }

        private void MenuLayersFlipHorizontal_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new FlipLayerHorizontalFunction(base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void MenuLayersFlipVertical_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new FlipLayerVerticalFunction(base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex));
            }
        }

        private void MenuLayersImportFromFile_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ImportFromFileAction());
            }
        }

        private void MenuLayersLayerProperties_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.Widgets.LayerForm.PerformPropertiesClick();
            }
        }

        private void MenuLayersMergeDown_Click(object sender, EventArgs e)
        {
            if ((base.AppWorkspace.ActiveDocumentWorkspace != null) && (base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex > 0))
            {
                int num = (base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex - 1).Clamp(0, base.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count - 1);
                base.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new MergeLayerDownFunction(base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex));
                base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex = num;
            }
        }

        private void MenuLayersRotateZoom_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.RunEffect(typeof(RotateZoomEffect));
            }
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            bool flag = base.AppWorkspace.ActiveDocumentWorkspace != null;
            this.menuLayersAddNewLayer.Enabled = flag;
            if (((base.AppWorkspace.ActiveDocumentWorkspace != null) && (base.AppWorkspace.ActiveDocumentWorkspace.Document != null)) && (base.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count > 1))
            {
                this.menuLayersDeleteLayer.Enabled = true;
            }
            else
            {
                this.menuLayersDeleteLayer.Enabled = false;
            }
            this.menuLayersDuplicateLayer.Enabled = flag;
            bool flag2 = (base.AppWorkspace.ActiveDocumentWorkspace != null) && (base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex > 0);
            this.menuLayersMergeLayerDown.Enabled = flag2;
            this.menuLayersImportFromFile.Enabled = flag;
            this.menuLayersFlipHorizontal.Enabled = flag;
            this.menuLayersFlipVertical.Enabled = flag;
            this.menuLayersRotateZoom.Enabled = flag;
            this.menuLayersLayerProperties.Enabled = flag;
            base.OnDropDownOpening(e);
        }
    }
}

