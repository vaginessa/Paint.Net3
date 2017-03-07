namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Forms;

    internal abstract class SelectionTool : PaintDotNet.Tools.Tool
    {
        private bool append;
        private SelectionCombineMode combineMode;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;
        private Cursor cursorMouseUpMinus;
        private Cursor cursorMouseUpPlus;
        private bool hasMoved;
        private Int32Point lastXY;
        private bool moveOriginMode;
        private Selection newSelection;
        private SelectionRenderer newSelectionRenderer;
        private DateTime startTime;
        private SegmentedList<Point> tracePoints;
        private bool tracking;
        private SelectionHistoryMemento undoAction;
        private bool wasNotEmpty;

        public SelectionTool(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, ToolBarConfigItems toolBarConfigItems) : base(documentWorkspace, toolBarImage, name, helpText, hotKey, false, toolBarConfigItems | (ToolBarConfigItems.None | ToolBarConfigItems.SelectionCombineMode))
        {
            this.tracking = false;
        }

        private Point[] CreateSelectionPoly()
        {
            SegmentedList<Point> list3;
            SegmentedList<Point> inputTracePoints = this.TrimShapePath(this.tracePoints);
            SegmentedList<Point> v = this.CreateShape(inputTracePoints);
            switch (this.combineMode)
            {
                case SelectionCombineMode.Exclude:
                case SelectionCombineMode.Xor:
                    list3 = v;
                    break;

                default:
                    list3 = Utility.SutherlandHodgman(base.Document.Bounds().ToRect(), v);
                    break;
            }
            return list3.ToArrayEx<Point>();
        }

        protected virtual SegmentedList<Point> CreateShape(SegmentedList<Point> inputTracePoints) => 
            inputTracePoints;

        private void Done()
        {
            if (this.tracking)
            {
                WhatToDo reset;
                Point[] pointArray = this.CreateSelectionPoly();
                this.hasMoved &= pointArray.Length > 1;
                TimeSpan span = (TimeSpan) (DateTime.Now - this.startTime);
                bool flag = span.TotalMilliseconds <= 50.0;
                bool flag2 = pointArray.Length == 0;
                bool flag3 = false;
                if (this.append)
                {
                    if ((!this.hasMoved || flag2) || flag3)
                    {
                        reset = WhatToDo.Reset;
                    }
                    else
                    {
                        reset = WhatToDo.Emit;
                    }
                }
                else if ((this.hasMoved && !flag) && (!flag2 && !flag3))
                {
                    reset = WhatToDo.Emit;
                }
                else
                {
                    reset = WhatToDo.Clear;
                }
                switch (reset)
                {
                    case WhatToDo.Clear:
                        if (this.wasNotEmpty)
                        {
                            this.undoAction.Name = DeselectFunction.StaticName;
                            this.undoAction.Image = DeselectFunction.StaticImage;
                            base.HistoryStack.PushNewMemento(this.undoAction);
                        }
                        base.Selection.Reset();
                        break;

                    case WhatToDo.Emit:
                        this.undoAction.Name = base.Name;
                        base.HistoryStack.PushNewMemento(this.undoAction);
                        base.Selection.CommitContinuation();
                        break;

                    case WhatToDo.Reset:
                        base.Selection.ResetContinuation();
                        break;
                }
                this.newSelection.Reset();
                this.tracking = false;
                base.DocumentWorkspace.EnableSelectionOutline = true;
            }
        }

        private Cursor GetCursor() => 
            this.GetCursor(base.IsMouseDown, (base.ModifierKeys & Keys.Control) != Keys.None, (base.ModifierKeys & Keys.Alt) != Keys.None);

        private Cursor GetCursor(bool mouseDown, bool ctrlDown, bool altDown)
        {
            if (mouseDown)
            {
                return this.cursorMouseDown;
            }
            if (ctrlDown)
            {
                return this.cursorMouseUpPlus;
            }
            if (altDown)
            {
                return this.cursorMouseUpMinus;
            }
            return this.cursorMouseUp;
        }

        protected override void OnActivate()
        {
            base.Cursor = this.GetCursor();
            base.DocumentWorkspace.EnableSelectionTinting = true;
            this.newSelection = new Selection();
            this.newSelectionRenderer = new SelectionRenderer(base.CanvasRenderer, this.newSelection, base.DocumentWorkspace);
            this.newSelectionRenderer.EnableSelectionTinting = false;
            this.newSelectionRenderer.Visible = false;
            base.CanvasRenderer.Add(this.newSelectionRenderer, true);
            base.OnActivate();
        }

        protected override void OnClick()
        {
            base.OnClick();
            if (!this.moveOriginMode)
            {
                this.Done();
            }
        }

        protected override void OnDeactivate()
        {
            base.DocumentWorkspace.EnableSelectionTinting = false;
            if (this.tracking)
            {
                this.Done();
            }
            base.OnDeactivate();
            this.SetCursors(null, null, null, null);
            base.CanvasRenderer.Remove(this.newSelectionRenderer);
            this.newSelectionRenderer.Dispose();
            this.newSelectionRenderer = null;
            this.newSelection = null;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (this.tracking)
            {
                this.Render();
            }
            base.Cursor = this.GetCursor();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (this.tracking)
            {
                this.Render();
            }
            base.Cursor = this.GetCursor();
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            base.Cursor = this.GetCursor();
            if (this.tracking)
            {
                this.moveOriginMode = true;
                this.lastXY = Int32Point.Truncate(e.Point());
                this.OnMouseMove(e);
            }
            else if (((e.Button & MouseButtons.Left) == MouseButtons.Left) || ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                this.tracking = true;
                this.hasMoved = false;
                this.startTime = DateTime.Now;
                this.tracePoints = new SegmentedList<Point>();
                this.tracePoints.Add((Point) Int32Point.Truncate(e.Point()));
                this.undoAction = new SelectionHistoryMemento("sentinel", base.Image, base.DocumentWorkspace);
                this.wasNotEmpty = !base.Selection.IsEmpty;
                if (((base.ModifierKeys & Keys.Control) != Keys.None) && (e.Button == MouseButtons.Left))
                {
                    this.combineMode = SelectionCombineMode.Union;
                }
                else if (((base.ModifierKeys & Keys.Alt) != Keys.None) && (e.Button == MouseButtons.Left))
                {
                    this.combineMode = SelectionCombineMode.Exclude;
                }
                else if (((base.ModifierKeys & Keys.Control) != Keys.None) && (e.Button == MouseButtons.Right))
                {
                    this.combineMode = SelectionCombineMode.Xor;
                }
                else if (((base.ModifierKeys & Keys.Alt) != Keys.None) && (e.Button == MouseButtons.Right))
                {
                    this.combineMode = SelectionCombineMode.Intersect;
                }
                else
                {
                    this.combineMode = base.AppEnvironment.SelectionCombineMode;
                }
                base.DocumentWorkspace.EnableSelectionOutline = false;
                this.newSelection.Restore(base.Selection.Save());
                switch (this.combineMode)
                {
                    case SelectionCombineMode.Replace:
                        this.append = false;
                        base.Selection.Reset();
                        break;

                    case SelectionCombineMode.Union:
                    case SelectionCombineMode.Exclude:
                    case SelectionCombineMode.Intersect:
                    case SelectionCombineMode.Xor:
                        this.append = true;
                        base.Selection.ResetContinuation();
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }
                this.newSelectionRenderer.Visible = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            if (this.moveOriginMode)
            {
                Int32Point point = Int32Point.Truncate(e.Point());
                Int32Point point2 = new Int32Point(point.X - this.lastXY.X, point.Y - this.lastXY.Y);
                for (int i = 0; i < this.tracePoints.Count; i++)
                {
                    Point point3 = this.tracePoints[i];
                    point3.X += point2.X;
                    point3.Y += point2.Y;
                    this.tracePoints[i] = point3;
                }
                this.lastXY = point;
                this.Render();
            }
            else if (this.tracking)
            {
                Point item = (Point) Int32Point.Truncate(e.Point());
                if (item != this.tracePoints[this.tracePoints.Count - 1])
                {
                    this.tracePoints.Add(item);
                }
                this.hasMoved = true;
                this.Render();
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            this.OnMouseMove(e);
            if (this.moveOriginMode)
            {
                this.moveOriginMode = false;
            }
            else
            {
                this.Done();
            }
            base.OnMouseUp(e);
            base.Cursor = this.GetCursor();
        }

        private void Render()
        {
            if ((this.tracePoints != null) && (this.tracePoints.Count > 2))
            {
                Point[] polygon = this.CreateSelectionPoly();
                if (polygon.Length > 2)
                {
                    SelectionCombineMode replace;
                    base.Selection.SetContinuation(polygon, this.combineMode);
                    if (this.SelectionMode == SelectionCombineMode.Replace)
                    {
                        replace = SelectionCombineMode.Replace;
                    }
                    else
                    {
                        replace = SelectionCombineMode.Xor;
                    }
                    this.newSelection.SetContinuation(polygon, replace);
                    base.Update();
                }
            }
        }

        protected void SetCursors(string cursorMouseUpResName, string cursorMouseUpMinusResName, string cursorMouseUpPlusResName, string cursorMouseDownResName)
        {
            if (this.cursorMouseUp != null)
            {
                this.cursorMouseUp.Dispose();
                this.cursorMouseUp = null;
            }
            if (cursorMouseUpResName != null)
            {
                this.cursorMouseUp = PdnResources.GetCursor2(cursorMouseUpResName);
            }
            if (this.cursorMouseUpMinus != null)
            {
                this.cursorMouseUpMinus.Dispose();
                this.cursorMouseUpMinus = null;
            }
            if (cursorMouseUpMinusResName != null)
            {
                this.cursorMouseUpMinus = PdnResources.GetCursor2(cursorMouseUpMinusResName);
            }
            if (this.cursorMouseUpPlus != null)
            {
                this.cursorMouseUpPlus.Dispose();
                this.cursorMouseUpPlus = null;
            }
            if (cursorMouseUpPlusResName != null)
            {
                this.cursorMouseUpPlus = PdnResources.GetCursor2(cursorMouseUpPlusResName);
            }
            if (this.cursorMouseDown != null)
            {
                this.cursorMouseDown.Dispose();
                this.cursorMouseDown = null;
            }
            if (cursorMouseDownResName != null)
            {
                this.cursorMouseDown = PdnResources.GetCursor2(cursorMouseDownResName);
            }
        }

        protected virtual SegmentedList<Point> TrimShapePath(SegmentedList<Point> trimTheseTracePoints) => 
            trimTheseTracePoints;

        protected SelectionCombineMode SelectionMode =>
            this.combineMode;

        private enum WhatToDo
        {
            Clear,
            Emit,
            Reset
        }
    }
}

