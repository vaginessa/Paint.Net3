namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class HistoryFastForwardAction : DocumentWorkspaceAction
    {
        public HistoryFastForwardAction() : base(ActionFlags.KeepToolActive)
        {
        }

        public override HistoryMemento PerformAction(DocumentWorkspace documentWorkspace)
        {
            DateTime now = DateTime.Now;
            documentWorkspace.History.BeginStepGroup();
            using (new WaitCursorChanger(documentWorkspace))
            {
                documentWorkspace.SuspendToolCursorChanges();
                while (documentWorkspace.History.RedoStack.Count > 0)
                {
                    documentWorkspace.History.StepForward(documentWorkspace);
                    Utility.GCFullCollect();
                    TimeSpan span = (TimeSpan) (DateTime.Now - now);
                    if (span.TotalMilliseconds >= 500.0)
                    {
                        documentWorkspace.History.EndStepGroup();
                        documentWorkspace.Update();
                        now = DateTime.Now;
                        documentWorkspace.History.BeginStepGroup();
                    }
                }
                documentWorkspace.ResumeToolCursorChanges();
            }
            documentWorkspace.History.EndStepGroup();
            Utility.GCFullCollect();
            documentWorkspace.Document.Invalidate();
            documentWorkspace.Update();
            return null;
        }
    }
}

