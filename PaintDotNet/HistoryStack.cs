namespace PaintDotNet
{
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class HistoryStack
    {
        private DocumentWorkspace documentWorkspace;
        private int isExecutingMemento;
        private List<HistoryMemento> redoStack;
        private int stepGroupDepth;
        private List<HistoryMemento> undoStack;

        public event EventHandler Changed;

        public event EventHandler Changing;

        public event ExecutedHistoryMementoEventHandler ExecutedHistoryMemento;

        public event ExecutingHistoryMementoEventHandler ExecutingHistoryMemento;

        public event EventHandler FinishedStepGroup;

        public event EventHandler HistoryFlushed;

        public event EventHandler NewHistoryMemento;

        public event EventHandler SteppedBackward;

        public event EventHandler SteppedForward;

        public HistoryStack(DocumentWorkspace documentWorkspace)
        {
            this.documentWorkspace = documentWorkspace;
            this.undoStack = new List<HistoryMemento>();
            this.redoStack = new List<HistoryMemento>();
        }

        private HistoryStack(List<HistoryMemento> undoStack, List<HistoryMemento> redoStack)
        {
            this.undoStack = new List<HistoryMemento>(undoStack);
            this.redoStack = new List<HistoryMemento>(redoStack);
        }

        public void BeginStepGroup()
        {
            this.stepGroupDepth++;
        }

        public void ClearAll()
        {
            this.OnChanging();
            foreach (HistoryMemento memento in this.undoStack)
            {
                memento.Flush();
            }
            foreach (HistoryMemento memento2 in this.redoStack)
            {
                memento2.Flush();
            }
            this.undoStack = new List<HistoryMemento>();
            this.redoStack = new List<HistoryMemento>();
            this.OnChanged();
            this.OnHistoryFlushed();
        }

        public void ClearRedoStack()
        {
            foreach (HistoryMemento memento in this.redoStack)
            {
                memento.Flush();
            }
            this.OnChanging();
            this.redoStack = new List<HistoryMemento>();
            this.OnChanged();
        }

        public void EndStepGroup()
        {
            this.stepGroupDepth--;
            if (this.stepGroupDepth == 0)
            {
                this.OnFinishedStepGroup();
            }
        }

        private void OnChanged()
        {
            if (this.Changed != null)
            {
                this.Changed(this, EventArgs.Empty);
            }
        }

        private void OnChanging()
        {
            if (this.Changing != null)
            {
                this.Changing(this, EventArgs.Empty);
            }
        }

        private void OnExecutedHistoryMemento(ExecutedHistoryMementoEventArgs e)
        {
            if (this.ExecutedHistoryMemento != null)
            {
                this.ExecutedHistoryMemento(this, e);
            }
        }

        private void OnExecutingHistoryMemento(ExecutingHistoryMementoEventArgs e)
        {
            if (this.ExecutingHistoryMemento != null)
            {
                this.ExecutingHistoryMemento(this, e);
            }
        }

        private void OnFinishedStepGroup()
        {
            if (this.FinishedStepGroup != null)
            {
                this.FinishedStepGroup(this, EventArgs.Empty);
            }
        }

        private void OnHistoryFlushed()
        {
            if (this.HistoryFlushed != null)
            {
                this.HistoryFlushed(this, EventArgs.Empty);
            }
        }

        private void OnNewHistoryMemento()
        {
            if (this.NewHistoryMemento != null)
            {
                this.NewHistoryMemento(this, EventArgs.Empty);
            }
        }

        private void OnSteppedBackward()
        {
            if (this.SteppedBackward != null)
            {
                this.SteppedBackward(this, EventArgs.Empty);
            }
        }

        private void OnSteppedForward()
        {
            if (this.SteppedForward != null)
            {
                this.SteppedForward(this, EventArgs.Empty);
            }
        }

        public void PerformChanged()
        {
            this.OnChanged();
        }

        private void PopExecutingMemento()
        {
            this.isExecutingMemento--;
        }

        private void PushExecutingMemento()
        {
            this.isExecutingMemento++;
        }

        public void PushNewMemento(HistoryMemento value)
        {
            Utility.GCFullCollect();
            this.OnChanging();
            this.ClearRedoStack();
            this.undoStack.Add(value);
            this.OnNewHistoryMemento();
            this.OnChanged();
            value.Flush();
            Utility.GCFullCollect();
        }

        public void StepBackward(IWin32Window owner)
        {
            this.PushExecutingMemento();
            try
            {
                this.StepBackwardImpl(owner);
            }
            finally
            {
                this.PopExecutingMemento();
            }
        }

        private void StepBackwardImpl(IWin32Window owner)
        {
            HistoryMemento historyMemento = this.undoStack[this.undoStack.Count - 1];
            ToolHistoryMemento memento2 = historyMemento as ToolHistoryMemento;
            if ((memento2 != null) && (memento2.ToolType != this.documentWorkspace.GetToolType()))
            {
                this.documentWorkspace.SetToolFromType(memento2.ToolType);
                this.StepBackward(owner);
            }
            else
            {
                this.OnChanging();
                ExecutingHistoryMementoEventArgs e = new ExecutingHistoryMementoEventArgs(historyMemento, true, false);
                if ((memento2 == null) && (historyMemento.SeriesGuid == Guid.Empty))
                {
                    e.SuspendTool = true;
                }
                this.OnExecutingHistoryMemento(e);
                if (e.SuspendTool)
                {
                    this.documentWorkspace.PushNullTool();
                }
                HistoryMemento memento3 = this.undoStack[this.undoStack.Count - 1];
                ExecutingHistoryMementoEventArgs args2 = new ExecutingHistoryMementoEventArgs(memento3, false, e.SuspendTool);
                this.OnExecutingHistoryMemento(args2);
                HistoryMemento item = this.undoStack[this.undoStack.Count - 1].PerformUndo();
                this.undoStack.RemoveAt(this.undoStack.Count - 1);
                this.redoStack.Insert(0, item);
                ExecutedHistoryMementoEventArgs args3 = new ExecutedHistoryMementoEventArgs(item);
                this.OnExecutedHistoryMemento(args3);
                this.OnChanged();
                this.OnSteppedBackward();
                item.Flush();
                if (e.SuspendTool)
                {
                    this.documentWorkspace.PopNullTool();
                }
            }
            if (this.stepGroupDepth == 0)
            {
                this.OnFinishedStepGroup();
            }
        }

        public void StepForward(IWin32Window owner)
        {
            this.PushExecutingMemento();
            try
            {
                this.StepForwardImpl(owner);
            }
            finally
            {
                this.PopExecutingMemento();
            }
        }

        private void StepForwardImpl(IWin32Window owner)
        {
            HistoryMemento historyMemento = this.redoStack[0];
            ToolHistoryMemento memento2 = historyMemento as ToolHistoryMemento;
            if ((memento2 != null) && (memento2.ToolType != this.documentWorkspace.GetToolType()))
            {
                this.documentWorkspace.SetToolFromType(memento2.ToolType);
                this.StepForward(owner);
            }
            else
            {
                this.OnChanging();
                ExecutingHistoryMementoEventArgs e = new ExecutingHistoryMementoEventArgs(historyMemento, true, false);
                if ((memento2 == null) && (historyMemento.SeriesGuid != Guid.Empty))
                {
                    e.SuspendTool = true;
                }
                this.OnExecutingHistoryMemento(e);
                if (e.SuspendTool)
                {
                    this.documentWorkspace.PushNullTool();
                }
                HistoryMemento memento3 = this.redoStack[0];
                ExecutingHistoryMementoEventArgs args2 = new ExecutingHistoryMementoEventArgs(memento3, false, e.SuspendTool);
                this.OnExecutingHistoryMemento(args2);
                HistoryMemento item = memento3.PerformUndo();
                this.redoStack.RemoveAt(0);
                this.undoStack.Add(item);
                ExecutedHistoryMementoEventArgs args3 = new ExecutedHistoryMementoEventArgs(item);
                this.OnExecutedHistoryMemento(args3);
                this.OnChanged();
                this.OnSteppedForward();
                item.Flush();
                if (e.SuspendTool)
                {
                    this.documentWorkspace.PopNullTool();
                }
            }
            if (this.stepGroupDepth == 0)
            {
                this.OnFinishedStepGroup();
            }
        }

        public bool IsExecutingMemento =>
            (this.isExecutingMemento > 0);

        public List<HistoryMemento> RedoStack =>
            this.redoStack;

        public List<HistoryMemento> UndoStack =>
            this.undoStack;
    }
}

