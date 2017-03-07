namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using System;
    using System.ComponentModel;
    using System.Drawing;

    internal sealed class MoveSelectionTool : MoveToolBase
    {
        public MoveSelectionTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource2("Icons.MoveSelectionToolIcon.png"), StaticName, PdnResources.GetString2("MoveSelectionTool.HelpText"), 'm', false, ToolBarConfigItems.None)
        {
            base.context = new MoveToolBase.Context();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                base.DestroyNubs();
                if (base.context != null)
                {
                    base.context.Dispose();
                    base.context = null;
                }
            }
        }

        protected override void Drop()
        {
            ContextHistoryMemento item = new ContextHistoryMemento(base.DocumentWorkspace, base.context, base.Name, base.Image);
            base.currentHistoryMementos.Add(item);
            SelectionHistoryMemento memento2 = new SelectionHistoryMemento(base.Name, base.Image, base.DocumentWorkspace);
            base.currentHistoryMementos.Add(memento2);
            base.context.Dispose();
            base.context = new MoveToolBase.Context();
            this.FlushHistoryMementos(PdnResources.GetString2("MoveSelectionTool.HistoryMemento.DropSelection"));
        }

        private void FlushHistoryMementos(string name)
        {
            if (base.currentHistoryMementos.Count > 0)
            {
                string str;
                CompoundHistoryMemento chm = new CompoundHistoryMemento(null, null, base.currentHistoryMementos.ToArrayEx<HistoryMemento>());
                if (name == null)
                {
                    str = base.Name;
                }
                else
                {
                    str = name;
                }
                ImageResource image = base.Image;
                MoveToolBase.CompoundToolHistoryMemento memento2 = new MoveToolBase.CompoundToolHistoryMemento(chm, base.DocumentWorkspace, str, image) {
                    SeriesGuid = base.context.seriesGuid
                };
                base.HistoryStack.PushNewMemento(memento2);
                base.currentHistoryMementos.Clear();
            }
        }

        protected override void OnActivate()
        {
            base.DocumentWorkspace.EnableSelectionTinting = true;
            base.moveToolCursor = PdnResources.GetCursor2("Cursors.MoveSelectionToolCursor.cur");
            base.Cursor = base.moveToolCursor;
            base.context.offset = new Point(0, 0);
            base.context.liftedBounds = base.Selection.GetBoundsF();
            base.tracking = false;
            base.PositionNubs(base.context.currentMode);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.DocumentWorkspace.EnableSelectionTinting = false;
            if (base.moveToolCursor != null)
            {
                base.moveToolCursor.Dispose();
                base.moveToolCursor = null;
            }
            if (base.context.lifted)
            {
                this.Drop();
            }
            base.tracking = false;
            base.DestroyNubs();
            base.OnDeactivate();
        }

        protected override void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
            if (base.context.lifted)
            {
                this.Render((Point) base.context.offset, true);
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
            if (e.MayAlterSuspendTool)
            {
                e.SuspendTool = false;
            }
        }

        protected override void OnLift(MouseEventArgsF e)
        {
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            base.OnMouseUp(e);
            if (base.tracking)
            {
                string str;
                this.OnMouseMove(e);
                base.rotateNub.Visible = false;
                base.tracking = false;
                base.PositionNubs(base.context.currentMode);
                switch (base.context.currentMode)
                {
                    case MoveToolBase.Mode.Translate:
                        str = "MoveSelectionTool.HistoryMemento.Translate";
                        break;

                    case MoveToolBase.Mode.Scale:
                        str = "MoveSelectionTool.HistoryMemento.Scale";
                        break;

                    case MoveToolBase.Mode.Rotate:
                        str = "MoveSelectionTool.HistoryMemento.Rotate";
                        break;

                    default:
                        throw new InvalidEnumArgumentException();
                }
                base.context.startAngle += base.angleDelta;
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

        protected override void PreRender()
        {
        }

        protected override void PushContextHistoryMemento()
        {
            ContextHistoryMemento item = new ContextHistoryMemento(base.DocumentWorkspace, base.context, null, null);
            base.currentHistoryMementos.Add(item);
        }

        protected override void Render(Point newOffset, bool useNewOffset)
        {
            base.PositionNubs(base.context.currentMode);
        }

        public static string StaticName =>
            PdnResources.GetString2("MoveSelectionTool.Name");

        private class ContextHistoryMemento : ToolHistoryMemento
        {
            public ContextHistoryMemento(DocumentWorkspace documentWorkspace, MoveToolBase.Context context, string name, ImageResource image) : base(documentWorkspace, name, image)
            {
                base.Data = new OurHistoryMementoData(context);
            }

            protected override HistoryMemento OnToolUndo()
            {
                MoveSelectionTool tool = base.DocumentWorkspace.Tool as MoveSelectionTool;
                if (tool == null)
                {
                    throw new InvalidOperationException("Current Tool is not the MoveSelectionTool");
                }
                MoveSelectionTool.ContextHistoryMemento memento = new MoveSelectionTool.ContextHistoryMemento(base.DocumentWorkspace, tool.context, base.Name, base.Image);
                OurHistoryMementoData data = (OurHistoryMementoData) base.Data;
                MoveToolBase.Context context = data.context;
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
            private class OurHistoryMementoData : HistoryMementoData
            {
                public MoveToolBase.Context context;

                public OurHistoryMementoData(MoveToolBase.Context context)
                {
                    this.context = (MoveToolBase.Context) context.Clone();
                }
            }
        }
    }
}

