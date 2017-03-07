namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;

    internal sealed class FileMenu : PdnMenuItem
    {
        private PdnMenuItem menuFileAcquire;
        private PdnMenuItem menuFileAcquireFromScannerOrCamera;
        private PdnMenuItem menuFileClose;
        private PdnMenuItem menuFileExit;
        private PdnMenuItem menuFileNew;
        private PdnMenuItem menuFileOpen;
        private PdnMenuItem menuFileOpenRecent;
        private PdnMenuItem menuFileOpenRecentSentinel;
        private PdnMenuItem menuFilePrint;
        private PdnMenuItem menuFileSave;
        private PdnMenuItem menuFileSaveAs;
        private ToolStripSeparator menuFileSeparator1;
        private ToolStripSeparator menuFileSeparator2;
        private ToolStripSeparator menuFileSeparator3;

        public FileMenu()
        {
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.F4, new Func<Keys, bool>(this.OnCtrlF4Typed));
            this.InitializeComponent();
        }

        private void ClearList_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new ClearMruListAction());
        }

        private void DoExit()
        {
            Startup.CloseApplication();
        }

        private ToolStripItem[] GetMenuItemsToAdd() => 
            new ToolStripItem[] { this.menuFileNew, this.menuFileOpen, this.menuFileOpenRecent, this.menuFileAcquire, this.menuFileClose, this.menuFileSeparator1, this.menuFileSave, this.menuFileSaveAs, this.menuFileSeparator2, this.menuFilePrint, this.menuFileSeparator3, this.menuFileExit };

        private void InitializeComponent()
        {
            this.menuFileNew = new PdnMenuItem();
            this.menuFileOpen = new PdnMenuItem();
            this.menuFileOpenRecent = new PdnMenuItem();
            this.menuFileOpenRecentSentinel = new PdnMenuItem();
            this.menuFileAcquire = new PdnMenuItem();
            this.menuFileAcquireFromScannerOrCamera = new PdnMenuItem();
            this.menuFileClose = new PdnMenuItem();
            this.menuFileSeparator1 = new ToolStripSeparator();
            this.menuFileSave = new PdnMenuItem();
            this.menuFileSaveAs = new PdnMenuItem();
            this.menuFileSeparator2 = new ToolStripSeparator();
            this.menuFilePrint = new PdnMenuItem();
            this.menuFileSeparator3 = new ToolStripSeparator();
            this.menuFileExit = new PdnMenuItem();
            base.DropDownItems.AddRange(this.GetMenuItemsToAdd());
            base.Name = "Menu.File";
            this.Text = PdnResources.GetString2("Menu.File.Text");
            this.menuFileNew.Name = "New";
            this.menuFileNew.ShortcutKeys = Keys.Control | Keys.N;
            this.menuFileNew.Click += new EventHandler(this.MenuFileNew_Click);
            this.menuFileOpen.Name = "Open";
            this.menuFileOpen.ShortcutKeys = Keys.Control | Keys.O;
            this.menuFileOpen.Click += new EventHandler(this.MenuFileOpen_Click);
            this.menuFileOpenRecent.Name = "OpenRecent";
            this.menuFileOpenRecent.DropDownItems.AddRange(new ToolStripItem[] { this.menuFileOpenRecentSentinel });
            this.menuFileOpenRecent.DropDownOpening += new EventHandler(this.MenuFileOpenRecent_DropDownOpening);
            this.menuFileOpenRecentSentinel.Text = "sentinel";
            this.menuFileAcquire.Name = "Acquire";
            this.menuFileAcquire.DropDownItems.AddRange(new ToolStripItem[] { this.menuFileAcquireFromScannerOrCamera });
            this.menuFileAcquire.DropDownOpening += new EventHandler(this.MenuFileAcquire_DropDownOpening);
            this.menuFileAcquireFromScannerOrCamera.Name = "FromScannerOrCamera";
            this.menuFileAcquireFromScannerOrCamera.Click += new EventHandler(this.MenuFileAcquireFromScannerOrCamera_Click);
            this.menuFileClose.Name = "Close";
            this.menuFileClose.Click += new EventHandler(this.MenuFileClose_Click);
            this.menuFileClose.ShortcutKeys = Keys.Control | Keys.W;
            this.menuFileSave.Name = "Save";
            this.menuFileSave.ShortcutKeys = Keys.Control | Keys.S;
            this.menuFileSave.Click += new EventHandler(this.MenuFileSave_Click);
            this.menuFileSaveAs.Name = "SaveAs";
            this.menuFileSaveAs.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            this.menuFileSaveAs.Click += new EventHandler(this.MenuFileSaveAs_Click);
            this.menuFilePrint.Name = "Print";
            this.menuFilePrint.ShortcutKeys = Keys.Control | Keys.P;
            this.menuFilePrint.Click += new EventHandler(this.MenuFilePrint_Click);
            this.menuFileExit.Name = "Exit";
            this.menuFileExit.Click += new EventHandler(this.MenuFileExit_Click);
        }

        private void MenuFileAcquire_DropDownOpening(object sender, EventArgs e)
        {
            bool flag = true;
            if (ScanningAndPrinting.IsComponentAvailable && !ScanningAndPrinting.CanScan)
            {
                flag = false;
            }
            this.menuFileAcquireFromScannerOrCamera.Enabled = flag;
        }

        private void MenuFileAcquireFromScannerOrCamera_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new AcquireFromScannerOrCameraAction());
        }

        private void MenuFileClose_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.DocumentWorkspaces.Length > 0)
            {
                base.AppWorkspace.PerformAction(new CloseWorkspaceAction());
            }
            else
            {
                this.DoExit();
            }
        }

        private void MenuFileExit_Click(object sender, EventArgs e)
        {
            this.DoExit();
        }

        private void MenuFileNew_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new NewImageAction());
        }

        private void MenuFileNewWindow_Click(object sender, EventArgs e)
        {
            Startup.StartNewInstance(base.AppWorkspace, null);
        }

        private void MenuFileOpen_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new OpenFileAction());
        }

        private void MenuFileOpenInNewWindow_Click(object sender, EventArgs e)
        {
            string str;
            string directoryName = Path.GetDirectoryName(base.AppWorkspace.ActiveDocumentWorkspace.FilePath);
            if (DocumentWorkspace.ChooseFile(base.AppWorkspace, out str, directoryName) == DialogResult.OK)
            {
                Startup.StartNewInstance(base.AppWorkspace, str);
            }
        }

        private void MenuFileOpenRecent_DropDownOpening(object sender, EventArgs e)
        {
            int num;
            base.AppWorkspace.MostRecentFiles.LoadMruList();
            MostRecentFile[] fileList = base.AppWorkspace.MostRecentFiles.GetFileList();
            MostRecentFile[] fileArray2 = new MostRecentFile[fileList.Length];
            for (num = 0; num < fileList.Length; num++)
            {
                fileArray2[(fileArray2.Length - num) - 1] = fileList[num];
            }
            foreach (ToolStripItem item in this.menuFileOpenRecent.DropDownItems)
            {
                item.Click -= new EventHandler(this.MenuFileOpenRecentFile_Click);
            }
            this.menuFileOpenRecent.DropDownItems.Clear();
            num = 0;
            foreach (MostRecentFile file in fileArray2)
            {
                string str;
                if (num < 9)
                {
                    str = "&";
                }
                else
                {
                    str = "";
                }
                int num4 = 1 + num;
                ToolStripMenuItem item2 = new ToolStripMenuItem(str + num4.ToString() + " " + Path.GetFileName(file.FileName));
                item2.Click += new EventHandler(this.MenuFileOpenRecentFile_Click);
                item2.ImageScaling = ToolStripItemImageScaling.None;
                item2.Image = (Image) file.Thumb.Clone();
                this.menuFileOpenRecent.DropDownItems.Add(item2);
                num++;
            }
            if (this.menuFileOpenRecent.DropDownItems.Count == 0)
            {
                ToolStripMenuItem item3 = new ToolStripMenuItem(PdnResources.GetString2("Menu.File.OpenRecent.None")) {
                    Enabled = false
                };
                this.menuFileOpenRecent.DropDownItems.Add(item3);
            }
            else
            {
                ToolStripSeparator separator = new ToolStripSeparator();
                this.menuFileOpenRecent.DropDownItems.Add(separator);
                ToolStripMenuItem item4 = new ToolStripMenuItem {
                    Text = PdnResources.GetString2("Menu.File.OpenRecent.ClearThisList")
                };
                this.menuFileOpenRecent.DropDownItems.Add(item4);
                Image reference = PdnResources.GetImageResource2("Icons.MenuEditEraseSelectionIcon.png").Reference;
                item4.ImageAlign = ContentAlignment.MiddleCenter;
                item4.ImageScaling = ToolStripItemImageScaling.None;
                int iconSize = base.AppWorkspace.MostRecentFiles.IconSize;
                Bitmap image = new Bitmap(iconSize + 2, iconSize + 2, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.Clear(Color.Transparent);
                    Point point = new Point((image.Width - reference.Width) / 2, (image.Height - reference.Height) / 2);
                    graphics.DrawImage(reference, point.X, point.Y, reference.Width, reference.Height);
                }
                item4.Image = image;
                item4.Click += new EventHandler(this.ClearList_Click);
            }
        }

        private void MenuFileOpenRecentFile_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = (ToolStripMenuItem) sender;
                int index = item.Text.IndexOf(" ");
                int num2 = int.Parse(item.Text.Substring(1, index - 1)) - 1;
                MostRecentFile[] fileList = base.AppWorkspace.MostRecentFiles.GetFileList();
                string fileName = fileList[(fileList.Length - num2) - 1].FileName;
                base.AppWorkspace.OpenFileInNewWorkspace(fileName);
            }
            catch (Exception)
            {
            }
        }

        private void MenuFilePrint_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new PrintAction());
            }
        }

        private void MenuFileSave_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.DoSave();
            }
        }

        private void MenuFileSaveAs_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.DoSaveAs();
            }
        }

        private bool OnCtrlF4Typed(Keys keys)
        {
            this.menuFileClose.PerformClick();
            return true;
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            base.DropDownItems.Clear();
            ToolStripItem[] menuItemsToAdd = this.GetMenuItemsToAdd();
            base.DropDownItems.AddRange(menuItemsToAdd);
            this.menuFileNew.Enabled = true;
            this.menuFileOpen.Enabled = true;
            this.menuFileOpenRecent.Enabled = true;
            this.menuFileOpenRecentSentinel.Enabled = true;
            this.menuFileAcquire.Enabled = true;
            this.menuFileAcquireFromScannerOrCamera.Enabled = true;
            this.menuFileExit.Enabled = true;
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.menuFileSave.Enabled = true;
                this.menuFileSaveAs.Enabled = true;
                this.menuFileClose.Enabled = true;
                this.menuFilePrint.Enabled = true;
            }
            else
            {
                this.menuFileSave.Enabled = false;
                this.menuFileSaveAs.Enabled = false;
                this.menuFileClose.Enabled = false;
                this.menuFilePrint.Enabled = false;
            }
            base.OnDropDownOpening(e);
        }
    }
}

