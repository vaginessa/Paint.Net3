namespace PaintDotNet
{
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class TaskDialogForm : PdnBaseForm
    {
        private TaskButton acceptTaskButton;
        private TaskAuxControl[] auxControls = new TaskAuxControl[0];
        private Control[] auxControlsConcrete = new Control[0];
        private TaskButton cancelTaskButton;
        private CommandButton[] commandButtons;
        private TaskButton dialogResult;
        private RichTextBox introTextBox;
        private Size introTextBoxSize;
        private bool scaleTaskImageWithDpi;
        private PaintDotNet.Controls.HeadingLabel separator;
        private TaskButton[] taskButtons;
        private PictureBox taskImagePB;

        public TaskDialogForm()
        {
            this.InitializeComponent();
        }

        private void AuxControl_ClientSizeChanged(object sender, EventArgs e)
        {
            base.PerformLayout();
        }

        private void CommandButton_Click(object sender, EventArgs e)
        {
            CommandButton button = (CommandButton) sender;
            this.dialogResult = (TaskButton) button.Tag;
            base.Close();
        }

        private void InitCommandButtons()
        {
            base.SuspendLayout();
            if (this.commandButtons != null)
            {
                foreach (CommandButton button in this.commandButtons)
                {
                    base.Controls.Remove(button);
                    button.Tag = null;
                    button.Click -= new EventHandler(this.CommandButton_Click);
                    button.Dispose();
                }
                this.commandButtons = null;
            }
            this.commandButtons = new CommandButton[this.taskButtons.Length];
            IButtonControl control = null;
            IButtonControl control2 = null;
            for (int i = 0; i < this.commandButtons.Length; i++)
            {
                TaskButton button2 = this.taskButtons[i];
                CommandButton button3 = new CommandButton {
                    ActionText = button2.ActionText,
                    ActionImage = button2.Image,
                    AutoSize = true,
                    ExplanationText = button2.ExplanationText,
                    Tag = button2
                };
                button3.Click += new EventHandler(this.CommandButton_Click);
                this.commandButtons[i] = button3;
                base.Controls.Add(button3);
                if (this.acceptTaskButton == button2)
                {
                    control = button3;
                }
                if (this.cancelTaskButton == button2)
                {
                    control2 = button3;
                }
            }
            base.AcceptButton = control;
            base.CancelButton = control2;
            if ((control != null) && (control is Control))
            {
                ((Control) control).Select();
            }
            base.ResumeLayout();
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            this.introTextBox = new RichTextBox();
            this.taskImagePB = new PictureBox();
            this.separator = new PaintDotNet.Controls.HeadingLabel();
            this.introTextBox.Name = "introTextBox";
            this.introTextBox.AllowDrop = false;
            this.introTextBox.Multiline = true;
            this.introTextBox.ReadOnly = true;
            this.introTextBox.BorderStyle = BorderStyle.None;
            this.introTextBox.TabStop = false;
            this.introTextBox.ContentsResized += delegate (object s, ContentsResizedEventArgs e) {
                this.introTextBoxSize = e.NewRectangle.Size;
                base.PerformLayout();
            };
            this.taskImagePB.Name = "taskImagePB";
            this.taskImagePB.SizeMode = PictureBoxSizeMode.StretchImage;
            this.separator.Name = "separator";
            this.separator.RightMargin = 0;
            base.Name = "TaskDialogForm";
            base.ClientSize = new Size(300, 100);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.Controls.AddRange(new Control[] { this.introTextBox, this.taskImagePB, this.separator });
            base.ResumeLayout();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int leftMargin = UI.ScaleWidth(8);
            int rightMargin = UI.ScaleWidth(8);
            int num = UI.ScaleHeight(8);
            UI.ScaleHeight(8);
            int num2 = UI.ScaleWidth(8);
            int topSectionToLinksVMargin = UI.ScaleHeight(8);
            int num3 = UI.ScaleHeight(0);
            int num4 = UI.ScaleHeight(8);
            int num5 = (base.ClientSize.Width - leftMargin) - rightMargin;
            if (this.taskImagePB.Image == null)
            {
                this.taskImagePB.Location = new Point(0, num);
                this.taskImagePB.Size = new Size(0, 0);
                this.taskImagePB.Visible = false;
            }
            else
            {
                this.taskImagePB.Location = new Point(leftMargin, num);
                if (this.scaleTaskImageWithDpi)
                {
                    this.taskImagePB.Size = UI.ScaleSize(this.taskImagePB.Image.Size);
                }
                else
                {
                    this.taskImagePB.Size = this.taskImagePB.Image.Size;
                }
                this.taskImagePB.Visible = true;
            }
            this.introTextBox.Location = new Point(this.taskImagePB.Right + num2, this.taskImagePB.Top);
            this.introTextBox.Width = (base.ClientSize.Width - this.introTextBox.Left) - rightMargin;
            this.introTextBox.Height = this.introTextBoxSize.Height;
            int y = Math.Max(this.taskImagePB.Bottom, this.introTextBox.Bottom);
            y += topSectionToLinksVMargin;
            this.auxControlsConcrete.ForEach<Control>(delegate (Control auxControl) {
                auxControl.Location = new Point(leftMargin, y);
                auxControl.PerformLayout();
                auxControl.Size = auxControl.GetPreferredSize(new Size((this.ClientSize.Width - auxControl.Left) - rightMargin, auxControl.Height));
                y += auxControl.Height;
                y += topSectionToLinksVMargin;
            });
            if (this.commandButtons != null)
            {
                this.separator.Location = new Point(leftMargin, y);
                this.separator.Width = num5;
                y += this.separator.Height;
                for (int i = 0; i < this.commandButtons.Length; i++)
                {
                    this.commandButtons[i].Location = new Point(leftMargin, y);
                    this.commandButtons[i].Width = num5;
                    this.commandButtons[i].PerformLayout();
                    y += this.commandButtons[i].Height + num3;
                }
                y += num4;
            }
            base.ClientSize = new Size(base.ClientSize.Width, y);
            base.OnLayout(levent);
        }

        private void VerifyNotShown()
        {
            if (base.IsShown)
            {
                throw new InvalidOperationException("Cannot set this after the dialog is shown");
            }
        }

        public TaskButton AcceptTaskButton
        {
            get => 
                this.acceptTaskButton;
            set
            {
                this.acceptTaskButton = value;
                IButtonControl control = null;
                for (int i = 0; i < this.commandButtons.Length; i++)
                {
                    TaskButton tag = this.commandButtons[i].Tag as TaskButton;
                    if (this.acceptTaskButton == tag)
                    {
                        control = this.commandButtons[i];
                    }
                }
                base.AcceptButton = control;
            }
        }

        public TaskAuxControl[] AuxControls
        {
            get => 
                this.auxControls.CloneT<TaskAuxControl>();
            set
            {
                this.VerifyNotShown();
                this.auxControlsConcrete.ForEach<Control>(delegate (Control auxControl) {
                    auxControl.ClientSizeChanged -= new EventHandler(this.AuxControl_ClientSizeChanged);
                    auxControl.Dispose();
                });
                this.auxControlsConcrete = null;
                if (value == null)
                {
                    this.auxControls = new TaskAuxControl[0];
                }
                else
                {
                    this.auxControls = value.CloneT<TaskAuxControl>();
                }
                this.auxControlsConcrete = (from aux in this.auxControls select aux.CreateControl()).ToArrayEx<Control>();
                this.auxControlsConcrete.ForEach<Control>(delegate (Control auxControl) {
                    auxControl.ClientSizeChanged += new EventHandler(this.AuxControl_ClientSizeChanged);
                    base.Controls.Add(auxControl);
                });
            }
        }

        public TaskButton CancelTaskButton
        {
            get => 
                this.cancelTaskButton;
            set
            {
                this.cancelTaskButton = value;
                IButtonControl control = null;
                for (int i = 0; i < this.commandButtons.Length; i++)
                {
                    TaskButton tag = this.commandButtons[i].Tag as TaskButton;
                    if (this.cancelTaskButton == tag)
                    {
                        control = this.commandButtons[i];
                    }
                }
                base.CancelButton = control;
            }
        }

        public TaskButton DialogResult =>
            this.dialogResult;

        public string IntroText
        {
            get => 
                this.introTextBox.Text;
            set
            {
                this.VerifyNotShown();
                this.introTextBox.Text = value;
                base.PerformLayout();
                base.Invalidate(true);
            }
        }

        public bool ScaleTaskImageWithDpi
        {
            get => 
                this.scaleTaskImageWithDpi;
            set
            {
                this.VerifyNotShown();
                this.scaleTaskImageWithDpi = value;
                base.PerformLayout();
                base.Invalidate(true);
            }
        }

        public TaskButton[] TaskButtons
        {
            get => 
                this.taskButtons.CloneT<TaskButton>();
            set
            {
                this.VerifyNotShown();
                this.taskButtons = value.CloneT<TaskButton>();
                this.InitCommandButtons();
                base.PerformLayout();
                base.Invalidate(true);
            }
        }

        public Image TaskImage
        {
            get => 
                this.taskImagePB.Image;
            set
            {
                this.taskImagePB.Image = value;
                base.PerformLayout();
                this.Refresh();
            }
        }
    }
}

