namespace PaintDotNet
{
    using PaintDotNet.Functional;
    using PaintDotNet.IO;
    using PaintDotNet.SystemLayer;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Threading;

    internal sealed class SaveTransaction : IIsDisposed, IDisposable
    {
        private GuardedStream guardedStream;
        private KernelTransaction kernelTx;
        private string path;
        private SaveTransactionState state;
        private readonly object sync = new object();
        private string tempPath;

        public SaveTransaction(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            lock (this.sync)
            {
                this.state = SaveTransactionState.Initializing;
                try
                {
                    System.IO.Stream stream;
                    this.path = path;
                    if (File.Exists(this.path) && ((File.GetAttributes(this.path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
                    {
                        throw new UnauthorizedAccessException("Target path has the read only flag set");
                    }
                    if (!OS.IsVistaOrLater)
                    {
                        stream = null;
                    }
                    else
                    {
                        this.kernelTx = new KernelTransaction();
                        Result<FileStream> result = this.kernelTx.TryOpenFile(path, mode, access, share, options, 0x1000);
                        if (result.IsValue)
                        {
                            stream = result.Value;
                        }
                        else
                        {
                            result.Observe();
                            this.kernelTx.Rollback();
                            this.kernelTx = null;
                            stream = null;
                        }
                    }
                    if (stream == null)
                    {
                        string str = FindUniqueFileName(this.path + ".", ".pdnSave");
                        stream = new FileStream(str, mode, access, share, 0x1000, options);
                        this.tempPath = str;
                    }
                    this.guardedStream = new GuardedStream(stream, true, 0x400);
                    this.state = SaveTransactionState.Initialized;
                }
                catch (Exception)
                {
                    this.state = SaveTransactionState.FailedInitialization;
                    throw;
                }
            }
        }

        public void Commit()
        {
            lock (this.sync)
            {
                switch (this.state)
                {
                    case SaveTransactionState.Initializing:
                    case SaveTransactionState.FailedInitialization:
                    case SaveTransactionState.Committing:
                    case SaveTransactionState.FailedCommit:
                    case SaveTransactionState.Committed:
                    case SaveTransactionState.RollingBack:
                    case SaveTransactionState.FailedRollback:
                    case SaveTransactionState.RolledBack:
                        throw new InvalidOperationException($"This transaction is not in a state that allows it to be committed ({this.state})");

                    case SaveTransactionState.Initialized:
                        break;

                    case SaveTransactionState.Disposed:
                        throw new ObjectDisposedException("SaveTransaction");

                    default:
                        throw new InternalErrorException(new InvalidEnumArgumentException("this.state", (int) this.state, typeof(SaveTransactionState)));
                }
                this.state = SaveTransactionState.Committing;
                try
                {
                    DisposableUtil.Free<GuardedStream>(ref this.guardedStream);
                    if (this.kernelTx != null)
                    {
                        this.kernelTx.Commit();
                        this.kernelTx = null;
                        this.tempPath = null;
                    }
                    else
                    {
                        string destFileName = FindUniqueFileName(this.path + ".", ".pdnBak");
                        bool flag = File.Exists(this.path);
                        if (flag)
                        {
                            File.Move(this.path, destFileName);
                        }
                        File.Move(this.tempPath, this.path);
                        this.tempPath = null;
                        if (flag)
                        {
                            File.Delete(destFileName);
                        }
                    }
                    this.state = SaveTransactionState.Committed;
                }
                catch (Exception)
                {
                    this.state = SaveTransactionState.FailedCommit;
                    throw;
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
            if (disposing)
            {
                lock (this.sync)
                {
                    try
                    {
                        switch (this.state)
                        {
                            case SaveTransactionState.Initializing:
                            case SaveTransactionState.FailedInitialization:
                            case SaveTransactionState.Committing:
                            case SaveTransactionState.Committed:
                            case SaveTransactionState.RollingBack:
                            case SaveTransactionState.FailedRollback:
                            case SaveTransactionState.RolledBack:
                            case SaveTransactionState.Disposed:
                                return;

                            case SaveTransactionState.Initialized:
                            case SaveTransactionState.FailedCommit:
                                this.Rollback();
                                return;
                        }
                        throw new InternalErrorException(new InvalidEnumArgumentException("this.state", (int) this.state, typeof(SaveTransactionState)));
                    }
                    finally
                    {
                        this.state = SaveTransactionState.Disposed;
                    }
                }
            }
            else
            {
                switch (this.state)
                {
                }
                this.state = SaveTransactionState.Disposed;
            }
        }

        ~SaveTransaction()
        {
            this.Dispose(false);
        }

        private static string FindUniqueFileName(string pathPrefix, string pathSuffix)
        {
            for (int i = 0; i < 0x7fffffff; i++)
            {
                string str = i.ToString("X");
                string path = pathPrefix + str + pathSuffix;
                if (!File.Exists(path))
                {
                    return path;
                }
            }
            throw new IOException("Could not find a unique filename to fit the pattern, " + pathPrefix + "N" + pathSuffix);
        }

        public void Rollback()
        {
            object obj2;
            Monitor.Enter(obj2 = this.sync);
            try
            {
                switch (this.state)
                {
                    case SaveTransactionState.Initializing:
                    case SaveTransactionState.FailedInitialization:
                    case SaveTransactionState.Committing:
                    case SaveTransactionState.Committed:
                    case SaveTransactionState.RollingBack:
                    case SaveTransactionState.FailedRollback:
                    case SaveTransactionState.RolledBack:
                        throw new InvalidOperationException($"This transaction is not in a state that allows it to be committed ({this.state})");

                    case SaveTransactionState.Initialized:
                    case SaveTransactionState.FailedCommit:
                        break;

                    case SaveTransactionState.Disposed:
                        throw new ObjectDisposedException("SaveTransaction");

                    default:
                        throw new InternalErrorException(new InvalidEnumArgumentException("this.state", (int) this.state, typeof(SaveTransactionState)));
                }
                this.state = SaveTransactionState.RollingBack;
                try
                {
                    DisposableUtil.Free<GuardedStream>(ref this.guardedStream);
                }
                catch (IOException)
                {
                }
                if (this.kernelTx != null)
                {
                    this.kernelTx.Rollback();
                    this.kernelTx = null;
                }
                else if (this.tempPath != null)
                {
                    File.Delete(this.tempPath);
                }
                this.tempPath = null;
                this.state = SaveTransactionState.RolledBack;
            }
            catch (Exception)
            {
                this.state = SaveTransactionState.FailedRollback;
                throw;
            }
            finally
            {
                Monitor.Exit(obj2);
            }
        }

        public bool IsDisposed
        {
            get
            {
                lock (this.sync)
                {
                    return (this.state == SaveTransactionState.Disposed);
                }
            }
        }

        public SaveTransactionState State
        {
            get
            {
                lock (this.sync)
                {
                    return this.state;
                }
            }
        }

        public GuardedStream Stream =>
            this.guardedStream;
    }
}

