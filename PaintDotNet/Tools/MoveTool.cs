namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.Serialization;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;

    internal sealed class MoveTool : MoveToolBase
    {
        private BitmapLayer activeLayer;
        private bool didPaste;
        private bool fullQuality;
        private RenderArgs renderArgs;

        public MoveTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.MoveToolIcon.png"), StaticName, PdnResources.GetString2("MoveTool.HelpText"), 'm', false, ToolBarConfigItems.None | ToolBarConfigItems.Resampling)
        {
            base.context = new MoveToolContext();
            base.enableOutline = false;
        }

        private void AppEnvironment_ResamplingAlgorithmChanged(object sender, EventArgs e)
        {
            if (this.ourContext.LiftedPixels != null)
            {
                bool fullQuality = this.fullQuality;
                this.fullQuality = true;
                this.PreRender();
                this.Render((System.Drawing.Point) base.context.offset, true);
                base.Update();
                this.fullQuality = fullQuality;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                base.DestroyNubs();
                if (this.renderArgs != null)
                {
                    this.renderArgs.Dispose();
                    this.renderArgs = null;
                }
                if (base.context != null)
                {
                    base.context.Dispose();
                    base.context = null;
                }
            }
        }

        protected override void Drop()
        {
            base.RestoreSavedRegion();
            GeometryList changedRegion = base.Selection.CreateGeometryList();
            HistoryMemento item = new BitmapHistoryMemento(base.Name, base.Image, base.DocumentWorkspace, base.ActiveLayerIndex, changedRegion);
            bool fullQuality = this.fullQuality;
            this.fullQuality = true;
            this.Render((System.Drawing.Point) base.context.offset, true);
            this.fullQuality = fullQuality;
            base.currentHistoryMementos.Add(item);
            this.activeLayer.Invalidate(changedRegion);
            base.Update();
            changedRegion.Dispose();
            changedRegion = null;
            ContextHistoryMemento memento2 = new ContextHistoryMemento(base.DocumentWorkspace, this.ourContext, base.Name, base.Image);
            base.currentHistoryMementos.Add(memento2);
            if (this.didPaste)
            {
                string localizedName = EnumLocalizer.GetLocalizedEnumValue(typeof(CommonAction), CommonAction.Paste).LocalizedName;
                PdnResources.GetImageResource2("Icons.MenuEditPasteIcon.png");
            }
            else
            {
                string name = base.Name;
                ImageResource image = base.Image;
            }
            this.didPaste = false;
            SelectionHistoryMemento memento3 = new SelectionHistoryMemento(base.Name, base.Image, base.DocumentWorkspace);
            base.currentHistoryMementos.Add(memento3);
            base.context.Dispose();
            base.context = new MoveToolContext();
            this.FlushHistoryMementos(PdnResources.GetString2("MoveTool.HistoryMemento.DropPixels"));
        }

        private void FlushHistoryMementos(string name)
        {
            if (base.currentHistoryMementos.Count > 0)
            {
                string str;
                ImageResource image;
                CompoundHistoryMemento chm = new CompoundHistoryMemento(null, null, base.currentHistoryMementos.ToArrayEx<HistoryMemento>());
                if (this.didPaste)
                {
                    str = PdnResources.GetString2("CommonAction.Paste");
                    image = PdnResources.GetImageResource2("Icons.MenuEditPasteIcon.png");
                    this.didPaste = false;
                }
                else
                {
                    if (name == null)
                    {
                        str = base.Name;
                    }
                    else
                    {
                        str = name;
                    }
                    image = base.Image;
                }
                MoveToolBase.CompoundToolHistoryMemento memento2 = new MoveToolBase.CompoundToolHistoryMemento(chm, base.DocumentWorkspace, str, image) {
                    SeriesGuid = base.context.seriesGuid
                };
                base.HistoryStack.PushNewMemento(memento2);
                base.currentHistoryMementos.Clear();
            }
        }

        protected override void OnActivate()
        {
            base.AppEnvironment.ResamplingAlgorithmChanged += new EventHandler(this.AppEnvironment_ResamplingAlgorithmChanged);
            base.moveToolCursor = PdnResources.GetCursor2("Cursors.MoveToolCursor.cur");
            base.Cursor = base.moveToolCursor;
            base.context.lifted = false;
            this.ourContext.LiftedPixels = null;
            base.context.offset = new System.Drawing.Point(0, 0);
            base.context.liftedBounds = base.Selection.GetBoundsF();
            this.activeLayer = (BitmapLayer) base.ActiveLayer;
            if (this.renderArgs != null)
            {
                this.renderArgs.Dispose();
                this.renderArgs = null;
            }
            if (this.activeLayer == null)
            {
                this.renderArgs = null;
            }
            else
            {
                this.renderArgs = new RenderArgs(this.activeLayer.Surface);
            }
            base.tracking = false;
            base.PositionNubs(base.context.currentMode);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.AppEnvironment.ResamplingAlgorithmChanged -= new EventHandler(this.AppEnvironment_ResamplingAlgorithmChanged);
            if (base.moveToolCursor != null)
            {
                base.moveToolCursor.Dispose();
                base.moveToolCursor = null;
            }
            if (base.context.lifted)
            {
                this.Drop();
            }
            this.activeLayer = null;
            if (this.renderArgs != null)
            {
                this.renderArgs.Dispose();
                this.renderArgs = null;
            }
            base.tracking = false;
            base.DestroyNubs();
            base.OnDeactivate();
        }

        protected override void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
            if (base.context.lifted)
            {
                bool fullQuality = this.fullQuality;
                this.fullQuality = false;
                this.Render((System.Drawing.Point) base.context.offset, true);
                base.ClearSavedMemory();
                this.fullQuality = fullQuality;
            }
            else
            {
                base.DestroyNubs();
                base.PositionNubs(base.context.currentMode);
            }
            base.dontDrop = false;
        }

        protected override void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
            base.dontDrop = true;
            base.RestoreSavedRegion();
            base.ClearSavedMemory();
            if (e.MayAlterSuspendTool)
            {
                e.SuspendTool = false;
            }
        }

        protected override void OnFinishedHistoryStepGroup()
        {
            if (base.context.lifted)
            {
                bool fullQuality = this.fullQuality;
                this.fullQuality = true;
                this.Render((System.Drawing.Point) base.context.offset, true, false);
                this.fullQuality = fullQuality;
            }
            base.OnFinishedHistoryStepGroup();
        }

        protected override void OnLift(MouseEventArgsF e)
        {
            GeometryList geometryMask = base.Selection.CreateGeometryList();
            this.ourContext.LiftedPixels = new MaskedSurface(this.activeLayer.Surface, geometryMask);
            HistoryMemento item = new BitmapHistoryMemento(base.Name, base.Image, base.DocumentWorkspace, base.ActiveLayerIndex, this.ourContext.poLiftedPixelsGuid);
            base.currentHistoryMementos.Add(item);
            if ((base.ModifierKeys & Keys.Control) == Keys.None)
            {
                ColorBgra secondaryColor = base.AppEnvironment.SecondaryColor;
                secondaryColor.A = 0;
                UnaryPixelOp op = new UnaryPixelOps.Constant(secondaryColor);
                using (GeometryList list2 = GeometryList.ClipToRect(geometryMask, base.Document.Bounds()))
                {
                    Int32Rect[] interiorScans = list2.GetInteriorScans();
                    op.Apply(this.renderArgs.Surface, interiorScans);
                    this.activeLayer.Invalidate(list2);
                }
            }
            DisposableUtil.Free<GeometryList>(ref geometryMask);
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            if (base.tracking)
            {
                string str;
                this.fullQuality = true;
                this.OnMouseMove(e);
                this.fullQuality = false;
                base.rotateNub.Visible = false;
                base.tracking = false;
                base.PositionNubs(base.context.currentMode);
                switch (base.context.currentMode)
                {
                    case MoveToolBase.Mode.Translate:
                        str = "MoveTool.HistoryMemento.Translate";
                        break;

                    case MoveToolBase.Mode.Scale:
                        str = "MoveTool.HistoryMemento.Scale";
                        break;

                    case MoveToolBase.Mode.Rotate:
                        str = "MoveTool.HistoryMemento.Rotate";
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }
                base.context.startAngle += base.angleDelta;
                base.context.liftTransform = Matrix.Identity;
                base.context.liftTransform = Matrix.Multiply(base.context.liftTransform, base.context.deltaTransform);
                string name = PdnResources.GetString2(str);
                this.FlushHistoryMementos(name);
            }
        }

        protected override void OnSelectionChanged()
        {
            if (!base.context.lifted)
            {
                base.DestroyNubs();
                base.PositionNubs(base.context.currentMode);
            }
            base.OnSelectionChanged();
        }

        protected override void OnSelectionChanging()
        {
            base.OnSelectionChanging();
            if (!base.dontDrop)
            {
                if (base.context.lifted)
                {
                    this.Drop();
                }
                if (base.tracking)
                {
                    base.tracking = false;
                }
            }
        }

        public void PasteMouseDown(MaskedSurface pixels, Int32Point offset)
        {
            if (base.context.lifted)
            {
                this.Drop();
            }
            GeometryList geometryMaskCopy = pixels.GetGeometryMaskCopy();
            HistoryMemento item = new BitmapHistoryMemento(base.Name, base.Image, base.DocumentWorkspace, base.ActiveLayerIndex, geometryMaskCopy);
            base.currentHistoryMementos.Add(item);
            this.PushContextHistoryMemento();
            base.context.seriesGuid = Guid.NewGuid();
            base.context.currentMode = MoveToolBase.Mode.Translate;
            base.context.startEdge = MoveToolBase.Edge.None;
            base.context.startAngle = 0.0;
            this.ourContext.LiftedPixels = pixels;
            base.context.lifted = true;
            base.context.liftTransform = Matrix.Identity;
            base.context.deltaTransform = Matrix.Identity;
            base.context.offset = new System.Drawing.Point(0, 0);
            bool dontDrop = base.dontDrop;
            base.dontDrop = true;
            SelectionHistoryMemento memento2 = new SelectionHistoryMemento(null, null, base.DocumentWorkspace);
            base.currentHistoryMementos.Add(memento2);
            base.Selection.PerformChanging();
            base.Selection.Reset();
            base.Selection.SetContinuation(ref geometryMaskCopy, true, SelectionCombineMode.Replace);
            base.Selection.CommitContinuation();
            base.Selection.PerformChanged();
            this.PushContextHistoryMemento();
            base.context.liftedBounds = base.Selection.GetBoundsF(false);
            base.context.startBounds = base.context.liftedBounds;
            base.context.baseTransform = new Matrix?(Matrix.Identity);
            base.tracking = true;
            base.dontDrop = dontDrop;
            this.didPaste = true;
            base.tracking = true;
            base.DestroyNubs();
            base.PositionNubs(base.context.currentMode);
            MouseEventArgsF e = new MouseEventArgsF(MouseButtons.Left, 0, 70000.0, 70000.0, 0);
            MouseEventArgsF sf2 = new MouseEventArgsF(MouseButtons.Left, 0, (double) (0x11170 + offset.X), (double) (0x11170 + offset.Y), 0);
            base.context.startMouseXY = new Int32Point(0x11170, 0x11170);
            this.OnMouseDown(e);
            this.OnMouseUp(sf2);
        }

        protected override void PreRender()
        {
            base.RestoreSavedRegion();
        }

        protected override void PushContextHistoryMemento()
        {
            ContextHistoryMemento item = new ContextHistoryMemento(base.DocumentWorkspace, this.ourContext, null, null);
            base.currentHistoryMementos.Add(item);
        }

        protected override void Render(System.Drawing.Point newOffset, bool useNewOffset)
        {
            this.Render(newOffset, useNewOffset, true);
        }

        private void Render(System.Drawing.Point newOffset, bool useNewOffset, bool saveRegion)
        {
            Int32Rect bounds = base.Selection.GetBounds();
            GeometryList saveMeGeometry = base.Selection.CreateGeometryList();
            if (saveRegion)
            {
                base.SaveRegion(saveMeGeometry, bounds);
            }
            WaitCursorChanger changer = null;
            if (this.fullQuality && (base.AppEnvironment.ResamplingAlgorithm == ResamplingAlgorithm.Bilinear))
            {
                changer = new WaitCursorChanger(base.DocumentWorkspace);
            }
            this.ourContext.LiftedPixels.Draw(this.renderArgs.Surface, base.context.deltaTransform, this.fullQuality ? base.AppEnvironment.ResamplingAlgorithm : ResamplingAlgorithm.NearestNeighbor);
            if (changer != null)
            {
                changer.Dispose();
                changer = null;
            }
            this.activeLayer.Invalidate(saveMeGeometry);
            base.PositionNubs(base.context.currentMode);
            DisposableUtil.Free<GeometryList>(ref saveMeGeometry);
        }

        private MoveToolContext ourContext =>
            ((MoveToolContext) base.context);

        public static string StaticName =>
            PdnResources.GetString2("MoveTool.Name");

        private class ContextHistoryMemento : ToolHistoryMemento
        {
            private int layerIndex;
            private object liftedPixelsRef;

            public ContextHistoryMemento(DocumentWorkspace documentWorkspace, MoveTool.MoveToolContext context, string name, ImageResource image) : base(documentWorkspace, name, image)
            {
                base.Data = new OurContextHistoryMementoData(context);
                this.layerIndex = base.DocumentWorkspace.ActiveLayerIndex;
                this.liftedPixelsRef = context.poLiftedPixels;
            }

            protected override HistoryMemento OnToolUndo()
            {
                MoveTool tool = base.DocumentWorkspace.Tool as MoveTool;
                if (tool == null)
                {
                    throw new InvalidOperationException("Current Tool is not the MoveTool");
                }
                MoveTool.ContextHistoryMemento memento = new MoveTool.ContextHistoryMemento(base.DocumentWorkspace, tool.ourContext, base.Name, base.Image);
                OurContextHistoryMementoData data = (OurContextHistoryMementoData) base.Data;
                MoveToolBase.Context context = data.context;
                if (tool.ActiveLayerIndex != this.layerIndex)
                {
                    bool deactivateOnLayerChange = tool.deactivateOnLayerChange;
                    tool.deactivateOnLayerChange = false;
                    tool.ActiveLayerIndex = this.layerIndex;
                    tool.deactivateOnLayerChange = deactivateOnLayerChange;
                    tool.activeLayer = (BitmapLayer) tool.ActiveLayer;
                    tool.renderArgs = new RenderArgs(tool.activeLayer.Surface);
                    tool.ClearSavedMemory();
                }
                tool.context.Dispose();
                tool.context = context;
                tool.DestroyNubs();
                if (tool.context.lifted)
                {
                    tool.PositionNubs(tool.context.currentMode);
                }
                return memento;
            }

            [Serializable]
            private class OurContextHistoryMementoData : HistoryMementoData
            {
                public MoveTool.MoveToolContext context;

                public OurContextHistoryMementoData(MoveToolBase.Context context)
                {
                    this.context = (MoveTool.MoveToolContext) context.Clone();
                }
            }
        }

        [Serializable]
        private sealed class MoveToolContext : MoveToolBase.Context
        {
            [NonSerialized]
            private MaskedSurface liftedPixels;
            [NonSerialized]
            public PersistedObject<MaskedSurface> poLiftedPixels;
            public Guid poLiftedPixelsGuid;

            public MoveToolContext()
            {
            }

            public MoveToolContext(MoveTool.MoveToolContext cloneMe) : base(cloneMe)
            {
                this.poLiftedPixelsGuid = cloneMe.poLiftedPixelsGuid;
                this.poLiftedPixels = cloneMe.poLiftedPixels;
                this.liftedPixels = cloneMe.liftedPixels;
            }

            public MoveToolContext(SerializationInfo info, StreamingContext context) : base(info, context)
            {
                this.poLiftedPixelsGuid = (Guid) info.GetValue("poLiftedPixelsGuid", typeof(Guid));
                this.poLiftedPixels = PersistedObjectLocker.Get<MaskedSurface>(this.poLiftedPixelsGuid);
            }

            public override object Clone() => 
                new MoveTool.MoveToolContext(this);

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue("poLiftedPixelsGuid", this.poLiftedPixelsGuid);
            }

            public MaskedSurface LiftedPixels
            {
                get
                {
                    if ((this.liftedPixels == null) && (this.poLiftedPixels != null))
                    {
                        this.liftedPixels = this.poLiftedPixels.Object;
                    }
                    return this.liftedPixels;
                }
                set
                {
                    if (value == null)
                    {
                        this.poLiftedPixels = null;
                        this.liftedPixels = null;
                    }
                    else
                    {
                        this.poLiftedPixels = new PersistedObject<MaskedSurface>(value, true);
                        this.poLiftedPixelsGuid = PersistedObjectLocker.Add<MaskedSurface>(this.poLiftedPixels);
                        this.liftedPixels = null;
                    }
                }
            }
        }
    }
}

