namespace PaintDotNet.Menus
{
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Windows.Forms;

    internal sealed class PdnMainMenu : MenuStripEx
    {
        private AdjustmentsMenu adjustmentsMenu;
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private EditMenu editMenu;
        private EffectsMenu effectsMenu;
        private FileMenu fileMenu;
        private HelpMenu helpMenu;
        private ImageMenu imageMenu;
        private LayersMenu layersMenu;
        private UtilitiesMenu utilitiesMenu;
        private ViewMenu viewMenu;
        private WindowMenu windowMenu;

        public PdnMainMenu()
        {
            this.InitializeComponent();
        }

        public void CheckForUpdates()
        {
            this.utilitiesMenu.CheckForUpdates();
        }

        private void InitializeComponent()
        {
            this.fileMenu = new FileMenu();
            this.editMenu = new EditMenu();
            this.viewMenu = new ViewMenu();
            this.imageMenu = new ImageMenu();
            this.adjustmentsMenu = new AdjustmentsMenu();
            this.effectsMenu = new EffectsMenu();
            this.layersMenu = new LayersMenu();
            this.utilitiesMenu = new UtilitiesMenu();
            this.windowMenu = new WindowMenu();
            this.helpMenu = new HelpMenu();
            base.SuspendLayout();
            base.Name = "PdnMainMenu";
            base.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.Items.AddRange(new ToolStripItem[] { this.fileMenu, this.editMenu, this.viewMenu, this.imageMenu, this.layersMenu, this.adjustmentsMenu, this.effectsMenu, this.utilitiesMenu, this.windowMenu, this.helpMenu });
            base.ResumeLayout();
        }

        public void PopulateEffects()
        {
            this.adjustmentsMenu.PopulateEffects();
            this.effectsMenu.PopulateEffects();
        }

        public void RunEffect(System.Type effectType)
        {
            this.adjustmentsMenu.RunEffect(effectType);
        }

        public PaintDotNet.Controls.AppWorkspace AppWorkspace
        {
            get => 
                this.appWorkspace;
            set
            {
                this.appWorkspace = value;
                this.fileMenu.AppWorkspace = value;
                this.editMenu.AppWorkspace = value;
                this.viewMenu.AppWorkspace = value;
                this.imageMenu.AppWorkspace = value;
                this.layersMenu.AppWorkspace = value;
                this.adjustmentsMenu.AppWorkspace = value;
                this.effectsMenu.AppWorkspace = value;
                this.utilitiesMenu.AppWorkspace = value;
                this.windowMenu.AppWorkspace = value;
                this.helpMenu.AppWorkspace = value;
            }
        }
    }
}

