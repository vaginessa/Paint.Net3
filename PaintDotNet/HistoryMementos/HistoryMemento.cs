namespace PaintDotNet.HistoryMementos
{
    using PaintDotNet;
    using System;
    using System.Threading;

    internal abstract class HistoryMemento
    {
        private PersistedObject<HistoryMementoData> historyMementoData;
        protected int id;
        private ImageResource image;
        private string name;
        private static int nextId;
        private Guid seriesGuid = Guid.Empty;

        public HistoryMemento(string name, ImageResource image)
        {
            this.name = name;
            this.image = image;
            this.id = Interlocked.Increment(ref nextId);
        }

        public void Flush()
        {
            if (this.historyMementoData != null)
            {
                this.historyMementoData.Flush();
            }
            this.OnFlush();
        }

        protected virtual void OnFlush()
        {
        }

        protected abstract HistoryMemento OnUndo();
        public HistoryMemento PerformUndo()
        {
            HistoryMemento memento = this.OnUndo();
            memento.ID = this.ID;
            memento.SeriesGuid = this.SeriesGuid;
            return memento;
        }

        protected HistoryMementoData Data
        {
            get => 
                this.historyMementoData?.Object;
            set
            {
                this.historyMementoData = new PersistedObject<HistoryMementoData>(value, false);
            }
        }

        public int ID
        {
            get => 
                this.id;
            set
            {
                this.id = value;
            }
        }

        public ImageResource Image
        {
            get => 
                this.image;
            set
            {
                this.image = value;
            }
        }

        public string Name
        {
            get => 
                this.name;
            set
            {
                this.name = value;
            }
        }

        public Guid SeriesGuid
        {
            get => 
                this.seriesGuid;
            set
            {
                this.seriesGuid = value;
            }
        }
    }
}

