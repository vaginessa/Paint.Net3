namespace PaintDotNet.Dialogs
{
    using Microsoft.Win32;
    using PaintDotNet;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal class FloatingToolForm : PdnBaseForm, ISnapObstacleHost
    {
        private IContainer components;
        private ControlEventHandler controlAddedDelegate;
        private ControlEventHandler controlRemovedDelegate;
        private KeyEventHandler keyUpDelegate;
        private bool moving;
        private System.Drawing.Size movingCursorDelta = System.Drawing.Size.Empty;
        private SnapObstacleController snapObstacle;

        public event CmdKeysEventHandler ProcessCmdKeyEvent;

        public event EventHandler RelinquishFocus;

        public FloatingToolForm()
        {
            base.KeyPreview = true;
            this.controlAddedDelegate = new ControlEventHandler(this.ControlAddedHandler);
            this.controlRemovedDelegate = new ControlEventHandler(this.ControlRemovedHandler);
            this.keyUpDelegate = new KeyEventHandler(this.KeyUpHandler);
            base.ControlAdded += this.controlAddedDelegate;
            base.ControlRemoved += this.controlRemovedDelegate;
            this.InitializeComponent();
            try
            {
                SystemEvents.SessionSwitch += new SessionSwitchEventHandler(this.SystemEvents_SessionSwitch);
                SystemEvents.DisplaySettingsChanged += new EventHandler(this.SystemEvents_DisplaySettingsChanged);
            }
            catch (Exception)
            {
            }
        }

        private void ControlAddedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded += this.controlAddedDelegate;
            e.Control.ControlRemoved += this.controlRemovedDelegate;
            e.Control.KeyUp += this.keyUpDelegate;
        }

        private void ControlRemovedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded -= this.controlAddedDelegate;
            e.Control.ControlRemoved -= this.controlRemovedDelegate;
            e.Control.KeyUp -= this.keyUpDelegate;
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
                try
                {
                    SystemEvents.SessionSwitch -= new SessionSwitchEventHandler(this.SystemEvents_SessionSwitch);
                    SystemEvents.DisplaySettingsChanged -= new EventHandler(this.SystemEvents_DisplaySettingsChanged);
                }
                catch (Exception)
                {
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new System.Drawing.Size(0x124, 0x10f);
            base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "FloatingToolForm";
            base.ShowInTaskbar = false;
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.ForceActiveTitleBar = true;
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnKeyUp(e);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            this.OnRelinquishFocus();
            base.OnClick(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (this.snapObstacle != null)
            {
                this.snapObstacle.Enabled = base.Enabled;
            }
            base.OnEnabledChanged(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (base.IsHandleCreated && (this.snapObstacle != null))
            {
                int snapDistance = this.snapObstacle.SnapDistance;
                int num2 = UI.GetExtendedFramePadding(this).Max();
                int newSnapDistance = 3 + num2;
                if (newSnapDistance != snapDistance)
                {
                    this.snapObstacle.SetSnapDistance(newSnapDistance);
                }
            }
            base.OnLayout(levent);
        }

        protected override void OnLoad(EventArgs e)
        {
            ISnapManagerHost owner = base.Owner as ISnapManagerHost;
            if (owner != null)
            {
                owner.SnapManager.AddSnapObstacle(this);
            }
            base.OnLoad(e);
        }

        protected override void OnMove(EventArgs e)
        {
            this.UpdateSnapObstacleBounds();
            base.OnMove(e);
        }

        protected override void OnMoving(MovingEventArgs mea)
        {
            ISnapManagerHost owner = base.Owner as ISnapManagerHost;
            if (owner != null)
            {
                SnapManager snapManager = owner.SnapManager;
                if (!this.moving)
                {
                    this.movingCursorDelta = new System.Drawing.Size(Cursor.Position.X - mea.Rectangle.X, Cursor.Position.Y - mea.Rectangle.Y);
                    this.moving = true;
                }
                mea.Rectangle = new Rectangle(Cursor.Position.X - this.movingCursorDelta.Width, Cursor.Position.Y - this.movingCursorDelta.Height, mea.Rectangle.Width, mea.Rectangle.Height);
                this.snapObstacle.SetBounds(mea.Rectangle.ToInt32Rect());
                Int32Point location = mea.Rectangle.Location;
                Int32Rect bounds = Int32RectUtil.From(snapManager.AdjustObstacleDestination(this.SnapObstacle, location), mea.Rectangle.Size.ToInt32Size());
                this.snapObstacle.SetBounds(bounds);
                mea.Rectangle = bounds.ToGdipRectangle();
            }
            base.OnMoving(mea);
        }

        protected virtual void OnRelinquishFocus()
        {
            if (!MenuStripEx.IsAnyMenuActive && (this.RelinquishFocus != null))
            {
                this.RelinquishFocus(this, EventArgs.Empty);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            this.UpdateSnapObstacleBounds();
            base.OnResize(e);
            this.UpdateParking();
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            this.UpdateSnapObstacleBounds();
            this.UpdateParking();
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            this.moving = false;
            this.UpdateSnapObstacleBounds();
            this.UpdateParking();
            base.OnResizeEnd(e);
            this.OnRelinquishFocus();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.UpdateSnapObstacleBounds();
            this.UpdateParking();
            base.OnSizeChanged(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (base.Visible)
            {
                base.EnsureFormIsOnScreen();
            }
            base.OnVisibleChanged(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool flag = false;
            if (keyData.IsArrowKey())
            {
                KeyEventArgs e = new KeyEventArgs(keyData);
                if (msg.Msg == 0x100)
                {
                    this.OnKeyDown(e);
                    return e.Handled;
                }
            }
            else if (this.ProcessCmdKeyEvent != null)
            {
                flag = this.ProcessCmdKeyEvent(this, ref msg, keyData);
            }
            if (!flag)
            {
                flag = base.ProcessCmdKey(ref msg, keyData);
            }
            return flag;
        }

        private void SnapObstacle_BoundsChangeRequested(object sender, HandledEventArgs<Int32Rect> e)
        {
            base.Bounds = e.Data.ToGdipRectangle();
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (base.Visible && base.IsShown)
            {
                base.EnsureFormIsOnScreen();
            }
        }

        private void SystemEvents_SessionSwitch(object sender, EventArgs e)
        {
            if (base.Visible && base.IsShown)
            {
                base.EnsureFormIsOnScreen();
            }
        }

        private void UpdateParking()
        {
            if (((base.FormBorderStyle == FormBorderStyle.Fixed3D) || (base.FormBorderStyle == FormBorderStyle.FixedDialog)) || (((base.FormBorderStyle == FormBorderStyle.FixedSingle) || (base.FormBorderStyle == FormBorderStyle.FixedToolWindow)) || (base.FormBorderStyle == FormBorderStyle.SizableToolWindow)))
            {
                ISnapManagerHost owner = base.Owner as ISnapManagerHost;
                if (owner != null)
                {
                    owner.SnapManager.ReparkObstacle(this);
                }
            }
        }

        private void UpdateSnapObstacleBounds()
        {
            if (this.snapObstacle != null)
            {
                this.snapObstacle.SetBounds(this.Bounds());
            }
        }

        public PaintDotNet.SnapObstacle SnapObstacle
        {
            get
            {
                if (this.snapObstacle == null)
                {
                    int num = UI.GetExtendedFramePadding(this).Max();
                    int snapDistance = 3 + num;
                    this.snapObstacle = new SnapObstacleController(base.Name, this.Bounds(), SnapRegion.Exterior, false, 15, snapDistance);
                    this.snapObstacle.BoundsChangeRequested += new HandledEventHandler<Int32Rect>(this.SnapObstacle_BoundsChangeRequested);
                }
                return this.snapObstacle;
            }
        }
    }
}

