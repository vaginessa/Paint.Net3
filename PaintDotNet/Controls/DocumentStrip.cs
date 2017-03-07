namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class DocumentStrip : ImageStrip, IDocumentList
    {
        private List<ImageStrip.Item> documentButtons = new List<ImageStrip.Item>();
        private List<DocumentWorkspace> documents = new List<DocumentWorkspace>();
        private Dictionary<DocumentWorkspace, ImageStrip.Item> dw2button = new Dictionary<DocumentWorkspace, ImageStrip.Item>();
        private bool ensureSelectedIsVisible = true;
        private DocumentWorkspace selectedDocument;
        private int suspendThumbnailUpdates;
        private ThumbnailManager thumbnailManager;
        private Dictionary<DocumentWorkspace, RenderArgs> thumbs = new Dictionary<DocumentWorkspace, RenderArgs>();
        private object thumbsLock = new object();

        public event EventHandler<EventArgs<Pair<DocumentWorkspace, DocumentClickAction>>> DocumentClicked;

        public event EventHandler DocumentListChanged;

        public DocumentStrip()
        {
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Tab, new Func<Keys, bool>(this.OnNextTabHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Next, new Func<Keys, bool>(this.OnNextTabHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Shift | Keys.Tab, new Func<Keys, bool>(this.OnPreviousTabHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.PageUp, new Func<Keys, bool>(this.OnPreviousTabHotKeyPressed));
            this.thumbnailManager = new ThumbnailManager(this);
            this.InitializeComponent();
            for (int i = 1; i <= 9; i++)
            {
                Keys keys = KeysUtil.FromLetterOrDigitChar((char) (i + 0x30));
                PdnBaseForm.RegisterFormHotKey(Keys.Control | keys, new Func<Keys, bool>(this.OnDigitHotKeyPressed));
                PdnBaseForm.RegisterFormHotKey(Keys.Alt | keys, new Func<Keys, bool>(this.OnDigitHotKeyPressed));
            }
            base.ShowCloseButtons = true;
        }

        public void AddDocumentWorkspace(DocumentWorkspace addMe)
        {
            this.documents.Add(addMe);
            ImageStrip.Item newItem = new ImageStrip.Item {
                Image = null,
                Tag = addMe
            };
            base.AddItem(newItem);
            this.documentButtons.Add(newItem);
            addMe.CompositionUpdated += new EventHandler(this.Workspace_CompositionUpdated);
            this.dw2button.Add(addMe, newItem);
            if (addMe.Document != null)
            {
                this.QueueThumbnailUpdate(addMe);
                newItem.Dirty = addMe.Document.Dirty;
                addMe.Document.DirtyChanged += new EventHandler(this.Document_DirtyChanged);
            }
            addMe.DocumentChanging += new EventHandler<EventArgs<Document>>(this.Workspace_DocumentChanging);
            addMe.DocumentChanged += new EventHandler(this.Workspace_DocumentChanged);
            this.OnDocumentListChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (this.documents.Count > 0)
                {
                    this.RemoveDocumentWorkspace(this.documents[this.documents.Count - 1]);
                }
                if (this.thumbnailManager != null)
                {
                    this.thumbnailManager.Dispose();
                    this.thumbnailManager = null;
                }
                foreach (DocumentWorkspace workspace in this.thumbs.Keys)
                {
                    RenderArgs args = this.thumbs[workspace];
                    args.ISurface.Dispose();
                    args.Dispose();
                }
                this.thumbs.Clear();
                this.thumbs = null;
            }
            base.Dispose(disposing);
        }

        private void Document_DirtyChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < this.documents.Count; i++)
            {
                if (object.ReferenceEquals(sender, this.documents[i].Document))
                {
                    ImageStrip.Item item = this.dw2button[this.documents[i]];
                    item.Dirty = ((Document) sender).Dirty;
                }
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int num;
            Size itemSize = base.ItemSize;
            if (base.ItemCount == 0)
            {
                num = 0;
            }
            else
            {
                num = itemSize.Width * this.DocumentCount;
            }
            return new Size(num, itemSize.Height);
        }

        private void InitializeComponent()
        {
            base.Name = "DocumentStrip";
        }

        public void LockDocumentWorkspaceDirtyValue(DocumentWorkspace lockMe, bool forceDirtyValue)
        {
            this.dw2button[lockMe].LockDirtyValue(forceDirtyValue);
        }

        public bool NextTab()
        {
            bool flag = false;
            if (this.selectedDocument != null)
            {
                int num2 = (this.documents.IndexOf(this.selectedDocument) + 1) % this.documents.Count;
                this.SelectedDocument = this.documents[num2];
                flag = true;
            }
            return flag;
        }

        private bool OnDigitHotKeyPressed(Keys keys)
        {
            keys &= ~Keys.Alt;
            keys &= ~Keys.Control;
            if ((keys >= Keys.D0) && (keys <= Keys.D9))
            {
                int num2;
                int num = ((int) keys) - 0x30;
                if (num == 0)
                {
                    num2 = 9;
                }
                else
                {
                    num2 = num - 1;
                }
                if (num2 < this.documents.Count)
                {
                    base.PerformItemClick(num2, ImageStrip.ItemPart.Image, MouseButtons.Left);
                    return true;
                }
            }
            return false;
        }

        protected virtual void OnDocumentClicked(DocumentWorkspace dw, DocumentClickAction action)
        {
            if (this.DocumentClicked != null)
            {
                this.DocumentClicked(this, new EventArgs<Pair<DocumentWorkspace, DocumentClickAction>>(Pair.Create<DocumentWorkspace, DocumentClickAction>(dw, action)));
            }
        }

        protected virtual void OnDocumentListChanged()
        {
            if (this.DocumentListChanged != null)
            {
                this.DocumentListChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnItemClicked(ImageStrip.Item item, ImageStrip.ItemPart itemPart, MouseButtons mouseButtons)
        {
            DocumentWorkspace tag = item.Tag as DocumentWorkspace;
            if (tag != null)
            {
                if (mouseButtons == MouseButtons.Middle)
                {
                    this.OnDocumentClicked(tag, DocumentClickAction.Close);
                }
                else
                {
                    switch (itemPart)
                    {
                        case ImageStrip.ItemPart.None:
                            break;

                        case ImageStrip.ItemPart.Image:
                            if (mouseButtons != MouseButtons.Left)
                            {
                                if (mouseButtons == MouseButtons.Right)
                                {
                                }
                                break;
                            }
                            this.SelectedDocument = tag;
                            break;

                        case ImageStrip.ItemPart.CloseButton:
                            if (mouseButtons == MouseButtons.Left)
                            {
                                this.OnDocumentClicked(tag, DocumentClickAction.Close);
                            }
                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }
                }
            }
            base.OnItemClicked(item, itemPart, mouseButtons);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if ((this.ensureSelectedIsVisible && !this.Focused) && (!base.LeftScrollButton.Focused && !base.RightScrollButton.Focused))
            {
                int index = this.documents.IndexOf(this.selectedDocument);
                base.EnsureItemFullyVisible(index);
            }
        }

        private bool OnNextTabHotKeyPressed(Keys keys) => 
            this.NextTab();

        private bool OnPreviousTabHotKeyPressed(Keys keys) => 
            this.PreviousTab();

        protected override void OnScrollArrowClicked(ArrowDirection arrowDirection)
        {
            int num = 0;
            switch (arrowDirection)
            {
                case ArrowDirection.Left:
                    num = -1;
                    break;

                case ArrowDirection.Right:
                    num = 1;
                    break;
            }
            int width = base.ItemSize.Width;
            base.ScrollOffset += num * width;
            base.OnScrollArrowClicked(arrowDirection);
        }

        private void OnThumbnailRendered(object sender, EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>> e)
        {
            RenderArgs args = null;
            DocumentWorkspace first = (DocumentWorkspace) e.Data.First;
            ISurface<ColorBgra> second = e.Data.Second;
            if (this.documents.Contains(first))
            {
                Int32Size size = second.Size<ColorBgra>();
                if (this.thumbs.ContainsKey(first))
                {
                    args = this.thumbs[first];
                    if (args.Size.ToInt32Size() != size)
                    {
                        args.ISurface.Dispose();
                        args.Dispose();
                        args = null;
                        this.thumbs.Remove(first);
                    }
                }
                if (args == null)
                {
                    args = new RenderArgs(e.Data.Second.ToSurface());
                    this.thumbs.Add(first, args);
                }
                e.Data.Second.Render<ColorBgra>(args.ISurface);
                e.Data.Second.Dispose();
                this.OnThumbnailUpdated(first);
            }
        }

        private void OnThumbnailUpdated(DocumentWorkspace dw)
        {
            if (this.dw2button.ContainsKey(dw))
            {
                ImageStrip.Item item = this.dw2button[dw];
                RenderArgs args = this.thumbs[dw];
                item.Image = args.Bitmap;
                item.Update();
            }
        }

        public bool PreviousTab()
        {
            bool flag = false;
            if (this.selectedDocument != null)
            {
                int num2 = (this.documents.IndexOf(this.selectedDocument) + (this.documents.Count - 1)) % this.documents.Count;
                this.SelectedDocument = this.documents[num2];
                flag = true;
            }
            return flag;
        }

        public void QueueThumbnailUpdate(DocumentWorkspace dw)
        {
            if (this.suspendThumbnailUpdates <= 0)
            {
                this.thumbnailManager.QueueThumbnailUpdate(dw, base.PreferredImageSize.Width - 2, new EventHandler<EventArgs<Pair<IThumbnailProvider, ISurface<ColorBgra>>>>(this.OnThumbnailRendered));
            }
        }

        public void RefreshAllThumbnails()
        {
            foreach (DocumentWorkspace workspace in this.documents)
            {
                this.QueueThumbnailUpdate(workspace);
            }
        }

        public void RefreshThumbnail(DocumentWorkspace dw)
        {
            if (this.documents.Contains(dw))
            {
                this.QueueThumbnailUpdate(dw);
            }
        }

        public void RemoveDocumentWorkspace(DocumentWorkspace removeMe)
        {
            removeMe.CompositionUpdated -= new EventHandler(this.Workspace_CompositionUpdated);
            if (this.selectedDocument == removeMe)
            {
                this.selectedDocument = null;
            }
            removeMe.DocumentChanging -= new EventHandler<EventArgs<Document>>(this.Workspace_DocumentChanging);
            removeMe.DocumentChanged -= new EventHandler(this.Workspace_DocumentChanged);
            if (removeMe.Document != null)
            {
                removeMe.Document.DirtyChanged -= new EventHandler(this.Document_DirtyChanged);
            }
            this.documents.Remove(removeMe);
            this.thumbnailManager.RemoveFromQueue(removeMe);
            ImageStrip.Item item = this.dw2button[removeMe];
            base.RemoveItem(item);
            this.dw2button.Remove(removeMe);
            this.documentButtons.Remove(item);
            if (this.thumbs.ContainsKey(removeMe))
            {
                RenderArgs args = this.thumbs[removeMe];
                ISurface<ColorBgra> iSurface = args.ISurface;
                args.Dispose();
                this.thumbs.Remove(removeMe);
                iSurface.Dispose();
            }
            this.OnDocumentListChanged();
        }

        public void ResumeThumbnailUpdates()
        {
            this.suspendThumbnailUpdates--;
        }

        public void SelectDocumentWorkspace(DocumentWorkspace selectMe)
        {
            UI.SuspendControlPainting(this);
            this.selectedDocument = selectMe;
            if (this.thumbs.ContainsKey(selectMe))
            {
                RenderArgs args = this.thumbs[selectMe];
                Bitmap bitmap = args.Bitmap;
            }
            else
            {
                this.QueueThumbnailUpdate(selectMe);
            }
            foreach (ImageStrip.Item item in this.documentButtons)
            {
                if ((item.Tag as DocumentWorkspace) == selectMe)
                {
                    base.EnsureItemFullyVisible(item);
                    item.Checked = true;
                }
                else
                {
                    item.Checked = false;
                }
            }
            UI.ResumeControlPainting(this);
            base.Invalidate(true);
        }

        public void SuspendThumbnailUpdates()
        {
            this.suspendThumbnailUpdates++;
        }

        public void UnlockDocumentWorkspaceDirtyValue(DocumentWorkspace unlockMe)
        {
            this.dw2button[unlockMe].UnlockDirtyValue();
        }

        private void Workspace_CompositionUpdated(object sender, EventArgs e)
        {
            DocumentWorkspace dw = (DocumentWorkspace) sender;
            this.QueueThumbnailUpdate(dw);
        }

        private void Workspace_DocumentChanged(object sender, EventArgs e)
        {
            DocumentWorkspace workspace = (DocumentWorkspace) sender;
            ImageStrip.Item item = this.dw2button[workspace];
            if (workspace.Document != null)
            {
                item.Dirty = workspace.Document.Dirty;
                workspace.Document.DirtyChanged += new EventHandler(this.Document_DirtyChanged);
            }
            else
            {
                item.Dirty = false;
            }
        }

        private void Workspace_DocumentChanging(object sender, EventArgs<Document> e)
        {
            if (e.Data != null)
            {
                e.Data.DirtyChanged -= new EventHandler(this.Document_DirtyChanged);
            }
        }

        public int DocumentCount =>
            this.documents.Count;

        public DocumentWorkspace[] DocumentList =>
            this.documents.ToArrayEx<DocumentWorkspace>();

        public Image[] DocumentThumbnails
        {
            get
            {
                Image[] imageArray = new Image[this.documents.Count];
                for (int i = 0; i < imageArray.Length; i++)
                {
                    RenderArgs args;
                    DocumentWorkspace key = this.documents[i];
                    if (!this.thumbs.TryGetValue(key, out args))
                    {
                        imageArray[i] = null;
                    }
                    else if (args == null)
                    {
                        imageArray[i] = null;
                    }
                    else
                    {
                        imageArray[i] = args.Bitmap;
                    }
                }
                return imageArray;
            }
        }

        public bool EnsureSelectedIsVisible
        {
            get => 
                this.ensureSelectedIsVisible;
            set
            {
                if (this.ensureSelectedIsVisible != value)
                {
                    this.ensureSelectedIsVisible = value;
                    base.PerformLayout();
                }
            }
        }

        public DocumentWorkspace SelectedDocument
        {
            get => 
                this.selectedDocument;
            set
            {
                if (!this.documents.Contains(value))
                {
                    throw new ArgumentException("DocumentWorkspace isn't being tracked by this instance of DocumentStrip");
                }
                if (this.selectedDocument != value)
                {
                    this.SelectDocumentWorkspace(value);
                    this.OnDocumentClicked(value, DocumentClickAction.Select);
                    this.Refresh();
                }
            }
        }

        public int SelectedDocumentIndex =>
            this.documents.IndexOf(this.selectedDocument);

        public int ThumbnailUpdateLatency
        {
            get => 
                this.thumbnailManager.UpdateLatency;
            set
            {
                this.thumbnailManager.UpdateLatency = value;
            }
        }
    }
}

