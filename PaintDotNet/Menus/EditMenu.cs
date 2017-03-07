namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class EditMenu : PdnMenuItem
    {
        private PdnMenuItem menuEditCopy;
        private PdnMenuItem menuEditCut;
        private PdnMenuItem menuEditDeselect;
        private PdnMenuItem menuEditEraseSelection;
        private PdnMenuItem menuEditFillSelection;
        private PdnMenuItem menuEditInvertSelection;
        private PdnMenuItem menuEditPaste;
        private PdnMenuItem menuEditPasteInToNewImage;
        private PdnMenuItem menuEditPasteInToNewLayer;
        private PdnMenuItem menuEditRedo;
        private PdnMenuItem menuEditSelectAll;
        private ToolStripSeparator menuEditSeparator1;
        private ToolStripSeparator menuEditSeparator2;
        private PdnMenuItem menuEditUndo;

        public EditMenu()
        {
            PdnBaseForm.RegisterFormHotKey(Keys.Back, new Func<Keys, bool>(this.OnBackspaceTyped));
            PdnBaseForm.RegisterFormHotKey(Keys.Shift | Keys.Delete, new Func<Keys, bool>(this.OnLeftHandedCutHotKey));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Insert, new Func<Keys, bool>(this.OnLeftHandedCopyHotKey));
            PdnBaseForm.RegisterFormHotKey(Keys.Shift | Keys.Insert, new Func<Keys, bool>(this.OnLeftHandedPasteHotKey));
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuEditUndo = new PdnMenuItem();
            this.menuEditRedo = new PdnMenuItem();
            this.menuEditSeparator1 = new ToolStripSeparator();
            this.menuEditCut = new PdnMenuItem();
            this.menuEditCopy = new PdnMenuItem();
            this.menuEditPaste = new PdnMenuItem();
            this.menuEditPasteInToNewLayer = new PdnMenuItem();
            this.menuEditPasteInToNewImage = new PdnMenuItem();
            this.menuEditSeparator2 = new ToolStripSeparator();
            this.menuEditEraseSelection = new PdnMenuItem();
            this.menuEditFillSelection = new PdnMenuItem();
            this.menuEditInvertSelection = new PdnMenuItem();
            this.menuEditSelectAll = new PdnMenuItem();
            this.menuEditDeselect = new PdnMenuItem();
            base.DropDownItems.AddRange(new ToolStripItem[] { this.menuEditUndo, this.menuEditRedo, this.menuEditSeparator1, this.menuEditCut, this.menuEditCopy, this.menuEditPaste, this.menuEditPasteInToNewLayer, this.menuEditPasteInToNewImage, this.menuEditSeparator2, this.menuEditEraseSelection, this.menuEditFillSelection, this.menuEditInvertSelection, this.menuEditSelectAll, this.menuEditDeselect });
            base.Name = "Menu.Edit";
            this.Text = PdnResources.GetString2("Menu.Edit.Text");
            this.menuEditUndo.Name = "Undo";
            this.menuEditUndo.ShortcutKeys = Keys.Control | Keys.Z;
            this.menuEditUndo.Click += new EventHandler(this.MenuEditUndo_Click);
            this.menuEditRedo.Name = "Redo";
            this.menuEditRedo.ShortcutKeys = Keys.Control | Keys.Y;
            this.menuEditRedo.Click += new EventHandler(this.MenuEditRedo_Click);
            this.menuEditCut.Name = "Cut";
            this.menuEditCut.ShortcutKeys = Keys.Control | Keys.X;
            this.menuEditCut.Click += new EventHandler(this.MenuEditCut_Click);
            this.menuEditCopy.Name = "Copy";
            this.menuEditCopy.ShortcutKeys = Keys.Control | Keys.C;
            this.menuEditCopy.Click += new EventHandler(this.MenuEditCopy_Click);
            this.menuEditPaste.Name = "Paste";
            this.menuEditPaste.ShortcutKeys = Keys.Control | Keys.V;
            this.menuEditPaste.Click += new EventHandler(this.MenuEditPaste_Click);
            this.menuEditPasteInToNewLayer.Name = "PasteInToNewLayer";
            this.menuEditPasteInToNewLayer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;
            this.menuEditPasteInToNewLayer.Click += new EventHandler(this.MenuEditPasteInToNewLayer_Click);
            this.menuEditPasteInToNewImage.Name = "PasteInToNewImage";
            this.menuEditPasteInToNewImage.ShortcutKeys = Keys.Alt | Keys.Control | Keys.V;
            this.menuEditPasteInToNewImage.Click += new EventHandler(this.MenuEditPasteInToNewImage_Click);
            this.menuEditEraseSelection.Name = "EraseSelection";
            this.menuEditEraseSelection.ShortcutKeys = Keys.Delete;
            this.menuEditEraseSelection.Click += new EventHandler(this.MenuEditClearSelection_Click);
            this.menuEditFillSelection.Name = "FillSelection";
            this.menuEditFillSelection.ShortcutKeyDisplayString = PdnResources.GetString2("Menu.Edit.FillSelection.ShortcutKeysDisplayString");
            this.menuEditFillSelection.Click += new EventHandler(this.MenuEditFillSelection_Click);
            this.menuEditInvertSelection.Name = "InvertSelection";
            this.menuEditInvertSelection.Click += new EventHandler(this.MenuEditInvertSelection_Click);
            this.menuEditInvertSelection.ShortcutKeys = Keys.Control | Keys.I;
            this.menuEditSelectAll.Name = "SelectAll";
            this.menuEditSelectAll.ShortcutKeys = Keys.Control | Keys.A;
            this.menuEditSelectAll.Click += new EventHandler(this.MenuEditSelectAll_Click);
            this.menuEditDeselect.Name = "Deselect";
            this.menuEditDeselect.ShortcutKeys = Keys.Control | Keys.D;
            this.menuEditDeselect.Click += new EventHandler(this.MenuEditDeselect_Click);
        }

        private void MenuEditClearSelection_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new EraseSelectionFunction());
            }
        }

        private void MenuEditCopy_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                new CopyToClipboardAction(base.AppWorkspace.ActiveDocumentWorkspace).PerformAction();
            }
        }

        private void MenuEditCut_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                new CutAction().PerformAction(base.AppWorkspace.ActiveDocumentWorkspace);
            }
        }

        private void MenuEditDeselect_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new DeselectFunction());
            }
        }

        private void MenuEditFillSelection_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new FillSelectionFunction(base.AppWorkspace.AppEnvironment.PrimaryColor));
            }
        }

        private void MenuEditInvertSelection_Click(object sender, EventArgs e)
        {
            if (((base.AppWorkspace.ActiveDocumentWorkspace != null) && !base.AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty) && (base.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new InvertSelectionFunction()) == HistoryFunctionResult.Success))
            {
                base.AppWorkspace.ActiveDocumentWorkspace.Selection.PerformChanging();
                base.AppWorkspace.ActiveDocumentWorkspace.Selection.PerformChanged();
            }
        }

        private void MenuEditPaste_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                new PasteAction(base.AppWorkspace.ActiveDocumentWorkspace).PerformAction();
            }
        }

        private void MenuEditPasteInToNewImage_Click(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new PasteInToNewImageAction());
        }

        private void MenuEditPasteInToNewLayer_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                new PasteInToNewLayerAction(base.AppWorkspace.ActiveDocumentWorkspace).PerformAction();
            }
        }

        private void MenuEditRedo_Click(object sender, EventArgs e)
        {
            if ((base.AppWorkspace.ActiveDocumentWorkspace != null) && !base.AppWorkspace.ActiveDocumentWorkspace.IsMouseCaptured())
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new HistoryRedoAction());
            }
        }

        private void MenuEditSelectAll_Click(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.ExecuteFunction(new SelectAllFunction());
                base.AppWorkspace.ActiveDocumentWorkspace.Selection.PerformChanging();
                base.AppWorkspace.ActiveDocumentWorkspace.Selection.PerformChanged();
            }
        }

        private void MenuEditUndo_Click(object sender, EventArgs e)
        {
            if ((base.AppWorkspace.ActiveDocumentWorkspace != null) && !base.AppWorkspace.ActiveDocumentWorkspace.IsMouseCaptured())
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new HistoryUndoAction());
            }
        }

        private bool OnBackspaceTyped(Keys keys)
        {
            if ((base.AppWorkspace.ActiveDocumentWorkspace != null) && !base.AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty)
            {
                this.menuEditFillSelection.PerformClick();
                return true;
            }
            return false;
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            bool flag;
            bool flag2;
            bool flag3;
            IDataObject dataObject;
            if (base.AppWorkspace.ActiveDocumentWorkspace == null)
            {
                flag = false;
                flag2 = false;
                this.menuEditSelectAll.Enabled = false;
            }
            else
            {
                flag = !base.AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty;
                flag2 = base.AppWorkspace.ActiveDocumentWorkspace.ActiveLayer is BitmapLayer;
                this.menuEditSelectAll.Enabled = true;
            }
            this.menuEditCopy.Enabled = flag;
            this.menuEditCut.Enabled = flag && flag2;
            this.menuEditEraseSelection.Enabled = flag;
            this.menuEditFillSelection.Enabled = flag;
            this.menuEditInvertSelection.Enabled = flag;
            this.menuEditDeselect.Enabled = flag;
            try
            {
                dataObject = Clipboard.GetDataObject();
                flag3 = ClipboardUtil.IsClipboardImageMaybeAvailable(base.AppWorkspace, dataObject);
            }
            catch (Exception)
            {
                flag3 = false;
                dataObject = null;
            }
            this.menuEditPaste.Enabled = flag3 && (base.AppWorkspace.ActiveDocumentWorkspace != null);
            if (((!this.menuEditPaste.Enabled && (dataObject != null)) && (!this.menuEditPaste.Enabled && (base.AppWorkspace.ActiveDocumentWorkspace != null))) && (base.AppWorkspace.ActiveDocumentWorkspace.Tool != null))
            {
                bool flag4;
                try
                {
                    base.AppWorkspace.ActiveDocumentWorkspace.Tool.PerformPasteQuery(dataObject, out flag4);
                }
                catch (ExternalException)
                {
                    flag4 = false;
                }
                if (flag4)
                {
                    this.menuEditPaste.Enabled = true;
                }
            }
            this.menuEditPasteInToNewLayer.Enabled = flag3 && (base.AppWorkspace.ActiveDocumentWorkspace != null);
            this.menuEditPasteInToNewImage.Enabled = flag3;
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.menuEditUndo.Enabled = base.AppWorkspace.ActiveDocumentWorkspace.History.UndoStack.Count > 1;
                this.menuEditRedo.Enabled = base.AppWorkspace.ActiveDocumentWorkspace.History.RedoStack.Count > 0;
            }
            else
            {
                this.menuEditUndo.Enabled = false;
                this.menuEditRedo.Enabled = false;
            }
            base.OnDropDownOpening(e);
        }

        private bool OnLeftHandedCopyHotKey(Keys keys)
        {
            this.menuEditCopy.PerformClick();
            return true;
        }

        private bool OnLeftHandedCutHotKey(Keys keys)
        {
            this.menuEditCut.PerformClick();
            return true;
        }

        private bool OnLeftHandedPasteHotKey(Keys keys)
        {
            this.menuEditPaste.PerformClick();
            return true;
        }
    }
}

