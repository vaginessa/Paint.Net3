namespace PaintDotNet.HistoryMementos
{
    using System;

    [Serializable]
    internal abstract class HistoryMementoData : IDisposable
    {
        protected HistoryMementoData()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~HistoryMementoData()
        {
            this.Dispose(false);
        }
    }
}

