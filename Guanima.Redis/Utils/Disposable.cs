using System;

namespace Guanima.Redis.Utils
{
    public class Disposable : IDisposable
    {
        ~Disposable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing) Release();
                Disposed = true;
            }
        }

        protected virtual void Release() { }

        protected bool Disposed
        {
            get;
            private set;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
