namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal sealed class LanguageMenu : PdnMenuItem
    {
        public LanguageMenu()
        {
            base.Name = "Language";
            base.DropDownItems.Add(new ToolStripMenuItem("-sentinel-"));
        }

        private string GetCultureInfoName(CultureInfo ci)
        {
            CultureInfo info = new CultureInfo("en-US");
            if (ci.Equals(info))
            {
                return this.GetCultureInfoName(ci.Parent);
            }
            return ci.NativeName;
        }

        private void LanguageMenuItem_Click(object sender, EventArgs e)
        {
            string name = PdnResources.Culture.Name;
            ToolStripMenuItem item = (ToolStripMenuItem) sender;
            string tag = (string) item.Tag;
            PdnResources.SetNewCulture(tag);
            Icon icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuUtilitiesLanguageIcon.png").Reference);
            string str3 = PdnResources.GetString2("ConfirmLanguageDialog.Title");
            Image image = null;
            string str4 = PdnResources.GetString2("ConfirmLanguageDialog.IntroText");
            Image reference = PdnResources.GetImageResource2("Icons.RightArrowBlue.png").Reference;
            string format = PdnResources.GetString2("ConfirmLanguageDialog.RestartTB.ExplanationText.Format");
            CultureInfo parent = new CultureInfo(tag);
            CultureInfo info2 = new CultureInfo("en-US");
            if (parent.Equals(info2))
            {
                parent = parent.Parent;
            }
            string nativeName = parent.NativeName;
            string explanationText = string.Format(format, nativeName);
            TaskButton button = new TaskButton(reference, PdnResources.GetString2("ConfirmLanguageDialog.RestartTB.ActionText"), explanationText);
            TaskButton button2 = new TaskButton(PdnResources.GetImageResource2("Icons.CancelIcon.png").Reference, PdnResources.GetString2("ConfirmLanguageDialog.CancelTB.ActionText"), PdnResources.GetString2("ConfirmLanguageDialog.CancelTB.ExplanationText"));
            int num = (TaskDialog.DefaultPixelWidth96Dpi * 5) / 4;
            TaskDialog dialog = new TaskDialog {
                Icon = icon,
                Title = str3,
                TaskImage = image,
                ScaleTaskImageWithDpi = true,
                IntroText = str4,
                TaskButtons = new TaskButton[] { 
                    button,
                    button2
                },
                AcceptButton = button,
                CancelButton = button2,
                PixelWidth96Dpi = num
            };
            if (dialog.Show(base.AppWorkspace) == button)
            {
                if (Shell.IsActivityQueuedForRestart)
                {
                    Utility.ErrorBox(base.AppWorkspace, PdnResources.GetString2("Effect.PluginErrorDialog.CantQueue2ndRestart"));
                }
                else
                {
                    CloseAllWorkspacesAction action = new CloseAllWorkspacesAction();
                    action.PerformAction(base.AppWorkspace);
                    if (!action.Cancelled)
                    {
                        Shell.RestartApplication();
                        Startup.CloseApplication();
                    }
                }
            }
            else
            {
                PdnResources.SetNewCulture(name);
            }
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            base.DropDownItems.Clear();
            string[] installedLocales = PdnResources.GetInstalledLocales();
            MenuTitleAndLocale[] array = new MenuTitleAndLocale[installedLocales.Length];
            for (int i = 0; i < installedLocales.Length; i++)
            {
                string name = installedLocales[i];
                CultureInfo info = new CultureInfo(name);
                array[i] = new MenuTitleAndLocale(info.DisplayName, name);
            }
            Array.Sort<MenuTitleAndLocale>(array, (x, y) => string.Compare(x.title, y.title, StringComparison.InvariantCultureIgnoreCase));
            foreach (MenuTitleAndLocale locale in array)
            {
                ToolStripMenuItem item = new ToolStripMenuItem {
                    Text = this.GetCultureInfoName(new CultureInfo(locale.locale)),
                    Tag = locale.locale
                };
                item.Click += new EventHandler(this.LanguageMenuItem_Click);
                if (string.Compare(locale.locale, CultureInfo.CurrentUICulture.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    item.Checked = true;
                }
                if (Shell.IsActivityQueuedForRestart)
                {
                    item.Enabled = false;
                }
                base.DropDownItems.Add(item);
            }
            base.OnDropDownOpening(e);
        }

        private class MenuTitleAndLocale
        {
            public string locale;
            public string title;

            public MenuTitleAndLocale(string title, string locale)
            {
                this.title = title;
                this.locale = locale;
            }
        }
    }
}

