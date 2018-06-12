using System;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.Connection.SuperSocket
{

    #region Delegates

    public delegate void ConnectionClosed<T>(Session<T> session);

    public delegate void ConnectionOpened<T>(Session<T> session);

    public delegate void MessageReceived<T>(Session<T> session, byte[] body);

    #endregion Delegates

    public interface IServer<T>
    {
        #region Events

        event ConnectionClosed<T> OnConnectionClosed;

        event ConnectionOpened<T> OnConnectionOpened;

        event MessageReceived<T> OnMessageReceived;

        #endregion Events

        #region Methods

        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>
        /// return true if start successfull, else false
        /// </returns>
        bool Start();

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        void Stop();

        #endregion Methods
    }
}