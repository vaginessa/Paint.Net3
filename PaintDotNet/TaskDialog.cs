namespace PaintDotNet
{
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class TaskDialog
    {
        public static int DefaultPixelWidth96Dpi = 300;
        private Image taskImage;
        internal EventHandler TaskImageChanged;

        public TaskDialog()
        {
            this.ScaleTaskImageWithDpi = true;
            this.PixelWidth96Dpi = DefaultPixelWidth96Dpi;
            this.EnableCloseButton = true;
        }

        public TaskButton Show(IWin32Window owner)
        {
            IDisposable first = null;
            TaskButton dialogResult;
            try
            {
                EventHandler handler = null;
                using (TaskDialogForm form = new TaskDialogForm())
                {
                    form.Icon = this.Icon;
                    form.IntroText = this.IntroText;
                    form.Text = this.Title;
                    form.TaskImage = this.TaskImage;
                    if (handler == null)
                    {
                        handler = delegate (object s, EventArgs e) {
                            form.TaskImage = this.TaskImage;
                        };
                    }
                    EventHandler taskImageChangedHandler = handler;
                    this.TaskImageChanged = (EventHandler) Delegate.Combine(this.TaskImageChanged, taskImageChangedHandler);
                    first = Disposable.Combine(first, Disposable.FromAction(delegate {
                        this.TaskImageChanged = (EventHandler) Delegate.Remove(this.TaskImageChanged, taskImageChangedHandler);
                    }));
                    form.ScaleTaskImageWithDpi = this.ScaleTaskImageWithDpi;
                    form.TaskButtons = this.TaskButtons;
                    form.AcceptTaskButton = this.AcceptButton;
                    form.CancelTaskButton = this.CancelButton;
                    form.AuxControls = this.AuxControls;
                    if (!this.EnableCloseButton)
                    {
                        UI.EnableCloseBox(form, false);
                    }
                    int width = UI.ScaleWidth(this.PixelWidth96Dpi);
                    form.ClientSize = new Size(width, form.ClientSize.Height);
                    if (owner == null)
                    {
                        form.StartPosition = FormStartPosition.CenterScreen;
                    }
                    form.ShowDialog(owner);
                    dialogResult = form.DialogResult;
                }
            }
            finally
            {
                if (first != null)
                {
                    first.Dispose();
                    first = null;
                }
            }
            return dialogResult;
        }

        public TaskButton AcceptButton { get; set; }

        public TaskAuxControl[] AuxControls { get; set; }

        public TaskButton CancelButton { get; set; }

        public bool EnableCloseButton { get; set; }

        public System.Drawing.Icon Icon { get; set; }

        public string IntroText { get; set; }

        public int PixelWidth96Dpi { get; set; }

        public bool ScaleTaskImageWithDpi { get; set; }

        public TaskButton[] TaskButtons { get; set; }

        public Image TaskImage
        {
            get => 
                this.taskImage;
            set
            {
                if (this.taskImage != value)
                {
                    this.taskImage = value;
                    if (this.TaskImageChanged != null)
                    {
                        this.TaskImageChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public string Title { get; set; }
    }
}

