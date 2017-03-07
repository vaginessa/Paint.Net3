namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows;
    using System.Windows.Forms;

    internal abstract class ShapeTool : PaintDotNet.Tools.Tool
    {
        private bool autoSnapAllPoints;
        private BitmapLayer bitmapLayer;
        private CompoundHistoryMemento chaAlreadyOnStack;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;
        private const char defaultShortcut = 'o';
        private ShapeDrawType forcedShapeDrawType;
        private bool forceShapeType;
        private GeometryList lastDrawnRegion;
        private System.Windows.Point lastXY;
        private MouseButtons mouseButton;
        private bool mouseDown;
        private bool moveOriginMode;
        private SegmentedList<System.Windows.Point> points;
        private RenderArgs renderArgs;
        private GeometryList saveRegion;
        private bool shapeWasCommited;
        private bool useDashStyle;

        public ShapeTool(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText) : this(documentWorkspace, toolBarImage, name, helpText, 'o', ToolBarConfigItems.None, ToolBarConfigItems.None)
        {
        }

        public ShapeTool(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, ToolBarConfigItems toolBarConfigItemsInclude, ToolBarConfigItems toolBarConfigItemsExclude) : this(documentWorkspace, toolBarImage, name, helpText, 'o', toolBarConfigItemsInclude, toolBarConfigItemsExclude)
        {
        }

        public ShapeTool(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, ToolBarConfigItems toolBarConfigItemsInclude, ToolBarConfigItems toolBarConfigItemsExclude) : base(documentWorkspace, toolBarImage, name, helpText, hotKey, false, (toolBarConfigItemsInclude | (ToolBarConfigItems.AlphaBlending | ToolBarConfigItems.Antialiasing | ToolBarConfigItems.Brush | ToolBarConfigItems.Pen | ToolBarConfigItems.ShapeType)) & ~toolBarConfigItemsExclude)
        {
            this.shapeWasCommited = true;
            this.forcedShapeDrawType = ShapeDrawType.Both;
            this.autoSnapAllPoints = true;
            this.mouseDown = false;
            this.points = null;
            this.cursorMouseUp = PdnResources.GetCursor2("Cursors.ShapeToolCursor.cur");
            this.cursorMouseDown = PdnResources.GetCursor2("Cursors.ShapeToolCursorMouseDown.cur");
        }

        protected void CommitShape()
        {
            this.OnShapeCommitting();
            this.mouseDown = false;
            GeometryList rhs = base.Selection.CreateGeometryListClippingMask();
            if (this.saveRegion != null)
            {
                using (GeometryList list2 = GeometryList.Combine(this.saveRegion, GeometryCombineMode.Intersect, rhs))
                {
                    if (!list2.IsEmpty)
                    {
                        BitmapHistoryMemento memento = new BitmapHistoryMemento(base.Name, base.Image, base.DocumentWorkspace, base.ActiveLayerIndex, this.saveRegion, base.ScratchSurface);
                        DisposableUtil.Free<GeometryList>(ref this.saveRegion);
                        if (this.chaAlreadyOnStack == null)
                        {
                            base.HistoryStack.PushNewMemento(memento);
                        }
                        else
                        {
                            this.chaAlreadyOnStack.PushNewAction(memento);
                            this.chaAlreadyOnStack = null;
                        }
                    }
                }
            }
            DisposableUtil.Free<GeometryList>(ref rhs);
            this.points = null;
            base.Update();
            this.shapeWasCommited = true;
        }

        protected abstract PdnGraphicsPath CreateShapePath(System.Windows.Point[] shapePoints);
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (this.cursorMouseUp != null)
                {
                    this.cursorMouseUp.Dispose();
                    this.cursorMouseUp = null;
                }
                if (this.cursorMouseDown != null)
                {
                    this.cursorMouseDown.Dispose();
                    this.cursorMouseDown = null;
                }
            }
        }

        public virtual PixelOffsetMode GetPixelOffsetMode() => 
            PixelOffsetMode.Half;

        protected SegmentedList<System.Windows.Point> GetTrimmedShapePath()
        {
            SegmentedList<System.Windows.Point> trimThesePoints = new SegmentedList<System.Windows.Point>(this.points, 7);
            return this.TrimShapePath(trimThesePoints);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            DisposableUtil.Free<GeometryList>(ref this.saveRegion);
            this.bitmapLayer = (BitmapLayer) base.ActiveLayer;
            this.renderArgs = new RenderArgs(this.bitmapLayer.Surface);
            this.lastDrawnRegion = new GeometryList();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            if (this.mouseDown)
            {
                System.Windows.Point point = this.points[this.points.Count - 1];
                this.OnMouseUp(new MouseEventArgsF(this.mouseButton, 0, point.X, point.Y, 0));
            }
            if (!this.shapeWasCommited)
            {
                this.CommitShape();
            }
            this.bitmapLayer = null;
            DisposableUtil.Free<RenderArgs>(ref this.renderArgs);
            DisposableUtil.Free<GeometryList>(ref this.saveRegion);
            this.points = null;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (this.mouseDown)
            {
                this.RenderShape();
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (this.mouseDown)
            {
                this.RenderShape();
            }
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            if (!this.shapeWasCommited)
            {
                this.CommitShape();
            }
            base.ClearSavedMemory();
            base.ClearSavedRegion();
            this.cursorMouseUp = base.Cursor;
            base.Cursor = this.cursorMouseDown;
            if (!this.mouseDown || (e.Button != this.mouseButton))
            {
                if (this.mouseDown)
                {
                    this.moveOriginMode = true;
                    this.lastXY = e.Point();
                    this.OnMouseMove(e);
                }
                else if (((e.Button & MouseButtons.Left) == MouseButtons.Left) || ((e.Button & MouseButtons.Right) == MouseButtons.Right))
                {
                    this.shapeWasCommited = false;
                    this.OnShapeBegin();
                    this.mouseDown = true;
                    this.mouseButton = e.Button;
                    using (GeometryList list = base.Selection.CreateGeometryListClippingMask())
                    {
                        this.renderArgs.Graphics.SetClip(list);
                    }
                    this.points = new SegmentedList<System.Windows.Point>();
                    this.OnMouseMove(e);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            if (this.moveOriginMode)
            {
                Vector vector = (Vector) (e.Point() - this.lastXY);
                for (int i = 0; i < this.points.Count; i++)
                {
                    System.Windows.Point point = this.points[i];
                    point.X += vector.X;
                    point.Y += vector.Y;
                    this.points[i] = point;
                }
                this.lastXY = e.Point();
            }
            else if (this.mouseDown && ((e.Button & this.mouseButton) != MouseButtons.None))
            {
                System.Windows.Point item = e.Point();
                this.points.Add(item);
            }
            if (this.mouseDown && ((e.Button & this.mouseButton) != MouseButtons.None))
            {
                this.RenderShape();
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            base.Cursor = this.cursorMouseUp;
            if (this.moveOriginMode)
            {
                this.moveOriginMode = false;
            }
            else if (this.mouseDown)
            {
                if (this.OnShapeEnd())
                {
                    this.CommitShape();
                }
                else
                {
                    CompoundHistoryMemento memento = new CompoundHistoryMemento(base.Name, base.Image, new List<HistoryMemento>());
                    base.HistoryStack.PushNewMemento(memento);
                    this.chaAlreadyOnStack = memento;
                }
            }
        }

        protected virtual void OnShapeBegin()
        {
        }

        protected virtual void OnShapeCommitting()
        {
        }

        protected virtual bool OnShapeEnd() => 
            true;

        protected void RenderShape()
        {
            Pen pen = null;
            Brush brush = null;
            System.Windows.Point[] pointArray;
            PenInfo penInfo = base.AppEnvironment.PenInfo;
            BrushInfo brushInfo = base.AppEnvironment.BrushInfo;
            ColorBgra primaryColor = base.AppEnvironment.PrimaryColor;
            ColorBgra secondaryColor = base.AppEnvironment.SecondaryColor;
            if (!this.ForceShapeDrawType && (base.AppEnvironment.ShapeDrawType == ShapeDrawType.Interior))
            {
                ObjectUtil.Swap<ColorBgra>(ref primaryColor, ref secondaryColor);
            }
            if ((this.mouseButton & MouseButtons.Left) == MouseButtons.Left)
            {
                pen = penInfo.CreatePen(base.AppEnvironment.BrushInfo, (Color) primaryColor, (Color) secondaryColor);
                brush = brushInfo.CreateBrush((Color) secondaryColor, primaryColor.ToColor());
            }
            else if ((this.mouseButton & MouseButtons.Right) == MouseButtons.Right)
            {
                pen = penInfo.CreatePen(base.AppEnvironment.BrushInfo, (Color) secondaryColor, (Color) primaryColor);
                brush = brushInfo.CreateBrush((Color) primaryColor, (Color) secondaryColor);
            }
            if (!this.useDashStyle)
            {
                pen.DashStyle = DashStyle.Solid;
            }
            pen.LineJoin = LineJoin.MiterClipped;
            pen.MiterLimit = 2f;
            if (this.saveRegion != null)
            {
                base.RestoreRegion(this.saveRegion);
            }
            this.renderArgs.Graphics.SmoothingMode = base.AppEnvironment.AntiAliasing ? SmoothingMode.AntiAlias : SmoothingMode.None;
            this.renderArgs.Graphics.PixelOffsetMode = this.GetPixelOffsetMode();
            ShapeDrawType type = this.ForceShapeDrawType ? this.ForcedShapeDrawType : base.AppEnvironment.ShapeDrawType;
            this.points = this.TrimShapePath(this.points);
            if (this.AutoSnapAllPoints)
            {
                pointArray = base.SnapPoints(this.points);
            }
            else
            {
                pointArray = this.points.ToArrayEx<System.Windows.Point>();
            }
            PdnGraphicsPath disposeMe = this.CreateShapePath(pointArray);
            if (disposeMe != null)
            {
                this.renderArgs.Graphics.CompositingMode = base.AppEnvironment.GetCompositingMode();
                PdnGraphicsPath path2 = disposeMe.Clone();
                try
                {
                    path2.Widen(pen);
                }
                catch (OutOfMemoryException)
                {
                    path2 = new PdnGraphicsPath();
                    using (Matrix matrix = new Matrix())
                    {
                        matrix.Reset();
                        path2.AddRectangle(disposeMe.GetBounds(matrix, pen));
                    }
                }
                Rect rect = path2.GetBounds().ToWpfRect();
                rect.Inflate((double) 2.0, (double) 2.0);
                base.SaveRegion(null, rect.Int32Bound());
                this.saveRegion = new GeometryList();
                this.saveRegion.AddRect(rect);
                if ((type & ShapeDrawType.Outline) != 0)
                {
                    disposeMe.Draw(this.renderArgs.Graphics, pen);
                }
                if ((type & ShapeDrawType.Interior) != 0)
                {
                    this.renderArgs.Graphics.FillPath(brush, (GraphicsPath) disposeMe);
                }
                this.bitmapLayer.Invalidate(rect.Int32Bound());
                base.Update();
                DisposableUtil.Free<PdnGraphicsPath>(ref path2);
            }
            DisposableUtil.Free<PdnGraphicsPath>(ref disposeMe);
            pen.Dispose();
            brush.Dispose();
        }

        protected void SetShapePath(SegmentedList<System.Windows.Point> newPoints)
        {
            this.points = newPoints;
        }

        protected virtual SegmentedList<System.Windows.Point> TrimShapePath(SegmentedList<System.Windows.Point> trimThesePoints) => 
            trimThesePoints;

        protected bool AutoSnapAllPoints
        {
            get => 
                this.autoSnapAllPoints;
            set
            {
                this.autoSnapAllPoints = value;
            }
        }

        protected ShapeDrawType ForcedShapeDrawType
        {
            get => 
                this.forcedShapeDrawType;
            set
            {
                this.forcedShapeDrawType = value;
            }
        }

        protected bool ForceShapeDrawType
        {
            get => 
                this.forceShapeType;
            set
            {
                this.forceShapeType = value;
            }
        }

        protected bool UseDashStyle
        {
            get => 
                this.useDashStyle;
            set
            {
                this.useDashStyle = value;
            }
        }
    }
}

