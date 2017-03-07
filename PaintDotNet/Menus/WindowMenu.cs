namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Dialogs;
    using System;
    using System.Windows.Forms;

    internal sealed class WindowMenu : PdnMenuItem
    {
        private PdnMenuItem menuWindowColors;
        private PdnMenuItem menuWindowGlassDialogButtons;
        private PdnMenuItem menuWindowHistory;
        private PdnMenuItem menuWindowLayers;
        private PdnMenuItem menuWindowNextTab;
        private PdnMenuItem menuWindowOpenMdiList;
        private PdnMenuItem menuWindowPreviousTab;
        private PdnMenuItem menuWindowResetWindowLocations;
        private ToolStripSeparator menuWindowSeparator3;
        private ToolStripSeparator menuWindowSeperator1;
        private ToolStripSeparator menuWindowSeperator2;
        private PdnMenuItem menuWindowTools;
        private PdnMenuItem menuWindowTranslucent;

        public WindowMenu()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuWindowResetWindowLocations = new PdnMenuItem();
            this.menuWindowSeperator1 = new ToolStripSeparator();
            this.menuWindowTranslucent = new PdnMenuItem();
            this.menuWindowSeperator2 = new ToolStripSeparator();
            this.menuWindowTools = new PdnMenuItem();
            this.menuWindowHistory = new PdnMenuItem();
            this.menuWindowLayers = new PdnMenuItem();
            this.menuWindowColors = new PdnMenuItem();
            this.menuWindowOpenMdiList = new PdnMenuItem();
            this.menuWindowNextTab = new PdnMenuItem();
            this.menuWindowPreviousTab = new PdnMenuItem();
            this.menuWindowGlassDialogButtons = new PdnMenuItem();
            this.menuWindowSeparator3 = new ToolStripSeparator();
            base.DropDownItems.AddRange(new ToolStripItem[] { this.menuWindowTools, this.menuWindowHistory, this.menuWindowLayers, this.menuWindowColors, this.menuWindowSeperator1, this.menuWindowOpenMdiList, this.menuWindowNextTab, this.menuWindowPreviousTab, this.menuWindowSeperator2, this.menuWindowTranslucent, this.menuWindowGlassDialogButtons, this.menuWindowSeparator3, this.menuWindowResetWindowLocations });
            base.Name = "Menu.Window";
            this.Text = PdnResources.GetString2("Menu.Window.Text");
            this.menuWindowResetWindowLocations.Name = "ResetWindowLocations";
            this.menuWindowResetWindowLocations.Click += new EventHandler(this.MenuWindowResetWindowLocations_Click);
            this.menuWindowTranslucent.Name = "Translucent";
            this.menuWindowTranslucent.Click += new EventHandler(this.MenuWindowTranslucent_Click);
            this.menuWindowGlassDialogButtons.Name = "GlassDialogButtons";
            this.menuWindowGlassDialogButtons.Click += new EventHandler(this.MenuWindowGlassDialogButtons_Click);
            this.menuWindowTools.Name = "Tools";
            this.menuWindowTools.ShortcutKeys = Keys.F5;
            this.menuWindowTools.Click += new EventHandler(this.MenuWindowTools_Click);
            this.menuWindowHistory.Name = "History";
            this.menuWindowHistory.ShortcutKeys = Keys.F6;
            this.menuWindowHistory.Click += new EventHandler(this.MenuWindowHistory_Click);
            this.menuWindowLayers.Name = "Layers";
            this.menuWindowLayers.ShortcutKeys = Keys.F7;
            this.menuWindowLayers.Click += new EventHandler(this.MenuWindowLayers_Click);
            this.menuWindowColors.Name = "Colors";
            this.menuWindowColors.ShortcutKeys = Keys.F8;
            this.menuWindowColors.Click += new EventHandler(this.MenuWindowColors_Click);
            this.menuWindowOpenMdiList.Name = "OpenMdiList";
            this.menuWindowOpenMdiList.ShortcutKeys = Keys.Control | Keys.Q;
            this.menuWindowOpenMdiList.Click += new EventHandler(this.MenuWindowOpenMdiList_Click);
            this.menuWindowNextTab.Name = "NextTab";
            this.menuWindowNextTab.ShortcutKeys = Keys.Control | Keys.Tab;
            this.menuWindowNextTab.Click += new EventHandler(this.MenuWindowNextTab_Click);
            this.menuWindowPreviousTab.Name = "PreviousTab";
            this.menuWindowPreviousTab.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Tab;
            this.menuWindowPreviousTab.Click += new EventHandler(this.MenuWindowPreviousTab_Click);
        }

        private void MenuWindowColors_Click(object sender, EventArgs e)
        {
            this.ToggleFormVisibility(base.AppWorkspace.Widgets.ColorsForm);
        }

        private void MenuWindowGlassDialogButtons_Click(object sender, EventArgs e)
        {
            PdnBaseForm.EnableAutoGlass = !PdnBaseForm.EnableAutoGlass;
        }

        private void MenuWindowHistory_Click(object sender, EventArgs e)
        {
            this.ToggleFormVisibility(base.AppWorkspace.Widgets.HistoryForm);
        }

        private void MenuWindowLayers_Click(object sender, EventArgs e)
        {
            this.ToggleFormVisibility(base.AppWorkspace.Widgets.LayerForm);
        }

        private void MenuWindowNextTab_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.ToolBar.DocumentStrip.NextTab();
        }

        private void MenuWindowOpenMdiList_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.ToolBar.ShowDocumentList();
        }

        private void MenuWindowPreviousTab_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.ToolBar.DocumentStrip.PreviousTab();
        }

        private void MenuWindowResetWindowLocations_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.ResetFloatingForms();
            base.AppWorkspace.Widgets.ToolsForm.Visible = true;
            base.AppWorkspace.Widgets.HistoryForm.Visible = true;
            base.AppWorkspace.Widgets.LayerForm.Visible = true;
            base.AppWorkspace.Widgets.ColorsForm.Visible = true;
        }

        private void MenuWindowTools_Click(object sender, EventArgs e)
        {
            this.ToggleFormVisibility(base.AppWorkspace.Widgets.ToolsForm);
        }

        private void MenuWindowTranslucent_Click(object sender, EventArgs e)
        {
            PdnBaseForm.EnableOpacity = !PdnBaseForm.EnableOpacity;
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            bool isGlassEffectivelyEnabled = false;
            Form form = base.AppWorkspace.FindForm();
            if (form != null)
            {
                PdnBaseForm form2 = form as PdnBaseForm;
                if (form2 != null)
                {
                    isGlassEffectivelyEnabled = form2.IsGlassEffectivelyEnabled;
                }
            }
            this.menuWindowGlassDialogButtons.Enabled = isGlassEffectivelyEnabled;
            this.menuWindowGlassDialogButtons.Checked = this.menuWindowGlassDialogButtons.Enabled && PdnBaseForm.EnableAutoGlass;
            this.menuWindowTranslucent.Checked = PdnBaseForm.EnableOpacity;
            this.menuWindowTools.Checked = base.AppWorkspace.Widgets.ToolsForm.Visible;
            this.menuWindowHistory.Checked = base.AppWorkspace.Widgets.HistoryForm.Visible;
            this.menuWindowLayers.Checked = base.AppWorkspace.Widgets.LayerForm.Visible;
            this.menuWindowColors.Checked = base.AppWorkspace.Widgets.ColorsForm.Visible;
            this.menuWindowOpenMdiList.Enabled = base.AppWorkspace.DocumentWorkspaces.Length > 0;
            bool flag2 = base.AppWorkspace.DocumentWorkspaces.Length > 1;
            this.menuWindowNextTab.Enabled = flag2;
            this.menuWindowPreviousTab.Enabled = flag2;
            base.OnDropDownOpening(e);
        }

        private void ToggleFormVisibility(FloatingToolForm ftf)
        {
            ftf.Visible = !ftf.Visible;
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.Focus();
            }
        }
    }
}

