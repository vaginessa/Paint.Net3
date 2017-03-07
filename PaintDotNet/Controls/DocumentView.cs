namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Functional;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal class DocumentView : UserControl2
    {
        private ControlShadow controlShadow;
        private PaintDotNet.Document document;
        private PaintDotNet.Controls.DocumentBox documentBox;
        private bool documentInvalid;
        private CanvasGridRenderer gridRenderer;
        private bool hookedMouseEvents;
        private Ruler leftRuler;
        private FormWindowState oldWindowState = FormWindowState.Minimized;
        private PanelEx panel;
        private bool raiseFirstInputAfterGotFocus;
        private int refreshSuspended;
        private bool rulersEnabled = true;
        private PaintDotNet.ScaleFactor scaleFactor = new PaintDotNet.ScaleFactor(1, 1);
        private Ruler topRuler;

        public event EventHandler CompositionUpdated;

        public event EventHandler DocumentChanged;

        public event EventHandler<EventArgs<PaintDotNet.Document>> DocumentChanging;

        public event EventHandler DocumentClick;

        public event KeyEventHandler DocumentKeyDown;

        public event KeyPressEventHandler DocumentKeyPress;

        public event KeyEventHandler DocumentKeyUp;

        public event EventHandler<MouseEventArgsF> DocumentMouseDown;

        public event EventHandler DocumentMouseEnter;

        public event EventHandler DocumentMouseLeave;

        public event EventHandler<MouseEventArgsF> DocumentMouseMove;

        public event EventHandler<MouseEventArgsF> DocumentMouseUp;

        public event EventHandler DrawGridChanged;

        public event EventHandler FirstInputAfterGotFocus;

        public event EventHandler RulersEnabledChanged;

        public event EventHandler ScaleFactorChanged;

        public DocumentView()
        {
            this.InitializeComponent();
            this.document = null;
            this.controlShadow = new ControlShadow();
            this.controlShadow.OccludingControl = this.documentBox;
            this.controlShadow.GdiPaint += new Action<ControlShadow, ISurface<ColorBgra>, Rectangle>(this.ControlShadow_GdiPaint);
            this.panel.Controls.Add(this.controlShadow);
            this.panel.Controls.SetChildIndex(this.controlShadow, this.panel.Controls.Count - 1);
            this.gridRenderer = new CanvasGridRenderer(this.documentBox.CanvasRenderer);
            this.gridRenderer.Visible = false;
            this.documentBox.CanvasRenderer.Add(this.gridRenderer, true);
            this.documentBox.CanvasRenderer.CanvasInvalidated += new EventHandler<RectsEventArgs>(this.Renderers_Invalidated);
            this.documentBox.GdiPaint += new Action(this.DocumentBoxGdiPaint);
        }

        private void CheckForFirstInputAfterGotFocus()
        {
            if (this.raiseFirstInputAfterGotFocus)
            {
                this.raiseFirstInputAfterGotFocus = false;
                this.OnFirstInputAfterGotFocus();
            }
        }

        private void ClickHandler(object sender, EventArgs e)
        {
            System.Drawing.Point autoScrollPosition = this.panel.AutoScrollPosition;
            this.panel.Focus();
            this.OnDocumentClick();
        }

        public System.Windows.Point ClientToDocument(Int32Point clientPt)
        {
            System.Drawing.Point p = base.PointToScreen((System.Drawing.Point) clientPt);
            System.Drawing.Point pt = this.documentBox.PointToClient(p);
            return this.documentBox.ClientToCanvas(((System.Windows.Point) pt.ToInt32Point()));
        }

        public Rect ClientToDocument(Int32Rect clientRect)
        {
            Rectangle r = base.RectangleToScreen(clientRect.ToGdipRectangle());
            Rect rect = this.documentBox.RectangleToClient(r).ToWpfRect();
            return this.documentBox.ClientToCanvas(rect);
        }

        private void ControlShadow_GdiPaint(ControlShadow sender, ISurface<ColorBgra> surface, Rectangle rect)
        {
            IEnumerable<CanvasLayer> canvasLayers = this.documentBox.CanvasRenderer.CanvasLayers;
            Rectangle rectangle = base.RectangleToScreen(this.controlShadow.Bounds);
            Rectangle rectangle2 = base.RectangleToScreen(this.documentBox.Bounds);
            System.Drawing.Point point = new System.Drawing.Point(rectangle2.X - rectangle.X, rectangle2.Y - rectangle.Y);
            System.Drawing.Point pt = new System.Drawing.Point(rect.X - point.X, rect.Y - point.Y);
            foreach (CanvasLayer layer in canvasLayers)
            {
                if (layer.Visible && !layer.ClipsToCanvas)
                {
                    layer.Render(surface, pt.ToInt32Point());
                }
            }
        }

        private void DocumentBoxGdiPaint()
        {
            if (this.documentInvalid)
            {
                this.OnCompositionUpdated();
            }
            this.documentInvalid = false;
        }

        private void DocumentInvalidated(object sender, InvalidateEventArgs e)
        {
            this.documentInvalid = true;
        }

        private void DocumentMetaDataChangedHandler(object sender, EventArgs e)
        {
            if (this.document != null)
            {
                this.leftRuler.Dpu = 1.0 / this.document.PixelToPhysicalY(1.0, this.leftRuler.MeasurementUnit);
                this.topRuler.Dpu = 1.0 / this.document.PixelToPhysicalY(1.0, this.topRuler.MeasurementUnit);
            }
        }

        private void DocumentSetImpl(PaintDotNet.Document value)
        {
            System.Windows.Point documentScrollPosition = this.DocumentScrollPosition;
            this.OnDocumentChanging(value);
            this.SuspendRefresh();
            try
            {
                if (this.document != null)
                {
                    this.document.Metadata.Changed -= new EventHandler(this.DocumentMetaDataChangedHandler);
                    this.document.Invalidated -= new InvalidateEventHandler(this.DocumentInvalidated);
                    this.documentInvalid = true;
                }
                this.document = value;
                if (this.document != null)
                {
                    this.documentBox.Document = this.document;
                    if (this.ScaleFactor != this.documentBox.ScaleFactor)
                    {
                        this.ScaleFactor = this.documentBox.ScaleFactor;
                    }
                    this.document.Metadata.Changed += new EventHandler(this.DocumentMetaDataChangedHandler);
                    this.document.Invalidated += new InvalidateEventHandler(this.DocumentInvalidated);
                    this.documentInvalid = true;
                }
                base.Invalidate(true);
                this.DocumentMetaDataChangedHandler(this, EventArgs.Empty);
                this.OnResize(EventArgs.Empty);
                this.OnDocumentChanged();
            }
            finally
            {
                this.ResumeRefresh();
            }
            this.DocumentScrollPosition = documentScrollPosition;
        }

        public System.Windows.Point DocumentToClient(System.Windows.Point documentPt)
        {
            System.Windows.Point clientPt = this.documentBox.CanvasToClient(documentPt);
            System.Windows.Point screenPt = this.documentBox.PointToScreen(clientPt);
            return this.PointToClient(screenPt);
        }

        public Rect DocumentToClient(Rect documentRect)
        {
            Rect clientRect = this.documentBox.CanvasToClient(documentRect);
            Rect screenRect = this.documentBox.RectangleToScreen(clientRect);
            return this.RectangleToClient(screenRect);
        }

        private void DoLayout()
        {
            if (this.panel.ClientRectangle != new Rectangle(0, 0, 0, 0))
            {
                int x = this.panel.AutoScrollPosition.X;
                int y = this.panel.AutoScrollPosition.Y;
                if (this.panel.ClientRectangle.Width > this.documentBox.Width)
                {
                    x = this.panel.AutoScrollPosition.X + ((this.panel.ClientRectangle.Width - this.documentBox.Width) / 2);
                }
                if (this.panel.ClientRectangle.Height > this.documentBox.Height)
                {
                    y = this.panel.AutoScrollPosition.Y + ((this.panel.ClientRectangle.Height - this.documentBox.Height) / 2);
                }
                System.Drawing.Point point = new System.Drawing.Point(x, y);
                if (this.documentBox.Location != point)
                {
                    this.documentBox.Location = point;
                }
            }
            this.UpdateRulerOffsets();
        }

        public void Focus()
        {
            this.panel.Focus();
        }

        private double GetZoomInFactorEpsilon()
        {
            double ratio = this.ScaleFactor.Ratio;
            double num2 = (ratio + 0.01) / ratio;
            double num3 = ((double) (this.documentBox.Width + 1)) / ((double) this.documentBox.Document.Width);
            double num4 = ((double) (this.documentBox.Height + 1)) / ((double) this.documentBox.Document.Height);
            double num6 = Math.Max(num3, num4) / ratio;
            return Math.Max(num2, num6);
        }

        private double GetZoomOutFactorEpsilon()
        {
            double ratio = this.ScaleFactor.Ratio;
            return ((ratio - 0.01) / ratio);
        }

        protected virtual void HandleMouseWheel(Control sender, MouseEventArgs e)
        {
            double num4;
            double num5;
            double num = ((double) e.Delta) / this.ScaleFactor.Ratio;
            double x = this.DocumentScrollPosition.X;
            double y = this.DocumentScrollPosition.Y;
            if (Control.ModifierKeys == Keys.Shift)
            {
                num4 = this.DocumentScrollPosition.X - num;
                num5 = this.DocumentScrollPosition.Y;
            }
            else if (Control.ModifierKeys == Keys.None)
            {
                num4 = this.DocumentScrollPosition.X;
                num5 = this.DocumentScrollPosition.Y - num;
            }
            else
            {
                num4 = this.DocumentScrollPosition.X;
                num5 = this.DocumentScrollPosition.Y;
            }
            if ((num4 != x) || (num5 != y))
            {
                this.DocumentScrollPosition = new System.Windows.Point(num4, num5);
                this.UpdateRulerOffsets();
            }
        }

        private void HookMouseEvents(Control c)
        {
            c.MouseEnter += new EventHandler(this.MouseEnterHandler);
            c.MouseLeave += new EventHandler(this.MouseLeaveHandler);
            c.MouseUp += new MouseEventHandler(this.MouseUpHandler);
            c.MouseMove += new MouseEventHandler(this.MouseMoveHandler);
            c.MouseDown += new MouseEventHandler(this.MouseDownHandler);
            c.Click += new EventHandler(this.ClickHandler);
            foreach (Control control in c.Controls)
            {
                this.HookMouseEvents(control);
            }
        }

        private void InitializeComponent()
        {
            this.topRuler = new Ruler();
            this.leftRuler = new Ruler();
            this.panel = new PanelEx();
            this.documentBox = new PaintDotNet.Controls.DocumentBox();
            this.panel.SuspendLayout();
            base.SuspendLayout();
            this.topRuler.BackColor = Color.White;
            this.topRuler.Dock = DockStyle.Top;
            this.topRuler.Location = new System.Drawing.Point(0, 0);
            this.topRuler.Name = "topRuler";
            this.topRuler.Offset = -16.0;
            this.topRuler.Size = UI.ScaleSize(new System.Drawing.Size(0x180, 0x10));
            this.topRuler.TabIndex = 3;
            this.leftRuler.BackColor = Color.White;
            this.leftRuler.Dock = DockStyle.Left;
            this.leftRuler.Location = new System.Drawing.Point(0, 0x10);
            this.leftRuler.Name = "leftRuler";
            this.leftRuler.Orientation = Orientation.Vertical;
            this.leftRuler.Size = UI.ScaleSize(new System.Drawing.Size(0x10, 0x130));
            this.leftRuler.TabIndex = 4;
            this.panel.AutoScroll = true;
            this.panel.Controls.Add(this.documentBox);
            this.panel.Dock = DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0x10, 0x10);
            this.panel.Name = "panel";
            this.panel.ScrollPosition = new System.Drawing.Point(0, 0);
            this.panel.Size = new System.Drawing.Size(0x170, 0x130);
            this.panel.TabIndex = 5;
            this.panel.Scroll += new ScrollEventHandler(this.Panel_Scroll);
            this.panel.KeyDown += new KeyEventHandler(this.Panel_KeyDown);
            this.panel.KeyUp += new KeyEventHandler(this.Panel_KeyUp);
            this.panel.KeyPress += new KeyPressEventHandler(this.Panel_KeyPress);
            this.panel.GotFocus += new EventHandler(this.Panel_GotFocus);
            this.panel.LostFocus += new EventHandler(this.Panel_LostFocus);
            this.documentBox.Location = new System.Drawing.Point(0, 0);
            this.documentBox.Name = "documentBox";
            this.documentBox.Document = null;
            this.documentBox.TabIndex = 0;
            base.Controls.Add(this.panel);
            base.Controls.Add(this.leftRuler);
            base.Controls.Add(this.topRuler);
            base.Name = "DocumentView";
            base.Size = new System.Drawing.Size(0x180, 320);
            this.panel.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void InvalidateControlShadow(Int32Rect sbClientRect)
        {
            if (this.document != null)
            {
                Int32Rect maxBounds = CanvasLayer.MaxBounds;
                Int32Size size = this.documentBox.Size();
                Int32Rect rect2 = Int32RectUtil.FromEdges(maxBounds.X, 0, 0, size.Height);
                Int32Rect rect3 = Int32RectUtil.FromEdges(maxBounds.X, maxBounds.Y, maxBounds.Right(), 0);
                Int32Rect rect4 = Int32RectUtil.FromEdges(size.Width, 0, maxBounds.Right(), size.Height);
                Int32Rect rect5 = Int32RectUtil.FromEdges(maxBounds.X, size.Height, maxBounds.Right(), maxBounds.Bottom());
                rect2 = Int32RectUtil.Intersect(rect2, sbClientRect);
                rect3 = Int32RectUtil.Intersect(rect3, sbClientRect);
                rect4 = Int32RectUtil.Intersect(rect4, sbClientRect);
                rect5 = Int32RectUtil.Intersect(rect5, sbClientRect);
                this.InvalidateControlShadowNoClipping(rect2);
                this.InvalidateControlShadowNoClipping(rect3);
                this.InvalidateControlShadowNoClipping(rect4);
                this.InvalidateControlShadowNoClipping(rect5);
            }
        }

        private void InvalidateControlShadowNoClipping(Int32Rect sbClientRect)
        {
            if ((sbClientRect.Width > 0) && (sbClientRect.Height > 0))
            {
                Int32Rect rect = this.SurfaceBoxToControlShadow(sbClientRect);
                this.controlShadow.Invalidate(rect);
            }
        }

        public void InvalidateSurface(Int32Rect docRect)
        {
            Int32Rect rect = this.documentBox.CanvasToClient(docRect.ToRect()).Int32Bound();
            this.documentBox.Invalidate(rect);
            this.InvalidateControlShadow(rect);
        }

        public override bool IsMouseCaptured()
        {
            if (((!base.Capture && !this.panel.Capture) && (!this.documentBox.Capture && !this.controlShadow.Capture)) && !this.leftRuler.Capture)
            {
                return this.topRuler.Capture;
            }
            return true;
        }

        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is Ruler))
            {
                System.Windows.Point point = this.MouseToDocument((Control) sender, new Int32Point(e.X, e.Y));
                System.Drawing.Point autoScrollPosition = this.panel.AutoScrollPosition;
                this.panel.Focus();
                this.OnDocumentMouseDown(new MouseEventArgsF(e.Button, e.Clicks, point.X, point.Y, e.Delta));
            }
        }

        private void MouseEnterHandler(object sender, EventArgs e)
        {
            this.OnDocumentMouseEnter(EventArgs.Empty);
        }

        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            this.OnDocumentMouseLeave(EventArgs.Empty);
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            System.Windows.Point point = this.MouseToDocument((Control) sender, new Int32Point(e.X, e.Y));
            if (this.RulersEnabled)
            {
                int num;
                int num2;
                if (point.X > 0.0)
                {
                    num = (int) Math.Truncate(point.X);
                }
                else if (point.X < 0.0)
                {
                    num = (int) Math.Truncate((double) (point.X - 1.0));
                }
                else
                {
                    num = 0;
                }
                if (point.Y > 0.0)
                {
                    num2 = (int) Math.Truncate(point.Y);
                }
                else if (point.Y < 0.0)
                {
                    num2 = (int) Math.Truncate((double) (point.Y - 1.0));
                }
                else
                {
                    num2 = 0;
                }
                this.topRuler.Value = num;
                this.leftRuler.Value = num2;
                this.UpdateRulerOffsets();
            }
            this.OnDocumentMouseMove(new MouseEventArgsF(e.Button, e.Clicks, point.X, point.Y, e.Delta));
        }

        public System.Windows.Point MouseToDocument(Control sender, Int32Point mouse)
        {
            System.Drawing.Point p = sender.PointToScreen(mouse.ToGdipPoint());
            System.Drawing.Point point2 = this.documentBox.PointToClient(p);
            return this.documentBox.ClientToCanvas(new System.Windows.Point((double) point2.X, (double) point2.Y));
        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is Ruler))
            {
                System.Windows.Point point = this.MouseToDocument((Control) sender, new Int32Point(e.X, e.Y));
                System.Drawing.Point autoScrollPosition = this.panel.AutoScrollPosition;
                this.panel.Focus();
                this.OnDocumentMouseUp(new MouseEventArgsF(e.Button, e.Clicks, point.X, point.Y, e.Delta));
            }
        }

        private void OnCompositionUpdated()
        {
            if (this.CompositionUpdated != null)
            {
                this.CompositionUpdated(this, EventArgs.Empty);
            }
        }

        protected virtual void OnDocumentChanged()
        {
            if (this.DocumentChanged != null)
            {
                this.DocumentChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnDocumentChanging(PaintDotNet.Document newDocument)
        {
            if (this.DocumentChanging != null)
            {
                this.DocumentChanging(this, new EventArgs<PaintDotNet.Document>(newDocument));
            }
        }

        protected void OnDocumentClick()
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentClick != null)
            {
                this.DocumentClick(this, EventArgs.Empty);
            }
        }

        protected void OnDocumentKeyDown(KeyEventArgs e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentKeyDown != null)
            {
                this.DocumentKeyDown(this, e);
            }
        }

        protected void OnDocumentKeyPress(KeyPressEventArgs e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentKeyPress != null)
            {
                this.DocumentKeyPress(this, e);
            }
        }

        protected void OnDocumentKeyUp(KeyEventArgs e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentKeyUp != null)
            {
                this.DocumentKeyUp(this, e);
            }
        }

        protected virtual void OnDocumentMouseDown(MouseEventArgsF e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentMouseDown != null)
            {
                this.DocumentMouseDown(this, e);
            }
        }

        protected virtual void OnDocumentMouseEnter(EventArgs e)
        {
            if (this.DocumentMouseEnter != null)
            {
                this.DocumentMouseEnter(this, e);
            }
        }

        protected virtual void OnDocumentMouseLeave(EventArgs e)
        {
            if (this.DocumentMouseLeave != null)
            {
                this.DocumentMouseLeave(this, e);
            }
        }

        protected virtual void OnDocumentMouseMove(MouseEventArgsF e)
        {
            if (this.DocumentMouseMove != null)
            {
                this.DocumentMouseMove(this, e);
            }
        }

        protected virtual void OnDocumentMouseUp(MouseEventArgsF e)
        {
            this.CheckForFirstInputAfterGotFocus();
            if (this.DocumentMouseUp != null)
            {
                this.DocumentMouseUp(this, e);
            }
        }

        protected virtual void OnDrawGridChanged()
        {
            if (this.DrawGridChanged != null)
            {
                this.DrawGridChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnFirstInputAfterGotFocus()
        {
            if (this.FirstInputAfterGotFocus != null)
            {
                this.FirstInputAfterGotFocus(this, EventArgs.Empty);
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            this.DoLayout();
            base.OnLayout(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!this.hookedMouseEvents)
            {
                this.hookedMouseEvents = true;
                foreach (Control control in base.Controls)
                {
                    this.HookMouseEvents(control);
                }
            }
            this.panel.Select();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.HandleMouseWheel(this, e);
            base.OnMouseWheel(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Form parentForm = base.ParentForm;
            if (parentForm != null)
            {
                if (parentForm.WindowState != this.oldWindowState)
                {
                    base.PerformLayout();
                }
                this.oldWindowState = parentForm.WindowState;
            }
            base.OnResize(e);
            this.DoLayout();
        }

        protected void OnRulersEnabledChanged()
        {
            if (this.RulersEnabledChanged != null)
            {
                this.RulersEnabledChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnScaleFactorChanged()
        {
            if (this.ScaleFactorChanged != null)
            {
                this.ScaleFactorChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnUnitsChanged()
        {
        }

        protected virtual void OnUnitsChanging()
        {
        }

        private void Panel_GotFocus(object sender, EventArgs e)
        {
            this.raiseFirstInputAfterGotFocus = true;
        }

        private void Panel_KeyDown(object sender, KeyEventArgs e)
        {
            this.CheckForFirstInputAfterGotFocus();
            this.OnDocumentKeyDown(e);
            if (!e.Handled)
            {
                System.Windows.Point documentScrollPosition = this.DocumentScrollPosition;
                System.Windows.Point point2 = documentScrollPosition;
                Rect visibleDocumentRect = this.VisibleDocumentRect;
                switch (e.KeyData)
                {
                    case Keys.PageUp:
                        point2.Y -= visibleDocumentRect.Height;
                        break;

                    case Keys.Next:
                        point2.Y += visibleDocumentRect.Height;
                        break;

                    case Keys.End:
                        if (visibleDocumentRect.Right >= (this.document.Width - 1))
                        {
                            point2.Y = this.document.Height;
                            break;
                        }
                        point2.X = this.document.Width;
                        break;

                    case Keys.Home:
                        if (documentScrollPosition.X != 0.0)
                        {
                            point2.X = 0.0;
                            break;
                        }
                        point2.Y = 0.0;
                        break;

                    case (Keys.Shift | Keys.PageUp):
                        point2.X -= visibleDocumentRect.Width;
                        break;

                    case (Keys.Shift | Keys.Next):
                        point2.X += visibleDocumentRect.Width;
                        break;
                }
                if (point2 != documentScrollPosition)
                {
                    this.DocumentScrollPosition = point2;
                    e.Handled = true;
                }
            }
        }

        private void Panel_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.OnDocumentKeyPress(e);
        }

        private void Panel_KeyUp(object sender, KeyEventArgs e)
        {
            this.OnDocumentKeyUp(e);
        }

        private void Panel_LostFocus(object sender, EventArgs e)
        {
            this.raiseFirstInputAfterGotFocus = false;
        }

        private void Panel_Scroll(object sender, ScrollEventArgs e)
        {
            this.OnScroll(e);
            this.UpdateRulerOffsets();
        }

        public void PerformDocumentMouseDown(MouseEventArgsF e)
        {
            this.OnDocumentMouseDown(e);
        }

        public void PerformDocumentMouseMove(MouseEventArgsF e)
        {
            this.OnDocumentMouseMove(e);
        }

        public void PerformDocumentMouseUp(MouseEventArgsF e)
        {
            this.OnDocumentMouseUp(e);
        }

        public void PerformMouseWheel(Control sender, MouseEventArgs e)
        {
            this.HandleMouseWheel(sender, e);
        }

        public void PopCacheStandby()
        {
            this.DocumentBox.PopCacheStandby();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys keys = keyData & Keys.KeyCode;
            if ((keyData.IsArrowKey() || (keys == Keys.Delete)) || (keys == Keys.Tab))
            {
                KeyEventArgs e = new KeyEventArgs(keyData);
                if (msg.Msg == 0x100)
                {
                    if (base.ContainsFocus)
                    {
                        this.OnDocumentKeyDown(e);
                        if (keyData.IsArrowKey())
                        {
                            e.Handled = true;
                        }
                    }
                    if (e.Handled)
                    {
                        return true;
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void PushCacheStandby()
        {
            this.DocumentBox.PushCacheStandby();
        }

        public void RecenterView(System.Windows.Point newCenter)
        {
            Rect visibleDocumentRect = this.VisibleDocumentRect;
            System.Windows.Point point = new System.Windows.Point(newCenter.X - (visibleDocumentRect.Width / 2.0), newCenter.Y - (visibleDocumentRect.Height / 2.0));
            this.DocumentScrollPosition = point;
        }

        private void Renderers_Invalidated(object sender, RectsEventArgs e)
        {
            if (this.document != null)
            {
                Rect[] rects = e.Rects;
                for (int i = 0; i < rects.Length; i++)
                {
                    Int32Rect sbClientRect = this.documentBox.CanvasRenderer.CanvasToRenderDst(rects[i]).Int32Bound();
                    this.InvalidateControlShadow(sbClientRect);
                }
            }
        }

        public void ResumeRefresh()
        {
            this.refreshSuspended--;
            this.documentBox.Visible = this.controlShadow.Visible = this.refreshSuspended <= 0;
        }

        public System.Windows.Point ScreenToDocument(System.Windows.Point screen)
        {
            System.Drawing.Point point = this.documentBox.PointToClient(new System.Drawing.Point(0, 0));
            System.Windows.Point clientPt = new System.Windows.Point(screen.X + point.X, screen.Y + point.Y);
            return this.documentBox.ClientToCanvas(clientPt);
        }

        public void SetHighlightRectangle(Rect rectF)
        {
            if ((rectF.Width == 0.0) || (rectF.Height == 0.0))
            {
                this.leftRuler.HighlightEnabled = false;
                this.topRuler.HighlightEnabled = false;
            }
            else
            {
                if (this.topRuler != null)
                {
                    this.topRuler.HighlightEnabled = true;
                    this.topRuler.HighlightStart = rectF.Left;
                    this.topRuler.HighlightLength = rectF.Width;
                }
                if (this.leftRuler != null)
                {
                    this.leftRuler.HighlightEnabled = true;
                    this.leftRuler.HighlightStart = rectF.Top;
                    this.leftRuler.HighlightLength = rectF.Height;
                }
            }
        }

        private Int32Rect SurfaceBoxToControlShadow(Int32Rect rect)
        {
            Int32Rect screenRect = this.documentBox.RectangleToScreen(rect);
            return this.controlShadow.RectangleToClient(screenRect);
        }

        public void SuspendRefresh()
        {
            this.refreshSuspended++;
            this.documentBox.Visible = this.controlShadow.Visible = this.refreshSuspended <= 0;
        }

        private void UpdateRulerOffsets()
        {
            this.topRuler.Offset = this.ScaleFactor.Unscale((double) (UI.ScaleWidth((float) -16f) - this.documentBox.Location.X));
            if (this.topRuler.Visible)
            {
                this.topRuler.Update();
            }
            this.leftRuler.Offset = this.ScaleFactor.Unscale((double) (0f - this.documentBox.Location.Y));
            if (this.leftRuler.Visible)
            {
                this.leftRuler.Update();
            }
        }

        public virtual void ZoomIn()
        {
            new Action(this.ZoomInImpl).Try().Observe();
        }

        public virtual void ZoomIn(double factor)
        {
            new Action<double>(this.ZoomInImpl).Try<double>(factor).Observe();
        }

        private void ZoomInImpl()
        {
            System.Windows.Point documentCenterPoint = this.DocumentCenterPoint;
            PaintDotNet.ScaleFactor scaleFactor = this.ScaleFactor;
            PaintDotNet.ScaleFactor nextLarger = this.ScaleFactor;
            int length = PaintDotNet.ScaleFactor.PresetValues.Length;
            do
            {
                nextLarger = nextLarger.GetNextLarger();
                this.ScaleFactor = nextLarger;
                length--;
            }
            while ((this.ScaleFactor == scaleFactor) && (length > 0));
            this.DocumentCenterPoint = documentCenterPoint;
        }

        private void ZoomInImpl(double factor)
        {
            System.Windows.Point documentCenterPoint = this.DocumentCenterPoint;
            PaintDotNet.ScaleFactor scaleFactor = this.ScaleFactor;
            PaintDotNet.ScaleFactor factor3 = this.ScaleFactor;
            int num = 3;
            double zoomInFactorEpsilon = this.GetZoomInFactorEpsilon();
            double num4 = Math.Max(factor, zoomInFactorEpsilon);
            do
            {
                factor3 = PaintDotNet.ScaleFactor.FromDouble(factor3.Ratio * num4);
                this.ScaleFactor = factor3;
                num--;
                num4 *= 1.1;
            }
            while ((this.ScaleFactor == scaleFactor) && (num > 0));
            this.DocumentCenterPoint = documentCenterPoint;
        }

        public virtual void ZoomOut()
        {
            new Action(this.ZoomOutImpl).Try().Observe();
        }

        public virtual void ZoomOut(double factor)
        {
            new Action<double>(this.ZoomOutImpl).Try<double>(factor).Observe();
        }

        private void ZoomOutImpl()
        {
            System.Windows.Point documentCenterPoint = this.DocumentCenterPoint;
            PaintDotNet.ScaleFactor scaleFactor = this.ScaleFactor;
            PaintDotNet.ScaleFactor nextSmaller = this.ScaleFactor;
            int length = PaintDotNet.ScaleFactor.PresetValues.Length;
            do
            {
                nextSmaller = nextSmaller.GetNextSmaller();
                this.ScaleFactor = nextSmaller;
                length--;
            }
            while ((this.ScaleFactor == scaleFactor) && (length > 0));
            this.DocumentCenterPoint = documentCenterPoint;
        }

        private void ZoomOutImpl(double factor)
        {
            System.Windows.Point documentCenterPoint = this.DocumentCenterPoint;
            PaintDotNet.ScaleFactor scaleFactor = this.ScaleFactor;
            PaintDotNet.ScaleFactor factor3 = this.ScaleFactor;
            int num = 3;
            double zoomOutFactorEpsilon = this.GetZoomOutFactorEpsilon();
            double num3 = 1.0 / factor;
            double num5 = Math.Min(num3, zoomOutFactorEpsilon);
            do
            {
                factor3 = PaintDotNet.ScaleFactor.FromDouble(factor3.Ratio * num5);
                this.ScaleFactor = factor3;
                num--;
                num5 *= 0.9;
            }
            while ((this.ScaleFactor == scaleFactor) && (num > 0));
            this.DocumentCenterPoint = documentCenterPoint;
        }

        public void ZoomToWindow()
        {
            if (this.document != null)
            {
                Int32Rect clientRectangleMax = this.ClientRectangleMax;
                PaintDotNet.ScaleFactor factor2 = PaintDotNet.ScaleFactor.Min(PaintDotNet.ScaleFactor.Min(clientRectangleMax.Width - 10, this.document.Width, clientRectangleMax.Height - 10, this.document.Height, PaintDotNet.ScaleFactor.MinValue), PaintDotNet.ScaleFactor.OneToOne);
                this.ScaleFactor = factor2;
            }
        }

        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get => 
                this.panel.BorderStyle;
            set
            {
                this.panel.BorderStyle = value;
            }
        }

        public PaintDotNet.Canvas.CanvasRenderer CanvasRenderer =>
            this.documentBox.CanvasRenderer;

        public Int32Rect ClientRectangleMax =>
            base.RectangleToClient(this.panel.RectangleToScreen(this.panel.Bounds)).ToInt32Rect();

        public Int32Rect ClientRectangleMin
        {
            get
            {
                Int32Rect clientRectangleMax = this.ClientRectangleMax;
                clientRectangleMax.Width -= SystemInformation.VerticalScrollBarWidth;
                clientRectangleMax.Height -= SystemInformation.HorizontalScrollBarHeight;
                return clientRectangleMax;
            }
        }

        [Browsable(false)]
        public PaintDotNet.Document Document
        {
            get => 
                this.document;
            set
            {
                if (base.InvokeRequired)
                {
                    Tracing.Log(TraceType.Warning, "DocumentView.Document was set from the wrong thread", new StackTrace(true));
                    base.Invoke(new Action<PaintDotNet.Document>(this.DocumentSetImpl), new object[] { value });
                }
                else
                {
                    this.DocumentSetImpl(value);
                }
            }
        }

        protected PaintDotNet.Controls.DocumentBox DocumentBox =>
            this.documentBox;

        [Browsable(false)]
        public System.Windows.Point DocumentCenterPoint
        {
            get
            {
                Rect visibleDocumentRect = this.VisibleDocumentRect;
                return new System.Windows.Point((visibleDocumentRect.Left + visibleDocumentRect.Right) / 2.0, (visibleDocumentRect.Top + visibleDocumentRect.Bottom) / 2.0);
            }
            set
            {
                Rect visibleDocumentRect = this.VisibleDocumentRect;
                System.Windows.Point point = new System.Windows.Point(value.X - (visibleDocumentRect.Width / 2.0), value.Y - (visibleDocumentRect.Height / 2.0));
                this.DocumentScrollPosition = point;
            }
        }

        [Browsable(false)]
        public System.Windows.Point DocumentScrollPosition
        {
            get
            {
                if ((this.panel != null) && (this.documentBox != null))
                {
                    return this.VisibleDocumentRect.Location;
                }
                return new System.Windows.Point(0.0, 0.0);
            }
            set
            {
                if (this.panel != null)
                {
                    Int32Point point2 = Int32Point.Round(this.documentBox.CanvasToClient(value));
                    if (this.panel.AutoScrollPosition != new System.Drawing.Point(-point2.X, -point2.Y))
                    {
                        this.panel.AutoScrollPosition = (System.Drawing.Point) point2;
                        this.UpdateRulerOffsets();
                    }
                }
            }
        }

        public bool DrawGrid
        {
            get => 
                this.gridRenderer.Visible;
            set
            {
                if (this.gridRenderer.Visible != value)
                {
                    this.gridRenderer.Visible = value;
                    this.OnDrawGridChanged();
                }
            }
        }

        [Browsable(false)]
        public override bool Focused
        {
            get
            {
                if (((!base.Focused && !this.panel.Focused) && (!this.documentBox.Focused && !this.controlShadow.Focused)) && !this.leftRuler.Focused)
                {
                    return this.topRuler.Focused;
                }
                return true;
            }
        }

        public PaintDotNet.ScaleFactor MaxScaleFactor
        {
            get
            {
                if ((this.document.Width == 0) || (this.document.Height == 0))
                {
                    return PaintDotNet.ScaleFactor.MaxValue;
                }
                double num = 32767.0 / ((double) this.document.Width);
                double num2 = 32767.0 / ((double) this.document.Height);
                return PaintDotNet.ScaleFactor.FromDouble(Math.Min(num, num2));
            }
        }

        public bool PanelAutoScroll
        {
            get => 
                this.panel.AutoScroll;
            set
            {
                if (this.panel.AutoScroll != value)
                {
                    this.panel.AutoScroll = value;
                }
            }
        }

        public bool RulersEnabled
        {
            get => 
                this.rulersEnabled;
            set
            {
                if (this.rulersEnabled != value)
                {
                    this.rulersEnabled = value;
                    if (this.topRuler != null)
                    {
                        this.topRuler.Enabled = value;
                        this.topRuler.Visible = value;
                    }
                    if (this.leftRuler != null)
                    {
                        this.leftRuler.Enabled = value;
                        this.leftRuler.Visible = value;
                    }
                    base.PerformLayout();
                    this.OnRulersEnabledChanged();
                }
            }
        }

        [Browsable(false)]
        public PaintDotNet.ScaleFactor ScaleFactor
        {
            get => 
                this.scaleFactor;
            set
            {
                UI.SuspendControlPainting(this);
                PaintDotNet.ScaleFactor factor = PaintDotNet.ScaleFactor.Min(value, this.MaxScaleFactor);
                if ((factor != this.scaleFactor) || (this.scaleFactor != PaintDotNet.ScaleFactor.OneToOne))
                {
                    Rect visibleDocumentRect = this.VisibleDocumentRect;
                    this.scaleFactor = factor;
                    System.Windows.Point newCenter = new System.Windows.Point(visibleDocumentRect.X + (visibleDocumentRect.Width / 2.0), visibleDocumentRect.Y + (visibleDocumentRect.Height / 2.0));
                    if (this.documentBox != null)
                    {
                        this.documentBox.Size = Int32Size.Truncate(this.scaleFactor.Scale(this.document.Size())).ToGdipSize();
                        this.scaleFactor = this.documentBox.ScaleFactor;
                        if (this.leftRuler != null)
                        {
                            this.leftRuler.ScaleFactor = this.scaleFactor;
                        }
                        if (this.topRuler != null)
                        {
                            this.topRuler.ScaleFactor = this.scaleFactor;
                        }
                    }
                    Rect rect1 = this.VisibleDocumentRect;
                    this.RecenterView(newCenter);
                }
                this.OnResize(EventArgs.Empty);
                this.OnScaleFactorChanged();
                UI.ResumeControlPainting(this);
                base.Invalidate(true);
            }
        }

        public bool ScrollBarsVisible
        {
            get
            {
                if (!base.HScroll)
                {
                    return base.VScroll;
                }
                return true;
            }
        }

        public MeasurementUnit Units
        {
            get => 
                this.leftRuler.MeasurementUnit;
            set
            {
                this.OnUnitsChanging();
                this.leftRuler.MeasurementUnit = value;
                this.topRuler.MeasurementUnit = value;
                this.DocumentMetaDataChangedHandler(this, EventArgs.Empty);
                this.OnUnitsChanged();
            }
        }

        [Browsable(false)]
        public Rect VisibleDocumentBounds
        {
            get
            {
                Rect clientRect = this.DocumentToClient(this.VisibleDocumentRect);
                return this.RectangleToScreen(clientRect);
            }
        }

        [Browsable(false)]
        public Rect VisibleDocumentRect
        {
            get
            {
                Rectangle a = this.panel.RectangleToScreen(this.panel.ClientRectangle);
                Rectangle b = this.documentBox.RectangleToScreen(this.documentBox.ClientRectangle);
                Rectangle r = Rectangle.Intersect(a, b);
                Rectangle rect = base.RectangleToClient(r);
                return this.ClientToDocument(rect.ToInt32Rect());
            }
        }

        public Int32Rect VisibleViewRect
        {
            get
            {
                Rectangle clientRectangle = this.panel.ClientRectangle;
                Rectangle r = this.panel.RectangleToScreen(clientRectangle);
                return base.RectangleToClient(r).ToInt32Rect();
            }
        }
    }
}

