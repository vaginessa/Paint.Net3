﻿namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class ReplaceDocumentHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;

        public ReplaceDocumentHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            ReplaceDocumentHistoryMementoData data = new ReplaceDocumentHistoryMementoData(this.historyWorkspace.Document);
            base.Data = data;
        }

        protected override HistoryMemento OnUndo()
        {
            ReplaceDocumentHistoryMemento memento = new ReplaceDocumentHistoryMemento(base.Name, base.Image, this.historyWorkspace);
            this.historyWorkspace.Document = ((ReplaceDocumentHistoryMementoData) base.Data).OldDocument;
            return memento;
        }

        [Serializable]
        private sealed class ReplaceDocumentHistoryMementoData : HistoryMementoData
        {
            private Document oldDocument;

            public ReplaceDocumentHistoryMementoData(Document oldDocument)
            {
                this.oldDocument = oldDocument;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.oldDocument != null))
                {
                    this.oldDocument.Dispose();
                    this.oldDocument = null;
                }
            }

            public Document OldDocument =>
                this.oldDocument;
        }
    }
}

