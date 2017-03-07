namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class ToolsControl : UserControl, IToolChooser
    {
        private Container components;
        private int ignoreToolClicked;
        private Control onePxSpacingLeft;
        private const int tbWidth = 2;
        private ToolStripEx toolStripEx;

        public event EventHandler RelinquishFocus;

        public event ToolClickedEventHandler ToolClicked;

        public ToolsControl()
        {
            this.InitializeComponent();
            this.toolStripEx.Renderer = new PdnToolStripRenderer();
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

        private void InitializeComponent()
        {
            this.toolStripEx = new ToolStripEx();
            this.onePxSpacingLeft = new Control();
            base.SuspendLayout();
            this.toolStripEx.Dock = DockStyle.Top;
            this.toolStripEx.GripStyle = ToolStripGripStyle.Hidden;
            this.toolStripEx.LayoutStyle = ToolStripLayoutStyle.Flow;
            this.toolStripEx.ItemClicked += new ToolStripItemClickedEventHandler(this.ToolStripEx_ItemClicked);
            this.toolStripEx.Name = "toolStripEx";
            this.toolStripEx.AutoSize = true;
            this.toolStripEx.RelinquishFocus += new EventHandler(this.ToolStripEx_RelinquishFocus);
            this.onePxSpacingLeft.Dock = DockStyle.Left;
            this.onePxSpacingLeft.Width = 1;
            this.onePxSpacingLeft.Name = "onePxSpacingLeft";
            base.Controls.Add(this.toolStripEx);
            base.Controls.Add(this.onePxSpacingLeft);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.Name = "MainToolBar";
            base.Size = new Size(0x30, 0x148);
            base.ResumeLayout(false);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            int width;
            if (this.toolStripEx.Items.Count > 0)
            {
                width = this.toolStripEx.Items[0].Width;
            }
            else
            {
                width = 0;
            }
            this.toolStripEx.Width = ((this.toolStripEx.Padding.Left + (width * 2)) + (this.toolStripEx.Margin.Horizontal * 2)) + this.toolStripEx.Padding.Right;
            this.toolStripEx.Height = this.toolStripEx.GetPreferredSize(this.toolStripEx.Size).Height;
            base.Width = this.toolStripEx.Width + this.onePxSpacingLeft.Width;
            base.Height = this.toolStripEx.Height;
            base.OnLayout(e);
        }

        private void OnRelinquishFocus()
        {
            if (this.RelinquishFocus != null)
            {
                this.RelinquishFocus(this, EventArgs.Empty);
            }
        }

        protected virtual void OnToolClicked(System.Type toolType)
        {
            if ((this.ignoreToolClicked <= 0) && (this.ToolClicked != null))
            {
                this.ToolClicked(this, new ToolClickedEventArgs(toolType));
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
                foreach (ToolStripButton button in this.toolStripEx.Items)
                {
                    if (((System.Type) button.Tag) == toolType)
                    {
                        this.ToolStripEx_ItemClicked(this, new ToolStripItemClickedEventArgs(button));
                        return;
                    }
                }
                throw new ArgumentException("Tool type not found");
            }
            finally
            {
                if (!raiseEvent)
                {
                    this.ignoreToolClicked--;
                }
            }
        }

        public void SetTools(ToolInfo[] toolInfos)
        {
            if (this.toolStripEx != null)
            {
                this.toolStripEx.Items.Clear();
            }
            ToolStripItem[] toolStripItems = new ToolStripItem[toolInfos.Length];
            string format = PdnResources.GetString2("ToolsControl.ToolToolTip.Format");
            for (int i = 0; i < toolInfos.Length; i++)
            {
                ToolInfo info = toolInfos[i];
                toolStripItems[i] = new ToolStripButton { 
                    Image = info.Image.Reference,
                    Tag = info.ToolType,
                    ToolTipText = string.Format(format, info.Name, char.ToUpperInvariant(info.HotKey).ToString())
                };
            }
            this.toolStripEx.Items.AddRange(toolStripItems);
        }

        private void ToolStripEx_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            foreach (ToolStripButton button in this.toolStripEx.Items)
            {
                button.Checked = button == e.ClickedItem;
            }
            this.OnToolClicked((System.Type) e.ClickedItem.Tag);
        }

        private void ToolStripEx_RelinquishFocus(object sender, EventArgs e)
        {
            this.OnRelinquishFocus();
        }
    }
}

