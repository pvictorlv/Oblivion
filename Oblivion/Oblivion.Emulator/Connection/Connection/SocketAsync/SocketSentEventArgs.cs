using System;

namespace Oblivion.Connection.Connection.SocketAsync
{
    public class SocketSentEventArgs<T> : EventArgs where T : SessionBase
    {
        /// <summary>
        /// The session who sends data.
        /// </summary>
        public T Session
        {
            get;
            private set;
        }

        /// <summary>
        /// The data sent to the client.
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// The amount of bytes sent to the client.
        /// </summary>
        public int BytesSent
        {
            get;
            private set;
        }

        internal SocketSentEventArgs(T session, byte[] data, int bytesSent)
        {
            Session = session;
            Data = data;
            BytesSent = bytesSent;
        }
    }

}