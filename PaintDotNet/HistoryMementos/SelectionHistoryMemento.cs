﻿namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal sealed class SelectionHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;

        public SelectionHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace) : this(name, image, historyWorkspace, historyWorkspace.Selection.Save())
        {
        }

        public SelectionHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace, object selectionData) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            base.Data = new SelectionHistoryMementoData(selectionData);
        }

        protected override HistoryMemento OnUndo()
        {
            SelectionHistoryMemento memento = new SelectionHistoryMemento(base.Name, base.Image, this.historyWorkspace);
            SelectionHistoryMementoData data = (SelectionHistoryMementoData) base.Data;
            object savedSelectionData = data.SavedSelectionData;
            this.historyWorkspace.Selection.Restore(savedSelectionData);
            return memento;
        }

        [Serializable]
        private sealed class SelectionHistoryMementoData : HistoryMementoData
        {
            private object savedSelectionData;

            public SelectionHistoryMementoData(object savedSelectionData)
            {
                this.savedSelectionData = savedSelectionData;
            }

            protected override void Dispose(bool disposing)
            {
                this.savedSelectionData = null;
                base.Dispose(disposing);
            }

            public object SavedSelectionData =>
                this.savedSelectionData;
        }
    }
}

