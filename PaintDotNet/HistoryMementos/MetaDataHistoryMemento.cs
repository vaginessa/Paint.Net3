namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;

    internal class MetaDataHistoryMemento : HistoryMemento
    {
        private IHistoryWorkspace historyWorkspace;

        public MetaDataHistoryMemento(string name, ImageResource image, IHistoryWorkspace historyWorkspace) : base(name, image)
        {
            this.historyWorkspace = historyWorkspace;
            Document document = new Document(1, 1);
            document.ReplaceMetaDataFrom(historyWorkspace.Document);
            MetaDataHistoryMementoData data = new MetaDataHistoryMementoData(document);
            base.Data = data;
        }

        protected override HistoryMemento OnUndo()
        {
            MetaDataHistoryMemento memento = new MetaDataHistoryMemento(base.Name, base.Image, this.historyWorkspace);
            MetaDataHistoryMementoData data = (MetaDataHistoryMementoData) base.Data;
            this.historyWorkspace.Document.ReplaceMetaDataFrom(data.Document);
            return memento;
        }

        [Serializable]
        private class MetaDataHistoryMementoData : HistoryMementoData
        {
            private PaintDotNet.Document document;

            public MetaDataHistoryMementoData(PaintDotNet.Document document)
            {
                this.document = document;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.document != null))
                {
                    this.document.Dispose();
                    this.document = null;
                }
                base.Dispose(disposing);
            }

            public PaintDotNet.Document Document =>
                this.document;
        }
    }
}

