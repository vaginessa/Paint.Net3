namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Dialogs;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class HelpMenu : PdnMenuItem
    {
        private PdnMenuItem menuHelpAbout;
        private PdnMenuItem menuHelpDonate;
        private PdnMenuItem menuHelpForum;
        private PdnMenuItem menuHelpHelpTopics;
        private PdnMenuItem menuHelpPdnSearch;
        private PdnMenuItem menuHelpPdnWebsite;
        private PdnMenuItem menuHelpPlugins;
        private PdnMenuItem menuHelpSendFeedback;
        private ToolStripSeparator menuHelpSeparator1;
        private ToolStripSeparator menuHelpSeparator2;
        private PdnMenuItem menuHelpTutorials;

        public HelpMenu()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuHelpHelpTopics = new PdnMenuItem();
            this.menuHelpSeparator1 = new ToolStripSeparator();
            this.menuHelpPdnWebsite = new PdnMenuItem();
            this.menuHelpPdnSearch = new PdnMenuItem();
            this.menuHelpDonate = new PdnMenuItem();
            this.menuHelpForum = new PdnMenuItem();
            this.menuHelpTutorials = new PdnMenuItem();
            this.menuHelpPlugins = new PdnMenuItem();
            this.menuHelpSendFeedback = new PdnMenuItem();
            this.menuHelpSeparator2 = new ToolStripSeparator();
            this.menuHelpAbout = new PdnMenuItem();
            base.DropDownItems.AddRange(new ToolStripItem[] { this.menuHelpHelpTopics, this.menuHelpSeparator1, this.menuHelpPdnWebsite, this.menuHelpPdnSearch, this.menuHelpDonate, this.menuHelpForum, this.menuHelpTutorials, this.menuHelpPlugins, this.menuHelpSendFeedback, this.menuHelpSeparator2, this.menuHelpAbout });
            base.Name = "Menu.Help";
            this.Text = PdnResources.GetString2("Menu.Help.Text");
            this.menuHelpHelpTopics.Name = "HelpTopics";
            this.menuHelpHelpTopics.ShortcutKeys = Keys.F1;
            this.menuHelpHelpTopics.Click += new EventHandler(this.MenuHelpHelpTopics_Click);
            this.menuHelpPdnWebsite.Name = "PdnWebsite";
            this.menuHelpPdnWebsite.Click += new EventHandler(this.MenuHelpPdnWebsite_Click);
            this.menuHelpPdnSearch.Name = "PdnSearch";
            this.menuHelpPdnSearch.Click += new EventHandler(this.MenuHelpPdnSearchEngine_Click);
            this.menuHelpPdnSearch.ShortcutKeys = Keys.Control | Keys.E;
            this.menuHelpDonate.Name = "Donate";
            this.menuHelpDonate.Click += new EventHandler(this.MenuHelpDonate_Click);
            this.menuHelpDonate.Font = FontUtil.CreateGdipFont(this.menuHelpDonate.Font.Name, this.menuHelpDonate.Font.Size, this.menuHelpDonate.Font.Style | FontStyle.Italic);
            this.menuHelpForum.Name = "Forum";
            this.menuHelpForum.Click += new EventHandler(this.MenuHelpForum_Click);
            this.menuHelpTutorials.Name = "Tutorials";
            this.menuHelpTutorials.Click += new EventHandler(this.MenuHelpTutorials_Click);
            this.menuHelpPlugins.Name = "Plugins";
            this.menuHelpPlugins.Click += new EventHandler(this.MenuHelpPlugins_Click);
            this.menuHelpSendFeedback.Name = "SendFeedback";
            this.menuHelpSendFeedback.Click += new EventHandler(this.MenuHelpSendFeedback_Click);
            this.menuHelpAbout.Name = "About";
            this.menuHelpAbout.Click += new EventHandler(this.MenuHelpAbout_Click);
        }

        private void MenuHelpAbout_Click(object sender, EventArgs e)
        {
            AboutDialog dialog;
            using (new WaitCursorChanger(base.AppWorkspace))
            {
                dialog = new AboutDialog();
            }
            dialog.ShowDialog(base.AppWorkspace);
        }

        private void MenuHelpDonate_Click(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite2(base.AppWorkspace, "/redirect/donate_hm.html");
        }

        private void MenuHelpForum_Click(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite2(base.AppWorkspace, "/redirect/forum_hm.html");
        }

        private void MenuHelpHelpTopics_Click(object sender, EventArgs e)
        {
            Utility.ShowHelp(base.AppWorkspace);
        }

        private void MenuHelpPdnSearchEngine_Click(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite2(base.AppWorkspace, "/redirect/search_hm.html");
        }

        private void MenuHelpPdnWebsite_Click(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite2(base.AppWorkspace, "/redirect/main_hm.html");
        }

        private void MenuHelpPlugins_Click(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite2(base.AppWorkspace, "/redirect/plugins_hm.html");
        }

        private void MenuHelpSendFeedback_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new SendFeedbackAction());
        }

        private void MenuHelpTutorials_Click(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite2(base.AppWorkspace, "/redirect/tutorials_hm.html");
        }
    }
}

