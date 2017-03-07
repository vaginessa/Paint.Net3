namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal class SavePaletteDialog : PdnBaseForm
    {
        private Button cancelButton;
        private IContainer components;
        private ListBox listBox;
        private Label palettesLabel;
        private Button saveButton;
        private TextBox textBox;
        private Label typeANameLabel;

        public SavePaletteDialog()
        {
            this.InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (base.Icon != null)
                {
                    Icon icon = base.Icon;
                    base.Icon = null;
                    icon.Dispose();
                    icon = null;
                }
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.typeANameLabel = new Label();
            this.textBox = new TextBox();
            this.listBox = new ListBox();
            this.saveButton = new Button();
            this.palettesLabel = new Label();
            this.cancelButton = new Button();
            base.SuspendLayout();
            this.typeANameLabel.AutoSize = true;
            this.typeANameLabel.Location = new Point(5, 8);
            this.typeANameLabel.Margin = new Padding(0);
            this.typeANameLabel.Name = "typeANameLabel";
            this.typeANameLabel.Size = new Size(50, 13);
            this.typeANameLabel.TabIndex = 0;
            this.typeANameLabel.Text = "infoLabel";
            this.textBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.textBox.Location = new Point(8, 0x19);
            this.textBox.Name = "textBox";
            this.textBox.Size = new Size(0x120, 20);
            this.textBox.TabIndex = 2;
            this.textBox.Validating += new CancelEventHandler(this.TextBox_Validating);
            this.textBox.TextChanged += new EventHandler(this.TextBox_TextChanged);
            this.palettesLabel.AutoSize = true;
            this.palettesLabel.Location = new Point(5, 50);
            this.palettesLabel.Margin = new Padding(0);
            this.palettesLabel.Name = "palettesLabel";
            this.palettesLabel.Size = new Size(0x23, 13);
            this.palettesLabel.TabIndex = 5;
            this.palettesLabel.Text = "label1";
            this.listBox.FormattingEnabled = true;
            this.listBox.Location = new Point(8, 0x43);
            this.listBox.Name = "listBox";
            this.listBox.Size = new Size(0x121, 0x6c);
            this.listBox.Sorted = true;
            this.listBox.TabIndex = 3;
            this.listBox.SelectedIndexChanged += new EventHandler(this.ListBox_SelectedIndexChanged);
            this.saveButton.DialogResult = DialogResult.Cancel;
            this.saveButton.Location = new Point(8, 0xb9);
            this.saveButton.Name = "saveButton2";
            this.saveButton.Size = new Size(0x4b, 0x17);
            this.saveButton.TabIndex = 4;
            this.saveButton.Text = "button1";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.FlatStyle = FlatStyle.System;
            this.saveButton.Click += new EventHandler(this.SaveButton_Click);
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Location = new Point(0x59, 0xb9);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(0x4b, 0x17);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "button1";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.FlatStyle = FlatStyle.System;
            this.cancelButton.Click += new EventHandler(this.CancelButton_Click);
            base.AcceptButton = this.saveButton;
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.CancelButton = this.cancelButton;
            base.ClientSize = new Size(310, 0xd9);
            base.Controls.Add(this.palettesLabel);
            base.Controls.Add(this.listBox);
            base.Controls.Add(this.textBox);
            base.Controls.Add(this.saveButton);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.typeANameLabel);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "SavePaletteDialog";
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "SavePaletteDialog";
            base.Controls.SetChildIndex(this.typeANameLabel, 0);
            base.Controls.SetChildIndex(this.cancelButton, 0);
            base.Controls.SetChildIndex(this.saveButton, 0);
            base.Controls.SetChildIndex(this.textBox, 0);
            base.Controls.SetChildIndex(this.listBox, 0);
            base.Controls.SetChildIndex(this.palettesLabel, 0);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox.SelectedItem != null)
            {
                this.textBox.Text = this.listBox.SelectedItem.ToString();
                this.textBox.Focus();
                this.listBox.SelectedItem = null;
            }
        }

        public override void LoadResources()
        {
            this.Text = PdnResources.GetString2("SavePaletteDialog.Text");
            base.Icon = Utility.ImageToIcon(PdnResources.GetImageResource2("Icons.MenuFileSaveAsIcon.png").Reference);
            this.cancelButton.Text = PdnResources.GetString2("Form.CancelButton.Text");
            this.saveButton.Text = PdnResources.GetString2("Form.SaveButton.Text");
            this.typeANameLabel.Text = PdnResources.GetString2("SavePaletteDialog.TypeANameLabel.Text");
            this.palettesLabel.Text = PdnResources.GetString2("SavePaletteDialog.PalettesLabel.Text");
            base.LoadResources();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.ValidatePaletteName();
            base.OnLoad(e);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            this.ValidatePaletteName();
        }

        private void TextBox_Validating(object sender, CancelEventArgs e)
        {
            this.ValidatePaletteName();
        }

        private void ValidatePaletteName()
        {
            if (!PaletteCollection.ValidatePaletteName(this.textBox.Text))
            {
                this.saveButton.Enabled = false;
                if (!string.IsNullOrEmpty(this.textBox.Text))
                {
                    this.textBox.BackColor = Color.Red;
                }
            }
            else
            {
                this.saveButton.Enabled = true;
                this.textBox.BackColor = SystemColors.Window;
            }
        }

        public string PaletteName
        {
            get => 
                this.textBox.Text;
            set
            {
                this.textBox.Text = value;
            }
        }

        public string[] PaletteNames
        {
            set
            {
                this.listBox.Items.Clear();
                AutoCompleteStringCollection strings = new AutoCompleteStringCollection();
                foreach (string str in value)
                {
                    strings.Add(str);
                    this.listBox.Items.Add(str);
                }
                this.textBox.AutoCompleteCustomSource = strings;
                this.textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            }
        }
    }
}

