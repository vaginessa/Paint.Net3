namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Rendering;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Forms;

    internal sealed class LineTool : ShapeTool
    {
        private bool controlKeyDown;
        private readonly TimeSpan controlKeyDownThreshold;
        private DateTime controlKeyDownTime;
        private const int controlPointCount = 4;
        private CurveType curveType;
        private int draggingNubIndex;
        private const double flattenConstant = 0.1;
        private bool inCurveMode;
        private Cursor lineToolCursor;
        private ImageResource lineToolIcon;
        private Cursor lineToolMouseDownCursor;
        private MoveNubRenderer[] moveNubs;
        private string statusTextFormat;
        private const int toggleDashOrdinal = 1;
        private const int toggleEndCapOrdinal = 2;
        private const int toggleStartCapOrdinal = 0;

        public LineTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.LineToolIcon.png"), PdnResources.GetString2("LineTool.Name"), PdnResources.GetString2("LineTool.HelpText"), ToolBarConfigItems.None | ToolBarConfigItems.PenCaps, ToolBarConfigItems.None | ToolBarConfigItems.ShapeType)
        {
            this.statusTextFormat = PdnResources.GetString2("LineTool.StatusText.Format");
            this.draggingNubIndex = -1;
            this.controlKeyDownTime = DateTime.MinValue;
            this.controlKeyDownThreshold = new TimeSpan(0, 0, 0, 0, 400);
            base.ForceShapeDrawType = true;
            base.ForcedShapeDrawType = ShapeDrawType.Outline;
            base.UseDashStyle = true;
            base.AutoSnapAllPoints = false;
        }

        private System.Windows.Point ConstrainPoints(System.Windows.Point a, System.Windows.Point b)
        {
            Vector vector = (Vector) (b - a);
            double num = Math.Atan2(vector.Y, vector.X);
            double length = vector.Length;
            double d = (Math.Round((double) ((12.0 * num) / 3.1415926535897931)) * 3.1415926535897931) / 12.0;
            return new System.Windows.Point(a.X + (length * Math.Cos(d)), a.Y + (length * Math.Sin(d)));
        }

        protected override PdnGraphicsPath CreateShapePath(System.Windows.Point[] points)
        {
            if (points.Length < 4)
            {
                string str;
                string str2;
                System.Windows.Point a = base.SnapPoint(points[0]);
                System.Windows.Point b = points[points.Length - 1];
                if (((base.ModifierKeys & Keys.Shift) != Keys.None) && (a != b))
                {
                    b = this.ConstrainPoints(a, b);
                }
                double num = (-180.0 * Math.Atan2(b.Y - a.Y, b.X - a.X)) / 3.1415926535897931;
                MeasurementUnit units = base.AppWorkspace.Units;
                double num2 = base.Document.PixelToPhysicalX(b.X - a.X, units);
                double num3 = base.Document.PixelToPhysicalY(b.Y - a.Y, units);
                double num4 = Math.Sqrt((num2 * num2) + (num3 * num3));
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
                string statusText = string.Format(this.statusTextFormat, new object[] { num2.ToString(str), str2, num3.ToString(str), str2, num4.ToString("F2"), str4, num.ToString("F2") });
                base.SetStatus(this.lineToolIcon, statusText);
                if (a == b)
                {
                    return null;
                }
                PdnGraphicsPath path2 = new PdnGraphicsPath();
                System.Windows.Point[] pointArray = this.LineToSpline(a, b, 4);
                path2.AddCurve((from p in pointArray select p.ToGdipPointF()).ToArrayEx<PointF>());
                using (Matrix matrix2 = new Matrix())
                {
                    path2.Flatten(matrix2, 0.1f);
                }
                return path2;
            }
            PointF[] tfArray = points.ToPointFArray();
            PdnGraphicsPath path = new PdnGraphicsPath();
            switch (this.curveType)
            {
                case CurveType.Bezier:
                    path.AddBezier(tfArray[0], tfArray[1], tfArray[2], tfArray[3]);
                    break;

                default:
                    path.AddCurve(tfArray);
                    break;
            }
            using (Matrix matrix = new Matrix())
            {
                path.Flatten(matrix, 0.1f);
            }
            return path;
        }

        public override PixelOffsetMode GetPixelOffsetMode() => 
            PixelOffsetMode.None;

        private System.Windows.Point[] LineToSpline(System.Windows.Point a, System.Windows.Point b, int points)
        {
            System.Windows.Point[] pointArray = new System.Windows.Point[points];
            for (int i = 0; i < pointArray.Length; i++)
            {
                double t = ((double) i) / ((double) (pointArray.Length - 1));
                pointArray[i] = PointUtil.Lerp(a, b, t);
            }
            return pointArray;
        }

        protected override void OnActivate()
        {
            this.lineToolCursor = PdnResources.GetCursor2("Cursors.LineToolCursor.cur");
            this.lineToolMouseDownCursor = PdnResources.GetCursor2("Cursors.GenericToolCursorMouseDown.cur");
            base.Cursor = this.lineToolCursor;
            this.lineToolIcon = base.Image;
            this.moveNubs = new MoveNubRenderer[4];
            for (int i = 0; i < this.moveNubs.Length; i++)
            {
                this.moveNubs[i] = new MoveNubRenderer(base.CanvasRenderer);
                this.moveNubs[i].Visible = false;
                base.CanvasRenderer.Add(this.moveNubs[i], false);
            }
            EventHandler handler = new EventHandler(this.RenderShapeBecauseOfEvent);
            base.AppEnvironment.PrimaryColorChanged += handler;
            base.AppEnvironment.SecondaryColorChanged += handler;
            base.AppEnvironment.AntiAliasingChanged += handler;
            base.AppEnvironment.AlphaBlendingChanged += handler;
            base.AppEnvironment.BrushInfoChanged += handler;
            base.AppEnvironment.PenInfoChanged += handler;
            base.AppWorkspace.UnitsChanged += handler;
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            EventHandler handler = new EventHandler(this.RenderShapeBecauseOfEvent);
            base.AppEnvironment.PrimaryColorChanged -= handler;
            base.AppEnvironment.SecondaryColorChanged -= handler;
            base.AppEnvironment.AntiAliasingChanged -= handler;
            base.AppEnvironment.AlphaBlendingChanged -= handler;
            base.AppEnvironment.BrushInfoChanged -= handler;
            base.AppEnvironment.PenInfoChanged -= handler;
            base.AppWorkspace.UnitsChanged -= handler;
            for (int i = 0; i < this.moveNubs.Length; i++)
            {
                base.CanvasRenderer.Remove(this.moveNubs[i]);
                this.moveNubs[i].Dispose();
                this.moveNubs[i] = null;
            }
            this.moveNubs = null;
            if (this.lineToolCursor != null)
            {
                this.lineToolCursor.Dispose();
                this.lineToolCursor = null;
            }
            if (this.lineToolMouseDownCursor != null)
            {
                this.lineToolMouseDownCursor.Dispose();
                this.lineToolMouseDownCursor = null;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.ControlKey) && !this.controlKeyDown)
            {
                this.controlKeyDown = true;
                this.controlKeyDownTime = DateTime.Now;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (this.inCurveMode)
            {
                switch (e.KeyChar)
                {
                    case '\r':
                        e.Handled = true;
                        base.CommitShape();
                        break;

                    case '\x001b':
                        if ((base.ModifierKeys & Keys.Control) == Keys.None)
                        {
                            e.Handled = true;
                            base.HistoryStack.StepBackward(base.AppWorkspace);
                        }
                        break;
                }
            }
            base.OnKeyPress(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                TimeSpan span = (TimeSpan) (DateTime.Now - this.controlKeyDownTime);
                if (span < this.controlKeyDownThreshold)
                {
                    for (int i = 0; i < this.moveNubs.Length; i++)
                    {
                        this.moveNubs[i].Visible = this.inCurveMode && !this.moveNubs[i].Visible;
                    }
                }
                this.controlKeyDown = false;
            }
            base.OnKeyUp(e);
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            bool flag = false;
            if (!this.inCurveMode)
            {
                flag = true;
            }
            else
            {
                System.Windows.Point ptF = new System.Windows.Point(e.Fx, e.Fy);
                double maxValue = double.MaxValue;
                for (int i = 0; i < this.moveNubs.Length; i++)
                {
                    if (this.moveNubs[i].IsPointTouching(ptF, true))
                    {
                        double num3 = ptF.DistanceTo(this.moveNubs[i].Location);
                        if (num3 < maxValue)
                        {
                            maxValue = num3;
                            this.draggingNubIndex = i;
                        }
                    }
                }
                if (this.draggingNubIndex == -1)
                {
                    flag = true;
                }
                else
                {
                    base.Cursor = base.handCursorMouseDown;
                    if (this.curveType == CurveType.NotDecided)
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            this.curveType = CurveType.Bezier;
                        }
                        else
                        {
                            this.curveType = CurveType.Spline;
                        }
                    }
                    for (int j = 0; j < this.moveNubs.Length; j++)
                    {
                        this.moveNubs[j].Visible = false;
                    }
                    string statusText = PdnResources.GetString2("LineTool.CurvingHelpText");
                    base.SetStatus(null, statusText);
                    this.OnMouseMove(e);
                }
            }
            if (flag)
            {
                base.OnMouseDown(e);
                base.Cursor = this.lineToolMouseDownCursor;
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            if (this.inCurveMode && (this.draggingNubIndex != -1))
            {
                System.Windows.Point point = new System.Windows.Point(e.Fx, e.Fy);
                this.moveNubs[this.draggingNubIndex].Location = point;
                SegmentedList<System.Windows.Point> trimmedShapePath = base.GetTrimmedShapePath();
                trimmedShapePath[this.draggingNubIndex] = point;
                base.SetShapePath(trimmedShapePath);
                base.RenderShape();
            }
            else
            {
                System.Windows.Point ptF = e.Point();
                bool flag = false;
                for (int i = 0; i < this.moveNubs.Length; i++)
                {
                    if (this.moveNubs[i].Visible && this.moveNubs[i].IsPointTouching(ptF, true))
                    {
                        base.Cursor = base.handCursor;
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    if (base.IsMouseDown)
                    {
                        base.Cursor = this.lineToolMouseDownCursor;
                    }
                    else
                    {
                        base.Cursor = this.lineToolCursor;
                    }
                }
                base.OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            if (!this.inCurveMode)
            {
                base.OnMouseUp(e);
            }
            else if (this.draggingNubIndex != -1)
            {
                this.OnMouseMove(e);
                this.draggingNubIndex = -1;
                base.Cursor = this.lineToolCursor;
                for (int i = 0; i < this.moveNubs.Length; i++)
                {
                    this.moveNubs[i].Visible = true;
                }
            }
        }

        protected override void OnPulse()
        {
            if (this.moveNubs != null)
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

        protected override void OnShapeCommitting()
        {
            for (int i = 0; i < this.moveNubs.Length; i++)
            {
                this.moveNubs[i].Visible = false;
            }
            this.inCurveMode = false;
            this.curveType = CurveType.NotDecided;
            base.Cursor = this.lineToolCursor;
            this.draggingNubIndex = -1;
            base.DocumentWorkspace.UpdateStatusBarToToolHelpText();
        }

        protected override bool OnShapeEnd()
        {
            SegmentedList<System.Windows.Point> trimmedShapePath = base.GetTrimmedShapePath();
            if (trimmedShapePath.Count < 2)
            {
                return true;
            }
            System.Windows.Point a = base.SnapPoint(trimmedShapePath[0]);
            System.Windows.Point b = trimmedShapePath[trimmedShapePath.Count - 1];
            if (((base.ModifierKeys & Keys.Shift) != Keys.None) && (a != b))
            {
                b = this.ConstrainPoints(a, b);
            }
            System.Windows.Point[] pointArray = this.LineToSpline(a, b, 4);
            SegmentedList<System.Windows.Point> newPoints = new SegmentedList<System.Windows.Point>();
            this.inCurveMode = true;
            for (int i = 0; i < this.moveNubs.Length; i++)
            {
                this.moveNubs[i].Location = pointArray[i];
                this.moveNubs[i].Visible = true;
                newPoints.Add(pointArray[i]);
            }
            string statusText = PdnResources.GetString2("LineTool.PreCurveHelpText");
            base.SetStatus(null, statusText);
            base.SetShapePath(newPoints);
            return false;
        }

        protected override bool OnWildShortcutKey(int ordinal)
        {
            switch (ordinal)
            {
                case 0:
                    base.AppWorkspace.Widgets.ToolConfigStrip.CyclePenStartCap();
                    return true;

                case 1:
                    base.AppWorkspace.Widgets.ToolConfigStrip.CyclePenDashStyle();
                    return true;

                case 2:
                    base.AppWorkspace.Widgets.ToolConfigStrip.CyclePenEndCap();
                    return true;
            }
            return base.OnWildShortcutKey(ordinal);
        }

        private void RenderShapeBecauseOfEvent(object sender, EventArgs e)
        {
            if (this.inCurveMode)
            {
                base.RenderShape();
            }
        }

        protected override SegmentedList<System.Windows.Point> TrimShapePath(SegmentedList<System.Windows.Point> points)
        {
            if (this.inCurveMode)
            {
                return points;
            }
            SegmentedList<System.Windows.Point> list = new SegmentedList<System.Windows.Point>();
            if (points.Count > 0)
            {
                list.Add(points[0]);
                if (points.Count > 1)
                {
                    list.Add(points[points.Count - 1]);
                }
            }
            return list;
        }

        private enum CurveType
        {
            NotDecided,
            Bezier,
            Spline
        }
    }
}

