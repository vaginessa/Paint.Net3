namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class LayerControl : UserControl
    {
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private Container components;
        private PaintDotNet.Document document;
        private EventHandler documentChangedDelegate;
        private EventHandler<EventArgs<PaintDotNet.Document>> documentChangingDelegate;
        private EventHandler elementClickDelegate;
        private EventHandler elementDoubleClickDelegate;
        private int elementHeight = (8 + UI.ScaleWidth(LayerElement.ThumbSizePreScaling));
        private KeyEventHandler keyUpDelegate;
        private EventHandler layerChangedDelegate;
        private PanelWithLayout layerControlPanel;
        private List<LayerElement> layerControls;
        private IndexEventHandler layerInsertedDelegate;
        private IndexEventHandler layerRemovedDelegate;
        private ThumbnailManager thumbnailManager;
        private int thumbnailSize;

        public event EventHandler<EventArgs<Layer>> ActiveLayerChanged;

        public event EventHandler<EventArgs<Layer>> ClickedOnLayer;

        public event EventHandler<EventArgs<Layer>> DoubleClickedOnLayer;

        public event EventHandler RelinquishFocus;

        public LayerControl()
        {
            this.InitializeComponent();
            this.elementClickDelegate = new EventHandler(this.ElementClickHandler);
            this.elementDoubleClickDelegate = new EventHandler(this.ElementDoubleClickHandler);
            this.documentChangedDelegate = new EventHandler(this.DocumentChangedHandler);
            this.documentChangingDelegate = new EventHandler<EventArgs<PaintDotNet.Document>>(this.DocumentChangingHandler);
            this.layerInsertedDelegate = new IndexEventHandler(this.LayerInsertedHandler);
            this.layerRemovedDelegate = new IndexEventHandler(this.LayerRemovedHandler);
            this.layerChangedDelegate = new EventHandler(this.LayerChangedHandler);
            this.keyUpDelegate = new KeyEventHandler(this.KeyUpHandler);
            this.thumbnailManager = new ThumbnailManager(this);
            this.thumbnailSize = UI.ScaleWidth(LayerElement.ThumbSizePreScaling);
            this.layerControls = new List<LayerElement>();
        }

        public void ClearLayerSelection()
        {
            LayerElement[] layers = this.Layers;
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].IsSelected = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
                if (this.thumbnailManager != null)
                {
                    this.thumbnailManager.Dispose();
                    this.thumbnailManager = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DocumentChangedHandler(object sender, EventArgs e)
        {
            this.SetupNewDocument(this.appWorkspace.ActiveDocumentWorkspace.Document);
        }

        private void DocumentChangingHandler(object sender, EventArgs<PaintDotNet.Document> e)
        {
            this.TearDownOldDocument();
        }

        private void ElementClickHandler(object sender, EventArgs e)
        {
            LayerElement lec = (LayerElement) sender;
            if (Control.ModifierKeys == Keys.Control)
            {
                lec.IsSelected = !lec.IsSelected;
            }
            else
            {
                this.ClearLayerSelection();
                lec.IsSelected = true;
            }
            this.SetActive(lec);
            this.OnClickedOnLayer(lec.Layer);
        }

        private void ElementDoubleClickHandler(object sender, EventArgs e)
        {
            this.OnDoubleClickedOnLayer(((LayerElement) sender).Layer);
        }

        private void InitializeComponent()
        {
            this.layerControlPanel = new PanelWithLayout();
            base.SuspendLayout();
            this.layerControlPanel.AutoScroll = true;
            this.layerControlPanel.Dock = DockStyle.Fill;
            this.layerControlPanel.Location = new Point(0, 0);
            this.layerControlPanel.Name = "layerControlPanel";
            this.layerControlPanel.ParentLayerControl = this;
            this.layerControlPanel.Size = new Size(150, 150);
            this.layerControlPanel.TabIndex = 2;
            this.layerControlPanel.Click += new EventHandler(this.LayerControlPanel_Click);
            base.Controls.Add(this.layerControlPanel);
            base.Name = "LayerControl";
            base.ResumeLayout(false);
        }

        private void InitializeLayerElement(LayerElement layerElement, Layer layer)
        {
            layerElement.Height = this.elementHeight;
            layerElement.Layer = layer;
            layerElement.Click += this.elementClickDelegate;
            layerElement.DoubleClick += this.elementDoubleClickDelegate;
            layerElement.KeyUp += this.keyUpDelegate;
            layerElement.IsSelected = false;
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        private void LayerChangedHandler(object sender, EventArgs e)
        {
            this.SetActive(this.appWorkspace.ActiveDocumentWorkspace.ActiveLayer);
        }

        private void LayerControlPanel_Click(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
        }

        private void LayerInsertedHandler(object sender, IndexEventArgs e)
        {
            base.SuspendLayout();
            this.layerControlPanel.SuspendLayout();
            Layer layer = (Layer) this.document.Layers[e.Index];
            LayerElement layerElement = new LayerElement {
                ThumbnailManager = this.thumbnailManager,
                ThumbnailSize = this.thumbnailSize
            };
            this.InitializeLayerElement(layerElement, layer);
            this.layerControls.Insert(e.Index, layerElement);
            this.layerControlPanel.Controls.Add(layerElement);
            this.layerControlPanel.ScrollControlIntoView(layerElement);
            layerElement.Select();
            this.SetActive(layerElement);
            layerElement.RefreshPreview();
            this.layerControlPanel.ResumeLayout(false);
            base.ResumeLayout(false);
            this.layerControlPanel.PerformLayout();
            base.PerformLayout();
            this.Refresh();
        }

        private void LayerRemovedHandler(object sender, IndexEventArgs e)
        {
            LayerElement item = this.layerControls[e.Index];
            this.thumbnailManager.RemoveFromQueue(item.Layer);
            item.Click -= this.elementClickDelegate;
            item.DoubleClick -= this.elementDoubleClickDelegate;
            item.KeyUp -= this.keyUpDelegate;
            item.Layer = null;
            this.layerControls.Remove(item);
            this.layerControlPanel.Controls.Remove(item);
            item.Dispose();
            base.PerformLayout();
        }

        private void OnActiveLayerChanged(Layer layer)
        {
            if (this.ActiveLayerChanged != null)
            {
                this.ActiveLayerChanged(this, new EventArgs<Layer>(layer));
            }
        }

        protected override void OnClick(EventArgs e)
        {
            this.OnRelinquishFocus();
            base.OnClick(e);
        }

        private void OnClickedOnLayer(Layer layer)
        {
            if (this.ClickedOnLayer != null)
            {
                this.ClickedOnLayer(this, new EventArgs<Layer>(layer));
            }
        }

        private void OnDoubleClickedOnLayer(Layer layer)
        {
            if (this.DoubleClickedOnLayer != null)
            {
                this.DoubleClickedOnLayer(this, new EventArgs<Layer>(layer));
            }
        }

        protected void OnRelinquishFocus()
        {
            if (this.RelinquishFocus != null)
            {
                this.RelinquishFocus(this, EventArgs.Empty);
            }
        }

        public void PositionLayers()
        {
            this.layerControlPanel.PositionLayers();
        }

        public void RefreshPreviews()
        {
            for (int i = 0; i < this.layerControls.Count; i++)
            {
                this.layerControls[i].RefreshPreview();
            }
        }

        public void ResumeLayerPreviewUpdates()
        {
            foreach (LayerElement element in this.layerControls)
            {
                element.ResumePreviewUpdates();
            }
        }

        private void SetActive(LayerElement lec)
        {
            this.SetActive(lec.Layer);
        }

        private void SetActive(Layer layer)
        {
            foreach (LayerElement element in this.layerControls)
            {
                bool flag = element.Layer == layer;
                element.IsSelected = flag;
                if (flag)
                {
                    this.OnActiveLayerChanged(element.Layer);
                    this.layerControlPanel.ScrollControlIntoView(element);
                    element.Select();
                    base.Update();
                }
            }
        }

        private void SetupNewDocument(PaintDotNet.Document newDocument)
        {
            this.document = newDocument;
            this.document.Layers.Inserted += this.layerInsertedDelegate;
            this.document.Layers.RemovedAt += this.layerRemovedDelegate;
            UI.SuspendControlPainting(this);
            for (int i = 0; i < this.document.Layers.Count; i++)
            {
                this.LayerInsertedHandler(this, new IndexEventArgs(i));
            }
            if (this.appWorkspace != null)
            {
                foreach (LayerElement element in this.layerControls)
                {
                    if (element.Layer == this.appWorkspace.ActiveDocumentWorkspace.ActiveLayer)
                    {
                        element.IsSelected = true;
                    }
                    else
                    {
                        element.IsSelected = false;
                    }
                }
            }
            UI.ResumeControlPainting(this);
            base.Invalidate(true);
            base.Update();
            this.OnActiveLayerChanged(this.ActiveLayer);
        }

        public void SuspendLayerPreviewUpdates()
        {
            foreach (LayerElement element in this.layerControls)
            {
                element.SuspendPreviewUpdates();
            }
        }

        private void TearDownOldDocument()
        {
            UI.SuspendControlPainting(this);
            base.SuspendLayout();
            foreach (LayerElement element in this.layerControls)
            {
                element.Click -= this.elementClickDelegate;
                element.DoubleClick -= this.elementDoubleClickDelegate;
                element.KeyUp -= this.keyUpDelegate;
                element.Layer = null;
                this.layerControlPanel.Controls.Remove(element);
                element.Dispose();
            }
            base.ResumeLayout(true);
            this.layerControls.Clear();
            UI.ResumeControlPainting(this);
            base.Invalidate(true);
            if (this.document != null)
            {
                this.document.Layers.Inserted -= this.layerInsertedDelegate;
                this.document.Layers.RemovedAt -= this.layerRemovedDelegate;
                this.document = null;
            }
        }

        private void Workspace_ActiveDocumentWorkspaceChanged(object sender, EventArgs e)
        {
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.DocumentChanging += this.documentChangingDelegate;
                this.appWorkspace.ActiveDocumentWorkspace.DocumentChanged += this.documentChangedDelegate;
                this.appWorkspace.ActiveDocumentWorkspace.ActiveLayerChanged += this.layerChangedDelegate;
                if (this.appWorkspace.ActiveDocumentWorkspace.Document != null)
                {
                    this.SetupNewDocument(this.appWorkspace.ActiveDocumentWorkspace.Document);
                }
            }
        }

        private void Workspace_ActiveDocumentWorkspaceChanging(object sender, EventArgs e)
        {
            this.TearDownOldDocument();
            if (this.appWorkspace.ActiveDocumentWorkspace != null)
            {
                this.appWorkspace.ActiveDocumentWorkspace.DocumentChanging -= this.documentChangingDelegate;
                this.appWorkspace.ActiveDocumentWorkspace.DocumentChanged -= this.documentChangedDelegate;
                this.appWorkspace.ActiveDocumentWorkspace.ActiveLayerChanged -= this.layerChangedDelegate;
            }
        }

        public Layer ActiveLayer
        {
            get
            {
                int[] selectedLayerIndexes = this.SelectedLayerIndexes;
                if (selectedLayerIndexes.Length == 1)
                {
                    return this.Layers[selectedLayerIndexes[0]].Layer;
                }
                return null;
            }
        }

        public int ActiveLayerIndex
        {
            get
            {
                int[] selectedLayerIndexes = this.SelectedLayerIndexes;
                if (selectedLayerIndexes.Length == 1)
                {
                    return selectedLayerIndexes[0];
                }
                return -1;
            }
        }

        [Browsable(false)]
        public PaintDotNet.Controls.AppWorkspace AppWorkspace
        {
            get => 
                this.appWorkspace;
            set
            {
                if (this.appWorkspace != value)
                {
                    if (this.appWorkspace != null)
                    {
                        this.TearDownOldDocument();
                        this.appWorkspace.ActiveDocumentWorkspaceChanging -= new EventHandler(this.Workspace_ActiveDocumentWorkspaceChanging);
                        this.appWorkspace.ActiveDocumentWorkspaceChanged -= new EventHandler(this.Workspace_ActiveDocumentWorkspaceChanged);
                    }
                    this.appWorkspace = value;
                    if (this.appWorkspace != null)
                    {
                        this.appWorkspace.ActiveDocumentWorkspaceChanging += new EventHandler(this.Workspace_ActiveDocumentWorkspaceChanging);
                        this.appWorkspace.ActiveDocumentWorkspaceChanged += new EventHandler(this.Workspace_ActiveDocumentWorkspaceChanged);
                        if (this.appWorkspace.ActiveDocumentWorkspace != null)
                        {
                            this.SetupNewDocument(this.appWorkspace.ActiveDocumentWorkspace.Document);
                        }
                    }
                }
            }
        }

        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get => 
                this.layerControlPanel.BorderStyle;
            set
            {
                this.layerControlPanel.BorderStyle = value;
            }
        }

        [Browsable(false)]
        public PaintDotNet.Document Document
        {
            get => 
                this.document;
            set
            {
                if (this.appWorkspace != null)
                {
                    throw new InvalidOperationException("Workspace property is already set");
                }
                if (this.document != null)
                {
                    this.TearDownOldDocument();
                }
                if (value != null)
                {
                    this.SetupNewDocument(value);
                }
            }
        }

        [Browsable(false)]
        public LayerElement[] Layers
        {
            get
            {
                if (this.layerControls == null)
                {
                    return new LayerElement[0];
                }
                return this.layerControls.ToArrayEx<LayerElement>();
            }
        }

        private int[] SelectedLayerIndexes
        {
            get
            {
                LayerElement[] layers = this.Layers;
                List<int> items = new List<int>();
                for (int i = 0; i < layers.Length; i++)
                {
                    if (layers[i].IsSelected)
                    {
                        items.Add(i);
                    }
                }
                return items.ToArrayEx<int>();
            }
        }

        private class PanelWithLayout : PanelEx
        {
            private LayerControl parentLayerControl;

            public PanelWithLayout()
            {
                base.HideHScroll = true;
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                this.PositionLayers();
                base.OnLayout(levent);
            }

            protected override void OnResize(EventArgs eventargs)
            {
                UI.SuspendControlPainting(this);
                this.PositionLayers();
                base.AutoScrollPosition = new Point(0, -this.AutoScrollOffset.Y);
                base.OnResize(eventargs);
                UI.ResumeControlPainting(this);
                base.Invalidate(true);
            }

            public void PositionLayers()
            {
                if ((this.parentLayerControl != null) && (this.parentLayerControl.layerControls != null))
                {
                    int y = base.AutoScrollPosition.Y;
                    int width = base.ClientRectangle.Width;
                    for (int i = this.parentLayerControl.layerControls.Count - 1; i >= 0; i--)
                    {
                        LayerElement element = this.parentLayerControl.layerControls[i];
                        element.Width = width;
                        element.Top = y;
                        y += element.Height;
                    }
                }
            }

            public LayerControl ParentLayerControl
            {
                get => 
                    this.parentLayerControl;
                set
                {
                    this.parentLayerControl = value;
                }
            }
        }
    }
}

