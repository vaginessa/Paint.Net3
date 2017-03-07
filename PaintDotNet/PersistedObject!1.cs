namespace PaintDotNet
{
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class PersistedObject<T> : IDisposable
    {
        private IntPtr bstrTempFileName;
        private bool disposed;
        private static ArrayList fileNames;
        private WeakReference objectRef;
        private string tempFileName;
        private ManualResetEvent theObjectSaved;

        static PersistedObject()
        {
            PersistedObject<T>.fileNames = ArrayList.Synchronized(new ArrayList());
            Application.ApplicationExit += new EventHandler(PersistedObject<T>.Application_ApplicationExit);
        }

        public PersistedObject(T theObject, bool background)
        {
            this.bstrTempFileName = IntPtr.Zero;
            this.theObjectSaved = new ManualResetEvent(false);
            this.objectRef = new WeakReference(theObject);
            this.tempFileName = FileSystem.GetTempFileName();
            PersistedObject<T>.fileNames.Add(this.tempFileName);
            this.bstrTempFileName = Marshal.StringToBSTR(this.tempFileName);
            if (background)
            {
                new Thread(new ParameterizedThreadStart(this.PersistToDiskThread)).Start(theObject);
            }
            else
            {
                this.PersistToDisk(theObject);
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            string[] fileNames = PersistedObject<T>.FileNames;
            if (fileNames.Length != 0)
            {
                foreach (string str in fileNames)
                {
                    FileInfo info = new FileInfo(str);
                    if (info.Exists)
                    {
                        FileSystem.TryDeleteFile(info.FullName);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (disposing)
                {
                    this.WaitForObjectSaved(0x3e8);
                }
                string fileName = Marshal.PtrToStringBSTR(this.bstrTempFileName);
                FileInfo info = new FileInfo(fileName);
                if (info.Exists)
                {
                    FileSystem.TryDeleteFile(info.FullName);
                    try
                    {
                        PersistedObject<T>.fileNames.Remove(fileName);
                    }
                    catch
                    {
                    }
                }
                Marshal.FreeBSTR(this.bstrTempFileName);
                this.bstrTempFileName = IntPtr.Zero;
                if (disposing)
                {
                    ManualResetEvent theObjectSaved = this.theObjectSaved;
                    this.theObjectSaved = null;
                    if (theObjectSaved != null)
                    {
                        theObjectSaved.Close();
                        theObjectSaved = null;
                    }
                }
            }
        }

        ~PersistedObject()
        {
            this.Dispose(false);
        }

        public void Flush()
        {
            this.WaitForObjectSaved();
            object weakObject = this.WeakObject;
            IDisposable disposable = weakObject as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
            this.objectRef = null;
        }

        private void PersistToDisk(object theObject)
        {
            try
            {
                FileStream serializationStream = new FileStream(this.tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                BinaryFormatter formatter = new BinaryFormatter();
                DeferredFormatter additional = new DeferredFormatter(null);
                StreamingContext context = new StreamingContext(formatter.Context.State, additional);
                formatter.Context = context;
                formatter.Serialize(serializationStream, theObject);
                additional.FinishSerialization(serializationStream);
                serializationStream.Flush();
                serializationStream.Close();
            }
            finally
            {
                this.theObjectSaved.Set();
                this.theObjectSaved = null;
            }
        }

        private void PersistToDiskThread(object theObject)
        {
            using (new ThreadBackground(ThreadBackgroundFlags.Cpu))
            {
                this.PersistToDisk(theObject);
            }
        }

        private void WaitForObjectSaved()
        {
            ManualResetEvent theObjectSaved = this.theObjectSaved;
            if (theObjectSaved != null)
            {
                theObjectSaved.WaitOne();
            }
        }

        private void WaitForObjectSaved(int timeoutMs)
        {
            ManualResetEvent theObjectSaved = this.theObjectSaved;
            if (theObjectSaved != null)
            {
                theObjectSaved.WaitOne(timeoutMs, false);
            }
        }

        public static string[] FileNames =>
            ((string[]) PersistedObject<T>.fileNames.ToArray(typeof(string)));

        public T Object
        {
            get
            {
                T target;
                if (this.disposed)
                {
                    throw new ObjectDisposedException("PersistedObject");
                }
                if (this.objectRef == null)
                {
                    target = default(T);
                }
                else
                {
                    target = (T) this.objectRef.Target;
                }
                if (target == null)
                {
                    FileStream serializationStream = new FileStream(Marshal.PtrToStringBSTR(this.bstrTempFileName), FileMode.Open, FileAccess.Read, FileShare.Read);
                    BinaryFormatter formatter = new BinaryFormatter();
                    DeferredFormatter additional = new DeferredFormatter();
                    StreamingContext context = new StreamingContext(formatter.Context.State, additional);
                    formatter.Context = context;
                    T local2 = (T) formatter.Deserialize(serializationStream);
                    additional.FinishDeserialization(serializationStream);
                    this.objectRef = new WeakReference(local2);
                    serializationStream.Close();
                    return local2;
                }
                return target;
            }
        }

        public T WeakObject
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException("PersistedObject");
                }
                if (this.objectRef == null)
                {
                    return default(T);
                }
                return (T) this.objectRef.Target;
            }
        }
    }
}

