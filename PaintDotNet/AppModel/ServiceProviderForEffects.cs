namespace PaintDotNet.AppModel
{
    using PaintDotNet;
    using System;
    using System.Collections.Generic;

    internal sealed class ServiceProviderForEffects : MarshalByRefObject, IServiceProvider, IIsDisposed, IDisposable
    {
        private bool disposed;
        private Dictionary<Type, object> serviceMap = new Dictionary<Type, object>();
        private object sync = new object();

        private object CreateService(Type serviceType)
        {
            if (serviceType == typeof(IAppInfoService))
            {
                return new AppInfoService();
            }
            if (serviceType != typeof(IShellService))
            {
                throw new KeyNotFoundException();
            }
            return new ShellService();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            lock (this.sync)
            {
                if (this.serviceMap != null)
                {
                    foreach (object obj2 in this.serviceMap.Values)
                    {
                        IDisposable disposable = obj2 as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    this.serviceMap.Clear();
                }
            }
            this.disposed = true;
        }

        public object GetService(Type serviceType)
        {
            this.VerifyNotDisposed();
            object obj2 = null;
            lock (this.sync)
            {
                if (this.serviceMap.TryGetValue(serviceType, out obj2))
                {
                    return obj2;
                }
                try
                {
                    obj2 = this.CreateService(serviceType);
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
                this.serviceMap.Add(serviceType, obj2);
            }
            return obj2;
        }

        private void VerifyNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("PdnServiceProvider");
            }
        }

        public bool IsDisposed =>
            this.disposed;
    }
}

