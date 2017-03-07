namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    internal class PdnMenuItem : ToolStripMenuItem, IFormAssociate
    {
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private bool iconsLoaded;
        private const char mnemonicPrefix = '&';
        private bool namesLoaded;
        private const char noMnemonicChar = '\0';
        private Keys registeredHotKey;
        private string textResourceName;

        public PdnMenuItem()
        {
            this.Constructor();
        }

        public PdnMenuItem(string name, Image image, EventHandler eventHandler) : base(name, image, eventHandler)
        {
            this.Constructor();
        }

        private void Constructor()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            base.DropDownOpening += new EventHandler(this.PdnMenuItem_DropDownOpening);
        }

        public void LoadIcons()
        {
            foreach (FieldInfo info in base.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (info.FieldType.IsSubclassOf(typeof(PdnMenuItem)) || (info.FieldType == typeof(PdnMenuItem)))
                {
                    char ch = info.Name[0];
                    string fileName = "Icons." + ch.ToString().ToUpper() + info.Name.Substring(1) + "Icon.png";
                    PdnMenuItem item = (PdnMenuItem) info.GetValue(this);
                    Stream stream = PdnResources.CreateResourceStream(fileName);
                    if (stream != null)
                    {
                        stream.Dispose();
                        item.SetIcon(fileName);
                    }
                }
            }
            this.iconsLoaded = true;
        }

        public void LoadNames(string baseName)
        {
            foreach (ToolStripItem item in base.DropDownItems)
            {
                string str = baseName + "." + item.Name;
                string stringName = str + ".Text";
                string str3 = PdnResources.GetString2(stringName);
                if (str3 != null)
                {
                    item.Text = str3;
                }
                PdnMenuItem item2 = item as PdnMenuItem;
                if (item2 != null)
                {
                    item2.textResourceName = stringName;
                    item2.LoadNames(str);
                }
            }
            this.namesLoaded = true;
        }

        private bool OnAccessHotKeyPressed(Keys keys)
        {
            base.ShowDropDown();
            return true;
        }

        protected virtual void OnAppWorkspaceChanged()
        {
            foreach (ToolStripItem item in base.DropDownItems)
            {
                PdnMenuItem item2 = item as PdnMenuItem;
                if (item2 != null)
                {
                    item2.AppWorkspace = this.AppWorkspace;
                }
            }
        }

        protected virtual void OnAppWorkspaceChanging()
        {
            foreach (ToolStripItem item in base.DropDownItems)
            {
                PdnMenuItem item2 = item as PdnMenuItem;
                if (item2 != null)
                {
                    item2.AppWorkspace = null;
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (Form.ActiveForm != null)
            {
                Form.ActiveForm.BeginInvoke(new Action(PdnBaseForm.UpdateAllForms));
            }
            if (base.Name == null)
            {
                string text = this.Text;
            }
            base.OnClick(e);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            foreach (ToolStripItem item in base.DropDownItems)
            {
                item.Enabled = true;
            }
            base.OnDropDownClosed(e);
        }

        protected virtual void OnDropDownOpening(EventArgs e)
        {
            if (!this.namesLoaded)
            {
                this.LoadNames(base.Name);
            }
            if (!this.iconsLoaded)
            {
                this.LoadIcons();
            }
        }

        private bool OnShortcutKeyPressed(Keys keys)
        {
            base.PerformClick();
            return true;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (this.registeredHotKey != Keys.None)
            {
                PdnBaseForm.UnregisterFormHotKey(this.registeredHotKey, new Func<Keys, bool>(this.OnAccessHotKeyPressed));
            }
            char mnemonic = this.Mnemonic;
            if ((mnemonic != '\0') && !base.IsOnDropDown)
            {
                Keys keys = KeysUtil.FromLetterOrDigitChar(mnemonic);
                PdnBaseForm.RegisterFormHotKey(Keys.Alt | keys, new Func<Keys, bool>(this.OnAccessHotKeyPressed));
            }
            base.OnTextChanged(e);
        }

        private void PdnMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            this.OnDropDownOpening(e);
        }

        public void PerformClickAsync()
        {
            base.Owner.BeginInvoke(new Action(this.PerformClick));
        }

        public void SetIcon(ImageResource image)
        {
            this.Image = image.Reference;
        }

        public void SetIcon(string imageName)
        {
            this.Image = PdnResources.GetImageResource2(imageName).Reference;
        }

        [Browsable(false)]
        public PaintDotNet.Controls.AppWorkspace AppWorkspace
        {
            get => 
                this.appWorkspace;
            set
            {
                if (value != this.appWorkspace)
                {
                    this.OnAppWorkspaceChanging();
                    this.appWorkspace = value;
                    this.OnAppWorkspaceChanged();
                }
            }
        }

        public Form AssociatedForm =>
            this.appWorkspace?.FindForm();

        public bool HasMnemonic =>
            (this.Mnemonic != '\0');

        public char Mnemonic
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Text))
                {
                    int index = this.Text.IndexOf('&');
                    if ((index >= 0) && (index < (this.Text.Length - 1)))
                    {
                        return this.Text[index + 1];
                    }
                }
                return '\0';
            }
        }

        public Keys ShortcutKeys
        {
            get => 
                base.ShortcutKeys;
            set
            {
                if (this.ShortcutKeys != Keys.None)
                {
                    PdnBaseForm.UnregisterFormHotKey(this.ShortcutKeys, new Func<Keys, bool>(this.OnShortcutKeyPressed));
                }
                PdnBaseForm.RegisterFormHotKey(value, new Func<Keys, bool>(this.OnShortcutKeyPressed));
                base.ShortcutKeys = value;
            }
        }
    }
}

