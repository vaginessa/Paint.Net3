namespace PaintDotNet.HistoryFunctions
{
    using PaintDotNet;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using System;

    internal sealed class InvertSelectionFunction : HistoryFunction
    {
        public InvertSelectionFunction() : base(ActionFlags.None)
        {
        }

        public override HistoryMemento OnExecute(IHistoryWorkspace historyWorkspace)
        {
            if (historyWorkspace.Selection.IsEmpty)
            {
                return null;
            }
            SelectionHistoryMemento memento = new SelectionHistoryMemento(StaticName, StaticImage, historyWorkspace);
            GeometryList disposeMe = historyWorkspace.Selection.CreateGeometryList();
            GeometryList rhs = GeometryList.FromNonOverlappingScans(disposeMe.GetInteriorScansUnsafeList());
            GeometryList lhs = new GeometryList(historyWorkspace.Document.Bounds());
            GeometryList geometry = GeometryList.Combine(lhs, GeometryCombineMode.Exclude, rhs);
            DisposableUtil.Free<GeometryList>(ref rhs);
            DisposableUtil.Free<GeometryList>(ref lhs);
            base.EnterCriticalRegion();
            historyWorkspace.Selection.PerformChanging();
            historyWorkspace.Selection.Reset();
            historyWorkspace.Selection.SetContinuation(ref geometry, true, SelectionCombineMode.Replace);
            historyWorkspace.Selection.CommitContinuation();
            historyWorkspace.Selection.PerformChanged();
            DisposableUtil.Free<GeometryList>(ref disposeMe);
            return memento;
        }

        public static ImageResource StaticImage =>
            PdnResources.GetImageResource2("Icons.MenuEditInvertSelectionIcon.png");

        public static string StaticName =>
            PdnResources.GetString2("InvertSelectionAction.Name");
    }
}

