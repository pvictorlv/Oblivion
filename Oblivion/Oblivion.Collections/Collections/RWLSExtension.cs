using System;
using System.Threading;

namespace Oblivion.Collections
{
    internal static class RWLSExtension
    {
        public static ReadLockExt ReadLock(this ReaderWriterLockSlim readerWriterLock) => new ReadLockExt(readerWriterLock);

        public static WriteLockExt WriteLock(this ReaderWriterLockSlim readerWriterLock) => new WriteLockExt(readerWriterLock);

        public struct ReadLockExt : IDisposable
        {
            private readonly ReaderWriterLockSlim readerWriterLock;

            public ReadLockExt(ReaderWriterLockSlim readerWriterLock)
            {
                readerWriterLock.EnterReadLock();
                this.readerWriterLock = readerWriterLock;
            }

            public void Dispose()
            {
                readerWriterLock.ExitReadLock();
                GC.SuppressFinalize(this);
            }
        }

        public struct WriteLockExt : IDisposable
        {
            private readonly ReaderWriterLockSlim readerWriterLock;

            public WriteLockExt(ReaderWriterLockSlim readerWriterLock)
            {
                readerWriterLock.EnterWriteLock();
                this.readerWriterLock = readerWriterLock;
            }

            public void Dispose()
            {
                readerWriterLock.ExitWriteLock();
                GC.SuppressFinalize(this);
            }
        }
    }
}