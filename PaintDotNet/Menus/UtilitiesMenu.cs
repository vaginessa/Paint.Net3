namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;

    internal sealed class UtilitiesMenu : PdnMenuItem
    {
        private CheckForUpdatesMenuItem menuUtilitiesCheckForUpdates;
        private LanguageMenu menuUtilitiesLanguage;
        private PdnMenuItem menuUtilitiesManageFonts;
        private PdnMenuItem menuUtilitiesViewPluginLoadErrors;

        public UtilitiesMenu()
        {
            this.InitializeComponent();
        }

        public void CheckForUpdates()
        {
            this.menuUtilitiesCheckForUpdates.PerformClick();
        }

        private void InitializeComponent()
        {
            this.menuUtilitiesManageFonts = new PdnMenuItem();
            this.menuUtilitiesCheckForUpdates = new CheckForUpdatesMenuItem();
            this.menuUtilitiesLanguage = new LanguageMenu();
            this.menuUtilitiesViewPluginLoadErrors = new PdnMenuItem();
            this.menuUtilitiesManageFonts.Name = "ManageFonts";
            this.menuUtilitiesManageFonts.Click += new EventHandler(this.MenuUtilitiesManageFonts_Click);
            this.menuUtilitiesViewPluginLoadErrors.Name = "ViewPluginLoadErrors";
            this.menuUtilitiesViewPluginLoadErrors.Click += new EventHandler(this.MenuUtilitiesViewPluginLoadErrors_Click);
            base.DropDownItems.AddRange(new ToolStripItem[] { this.menuUtilitiesViewPluginLoadErrors, this.menuUtilitiesManageFonts, this.menuUtilitiesLanguage, this.menuUtilitiesCheckForUpdates });
            base.Name = "Menu.Utilities";
            this.Text = PdnResources.GetString2("Menu.Utilities.Text");
        }

        private void MenuUtilitiesManageFonts_Click(object sender, EventArgs e)
        {
            try
            {
                string virtualPath = Shell.GetVirtualPath(VirtualFolderName.SystemFonts, false);
                Shell.BrowseFolder2(base.AppWorkspace, virtualPath);
            }
            catch (Exception)
            {
            }
        }

        private void MenuUtilitiesViewPluginLoadErrors_Click(object sender, EventArgs e)
        {
            IList<Triple<Assembly, System.Type, Exception>> effectLoadErrors = base.AppWorkspace.GetEffectLoadErrors();
            using (ViewPluginLoadErrorsForm form = new ViewPluginLoadErrorsForm(this.RemoveDuplicates(effectLoadErrors)))
            {
                form.ShowDialog(base.AppWorkspace);
            }
        }

        protected override void OnAppWorkspaceChanged()
        {
            this.menuUtilitiesCheckForUpdates.AppWorkspace = base.AppWorkspace;
            this.menuUtilitiesLanguage.AppWorkspace = base.AppWorkspace;
            base.OnAppWorkspaceChanged();
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            this.menuUtilitiesLanguage.Enabled = !Shell.IsActivityQueuedForRestart;
            IList<Triple<Assembly, System.Type, Exception>> effectLoadErrors = base.AppWorkspace.GetEffectLoadErrors();
            this.menuUtilitiesViewPluginLoadErrors.Enabled = effectLoadErrors.Count > 0;
            base.OnDropDownOpening(e);
        }

        private List<Triple<Assembly, System.Type, Exception>> RemoveDuplicates(IList<Triple<Assembly, System.Type, Exception>> allErrors)
        {
            HashSet<Triple<Assembly, System.Type, string>> set = new HashSet<Triple<Assembly, System.Type, string>>();
            List<Triple<Assembly, System.Type, Exception>> list = new List<Triple<Assembly, System.Type, Exception>>();
            for (int i = 0; i < allErrors.Count; i++)
            {
                Triple<Assembly, System.Type, Exception> triple2 = allErrors[i];
                Triple<Assembly, System.Type, Exception> triple3 = allErrors[i];
                Triple<Assembly, System.Type, Exception> triple4 = allErrors[i];
                Triple<Assembly, System.Type, string> item = Triple.Create<Assembly, System.Type, string>(triple2.First, triple3.Second, string.Intern(triple4.Third.ToString()));
                if (!set.Contains(item))
                {
                    set.Add(item);
                    list.Add(allErrors[i]);
                }
            }
            return list;
        }

        internal sealed class ViewPluginLoadErrorsForm : PdnBaseForm
        {
            private List<Triple<Assembly, System.Type, Exception>> errors;
            private TextBox errorsBox;
            private Label messageLabel;

            public ViewPluginLoadErrorsForm(IEnumerable<Triple<Assembly, System.Type, Exception>> errors)
            {
                this.errors = errors.ToList<Triple<Assembly, System.Type, Exception>>();
                base.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuUtilitiesViewPluginLoadErrorsIcon.png").Reference);
                this.Text = PdnResources.GetString2("Effects.PluginLoadErrorsDialog.Text");
                this.messageLabel = new Label();
                this.messageLabel.Name = "messageLabel";
                this.messageLabel.Text = PdnResources.GetString2("Effects.PluginLoadErrorsDialog.Message.Text");
                this.errorsBox = new TextBox();
                this.errorsBox.Font = new Font(FontFamily.GenericMonospace, this.errorsBox.Font.Size);
                this.errorsBox.ReadOnly = true;
                this.errorsBox.Multiline = true;
                this.errorsBox.ScrollBars = ScrollBars.Vertical;
                StringBuilder builder = new StringBuilder();
                string format = PdnResources.GetString2("EffectErrorMessage.HeaderFormat");
                for (int i = 0; i < this.errors.Count; i++)
                {
                    Triple<Assembly, System.Type, Exception> triple = this.errors[i];
                    Assembly first = triple.First;
                    Triple<Assembly, System.Type, Exception> triple2 = this.errors[i];
                    System.Type second = triple2.Second;
                    Triple<Assembly, System.Type, Exception> triple3 = this.errors[i];
                    Exception third = triple3.Third;
                    string str2 = string.Format(format, i + 1, this.errors.Count);
                    string str3 = AppWorkspace.GetLocalizedEffectErrorMessage(first, second, third);
                    builder.Append(str2);
                    builder.Append(Environment.NewLine);
                    builder.Append(str3);
                    if (i != (this.errors.Count - 1))
                    {
                        builder.Append(Environment.NewLine);
                    }
                }
                this.errorsBox.Text = builder.ToString();
                base.StartPosition = FormStartPosition.CenterParent;
                base.ShowInTaskbar = false;
                base.MinimizeBox = false;
                base.Width *= 2;
                base.Size = UI.ScaleSize(base.Size);
                base.Controls.Add(this.messageLabel);
                base.Controls.Add(this.errorsBox);
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                int x = UI.ScaleWidth(8);
                int y = UI.ScaleHeight(8);
                int num3 = base.ClientSize.Width - (x * 2);
                this.messageLabel.Location = new Point(x, y);
                this.messageLabel.Width = num3;
                this.messageLabel.Size = this.messageLabel.GetPreferredSize(new Size(this.messageLabel.Width, 1));
                this.errorsBox.Location = new Point(x, this.messageLabel.Bottom + y);
                this.errorsBox.Width = num3;
                this.errorsBox.Height = (base.ClientSize.Height - y) - this.errorsBox.Top;
                base.OnLayout(levent);
            }
        }
    }
}

