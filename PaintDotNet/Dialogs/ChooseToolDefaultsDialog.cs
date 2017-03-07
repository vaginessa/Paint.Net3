namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ChooseToolDefaultsDialog : PdnBaseForm
    {
        private PaintDotNet.Controls.SeparatorLine bottomSeparator;
        private Button cancelButton;
        private Label defaultToolText;
        private Label introText;
        private Button loadFromToolBarButton;
        private Button resetButton;
        private Button saveButton;
        private AppEnvironment toolBarAppEnvironment;
        private System.Type toolBarToolType;
        private ToolChooserStrip toolChooserStrip;
        private List<ToolConfigRow> toolConfigRows;
        private System.Type toolType;

        public ChooseToolDefaultsDialog()
        {
            Func<Keys, bool> callback = null;
            this.toolType = PaintDotNet.Tools.Tool.DefaultToolType;
            this.toolConfigRows = new List<ToolConfigRow>();
            base.SuspendLayout();
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            base.IsGlassDesired = true;
            this.InitializeComponent();
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.Brush | ToolBarConfigItems.Pen | ToolBarConfigItems.PenCaps | ToolBarConfigItems.ShapeType));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.None | ToolBarConfigItems.SelectionCombineMode | ToolBarConfigItems.SelectionDrawMode));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.None | ToolBarConfigItems.Text));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.Gradient));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.FloodMode | ToolBarConfigItems.Tolerance));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.ColorPickerBehavior));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.None | ToolBarConfigItems.Resampling));
            this.toolConfigRows.Add(new ToolConfigRow(ToolBarConfigItems.AlphaBlending | ToolBarConfigItems.Antialiasing));
            PdnToolStripRenderer renderer = new PdnToolStripRenderer();
            for (int i = 0; i < this.toolConfigRows.Count; i++)
            {
                base.Controls.Add(this.toolConfigRows[i].HeaderLabel);
                base.Controls.Add(this.toolConfigRows[i].ToolConfigStrip);
                this.toolConfigRows[i].ToolConfigStrip.Renderer = renderer;
            }
            this.toolChooserStrip.Renderer = renderer;
            base.ResumeLayout();
            base.PerformLayout();
            this.toolChooserStrip.SetTools(DocumentWorkspace.ToolInfos);
            if (callback == null)
            {
                callback = delegate (Keys keys) {
                    this.cancelButton.PerformClick();
                    return true;
                };
            }
            PdnBaseForm.RegisterFormHotKey(Keys.Escape, callback);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        public AppEnvironment CreateAppEnvironmentFromUI()
        {
            AppEnvironment environment = new AppEnvironment();
            foreach (ToolConfigRow row in this.toolConfigRows)
            {
                if ((row.ToolBarConfigItems & ToolBarConfigItems.AlphaBlending) != ToolBarConfigItems.None)
                {
                    environment.AlphaBlending = row.ToolConfigStrip.AlphaBlending;
                }
                if ((row.ToolBarConfigItems & ToolBarConfigItems.Antialiasing) != ToolBarConfigItems.None)
                {
                    environment.AntiAliasing = row.ToolConfigStrip.AntiAliasing;
                }
                if ((row.ToolBarConfigItems & ToolBarConfigItems.Brush) != ToolBarConfigItems.None)
                {
                    environment.BrushInfo = row.ToolConfigStrip.BrushInfo;
                }
                if ((row.ToolBarConfigItems & ToolBarConfigItems.ColorPickerBehavior) != ToolBarConfigItems.None)
                {
                    environment.ColorPickerClickBehavior = row.ToolConfigStrip.ColorPickerClickBehavior;
                }
                if ((row.ToolBarConfigItems & ToolBarConfigItems.FloodMode) != ToolBarConfigItems.None)
                {
                    environment.FloodMode = row.ToolConfigStrip.FloodMode;
                }
                if ((row.ToolBarConfigItems & ToolBarConfigItems.Gradient) != ToolBarConfigItems.None)
                {
                    environment.GradientInfo = row.ToolConfigStrip.GradientInfo;
                }
                if (((row.ToolBarConfigItems & (ToolBarConfigItems.None | ToolBarConfigItems.Pen)) != ToolBarConfigItems.None) || ((row.ToolBarConfigItems & (ToolBarConfigItems.None | ToolBarConfigItems.PenCaps)) != ToolBarConfigItems.None))
                {
                    environment.PenInfo = row.ToolConfigStrip.PenInfo;
                }
                if ((row.ToolBarConfigItems & (ToolBarConfigItems.None | ToolBarConfigItems.Resampling)) != ToolBarConfigItems.None)
                {
                    environment.ResamplingAlgorithm = row.ToolConfigStrip.ResamplingAlgorithm;
                }
                if ((row.ToolBarConfigItems & (ToolBarConfigItems.None | ToolBarConfigItems.SelectionCombineMode)) != ToolBarConfigItems.None)
                {
                    environment.SelectionCombineMode = row.ToolConfigStrip.SelectionCombineMode;
                }
                if ((row.ToolBarConfigItems & (ToolBarConfigItems.None | ToolBarConfigItems.SelectionDrawMode)) != ToolBarConfigItems.None)
                {
                    environment.SelectionDrawModeInfo = row.ToolConfigStrip.SelectionDrawModeInfo;
                }
                if ((row.ToolBarConfigItems & (ToolBarConfigItems.None | ToolBarConfigItems.ShapeType)) != ToolBarConfigItems.None)
                {
                    environment.ShapeDrawType = row.ToolConfigStrip.ShapeDrawType;
                }
                if ((row.ToolBarConfigItems & (ToolBarConfigItems.None | ToolBarConfigItems.Text)) != ToolBarConfigItems.None)
                {
                    environment.FontInfo = row.ToolConfigStrip.FontInfo;
                    environment.FontSmoothing = row.ToolConfigStrip.FontSmoothing;
                    environment.TextAlignment = row.ToolConfigStrip.FontAlignment;
                }
                if ((row.ToolBarConfigItems & (ToolBarConfigItems.None | ToolBarConfigItems.Tolerance)) != ToolBarConfigItems.None)
                {
                    environment.Tolerance = row.ToolConfigStrip.Tolerance;
                }
            }
            return environment;
        }

        private void InitializeComponent()
        {
            this.cancelButton = new Button();
            this.saveButton = new Button();
            this.introText = new Label();
            this.defaultToolText = new Label();
            this.resetButton = new Button();
            this.loadFromToolBarButton = new Button();
            this.toolChooserStrip = new ToolChooserStrip();
            this.bottomSeparator = new PaintDotNet.Controls.SeparatorLine();
            base.SuspendLayout();
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.AutoSize = true;
            this.cancelButton.Click += new EventHandler(this.CancelButton_Click);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.FlatStyle = FlatStyle.System;
            this.saveButton.Name = "saveButton1";
            this.saveButton.AutoSize = true;
            this.saveButton.Click += new EventHandler(this.SaveButton_Click);
            this.saveButton.TabIndex = 2;
            this.saveButton.FlatStyle = FlatStyle.System;
            this.introText.Name = "introText";
            this.introText.TabStop = false;
            this.defaultToolText.Name = "defaultToolText";
            this.defaultToolText.AutoSize = true;
            this.defaultToolText.TabStop = false;
            this.resetButton.Name = "resetButton";
            this.resetButton.AutoSize = true;
            this.resetButton.Click += new EventHandler(this.ResetButton_Click);
            this.resetButton.TabIndex = 0;
            this.resetButton.FlatStyle = FlatStyle.System;
            this.loadFromToolBarButton.Name = "loadFromToolBarButton";
            this.loadFromToolBarButton.AutoSize = true;
            this.loadFromToolBarButton.Click += new EventHandler(this.LoadFromToolBarButton_Click);
            this.loadFromToolBarButton.FlatStyle = FlatStyle.System;
            this.loadFromToolBarButton.TabIndex = 1;
            this.toolChooserStrip.Name = "toolChooserStrip";
            this.toolChooserStrip.Dock = DockStyle.None;
            this.toolChooserStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.toolChooserStrip.ShowChooseDefaults = false;
            this.toolChooserStrip.UseToolNameForLabel = true;
            this.toolChooserStrip.ToolClicked += new ToolClickedEventHandler(this.ToolChooserStrip_ToolClicked);
            this.bottomSeparator.Name = "bottomSeparator";
            base.AcceptButton = this.saveButton;
            base.AutoHandleGlassRelatedOptimizations = true;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.cancelButton;
            base.ClientSize = new Size(100, 0xad);
            base.Controls.Add(this.resetButton);
            base.Controls.Add(this.loadFromToolBarButton);
            base.Controls.Add(this.introText);
            base.Controls.Add(this.defaultToolText);
            base.Controls.Add(this.saveButton);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.toolChooserStrip);
            base.Controls.Add(this.bottomSeparator);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Location = new Point(0, 0);
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.Name = "ChooseToolDefaultsDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void LoadFromToolBarButton_Click(object sender, EventArgs e)
        {
            this.ToolType = this.toolBarToolType;
            this.LoadUIFromAppEnvironment(this.toolBarAppEnvironment);
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString2("ChooseToolDefaultsDialog.Text");
            base.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuLayersLayerPropertiesIcon.png").Reference);
            this.introText.Text = PdnResources.GetString2("ChooseToolDefaultsDialog.IntroText.Text");
            this.defaultToolText.Text = PdnResources.GetString2("ChooseToolDefaultsDialog.DefaultToolText.Text");
            this.loadFromToolBarButton.Text = PdnResources.GetString2("ChooseToolDefaultsDialog.LoadFromToolBarButton.Text");
            this.cancelButton.Text = PdnResources.GetString2("Form.CancelButton.Text");
            this.saveButton.Text = PdnResources.GetString2("Form.SaveButton.Text");
            this.resetButton.Text = PdnResources.GetString2("Form.ResetButton.Text");
            base.LoadResources();
        }

        public void LoadUIFromAppEnvironment(AppEnvironment newAppEnvironment)
        {
            base.SuspendLayout();
            foreach (ToolConfigRow row in this.toolConfigRows)
            {
                row.ToolConfigStrip.LoadFromAppEnvironment(newAppEnvironment);
            }
            base.ResumeLayout();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UI.ScaleWidth(8);
            int num2 = UI.ScaleHeight(8);
            int x = UI.ScaleWidth(8);
            int num4 = UI.ScaleWidth(8);
            int y = UI.ScaleHeight(8);
            int num6 = Math.Max(0, num2 - base.ExtendedFramePadding.Bottom);
            int num7 = UI.ScaleWidth(7);
            int num8 = UI.ScaleHeight(0x10);
            int num9 = UI.ScaleHeight(3);
            this.resetButton.PerformLayout();
            this.loadFromToolBarButton.PerformLayout();
            this.saveButton.PerformLayout();
            this.cancelButton.PerformLayout();
            int num10 = (((((this.resetButton.Width + num7) + this.loadFromToolBarButton.Width) + num7) + this.saveButton.Width) + num7) + this.cancelButton.Width;
            this.defaultToolText.PerformLayout();
            this.toolChooserStrip.PerformLayout();
            int num11 = (this.defaultToolText.Width + num) + this.toolChooserStrip.Width;
            int num12 = 0;
            for (int i = 0; i < this.toolConfigRows.Count; i++)
            {
                Size preferredSize = this.toolConfigRows[i].ToolConfigStrip.GetPreferredSize(new Size(1, 1));
                num12 = Math.Max(num12, preferredSize.Width);
            }
            int width = Math.Max(num10, Math.Max(num11, num12));
            this.introText.Location = new Point(x, y);
            this.introText.Width = width;
            this.introText.Height = this.introText.GetPreferredSize(this.introText.Size).Height;
            this.defaultToolText.Location = new Point(x, this.introText.Bottom + num8);
            this.toolChooserStrip.Location = new Point(this.defaultToolText.Right + num, this.defaultToolText.Top + ((this.defaultToolText.Height - this.toolChooserStrip.Height) / 2));
            int num16 = num2 + Math.Max(this.defaultToolText.Bottom, this.toolChooserStrip.Bottom);
            for (int j = 0; j < this.toolConfigRows.Count; j++)
            {
                this.toolConfigRows[j].HeaderLabel.Location = new Point(x, num16);
                this.toolConfigRows[j].HeaderLabel.Width = width;
                num16 = this.toolConfigRows[j].HeaderLabel.Bottom + num9;
                this.toolConfigRows[j].ToolConfigStrip.Location = new Point(x + 3, num16);
                Size size2 = this.toolConfigRows[j].ToolConfigStrip.GetPreferredSize(new Size(1, 1));
                this.toolConfigRows[j].ToolConfigStrip.Size = size2;
                num16 = this.toolConfigRows[j].ToolConfigStrip.Bottom + num2;
            }
            this.bottomSeparator.Location = new Point(x, num16);
            this.bottomSeparator.Size = this.bottomSeparator.GetPreferredSize(new Size(width, 1));
            this.bottomSeparator.Visible = !base.IsGlassEffectivelyEnabled;
            num16 += this.bottomSeparator.Height;
            num16 += num2;
            int num18 = base.IsGlassEffectivelyEnabled ? -1 : x;
            int num19 = num18;
            this.resetButton.Location = new Point(num18, num16);
            this.loadFromToolBarButton.Location = new Point(this.resetButton.Right + num7, num16);
            this.cancelButton.Location = new Point((((x + width) + num4) - num19) - this.cancelButton.Width, num16);
            this.saveButton.Location = new Point((this.cancelButton.Left - num7) - this.saveButton.Width, num16);
            num16 = num6 + Math.Max(this.resetButton.Bottom, Math.Max(this.loadFromToolBarButton.Bottom, Math.Max(this.saveButton.Bottom, this.cancelButton.Bottom)));
            base.ClientSize = new Size((x + width) + num4, num16);
            base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.bottomSeparator.Top);
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.saveButton.Select();
            base.OnLoad(e);
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            AppEnvironment newAppEnvironment = new AppEnvironment();
            newAppEnvironment.SetToDefaults();
            this.ToolType = PaintDotNet.Tools.Tool.DefaultToolType;
            this.LoadUIFromAppEnvironment(newAppEnvironment);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Yes;
            base.Close();
        }

        public void SetDefaultToolType(System.Type newDefaultToolType)
        {
            this.toolChooserStrip.SelectTool(newDefaultToolType);
        }

        public void SetToolBarSettings(System.Type newToolType, AppEnvironment newToolBarAppEnvironment)
        {
            this.toolBarToolType = newToolType;
            this.toolBarAppEnvironment = newToolBarAppEnvironment.Clone();
        }

        private void ToolChooserStrip_ToolClicked(object sender, ToolClickedEventArgs e)
        {
            this.ToolType = e.ToolType;
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                UI.AddCompositedToCP(createParams);
                return createParams;
            }
        }

        public System.Type ToolType
        {
            get => 
                this.toolType;
            set
            {
                this.toolChooserStrip.SelectTool(value);
                this.toolType = value;
            }
        }

        private sealed class ToolConfigRow
        {
            private PaintDotNet.Controls.HeadingLabel headerLabel;
            private PaintDotNet.ToolBarConfigItems toolBarConfigItems;
            private PaintDotNet.Controls.ToolConfigStrip toolConfigStrip;

            public ToolConfigRow(PaintDotNet.ToolBarConfigItems toolBarConfigItems)
            {
                this.toolBarConfigItems = toolBarConfigItems;
                this.headerLabel = new PaintDotNet.Controls.HeadingLabel();
                this.headerLabel.Name = "headerLabel:" + toolBarConfigItems.ToString();
                this.headerLabel.Text = PdnResources.GetString2(this.GetHeaderResourceName());
                this.headerLabel.RightMargin = 0;
                this.toolConfigStrip = new PaintDotNet.Controls.ToolConfigStrip();
                this.toolConfigStrip.Name = "toolConfigStrip:" + toolBarConfigItems.ToString();
                this.toolConfigStrip.AutoSize = true;
                this.toolConfigStrip.Dock = DockStyle.None;
                this.toolConfigStrip.GripStyle = ToolStripGripStyle.Hidden;
                this.toolConfigStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                this.toolConfigStrip.ToolBarConfigItems = this.toolBarConfigItems;
                this.toolConfigStrip.Renderer = new PdnToolStripRenderer();
            }

            private string GetHeaderResourceName()
            {
                string str2 = this.toolBarConfigItems.ToString().Replace(", ", "");
                return ("ChooseToolDefaultsDialog.ToolConfigRow." + str2 + ".HeaderLabel.Text");
            }

            public PaintDotNet.Controls.HeadingLabel HeaderLabel =>
                this.headerLabel;

            public PaintDotNet.ToolBarConfigItems ToolBarConfigItems =>
                this.toolBarConfigItems;

            public PaintDotNet.Controls.ToolConfigStrip ToolConfigStrip =>
                this.toolConfigStrip;
        }
    }
}

