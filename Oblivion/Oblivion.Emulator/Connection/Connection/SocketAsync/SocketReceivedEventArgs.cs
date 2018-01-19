using System;

namespace Oblivion.Connection.Connection.SocketAsync
{
    public class SocketReceivedEventArgs<T> : EventArgs where T : SessionBase
    {
        /// <summary>
        /// The session who received data.
        /// </summary>
        public T Session
        {
            get;
            private set;
        }

        /// <summary>
        /// The data received from the client.
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// The amount of bytes received from the client.
        /// </summary>
        public int BytesReceived
        {
            get;
            private set;
        }

        internal SocketReceivedEventArgs(ref T session, byte[] data, int bytesReceived)
        {
            Session = session;
            Data = data;
            BytesReceived = bytesReceived;
        }
    }
}