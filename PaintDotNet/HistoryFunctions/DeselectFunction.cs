namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using System;

    internal sealed class DeselectFunction : HistoryFunction
    {
        public DeselectFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }
            SelectionHistoryMemento memento = new SelectionHistoryMemento(StaticName, StaticImage, historyWorkspace);
            base.EnterCriticalRegion();
            historyWorkspace.Selection.Reset();
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuEditDeselectIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("DeselectAction.Name");
    }
}

