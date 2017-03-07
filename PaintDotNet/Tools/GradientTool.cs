namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Drawing.Drawing2D;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class GradientTool : PaintDotNet.Tools.Tool
    {
        private bool controlKeyDown;
        private readonly TimeSpan controlKeyDownThreshold;
        private DateTime controlKeyDownTime;
        private MoveNubRenderer endNub;
        private Point endPoint;
        private bool gradientActive;
        private string helpTextAdjustable;
        private string helpTextInitial;
        private string helpTextWhileAdjustingFormat;
        private CompoundHistoryMemento historyMemento;
        private MouseButtons mouseButton;
        private MoveNubRenderer mouseNub;
        private MoveNubRenderer[] moveNubs;
        private bool shouldConstrain;
        private bool shouldMoveBothNubs;
        private bool shouldSwapColors;
        private MoveNubRenderer startNub;
        private Point startPoint;
        private PrivateThreadPool threadPool;
        private Cursor toolCursor;
        private ImageResource toolIcon;
        private Cursor toolMouseDownCursor;

        public GradientTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, StaticImage, StaticName, PdnResources.GetString2("GradientTool.HelpText"), 'g', false, ToolBarConfigItems.AlphaBlending | ToolBarConfigItems.Gradient)
        {
            this.helpTextInitial = PdnResources.GetString2("GradientTool.HelpText");
            this.helpTextWhileAdjustingFormat = PdnResources.GetString2("GradientTool.HelpText.WhileAdjusting.Format");
            this.helpTextAdjustable = PdnResources.GetString2("GradientTool.HelpText.Adjustable");
            this.controlKeyDownTime = DateTime.MinValue;
            this.controlKeyDownThreshold = new TimeSpan(0, 0, 0, 0, 400);
        }

        private void CommitGradient()
        {
            if (!this.gradientActive)
            {
                throw new InvalidOperationException("CommitGradient() called when a gradient was not active");
            }
            this.RenderGradient();
            using (GeometryList list = base.DocumentWorkspace.Selection.CreateGeometryListClippingMask())
            {
                BitmapHistoryMemento newHA = new BitmapHistoryMemento(StaticName, StaticImage, base.DocumentWorkspace, base.DocumentWorkspace.ActiveLayerIndex, list, base.ScratchSurface);
                this.historyMemento.PushNewAction(newHA);
                this.historyMemento = null;
            }
            this.startNub.Visible = false;
            this.endNub.Visible = false;
            base.ClearSavedRegion();
            base.ClearSavedMemory();
            this.gradientActive = false;
            base.SetStatus(this.toolIcon, this.helpTextInitial);
        }

        private Point ConstrainPoints(Point a, Point b)
        {
            Vector vector = (Vector) (b - a);
            double d = Math.Atan2(vector.Y, vector.X);
            double num2 = Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
            d = (Math.Round((double) ((12.0 * d) / 3.1415926535897931)) * 3.1415926535897931) / 12.0;
            return new Point(a.X + (num2 * Math.Cos(d)), a.Y + (num2 * Math.Sin(d)));
        }

        protected override void OnActivate()
        {
            this.threadPool = new PrivateThreadPool(Processor.LogicalCpuCount, true);
            this.toolCursor = PdnResources.GetCursor2("Cursors.GenericToolCursor.cur");
            this.toolMouseDownCursor = PdnResources.GetCursor2("Cursors.GenericToolCursorMouseDown.cur");
            base.Cursor = this.toolCursor;
            this.toolIcon = base.Image;
            this.startNub = new MoveNubRenderer(base.CanvasRenderer);
            this.startNub.Visible = false;
            this.startNub.Shape = MoveNubShape.Circle;
            base.CanvasRenderer.Add(this.startNub, false);
            this.endNub = new MoveNubRenderer(base.CanvasRenderer);
            this.endNub.Visible = false;
            this.endNub.Shape = MoveNubShape.Circle;
            base.CanvasRenderer.Add(this.endNub, false);
            this.moveNubs = new MoveNubRenderer[] { this.startNub, this.endNub };
            base.AppEnvironment.PrimaryColorChanged += new EventHandler(this.RenderBecauseOfEvent);
            base.AppEnvironment.SecondaryColorChanged += new EventHandler(this.RenderBecauseOfEvent);
            base.AppEnvironment.GradientInfoChanged += new EventHandler(this.RenderBecauseOfEvent);
            base.AppEnvironment.AlphaBlendingChanged += new EventHandler(this.RenderBecauseOfEvent);
            base.AppWorkspace.UnitsChanged += new EventHandler(this.RenderBecauseOfEvent);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.AppEnvironment.PrimaryColorChanged -= new EventHandler(this.RenderBecauseOfEvent);
            base.AppEnvironment.SecondaryColorChanged -= new EventHandler(this.RenderBecauseOfEvent);
            base.AppEnvironment.GradientInfoChanged -= new EventHandler(this.RenderBecauseOfEvent);
            base.AppEnvironment.AlphaBlendingChanged -= new EventHandler(this.RenderBecauseOfEvent);
            base.AppWorkspace.UnitsChanged -= new EventHandler(this.RenderBecauseOfEvent);
            if (this.gradientActive)
            {
                this.CommitGradient();
                this.mouseButton = MouseButtons.None;
            }
            if (this.startNub != null)
            {
                base.CanvasRenderer.Remove(this.startNub);
                this.startNub.Dispose();
                this.startNub = null;
            }
            if (this.endNub != null)
            {
                base.CanvasRenderer.Remove(this.endNub);
                this.endNub.Dispose();
                this.endNub = null;
            }
            this.moveNubs = null;
            if (this.toolCursor != null)
            {
                this.toolCursor.Dispose();
                this.toolCursor = null;
            }
            if (this.toolMouseDownCursor != null)
            {
                this.toolMouseDownCursor.Dispose();
                this.toolMouseDownCursor = null;
            }
            if (this.threadPool != null)
            {
                this.threadPool.Dispose();
                this.threadPool = null;
            }
            base.OnDeactivate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                {
                    bool shouldConstrain = this.shouldConstrain;
                    this.shouldConstrain = true;
                    if ((this.gradientActive && (this.mouseButton != MouseButtons.None)) && !shouldConstrain)
                    {
                        this.RenderGradient();
                    }
                    break;
                }
                case Keys.ControlKey:
                    if (!this.controlKeyDown)
                    {
                        this.controlKeyDown = true;
                        this.controlKeyDownTime = DateTime.Now;
                    }
                    break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (this.gradientActive)
            {
                switch (e.KeyChar)
                {
                    case '\r':
                        e.Handled = true;
                        this.CommitGradient();
                        break;

                    case '\x001b':
                        e.Handled = true;
                        this.CommitGradient();
                        base.HistoryStack.StepBackward(base.AppWorkspace);
                        break;
                }
            }
            base.OnKeyPress(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                    this.shouldConstrain = false;
                    if (this.gradientActive && (this.mouseButton != MouseButtons.None))
                    {
                        this.RenderGradient();
                    }
                    goto Label_009D;

                case Keys.ControlKey:
                {
                    TimeSpan span = (TimeSpan) (DateTime.Now - this.controlKeyDownTime);
                    if (span < this.controlKeyDownThreshold)
                    {
                        for (int i = 0; i < this.moveNubs.Length; i++)
                        {
                            this.moveNubs[i].Visible = this.gradientActive && !this.moveNubs[i].Visible;
                        }
                    }
                    break;
                }
                default:
                    goto Label_009D;
            }
            this.controlKeyDown = false;
        Label_009D:
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            Point mousePtF = new Point((double) e.X, (double) e.Y);
            MoveNubRenderer renderer = this.PointToNub(mousePtF);
            if (this.mouseButton != MouseButtons.None)
            {
                this.shouldMoveBothNubs = !this.shouldMoveBothNubs;
            }
            else
            {
                bool flag = true;
                this.mouseButton = e.Button;
                if (!this.gradientActive)
                {
                    this.shouldSwapColors = this.mouseButton == MouseButtons.Right;
                }
                else
                {
                    this.shouldMoveBothNubs = false;
                    if (renderer == null)
                    {
                        this.CommitGradient();
                        flag = true;
                        this.shouldSwapColors = this.mouseButton == MouseButtons.Right;
                    }
                    else
                    {
                        base.Cursor = base.handCursorMouseDown;
                        this.mouseNub = renderer;
                        this.mouseNub.Location = mousePtF;
                        if (this.mouseNub == this.startNub)
                        {
                            this.startPoint = mousePtF;
                        }
                        else
                        {
                            this.endPoint = mousePtF;
                        }
                        if (this.mouseButton == MouseButtons.Right)
                        {
                            this.shouldSwapColors = !this.shouldSwapColors;
                        }
                        this.RenderGradient();
                        flag = false;
                    }
                }
                if (flag)
                {
                    this.startPoint = mousePtF;
                    this.startNub.Location = mousePtF;
                    this.startNub.Visible = true;
                    this.endNub.Location = mousePtF;
                    this.endNub.Visible = true;
                    this.endPoint = mousePtF;
                    this.mouseNub = renderer;
                    base.Cursor = this.toolMouseDownCursor;
                    this.gradientActive = true;
                    base.ClearSavedRegion();
                    this.RenderGradient();
                    this.historyMemento = new CompoundHistoryMemento(StaticName, StaticImage);
                    base.HistoryStack.PushNewMemento(this.historyMemento);
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            Point mousePtF = new Point((double) e.X, (double) e.Y);
            MoveNubRenderer renderer = this.PointToNub(mousePtF);
            if (this.mouseButton == MouseButtons.None)
            {
                this.mouseNub = renderer;
                if ((this.mouseNub == this.startNub) || (this.mouseNub == this.endNub))
                {
                    base.Cursor = base.handCursor;
                }
                else
                {
                    base.Cursor = this.toolCursor;
                }
            }
            else
            {
                if (this.mouseNub == this.startNub)
                {
                    if (this.shouldConstrain && !this.shouldMoveBothNubs)
                    {
                        mousePtF = this.ConstrainPoints(this.endPoint, mousePtF);
                    }
                    this.startNub.Location = mousePtF;
                    Vector vector = new Vector(this.startNub.Location.X - this.startPoint.X, this.startNub.Location.Y - this.startPoint.Y);
                    this.startPoint = mousePtF;
                    if (this.shouldMoveBothNubs)
                    {
                        this.endNub.Location += vector;
                        this.endPoint += vector;
                    }
                }
                else if (this.mouseNub == this.endNub)
                {
                    if (this.shouldConstrain && !this.shouldMoveBothNubs)
                    {
                        mousePtF = this.ConstrainPoints(this.startPoint, mousePtF);
                    }
                    this.endNub.Location = mousePtF;
                    Vector vector2 = new Vector(this.endNub.Location.X - this.endPoint.X, this.endNub.Location.Y - this.endPoint.Y);
                    this.endPoint = mousePtF;
                    if (this.shouldMoveBothNubs)
                    {
                        this.startNub.Location += vector2;
                        this.startPoint += vector2;
                    }
                }
                else
                {
                    if (this.shouldMoveBothNubs)
                    {
                        Vector vector3 = new Vector(this.endNub.Location.X - mousePtF.X, this.endNub.Location.Y - mousePtF.Y);
                        this.startNub.Location -= vector3;
                        this.startPoint -= vector3;
                    }
                    else if (this.shouldConstrain)
                    {
                        mousePtF = this.ConstrainPoints(this.startPoint, mousePtF);
                    }
                    this.endNub.Location = mousePtF;
                    this.endPoint = mousePtF;
                }
                this.RenderGradient();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            Point b = new Point((double) e.X, (double) e.Y);
            if (this.gradientActive)
            {
                if (e.Button != this.mouseButton)
                {
                    this.shouldMoveBothNubs = !this.shouldMoveBothNubs;
                }
                else
                {
                    if (this.mouseNub == this.startNub)
                    {
                        if (this.shouldConstrain)
                        {
                            b = this.ConstrainPoints(this.endPoint, b);
                        }
                        this.startNub.Location = b;
                        this.startPoint = b;
                    }
                    else if (this.mouseNub == this.endNub)
                    {
                        if (this.shouldConstrain)
                        {
                            b = this.ConstrainPoints(this.startPoint, b);
                        }
                        this.endNub.Location = b;
                        this.endPoint = b;
                    }
                    else
                    {
                        if (this.shouldConstrain)
                        {
                            b = this.ConstrainPoints(this.startPoint, b);
                        }
                        this.endNub.Location = b;
                        this.endPoint = b;
                    }
                    this.startNub.Visible = true;
                    this.endNub.Visible = true;
                    this.mouseButton = MouseButtons.None;
                    this.gradientActive = true;
                    base.SetStatus(this.toolIcon, this.helpTextAdjustable);
                }
            }
            base.OnMouseUp(e);
        }

        protected override void OnPulse()
        {
            if (this.gradientActive && (this.moveNubs != null))
            {
                for (int i = 0; i < this.moveNubs.Length; i++)
                {
                    if (this.moveNubs[i].Visible)
                    {
                        long num2 = (DateTime.Now.Ticks % 0x1312d00L) + (i * (0x1312d00 / this.moveNubs.Length));
                        double num3 = Math.Sin((((double) num2) / 20000000.0) * 6.2831853071795862);
                        num3 = Math.Min(0.5, num3) + 1.0;
                        num3 /= 2.0;
                        num3 += 0.25;
                        int num5 = ((int) (num3 * 255.0)).Clamp(0, 0xff);
                        this.moveNubs[i].Alpha = num5;
                    }
                }
            }
            base.OnPulse();
        }

        private MoveNubRenderer PointToNub(Point mousePtF)
        {
            double num = mousePtF.DistanceTo(this.startNub.Location);
            double num2 = mousePtF.DistanceTo(this.endNub.Location);
            if ((this.startNub.Visible && (num < num2)) && this.startNub.IsPointTouching(mousePtF, true))
            {
                return this.startNub;
            }
            if (this.endNub.Visible && this.endNub.IsPointTouching(mousePtF, true))
            {
                return this.endNub;
            }
            return null;
        }

        private void RenderBecauseOfEvent(object sender, EventArgs e)
        {
            if (this.gradientActive)
            {
                this.RenderGradient();
            }
        }

        private void RenderGradient()
        {
            string str;
            string str2;
            ColorBgra primaryColor = base.AppEnvironment.PrimaryColor;
            ColorBgra secondaryColor = base.AppEnvironment.SecondaryColor;
            if (this.shouldSwapColors)
            {
                if (base.AppEnvironment.GradientInfo.AlphaOnly)
                {
                    byte a = primaryColor.A;
                    primaryColor.A = (byte) (0xff - secondaryColor.A);
                    secondaryColor.A = (byte) (0xff - a);
                }
                else
                {
                    ObjectUtil.Swap<ColorBgra>(ref primaryColor, ref secondaryColor);
                }
            }
            Point startPoint = this.startPoint;
            Point endPoint = this.endPoint;
            if (this.shouldConstrain)
            {
                if (this.mouseNub == this.startNub)
                {
                    startPoint = this.ConstrainPoints(endPoint, startPoint);
                }
                else
                {
                    endPoint = this.ConstrainPoints(startPoint, endPoint);
                }
            }
            base.RestoreSavedRegion();
            Surface surface = ((BitmapLayer) base.DocumentWorkspace.ActiveLayer).Surface;
            GeometryList saveMeGeometry = base.DocumentWorkspace.Selection.CreateGeometryListClippingMask();
            base.SaveRegion(saveMeGeometry, saveMeGeometry.Bounds.Int32Bound());
            this.RenderGradient(surface, saveMeGeometry, base.AppEnvironment.GetCompositingMode(), startPoint, primaryColor, endPoint, secondaryColor);
            base.DocumentWorkspace.ActiveLayer.Invalidate(saveMeGeometry);
            saveMeGeometry.Dispose();
            double num2 = (-180.0 * Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X)) / 3.1415926535897931;
            MeasurementUnit units = base.AppWorkspace.Units;
            double num3 = base.Document.PixelToPhysicalX(endPoint.X - startPoint.X, units);
            double num4 = base.Document.PixelToPhysicalY(endPoint.Y - startPoint.Y, units);
            double num5 = Math.Sqrt((num3 * num3) + (num4 * num4));
            if (units != MeasurementUnit.Pixel)
            {
                str2 = PdnResources.GetString2("MeasurementUnit." + units.ToString() + ".Abbreviation");
                str = "F2";
            }
            else
            {
                str2 = string.Empty;
                str = "F0";
            }
            string str4 = PdnResources.GetString2("MeasurementUnit." + units.ToString() + ".Plural");
            string statusText = string.Format(this.helpTextWhileAdjustingFormat, new object[] { num3.ToString(str), str2, num4.ToString(str), str2, num5.ToString("F2"), str4, num2.ToString("F2") });
            base.SetStatus(this.toolIcon, statusText);
            base.Update();
        }

        private void RenderGradient(Surface surface, GeometryList clipGeometry, CompositingMode compositingMode, Point startPointF, ColorBgra startColor, Point endPointF, ColorBgra endColor)
        {
            Int32Rect[] rectArray2;
            GradientRenderer renderer = base.AppEnvironment.GradientInfo.CreateGradientRenderer();
            renderer.StartColor = startColor;
            renderer.EndColor = endColor;
            renderer.StartPoint = startPointF;
            renderer.EndPoint = endPointF;
            renderer.AlphaBlending = compositingMode == CompositingMode.SourceOver;
            renderer.BeforeRender();
            Int32Rect[] interiorScans = clipGeometry.GetInteriorScans();
            if (interiorScans.Length == 1)
            {
                rectArray2 = new Int32Rect[Processor.LogicalCpuCount];
                Utility.SplitRectangle(interiorScans[0], rectArray2);
            }
            else
            {
                rectArray2 = interiorScans;
            }
            RenderContext context = new RenderContext {
                surface = surface,
                rois = rectArray2,
                renderer = renderer
            };
            WaitCallback callback = new WaitCallback(context.Render);
            for (int i = 0; i < Processor.LogicalCpuCount; i++)
            {
                if (i == (Processor.LogicalCpuCount - 1))
                {
                    callback(BoxedConstants.GetInt32(i));
                }
                else
                {
                    this.threadPool.QueueUserWorkItem(callback, BoxedConstants.GetInt32(i));
                }
            }
            this.threadPool.Drain();
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.GradientToolIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("GradientTool.Name");

        private sealed class RenderContext
        {
            public GradientRenderer renderer;
            public Int32Rect[] rois;
            public Surface surface;

            public void Render(object cpuIndexObj)
            {
                int logicalCpuCount = Processor.LogicalCpuCount;
                int num2 = (int) cpuIndexObj;
                int startIndex = (this.rois.Length * num2) / logicalCpuCount;
                int num4 = (this.rois.Length * (num2 + 1)) / logicalCpuCount;
                this.renderer.Render(this.surface, this.rois, startIndex, num4 - startIndex);
            }
        }
    }
}

