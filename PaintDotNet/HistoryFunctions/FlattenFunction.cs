namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;
    using System.Collections.Generic;

    internal sealed class FlattenFunction : HistoryFunction
    {
        public FlattenFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            object state = null;
            List<HistoryMemento> actions = new List<HistoryMemento>();
            if (!historyWorkspace.Selection.IsEmpty)
            {
                state = historyWorkspace.Selection.Save();
                HistoryMemento memento = new DeselectFunction().Execute(historyWorkspace);
                actions.Add(memento);
            }
            ReplaceDocumentHistoryMemento item = new ReplaceDocumentHistoryMemento(null, null, historyWorkspace);
            actions.Add(item);
            CompoundHistoryMemento memento3 = new CompoundHistoryMemento(StaticName, PdnResources.GetImageResource2("Icons.MenuImageFlattenIcon.png"), actions);
            Document document = historyWorkspace.Document.Flatten();
            base.EnterCriticalRegion();
            historyWorkspace.Document = document;
            if (state != null)
            {
                SelectionHistoryMemento newHA = new SelectionHistoryMemento(null, null, historyWorkspace);
                historyWorkspace.Selection.Restore(state);
                memento3.PushNewAction(newHA);
            }
            return memento3;
        }

        public static string StaticName =>
            PdnResources.GetString2("FlattenFunction.Name");
    }
}

