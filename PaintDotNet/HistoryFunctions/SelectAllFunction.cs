namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class SelectAllFunction : HistoryFunction
    {
        public SelectAllFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            SelectionHistoryMemento memento = new SelectionHistoryMemento(StaticName, PdnResources.GetImageResource2("Icons.MenuEditSelectAllIcon.png"), historyWorkspace);
            base.EnterCriticalRegion();
            historyWorkspace.Selection.PerformChanging();
            historyWorkspace.Selection.Reset();
            historyWorkspace.Selection.SetContinuation(historyWorkspace.Document.Bounds(), SelectionCombineMode.Replace);
            historyWorkspace.Selection.CommitContinuation();
            historyWorkspace.Selection.PerformChanged();
            return memento;
        }

        public static string StaticName =>
            PdnResources.GetString2("SelectAllAction.Name");
    }
}

