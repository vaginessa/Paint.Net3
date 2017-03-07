namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class LayerForm : FloatingToolForm
    {
        private ToolStripButton addNewLayerButton;
        private IContainer components;
        private ToolStripButton deleteLayerButton;
        private ToolStripButton duplicateLayerButton;
        private PaintDotNet.Controls.LayerControl layerControl;
        private ToolStripButton mergeLayerDownButton;
        private ToolStripButton moveLayerDownButton;
        private ToolStripButton moveLayerUpButton;
        private ToolStripButton propertiesButton;
        private ToolStripEx toolStrip;

        public event EventHandler DeleteLayerButtonClick;

        public event EventHandler DuplicateLayerButtonClick;

        public event EventHandler MergeLayerDownClick;

        public event EventHandler MoveLayerDownButtonClick;

        public event EventHandler MoveLayerUpButtonClick;

        public event EventHandler NewLayerButtonClick;

        public event EventHandler PropertiesButtonClick;

        public LayerForm()
        {
            this.InitializeComponent();
            this.addNewLayerButton.Image = PdnResources.GetImageResource2("Icons.MenuLayersAddNewLayerIcon.png").Reference;
            this.deleteLayerButton.Image = PdnResources.GetImageResource2("Icons.MenuLayersDeleteLayerIcon.png").Reference;
            this.moveLayerUpButton.Image = PdnResources.GetImageResource2("Icons.MenuLayersMoveLayerUpIcon.png").Reference;
            this.moveLayerDownButton.Image = PdnResources.GetImageResource2("Icons.MenuLayersMoveLayerDownIcon.png").Reference;
            this.duplicateLayerButton.Image = PdnResources.GetImageResource2("Icons.MenuLayersDuplicateLayerIcon.png").Reference;
            this.mergeLayerDownButton.Image = PdnResources.GetImageResource2("Icons.MenuLayersMergeLayerDownIcon.png").Reference;
            this.propertiesButton.Image = PdnResources.GetImageResource2("Icons.MenuLayersLayerPropertiesIcon.png").Reference;
            this.layerControl.KeyUp += new KeyEventHandler(this.LayerControl_KeyUp);
            this.Text = PdnResources.GetString2("LayerForm.Text");
            this.addNewLayerButton.ToolTipText = PdnResources.GetString2("LayerForm.AddNewLayerButton.ToolTipText");
            this.deleteLayerButton.ToolTipText = PdnResources.GetString2("LayerForm.DeleteLayerButton.ToolTipText");
            this.duplicateLayerButton.ToolTipText = PdnResources.GetString2("LayerForm.DuplicateLayerButton.ToolTipText");
            this.mergeLayerDownButton.ToolTipText = PdnResources.GetString2("LayerForm.MergeLayerDownButton.ToolTipText");
            this.moveLayerUpButton.ToolTipText = PdnResources.GetString2("LayerForm.MoveLayerUpButton.ToolTipText");
            this.moveLayerDownButton.ToolTipText = PdnResources.GetString2("LayerForm.MoveLayerDownButton.ToolTipText");
            this.propertiesButton.ToolTipText = PdnResources.GetString2("LayerForm.PropertiesButton.ToolTipText");
            this.MinimumSize = base.Size;
            this.toolStrip.Renderer = new PdnToolStripRenderer();
        }

        private void DeleteLayerButton_Click(object sender, EventArgs e)
        {
            this.OnDeleteLayerButtonClick();
        }

        private void DetermineButtonEnableStates()
        {
            this.DetermineButtonEnableStates(this.layerControl.ActiveLayerIndex);
        }

        private void DetermineButtonEnableStates(int index)
        {
            if (this.layerControl.AppWorkspace != null)
            {
                if (((this.layerControl.AppWorkspace.ActiveDocumentWorkspace == null) || (this.layerControl.AppWorkspace.ActiveDocumentWorkspace.Document == null)) || (index == 0))
                {
                    this.moveLayerDownButton.Enabled = false;
                }
                else
                {
                    this.moveLayerDownButton.Enabled = true;
                }
                if (((this.layerControl.AppWorkspace.ActiveDocumentWorkspace == null) || (this.layerControl.AppWorkspace.ActiveDocumentWorkspace.Document == null)) || (index == (this.layerControl.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count - 1)))
                {
                    this.moveLayerUpButton.Enabled = false;
                }
                else
                {
                    this.moveLayerUpButton.Enabled = true;
                }
                if (((this.layerControl.AppWorkspace.ActiveDocumentWorkspace == null) || (this.layerControl.AppWorkspace.ActiveDocumentWorkspace.Document == null)) || (this.layerControl.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count <= 1))
                {
                    this.deleteLayerButton.Enabled = false;
                }
                else
                {
                    this.deleteLayerButton.Enabled = true;
                }
                if (((this.layerControl.AppWorkspace.ActiveDocumentWorkspace == null) || (this.layerControl.AppWorkspace.ActiveDocumentWorkspace.Document == null)) || ((this.layerControl.AppWorkspace.ActiveDocumentWorkspace.ActiveLayerIndex == 0) || (this.layerControl.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.Count < 2)))
                {
                    this.mergeLayerDownButton.Enabled = false;
                }
                else
                {
                    this.mergeLayerDownButton.Enabled = true;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                this.components = null;
            }
            base.Dispose(disposing);
        }

        private void DuplicateLayerButton_Click(object sender, EventArgs e)
        {
            this.OnDuplicateLayerButtonClick();
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.layerControl = new PaintDotNet.Controls.LayerControl();
            this.toolStrip = new ToolStripEx();
            this.addNewLayerButton = new ToolStripButton();
            this.deleteLayerButton = new ToolStripButton();
            this.duplicateLayerButton = new ToolStripButton();
            this.mergeLayerDownButton = new ToolStripButton();
            this.moveLayerUpButton = new ToolStripButton();
            this.moveLayerDownButton = new ToolStripButton();
            this.propertiesButton = new ToolStripButton();
            this.toolStrip.SuspendLayout();
            base.SuspendLayout();
            this.layerControl.Dock = DockStyle.Fill;
            this.layerControl.Document = null;
            this.layerControl.Location = new Point(0, 0);
            this.layerControl.Name = "layerControl";
            this.layerControl.Size = new Size(160, 0x9e);
            this.layerControl.TabIndex = 5;
            this.layerControl.AppWorkspace = null;
            this.layerControl.ActiveLayerChanged += new EventHandler<EventArgs<Layer>>(this.LayerControl_ClickOnLayer);
            this.layerControl.ClickedOnLayer += new EventHandler<EventArgs<Layer>>(this.LayerControl_ClickOnLayer);
            this.layerControl.DoubleClickedOnLayer += new EventHandler<EventArgs<Layer>>(this.LayerControl_DoubleClickedOnLayer);
            this.layerControl.RelinquishFocus += new EventHandler(this.LayerControl_RelinquishFocus);
            this.toolStrip.Dock = DockStyle.Bottom;
            this.toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new ToolStripItem[] { this.addNewLayerButton, this.deleteLayerButton, this.duplicateLayerButton, this.mergeLayerDownButton, this.moveLayerUpButton, this.moveLayerDownButton, this.propertiesButton });
            this.toolStrip.LayoutStyle = ToolStripLayoutStyle.Flow;
            this.toolStrip.Location = new Point(0, 0x84);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new Size(160, 0x1a);
            this.toolStrip.TabIndex = 7;
            this.toolStrip.TabStop = true;
            this.toolStrip.RelinquishFocus += new EventHandler(this.ToolStrip_RelinquishFocus);
            this.addNewLayerButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.addNewLayerButton.Name = "addNewLayerButton";
            this.addNewLayerButton.Size = new Size(0x17, 4);
            this.addNewLayerButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.deleteLayerButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.deleteLayerButton.Name = "deleteLayerButton";
            this.deleteLayerButton.Size = new Size(0x17, 4);
            this.deleteLayerButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.duplicateLayerButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.duplicateLayerButton.Name = "duplicateLayerButton";
            this.duplicateLayerButton.Size = new Size(0x17, 4);
            this.duplicateLayerButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.mergeLayerDownButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.mergeLayerDownButton.Name = "mergeLayerDownButton";
            this.mergeLayerDownButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.moveLayerUpButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.moveLayerUpButton.Name = "moveLayerUpButton";
            this.moveLayerUpButton.Size = new Size(0x17, 4);
            this.moveLayerUpButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.moveLayerDownButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.moveLayerDownButton.Name = "moveLayerDownButton";
            this.moveLayerDownButton.Size = new Size(0x17, 4);
            this.moveLayerDownButton.Click += new EventHandler(this.OnToolStripButtonClick);
            this.propertiesButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.propertiesButton.Name = "propertiesButton";
            this.propertiesButton.Size = new Size(0x17, 4);
            this.propertiesButton.Click += new EventHandler(this.OnToolStripButtonClick);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoValidate = AutoValidate.EnablePreventFocusChange;
            base.ClientSize = new Size(0xa5, 0x9e);
            base.Controls.Add(this.toolStrip);
            base.Controls.Add(this.layerControl);
            base.Name = "LayersForm";
            base.Controls.SetChildIndex(this.layerControl, 0);
            base.Controls.SetChildIndex(this.toolStrip, 0);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void LayerControl_ClickOnLayer(object sender, EventArgs<Layer> ce)
        {
            int index = this.layerControl.AppWorkspace.ActiveDocumentWorkspace.Document.Layers.IndexOf(ce.Data);
            this.DetermineButtonEnableStates(index);
        }

        private void LayerControl_DoubleClickedOnLayer(object sender, EventArgs<Layer> ce)
        {
            this.OnPropertiesButtonClick();
            this.OnRelinquishFocus();
        }

        private void LayerControl_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) && (e.Modifiers == Keys.None))
            {
                this.OnDeleteLayerButtonClick();
                e.Handled = true;
            }
        }

        private void LayerControl_RelinquishFocus(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            this.OnMoveLayerDownButtonClick();
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            this.OnMoveLayerUpButtonClick();
        }

        private void NewLayerButton_Click(object sender, EventArgs e)
        {
            this.OnNewLayerButtonClick();
        }

        private void OnDeleteLayerButtonClick()
        {
            if (this.DeleteLayerButtonClick != null)
            {
                this.DeleteLayerButtonClick(this, EventArgs.Empty);
            }
        }

        private void OnDuplicateLayerButtonClick()
        {
            if (this.DuplicateLayerButtonClick != null)
            {
                this.DuplicateLayerButtonClick(this, EventArgs.Empty);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (this.layerControl != null)
            {
                this.layerControl.Size = new Size(base.ClientRectangle.Width, base.ClientRectangle.Height - (this.toolStrip.Height + (base.ClientRectangle.Height - base.ClientRectangle.Bottom)));
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void OnMergeLayerDownButtonClick()
        {
            if (this.MergeLayerDownClick != null)
            {
                this.MergeLayerDownClick(this, EventArgs.Empty);
            }
        }

        private void OnMoveLayerDownButtonClick()
        {
            if (this.MoveLayerDownButtonClick != null)
            {
                this.MoveLayerDownButtonClick(this, EventArgs.Empty);
            }
        }

        private void OnMoveLayerUpButtonClick()
        {
            if (this.MoveLayerUpButtonClick != null)
            {
                this.MoveLayerUpButtonClick(this, EventArgs.Empty);
            }
        }

        private void OnNewLayerButtonClick()
        {
            if (this.NewLayerButtonClick != null)
            {
                this.NewLayerButtonClick(this, EventArgs.Empty);
            }
        }

        private void OnPropertiesButtonClick()
        {
            if (this.PropertiesButtonClick != null)
            {
                this.PropertiesButtonClick(this, EventArgs.Empty);
            }
        }

        private void OnToolStripButtonClick(object sender, EventArgs e)
        {
            UI.SuspendControlPainting(this.layerControl);
            if (sender == this.addNewLayerButton)
            {
                this.OnNewLayerButtonClick();
            }
            else if (sender == this.deleteLayerButton)
            {
                this.OnDeleteLayerButtonClick();
            }
            else if (sender == this.duplicateLayerButton)
            {
                this.OnDuplicateLayerButtonClick();
            }
            else if (sender == this.mergeLayerDownButton)
            {
                this.OnMergeLayerDownButtonClick();
            }
            else if (sender == this.moveLayerUpButton)
            {
                this.OnMoveLayerUpButtonClick();
            }
            else if (sender == this.moveLayerDownButton)
            {
                this.OnMoveLayerDownButtonClick();
            }
            UI.ResumeControlPainting(this.layerControl);
            this.layerControl.Invalidate(true);
            if (sender == this.propertiesButton)
            {
                this.OnPropertiesButtonClick();
            }
            this.DetermineButtonEnableStates();
            this.OnRelinquishFocus();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (base.Visible)
            {
                foreach (LayerElement element in this.layerControl.Layers)
                {
                    element.RefreshPreview();
                }
            }
            base.OnVisibleChanged(e);
        }

        public void PerformDeleteLayerClick()
        {
            this.OnDeleteLayerButtonClick();
        }

        public void PerformDuplicateLayerClick()
        {
            this.OnDuplicateLayerButtonClick();
        }

        public void PerformMoveLayerDownClick()
        {
            this.OnMoveLayerDownButtonClick();
        }

        public void PerformMoveLayerUpClick()
        {
            this.OnMoveLayerUpButtonClick();
        }

        public void PerformNewLayerClick()
        {
            this.OnNewLayerButtonClick();
        }

        public void PerformPropertiesClick()
        {
            this.OnPropertiesButtonClick();
        }

        private void PropertiesButton_Click(object sender, EventArgs e)
        {
            this.OnPropertiesButtonClick();
        }

        private void ToolStrip_RelinquishFocus(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
        }

        public PaintDotNet.Controls.LayerControl LayerControl =>
            this.layerControl;
    }
}

