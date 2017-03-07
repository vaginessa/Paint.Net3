namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;

    internal abstract class MoveToolBase : PaintDotNet.Tools.Tool
    {
        protected double angleDelta;
        protected Context context;
        protected List<HistoryMemento> currentHistoryMementos;
        protected bool deactivateOnLayerChange;
        protected bool dontDrop;
        protected bool enableOutline;
        protected double hostAngle;
        protected bool hostShouldShowAngle;
        protected MoveNubRenderer[] moveNubs;
        protected Cursor moveToolCursor;
        protected RotateNubRenderer rotateNub;
        protected bool tracking;

        public MoveToolBase(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, bool skipIfActiveOnHotKey, ToolBarConfigItems toolBarConfigItems) : base(documentWorkspace, toolBarImage, name, helpText, hotKey, skipIfActiveOnHotKey, toolBarConfigItems)
        {
            this.currentHistoryMementos = new List<HistoryMemento>();
            this.deactivateOnLayerChange = true;
            this.enableOutline = true;
        }

        protected double ConstrainAngle(double angle)
        {
            double num6;
            while (angle < 0.0)
            {
                angle += 360.0;
            }
            int num = (int) angle;
            int num2 = (num / 15) * 15;
            int num3 = num2 + 15;
            double num4 = Math.Abs((double) (angle - num2));
            double num5 = Math.Abs((double) (angle - num3));
            if (num4 < num5)
            {
                num6 = num2;
            }
            else
            {
                num6 = num3;
            }
            if (num6 > 180.0)
            {
                num6 -= 360.0;
            }
            return num6;
        }

        protected void ConstrainScaling(Rect liftedBounds, double startWidth, double startHeight, double newWidth, double newHeight, out double newXScale, out double newYScale)
        {
            double num = newWidth / liftedBounds.Width;
            double num2 = newHeight / liftedBounds.Height;
            double num3 = Math.Min(num, num2);
            double num4 = liftedBounds.Width * num3;
            double num5 = liftedBounds.Height * num3;
            newXScale = num4 / startWidth;
            newYScale = num5 / startHeight;
        }

        protected void DestroyNubs()
        {
            if (this.moveNubs != null)
            {
                for (int i = 0; i < this.moveNubs.Length; i++)
                {
                    base.CanvasRenderer.Remove(this.moveNubs[i]);
                    this.moveNubs[i].Dispose();
                    this.moveNubs[i] = null;
                }
                this.moveNubs = null;
            }
            if (this.rotateNub != null)
            {
                base.CanvasRenderer.Remove(this.rotateNub);
                this.rotateNub.Dispose();
                this.rotateNub = null;
            }
        }

        protected void DetermineMoveMode(MouseEventArgsF e, out Mode mode, out Edge edge)
        {
            mode = Mode.Translate;
            edge = Edge.None;
            if (e.Button == MouseButtons.Right)
            {
                mode = Mode.Rotate;
            }
            else
            {
                double maxValue = double.MaxValue;
                System.Windows.Point ptF = e.Point();
                for (int i = 0; i < this.moveNubs.Length; i++)
                {
                    MoveNubRenderer renderer = this.moveNubs[i];
                    if (renderer.IsPointTouching(ptF, false))
                    {
                        Vector vector = (Vector) (ptF - renderer.Location);
                        double length = vector.Length;
                        if (length < maxValue)
                        {
                            maxValue = length;
                            mode = Mode.Scale;
                            edge = (Edge) i;
                        }
                    }
                }
            }
        }

        protected abstract void Drop();
        protected Edge FlipEdgeVertically(Edge flipMe)
        {
            switch (flipMe)
            {
                case Edge.TopLeft:
                    return Edge.BottomLeft;

                case Edge.Top:
                    return Edge.Bottom;

                case Edge.TopRight:
                    return Edge.BottomRight;

                case Edge.Right:
                    return Edge.Right;

                case Edge.BottomRight:
                    return Edge.TopRight;

                case Edge.Bottom:
                    return Edge.Top;

                case Edge.BottomLeft:
                    return Edge.TopLeft;

                case Edge.Left:
                    return Edge.Left;

                case Edge.None:
                    return Edge.None;
            }
            throw new InvalidEnumArgumentException();
        }

        protected Vector GetEdgeVector(Edge edge)
        {
            switch (edge)
            {
                case Edge.TopLeft:
                    return new Vector(-1.0, -1.0);

                case Edge.Top:
                    return new Vector(0.0, -1.0);

                case Edge.TopRight:
                    return new Vector(1.0, -1.0);

                case Edge.Right:
                    return new Vector(1.0, 0.0);

                case Edge.BottomRight:
                    return new Vector(1.0, 1.0);

                case Edge.Bottom:
                    return new Vector(0.0, 1.0);

                case Edge.BottomLeft:
                    return new Vector(-1.0, 1.0);

                case Edge.Left:
                    return new Vector(-1.0, 0.0);
            }
            throw new InvalidEnumArgumentException();
        }

        protected void HideNubs()
        {
            if (this.moveNubs != null)
            {
                MoveNubRenderer[] moveNubs = this.moveNubs;
                for (int i = 0; i < moveNubs.Length; i++)
                {
                    CanvasLayer layer = moveNubs[i];
                    layer.Visible = false;
                }
            }
            if (this.rotateNub != null)
            {
                this.rotateNub.Visible = false;
            }
        }

        protected void Lift(MouseEventArgsF e)
        {
            this.PushContextHistoryMemento();
            this.context.seriesGuid = Guid.NewGuid();
            this.DetermineMoveMode(e, out this.context.currentMode, out this.context.startEdge);
            this.context.startBounds = this.context.liftedBounds;
            this.context.liftedBounds = base.Selection.GetBoundsF(false);
            this.context.startMouseXY = Int32Point.Truncate(e.Point());
            this.context.offset = new System.Drawing.Point(0, 0);
            this.context.startAngle = 0.0;
            this.context.lifted = true;
            this.context.liftTransform = base.Selection.GetCumulativeTransformCopy();
            this.OnLift(e);
            this.PositionNubs(this.context.currentMode);
        }

        protected override void OnKeyPress(Keys key)
        {
            if (!this.tracking)
            {
                int num = 0;
                int num2 = 0;
                if ((key & Keys.KeyCode) == Keys.Left)
                {
                    num = -1;
                }
                else if ((key & Keys.KeyCode) == Keys.Right)
                {
                    num = 1;
                }
                else if ((key & Keys.KeyCode) == Keys.Up)
                {
                    num2 = -1;
                }
                else if ((key & Keys.KeyCode) == Keys.Down)
                {
                    num2 = 1;
                }
                if ((key & Keys.Control) != Keys.None)
                {
                    num *= 10;
                    num2 *= 10;
                }
                if ((num != 0) || (num2 != 0))
                {
                    System.Drawing.Point position = Cursor.Position;
                    System.Drawing.Point point = new System.Drawing.Point(-70000, -70000);
                    System.Drawing.Point point2 = new System.Drawing.Point(point.X + num, point.Y + num2);
                    this.OnMouseDown(new MouseEventArgsF(MouseButtons.Left, 0, (double) point.X, (double) point.Y, 0));
                    this.OnMouseMove(new MouseEventArgsF(MouseButtons.Left, 0, (double) point2.X, (double) point2.Y, 0));
                    this.OnMouseUp(new MouseEventArgsF(MouseButtons.Left, 0, (double) point2.X, (double) point2.Y, 0));
                }
            }
            else
            {
                base.OnKeyPress(key);
            }
        }

        protected abstract void OnLift(MouseEventArgsF e);
        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            if (!this.tracking)
            {
                bool flag = false;
                Mode translate = Mode.Translate;
                Edge none = Edge.None;
                if (base.Selection.IsEmpty)
                {
                    SelectionHistoryMemento memento = new SelectionHistoryMemento(SelectAllFunction.StaticName, PdnResources.GetImageResource2("Icons.MenuEditSelectAllIcon.png"), base.DocumentWorkspace);
                    base.DocumentWorkspace.History.PushNewMemento(memento);
                    base.DocumentWorkspace.Selection.PerformChanging();
                    base.DocumentWorkspace.Selection.Reset();
                    base.DocumentWorkspace.Selection.SetContinuation(base.Document.Bounds(), SelectionCombineMode.Replace);
                    base.DocumentWorkspace.Selection.CommitContinuation();
                    base.DocumentWorkspace.Selection.PerformChanged();
                    if (e.Button == MouseButtons.Right)
                    {
                        translate = Mode.Rotate;
                    }
                    else
                    {
                        translate = Mode.Translate;
                    }
                    none = Edge.None;
                    flag = true;
                }
                base.DocumentWorkspace.EnableSelectionOutline = this.enableOutline;
                if (!this.context.lifted)
                {
                    this.Lift(e);
                }
                this.PushContextHistoryMemento();
                if (!flag)
                {
                    this.DetermineMoveMode(e, out translate, out none);
                    flag = true;
                }
                this.context.deltaTransform = Matrix.Identity;
                if (((translate == Mode.Translate) || (translate == Mode.Scale)) || ((translate != this.context.currentMode) || (translate == Mode.Rotate)))
                {
                    this.context.startBounds = base.Selection.GetBoundsF();
                    this.context.startMouseXY = Int32Point.Truncate(e.Point());
                    this.context.offset = new System.Drawing.Point(0, 0);
                    this.context.baseTransform = new Matrix?(base.Selection.GetInterimTransformCopy());
                }
                this.context.startEdge = none;
                this.context.currentMode = translate;
                this.PositionNubs(this.context.currentMode);
                this.tracking = true;
                this.rotateNub.Visible = this.context.currentMode == Mode.Rotate;
                if (this.context.startPath != null)
                {
                    this.context.startPath.Dispose();
                    this.context.startPath = null;
                }
                this.context.startPath = base.Selection.CreateGeometryList();
                this.context.startAngle = base.Selection.GetInterimTransformCopy().GetAngleOfTransform();
                SelectionHistoryMemento item = new SelectionHistoryMemento(base.Name, base.Image, base.DocumentWorkspace);
                this.currentHistoryMementos.Add(item);
                this.OnMouseMove(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            this.OnMouseMoveImpl(e);
        }

        private void OnMouseMoveImpl(MouseEventArgsF e)
        {
            Int32Point point3;
            Matrix identity;
            double num6;
            double num7;
            System.Windows.Point point6;
            double angleOfTransform;
            Rect rect3;
            double num9;
            double num10;
            bool flag2;
            double num13;
            double num14;
            System.Windows.Point ptF = e.Point();
            if (this.tracking)
            {
                if (this.context.currentMode != Mode.Translate)
                {
                    base.Cursor = base.handCursorMouseDown;
                }
                Int32Point point2 = base.SnapPoint(e.Point()).RoundCopy();
                point3 = new Int32Point(point2.X - this.context.startMouseXY.X, point2.Y - this.context.startMouseXY.Y);
                this.PreRender();
                this.dontDrop = true;
                base.Selection.PerformChanging();
                identity = Matrix.Identity;
                if (this.context.baseTransform.HasValue)
                {
                    base.Selection.SetInterimTransform(this.context.baseTransform.Value);
                }
                Matrix interimTransformCopy = base.Selection.GetInterimTransformCopy();
                switch (this.context.currentMode)
                {
                    case Mode.Translate:
                        identity.Translate((double) point3.X, (double) point3.Y);
                        goto Label_0677;

                    case Mode.Scale:
                    {
                        Vector vector9;
                        Vector vector10;
                        Vector vector11;
                        Vector vector12;
                        Vector edgeVector = this.GetEdgeVector(this.context.startEdge);
                        Vector vector = new Vector(edgeVector.X, 0.0);
                        Vector vector3 = new Vector(0.0, edgeVector.Y);
                        Vector vec = interimTransformCopy.Transform(vector);
                        Vector vector5 = interimTransformCopy.Transform(vector3);
                        Vector u = vec.NormalizeOrZeroCopy();
                        Vector vector7 = vector5.NormalizeOrZeroCopy();
                        Vector y = ((System.Windows.Point) point3).ToVector();
                        y.GetProjection(u, out vector9, out num6, out vector10);
                        y.GetProjection(vector7, out vector11, out num7, out vector12);
                        GeometryList list = this.context.startPath.Clone();
                        Rect bounds = list.Bounds;
                        point6 = new System.Windows.Point((bounds.Left + bounds.Right) / 2.0, (bounds.Top + bounds.Bottom) / 2.0);
                        angleOfTransform = interimTransformCopy.GetAngleOfTransform();
                        bool flag = interimTransformCopy.IsTransformFlipped();
                        Matrix matrix = Matrix.Identity;
                        matrix.RotateAt(-angleOfTransform, point6.X, point6.Y);
                        identity.RotateAt(-angleOfTransform, point6.X, point6.Y);
                        list.Transform(matrix);
                        rect3 = list.Bounds;
                        list.Dispose();
                        list = null;
                        Edge startEdge = this.context.startEdge;
                        if (flag)
                        {
                            startEdge = this.FlipEdgeVertically(startEdge);
                        }
                        switch (startEdge)
                        {
                            case Edge.TopLeft:
                                flag2 = true;
                                num9 = -rect3.X - rect3.Width;
                                num10 = -rect3.Y - rect3.Height;
                                goto Label_0549;

                            case Edge.Top:
                                flag2 = false;
                                num9 = 0.0;
                                num10 = -rect3.Y - rect3.Height;
                                goto Label_0549;

                            case Edge.TopRight:
                                flag2 = true;
                                num9 = -rect3.X;
                                num10 = -rect3.Y - rect3.Height;
                                goto Label_0549;

                            case Edge.Right:
                                flag2 = false;
                                num9 = -rect3.X;
                                num10 = 0.0;
                                goto Label_0549;

                            case Edge.BottomRight:
                                flag2 = true;
                                num9 = -rect3.X;
                                num10 = -rect3.Y;
                                goto Label_0549;

                            case Edge.Bottom:
                                flag2 = false;
                                num9 = 0.0;
                                num10 = -rect3.Y;
                                goto Label_0549;

                            case Edge.BottomLeft:
                                flag2 = true;
                                num9 = -rect3.X - rect3.Width;
                                num10 = -rect3.Y;
                                goto Label_0549;

                            case Edge.Left:
                                flag2 = false;
                                num9 = -rect3.X - rect3.Width;
                                num10 = 0.0;
                                goto Label_0549;
                        }
                        throw new InvalidEnumArgumentException();
                    }
                    case Mode.Rotate:
                    {
                        Rect liftedBounds = this.context.liftedBounds;
                        System.Windows.Point point = new System.Windows.Point(liftedBounds.X + (liftedBounds.Width / 2.0), liftedBounds.Y + (liftedBounds.Height / 2.0));
                        System.Windows.Point point5 = interimTransformCopy.Transform(point);
                        double num2 = Math.Atan2(this.context.startMouseXY.Y - point5.Y, this.context.startMouseXY.X - point5.X);
                        double num4 = Math.Atan2(e.Y - point5.Y, e.X - point5.X) - num2;
                        this.angleDelta = num4 * 57.295779513082323;
                        double angle = this.context.startAngle + this.angleDelta;
                        if ((base.ModifierKeys & Keys.Shift) != Keys.None)
                        {
                            angle = this.ConstrainAngle(angle);
                            this.angleDelta = angle - this.context.startAngle;
                        }
                        identity.RotateAt(this.angleDelta, point5.X, point5.Y);
                        this.rotateNub.Location = point5;
                        this.rotateNub.Angle = this.context.startAngle + this.angleDelta;
                        goto Label_0677;
                    }
                }
                throw new InvalidEnumArgumentException();
            }
            Cursor moveToolCursor = this.moveToolCursor;
            if (this.moveNubs != null)
            {
                for (int i = 0; i < this.moveNubs.Length; i++)
                {
                    MoveNubRenderer renderer = this.moveNubs[i];
                    if (renderer.Visible && renderer.IsPointTouching(ptF, false))
                    {
                        moveToolCursor = base.handCursor;
                        break;
                    }
                }
            }
            base.Cursor = moveToolCursor;
            return;
        Label_0549:
            identity.Translate(num9, num10);
            double newWidth = rect3.Width + num6;
            double newHeight = rect3.Height + num7;
            if (rect3.Width == 0.0)
            {
                num13 = 0.0;
            }
            else
            {
                num13 = newWidth / rect3.Width;
            }
            if (rect3.Height == 0.0)
            {
                num14 = 0.0;
            }
            else
            {
                num14 = newHeight / rect3.Height;
            }
            if (num13 == 0.0)
            {
                num13 = 1E-08;
            }
            if (num14 == 0.0)
            {
                num14 = 1E-08;
            }
            if (flag2 && ((base.ModifierKeys & Keys.Shift) != Keys.None))
            {
                this.ConstrainScaling(this.context.liftedBounds, rect3.Width, rect3.Height, newWidth, newHeight, out num13, out num14);
            }
            identity.Scale(num13, num14);
            identity.VerifyFinite();
            identity.Translate(-num9, -num10);
            identity.VerifyFinite();
            identity.RotateAt(angleOfTransform, point6.X, point6.Y);
            identity.VerifyFinite();
        Label_0677:
            this.context.deltaTransform = Matrix.Identity;
            this.context.deltaTransform = Matrix.Multiply(this.context.deltaTransform, this.context.liftTransform);
            this.context.deltaTransform = Matrix.Multiply(this.context.deltaTransform, identity);
            if (this.context.baseTransform.HasValue)
            {
                identity = Matrix.Multiply(this.context.baseTransform.Value, identity);
            }
            base.Selection.SetInterimTransform(identity);
            this.hostShouldShowAngle = this.rotateNub.Visible;
            this.hostAngle = -this.rotateNub.Angle;
            base.Selection.PerformChanged();
            this.dontDrop = false;
            Int32Point point7 = Int32Point.Truncate((System.Windows.Point) point3);
            this.Render((System.Drawing.Point) point7, true);
            base.Update();
            this.context.offset = point7;
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.DocumentWorkspace.EnableSelectionOutline = true;
            base.OnMouseUp(e);
        }

        protected override void OnPulse()
        {
            if (this.moveNubs != null)
            {
                for (int i = 0; i < this.moveNubs.Length; i++)
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
            base.OnPulse();
        }

        protected void PositionNubs(Mode currentMode)
        {
            if (this.moveNubs == null)
            {
                this.moveNubs = new MoveNubRenderer[8];
                for (int i = 0; i < this.moveNubs.Length; i++)
                {
                    this.moveNubs[i] = new MoveNubRenderer(base.CanvasRenderer);
                    base.CanvasRenderer.Add(this.moveNubs[i], false);
                }
                Rect boundsF = base.Selection.GetBoundsF(false);
                this.moveNubs[0].Location = new System.Windows.Point(boundsF.Left, boundsF.Top);
                this.moveNubs[0].Shape = MoveNubShape.Circle;
                this.moveNubs[1].Location = new System.Windows.Point((boundsF.Left + boundsF.Right) / 2.0, boundsF.Top);
                this.moveNubs[2].Location = new System.Windows.Point(boundsF.Right, boundsF.Top);
                this.moveNubs[2].Shape = MoveNubShape.Circle;
                this.moveNubs[7].Location = new System.Windows.Point(boundsF.Left, (boundsF.Top + boundsF.Bottom) / 2.0);
                this.moveNubs[3].Location = new System.Windows.Point(boundsF.Right, (boundsF.Top + boundsF.Bottom) / 2.0);
                this.moveNubs[6].Location = new System.Windows.Point(boundsF.Left, boundsF.Bottom);
                this.moveNubs[6].Shape = MoveNubShape.Circle;
                this.moveNubs[5].Location = new System.Windows.Point((boundsF.Left + boundsF.Right) / 2.0, boundsF.Bottom);
                this.moveNubs[4].Location = new System.Windows.Point(boundsF.Right, boundsF.Bottom);
                this.moveNubs[4].Shape = MoveNubShape.Circle;
            }
            if (this.rotateNub == null)
            {
                this.rotateNub = new RotateNubRenderer(base.CanvasRenderer);
                this.rotateNub.Visible = false;
                base.CanvasRenderer.Add(this.rotateNub, false);
            }
            if (base.Selection.IsEmpty)
            {
                MoveNubRenderer[] moveNubs = this.moveNubs;
                for (int j = 0; j < moveNubs.Length; j++)
                {
                    CanvasLayer layer = moveNubs[j];
                    layer.Visible = false;
                }
                this.rotateNub.Visible = false;
            }
            else
            {
                foreach (MoveNubRenderer renderer in this.moveNubs)
                {
                    renderer.Visible = !this.tracking || (currentMode == Mode.Scale);
                    renderer.Transform = base.Selection.GetInterimTransformCopy();
                }
            }
        }

        protected abstract void PreRender();
        protected abstract void PushContextHistoryMemento();
        protected abstract void Render(System.Drawing.Point newOffset, bool useNewOffset);

        public override bool DeactivateOnLayerChange =>
            this.deactivateOnLayerChange;

        public double HostAngle =>
            this.hostAngle;

        public bool HostShouldShowAngle =>
            this.hostShouldShowAngle;

        protected class CompoundToolHistoryMemento : ToolHistoryMemento
        {
            private PaintDotNet.HistoryMementos.CompoundHistoryMemento compoundHistoryMemento;

            public CompoundToolHistoryMemento(PaintDotNet.HistoryMementos.CompoundHistoryMemento chm, DocumentWorkspace documentWorkspace, string name, ImageResource image) : base(documentWorkspace, name, image)
            {
                this.compoundHistoryMemento = chm;
            }

            protected override HistoryMemento OnToolUndo() => 
                new MoveToolBase.CompoundToolHistoryMemento((PaintDotNet.HistoryMementos.CompoundHistoryMemento) this.compoundHistoryMemento.PerformUndo(), base.DocumentWorkspace, base.Name, base.Image);

            public PaintDotNet.HistoryMementos.CompoundHistoryMemento CompoundHistoryMemento =>
                this.compoundHistoryMemento;
        }

        [Serializable]
        protected class Context : ICloneable, ISerializable, IDisposable
        {
            public Matrix? baseTransform;
            public MoveToolBase.Mode currentMode;
            public Matrix deltaTransform;
            public bool lifted;
            public Rect liftedBounds;
            public Matrix liftTransform;
            public Int32Point offset;
            public Guid seriesGuid;
            public double startAngle;
            public Rect startBounds;
            public MoveToolBase.Edge startEdge;
            public Int32Point startMouseXY;
            public GeometryList startPath;

            public Context()
            {
            }

            public Context(MoveToolBase.Context cloneMe)
            {
                this.lifted = cloneMe.lifted;
                this.seriesGuid = cloneMe.seriesGuid;
                this.baseTransform = cloneMe.baseTransform;
                this.deltaTransform = cloneMe.deltaTransform;
                this.liftTransform = cloneMe.liftTransform;
                this.liftedBounds = cloneMe.liftedBounds;
                this.startBounds = cloneMe.startBounds;
                this.startAngle = cloneMe.startAngle;
                if (cloneMe.startPath == null)
                {
                    this.startPath = null;
                }
                else
                {
                    this.startPath = cloneMe.startPath.Clone();
                }
                this.currentMode = cloneMe.currentMode;
                this.startEdge = cloneMe.startEdge;
                this.startMouseXY = cloneMe.startMouseXY;
                this.offset = cloneMe.offset;
            }

            public Context(SerializationInfo info, StreamingContext context)
            {
                this.lifted = info.GetValue<bool>("lifted");
                this.seriesGuid = info.GetValue<Guid>("seriesGuid");
                this.baseTransform = info.GetValue<Matrix?>("baseTransform");
                this.deltaTransform = info.GetValue<Matrix>("deltaTransform");
                this.liftTransform = info.GetValue<Matrix>("liftTransform");
                this.liftedBounds = info.GetValue<Rect>("liftedBounds");
                this.startBounds = info.GetValue<Rect>("startBounds");
                this.startAngle = info.GetValue<double>("startAngle");
                this.startPath = info.GetValue<GeometryList>("startPath");
                this.currentMode = info.GetValue<MoveToolBase.Mode>("currentMode");
                this.startEdge = info.GetValue<MoveToolBase.Edge>("startEdge");
                this.startMouseXY = info.GetValue<Int32Point>("startMouseXY");
                this.offset = info.GetValue<Int32Point>("offset");
            }

            public virtual object Clone() => 
                new MoveToolBase.Context(this);

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing && (this.startPath != null))
                {
                    this.startPath.Dispose();
                    this.startPath = null;
                }
            }

            ~Context()
            {
                this.Dispose(false);
            }

            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("lifted", this.lifted);
                info.AddValue("seriesGuid", this.seriesGuid);
                info.AddValue("baseTransform", this.baseTransform);
                info.AddValue("deltaTransform", this.deltaTransform);
                info.AddValue("liftTransform", this.liftTransform);
                info.AddValue("liftedBounds", this.liftedBounds);
                info.AddValue("startBounds", this.startBounds);
                info.AddValue("startAngle", this.startAngle);
                info.AddValue("startPath", this.startPath);
                info.AddValue("currentMode", this.currentMode);
                info.AddValue("startEdge", this.startEdge);
                info.AddValue("startMouseXY", this.startMouseXY);
                info.AddValue("offset", this.offset);
            }
        }

        protected enum Edge
        {
            Bottom = 5,
            BottomLeft = 6,
            BottomRight = 4,
            Left = 7,
            None = 0x63,
            Right = 3,
            Top = 1,
            TopLeft = 0,
            TopRight = 2
        }

        protected enum Mode
        {
            Translate,
            Scale,
            Rotate
        }
    }
}

