namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using System;
    using System.Threading;
    using System.Windows.Forms;

    internal class ToolChooserStrip : ToolStripEx, IToolChooser
    {
        private System.Type activeTool;
        private PdnToolStripSplitButton chooseToolButton;
        private string chooseToolLabelText = PdnResources.GetString2("ToolStripChooser.ChooseToolButton.Text");
        private int ignoreToolClicked;
        private bool showChooseDefaults = true;
        private ToolInfo[] toolInfos;
        private bool useToolNameForLabel;

        public event EventHandler ChooseDefaultsClicked;

        public event ToolClickedEventHandler ToolClicked;

        public ToolChooserStrip()
        {
            this.InitializeComponent();
        }

        private void ChooseTool_Click(object sender, EventArgs e)
        {
            this.OnChooseDefaultsClicked();
        }

        private void ChooseToolButton_DropDownClosed(object sender, EventArgs e)
        {
            this.chooseToolButton.DropDownItems.Clear();
        }

        private void ChooseToolButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolInfo tag = e.ClickedItem.Tag as ToolInfo;
            if (tag != null)
            {
                this.OnToolClicked(tag.ToolType);
            }
        }

        private void ChooseToolButton_DropDownOpening(object sender, EventArgs e)
        {
            this.chooseToolButton.DropDownItems.Clear();
            if (this.showChooseDefaults)
            {
                string text = PdnResources.GetString2("ToolChooserStrip.ChooseToolDefaults.Text");
                ImageResource resource = PdnResources.GetImageResource2("Icons.MenuLayersLayerPropertiesIcon.png");
                ToolStripMenuItem item = new ToolStripMenuItem(text, resource.Reference, new EventHandler(this.ChooseTool_Click));
                this.chooseToolButton.DropDownItems.Add(item);
                this.chooseToolButton.DropDownItems.Add(new ToolStripSeparator());
            }
            for (int i = 0; i < this.toolInfos.Length; i++)
            {
                ToolStripMenuItem item2 = new ToolStripMenuItem {
                    Image = this.toolInfos[i].Image.Reference,
                    Text = this.toolInfos[i].Name,
                    Tag = this.toolInfos[i]
                };
                if (this.toolInfos[i].ToolType == this.activeTool)
                {
                    item2.Checked = true;
                }
                else
                {
                    item2.Checked = false;
                }
                this.chooseToolButton.DropDownItems.Add(item2);
            }
        }

        private void InitializeComponent()
        {
            this.chooseToolButton = new PdnToolStripSplitButton();
            base.SuspendLayout();
            this.chooseToolButton.Name = "chooseToolButton";
            this.chooseToolButton.Text = this.chooseToolLabelText;
            this.chooseToolButton.TextImageRelation = TextImageRelation.TextBeforeImage;
            this.chooseToolButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.chooseToolButton.DropDownOpening += new EventHandler(this.ChooseToolButton_DropDownOpening);
            this.chooseToolButton.DropDownClosed += new EventHandler(this.ChooseToolButton_DropDownClosed);
            this.chooseToolButton.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.ChooseToolButton_DropDownItemClicked);
            this.chooseToolButton.Click += (sender, e) => this.chooseToolButton.ShowDropDown();
            this.Items.Add(new ToolStripSeparator());
            this.Items.Add(this.chooseToolButton);
            base.ResumeLayout(false);
        }

        protected virtual void OnChooseDefaultsClicked()
        {
            if (this.ChooseDefaultsClicked != null)
            {
                this.ChooseDefaultsClicked(this, EventArgs.Empty);
            }
        }

        protected virtual void OnToolClicked(System.Type toolType)
        {
            if (this.ignoreToolClicked <= 0)
            {
                this.SetToolButtonLabel();
                if (this.ToolClicked != null)
                {
                    this.ToolClicked(this, new ToolClickedEventArgs(toolType));
                }
            }
        }

        public void SelectTool(System.Type toolType)
        {
            this.SelectTool(toolType, true);
        }

        public void SelectTool(System.Type toolType, bool raiseEvent)
        {
            if (!raiseEvent)
            {
                this.ignoreToolClicked++;
            }
            try
            {
                if (toolType != this.activeTool)
                {
                    foreach (ToolInfo info in this.toolInfos)
                    {
                        if (info.ToolType == toolType)
                        {
                            this.chooseToolButton.Image = info.Image.Reference;
                            this.activeTool = toolType;
                            this.SetToolButtonLabel();
                            return;
                        }
                    }
                }
            }
            finally
            {
                if (!raiseEvent)
                {
                    this.ignoreToolClicked--;
                }
            }
        }

        private void SetToolButtonLabel()
        {
            Predicate<ToolInfo> match = null;
            if (!this.useToolNameForLabel)
            {
                this.chooseToolButton.TextImageRelation = TextImageRelation.TextBeforeImage;
                this.chooseToolButton.Text = this.chooseToolLabelText;
            }
            else
            {
                this.chooseToolButton.TextImageRelation = TextImageRelation.ImageBeforeText;
                ToolInfo info = null;
                if (this.toolInfos != null)
                {
                    if (match == null)
                    {
                        match = check => check.ToolType == this.activeTool;
                    }
                    info = Array.Find<ToolInfo>(this.toolInfos, match);
                }
                if (info == null)
                {
                    this.chooseToolButton.Text = string.Empty;
                }
                else
                {
                    this.chooseToolButton.Text = info.Name;
                }
            }
        }

        public void SetTools(ToolInfo[] newToolInfos)
        {
            this.toolInfos = newToolInfos;
            this.SetToolButtonLabel();
        }

        public bool ShowChooseDefaults
        {
            get => 
                this.showChooseDefaults;
            set
            {
                this.showChooseDefaults = value;
            }
        }

        public bool UseToolNameForLabel
        {
            get => 
                this.useToolNameForLabel;
            set
            {
                this.useToolNameForLabel = value;
                this.SetToolButtonLabel();
            }
        }
    }
}

