using System.Collections;

namespace Oblivion.Util
{
    /// <summary>
    /// Class MemoryContainer.
    /// </summary>
    public class MemoryContainer
    {
        /// <summary>
        /// The _container
        /// </summary>
        private readonly Queue _container;
        /// <summary>
        /// The _buffer size
        /// </summary>
        private readonly int _bufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryContainer"/> class.
        /// </summary>
        /// <param name="initSize">Size of the initialize.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        public MemoryContainer(int initSize, int bufferSize)
        {
            _container = new Queue(initSize);
            _bufferSize = bufferSize;

            for (int i = 0; i < initSize; i++)
                _container.Enqueue(new byte[bufferSize]);
        }

        /// <summary>
        /// Takes the buffer.
        /// </summary>
        /// <returns>System.Byte[].</returns>
        public byte[] TakeBuffer()
        {
            if (_container.Count > 0)
                lock (_container.SyncRoot)
                    return (byte[])_container.Dequeue();

            return new byte[_bufferSize];
        }

        /// <summary>
        /// Gives the buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void GiveBuffer(byte[] buffer)
        {
            lock (_container.SyncRoot)
                _container.Enqueue(buffer);
        }
    }
}