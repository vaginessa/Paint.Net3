namespace PaintDotNet.Dialogs
{
    using PaintDotNet;
    using PaintDotNet.Concurrency;
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class TaskProgressDialog : PdnBaseForm
    {
        private Button cancelButton;
        private bool closeOnFinished = true;
        private ControlDispatcher dispatcher;
        private Label headerLabel;
        private ProgressBar progressBar;
        private PaintDotNet.Controls.SeparatorLine separator;
        private PaintDotNet.Threading.Task task;
        private List<Action> taskEventTickets;

        public TaskProgressDialog()
        {
            base.SuspendLayout();
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = true;
            this.dispatcher = new ControlDispatcher(this);
            this.headerLabel = new Label();
            this.progressBar = new ProgressBar();
            this.separator = new PaintDotNet.Controls.SeparatorLine();
            this.cancelButton = new Button();
            this.headerLabel.Name = "headerLabel";
            this.progressBar.Name = "progressBar";
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.Minimum = 0;
            this.progressBar.Maximum = 100;
            this.separator.Name = "separator";
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.AutoSize = true;
            this.cancelButton.Click += new EventHandler(this.CancelButton_Click);
            this.cancelButton.FlatStyle = FlatStyle.System;
            base.AutoScaleMode = AutoScaleMode.None;
            base.AcceptButton = null;
            base.CancelButton = this.cancelButton;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.Controls.AddRange(new Control[] { this.headerLabel, this.progressBar, this.separator, this.cancelButton });
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (this.Task != null)
            {
                this.Task.RequestCancel();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Task = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnClosed(EventArgs e)
        {
            Taskbar.SetProgress(0L, 0L, TaskbarProgressState.NoProgress);
            Taskbar.SetOverlayIcon(null);
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((this.Task != null) && (this.Task.State != TaskState.Finished))
            {
                this.Task.RequestCancel();
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int x = UI.ScaleWidth(8);
            int y = UI.ScaleHeight(8);
            int num3 = Math.Max(0, y - base.ExtendedFramePadding.Bottom);
            int width = UI.ScaleWidth(300);
            this.headerLabel.Location = new Point(x - 3, y);
            this.headerLabel.Size = this.headerLabel.GetPreferredSize(new Size(width, 1));
            this.progressBar.Bounds = new Rectangle(x + 1, this.headerLabel.Bottom + y, (width - (2 * x)) - 2, UI.ScaleHeight(0x12));
            this.separator.Visible = this.cancelButton.Visible;
            this.separator.Location = new Point(x, (1 + this.progressBar.Bottom) + y);
            if (this.separator.Visible)
            {
                this.separator.Size = this.separator.GetPreferredSize(new Size(width - (2 * x), 1));
            }
            else
            {
                this.separator.Size = new Size(width - x, 0);
            }
            this.cancelButton.Text = PdnResources.GetString2("Form.CancelButton.Text");
            this.cancelButton.Size = UI.ScaleSize(0x4b, 0x17);
            this.cancelButton.PerformLayout();
            int num5 = base.IsGlassEffectivelyEnabled ? -1 : x;
            this.cancelButton.Location = new Point((width - num5) - this.cancelButton.Width, this.separator.Bottom + y);
            if (!this.cancelButton.Visible)
            {
                this.cancelButton.Height = 0;
                this.cancelButton.Location = new Point(x, this.progressBar.Bottom + y);
            }
            int height = this.cancelButton.Bottom + num3;
            base.ClientSize = new Size(width, height);
            if (base.IsGlassEffectivelyEnabled && this.cancelButton.Visible)
            {
                this.separator.Visible = false;
                base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.separator.Top);
            }
            else
            {
                this.separator.Visible = this.separator.Visible;
                base.GlassInset = new Padding(0);
            }
            base.OnLayout(levent);
        }

        protected override void OnShown(EventArgs e)
        {
            Icon icon = base.Icon;
            if (icon != null)
            {
                using (Bitmap bitmap = icon.ToBitmap())
                {
                    Taskbar.SetOverlayIcon(bitmap);
                }
            }
            if (this.Task != null)
            {
                this.Sync(this.Task);
            }
            if (((this.Task != null) && (this.Task.State == TaskState.Finished)) && this.CloseOnFinished)
            {
                base.Close();
            }
            base.OnShown(e);
        }

        public void SetHeaderTextAsync(string newHeaderText)
        {
            Action f = null;
            if (base.InvokeRequired)
            {
                if (f == null)
                {
                    f = (Action) (() => (this.HeaderText = newHeaderText));
                }
                base.Dispatcher.BeginTry(f).Observe();
            }
            else
            {
                this.HeaderText = newHeaderText;
            }
        }

        private void Sync(PaintDotNet.Threading.Task syncToTask)
        {
            double? progress;
            this.VerifyAccess();
            TaskState state = syncToTask.State;
            bool isCancelRequested = syncToTask.IsCancelRequested;
            this.cancelButton.Enabled = !isCancelRequested && (state != TaskState.Finished);
            switch (state)
            {
                case TaskState.Finished:
                    progress = 1.0;
                    break;

                case TaskState.NotYetRunning:
                    progress = null;
                    break;

                default:
                    if (isCancelRequested)
                    {
                        progress = null;
                    }
                    else
                    {
                        if (state != TaskState.Running)
                        {
                            throw new InvalidEnumArgumentException();
                        }
                        progress = syncToTask.Progress;
                    }
                    break;
            }
            if (progress.HasValue)
            {
                double num = this.progressBar.Minimum + (progress.Value * (this.progressBar.Maximum - this.progressBar.Minimum));
                int num2 = (int) num;
                this.progressBar.Value = num2;
                this.progressBar.Style = ProgressBarStyle.Continuous;
                Taskbar.SetProgress((long) num2, 100L, TaskbarProgressState.Normal);
            }
            else
            {
                this.progressBar.Style = ProgressBarStyle.Marquee;
                Taskbar.SetProgress(0L, 0L, TaskbarProgressState.Indeterminate);
            }
            if (((state == TaskState.Finished) && this.closeOnFinished) && (base.IsShown && base.IsHandleCreated))
            {
                base.Close();
            }
            if (((state == TaskState.Finished) && !this.closeOnFinished) && (base.IsShown && base.IsHandleCreated))
            {
                this.cancelButton.Text = PdnResources.GetString2("Form.CloseButton.Text");
            }
        }

        public bool CloseOnFinished
        {
            get => 
                this.closeOnFinished;
            set
            {
                this.closeOnFinished = value;
                if (((value && (this.Task != null)) && ((this.Task.State == TaskState.Finished) && base.IsShown)) && base.IsHandleCreated)
                {
                    base.Close();
                }
            }
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

        public string HeaderText
        {
            get => 
                this.headerLabel.Text;
            set
            {
                if (this.headerLabel.Text != value)
                {
                    this.headerLabel.Text = value;
                    base.PerformLayout();
                }
            }
        }

        public bool ShowCancelButton
        {
            get => 
                this.cancelButton.Visible;
            set
            {
                UI.EnableCloseBox(this, value);
                this.cancelButton.Visible = value;
                base.PerformLayout();
            }
        }

        public PaintDotNet.Threading.Task Task
        {
            get => 
                this.task;
            set
            {
                Action action = null;
                if ((this.task != null) && (this.taskEventTickets != null))
                {
                    this.taskEventTickets.ForEach(f => f());
                    this.taskEventTickets = null;
                }
                if (value != null)
                {
                    this.task = value;
                    List<Action> list = new List<Action>();
                    if (action == null)
                    {
                        action = () => base.Dispatcher.BeginTry(() => this.Sync(this.task)).Observe();
                    }
                    Action syncFn = action;
                    EventHandler cancelRequestedHandler = (s2, e2) => syncFn();
                    EventHandler<NewValueEventArgs<double?>> progressChangedHandler = (s2, e2) => syncFn();
                    EventHandler<NewValueEventArgs<TaskState>> taskStateChangedHandler = (s2, e2) => syncFn();
                    this.task.CancelRequested += cancelRequestedHandler;
                    this.task.ProgressChanged += progressChangedHandler;
                    this.task.StateChanged += taskStateChangedHandler;
                    list.Add(delegate {
                        this.task.CancelRequested -= cancelRequestedHandler;
                    });
                    list.Add(delegate {
                        this.task.ProgressChanged -= progressChangedHandler;
                    });
                    list.Add(delegate {
                        this.task.StateChanged -= taskStateChangedHandler;
                    });
                    this.taskEventTickets = list;
                }
            }
        }
    }
}

