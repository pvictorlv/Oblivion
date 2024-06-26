using System;
using System.Threading.Tasks;
using Oblivion.Connection;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.Messages.Parsers
{
    /// <summary>
    /// Interface IDataParser
    /// </summary>
    public interface IDataParser : IDisposable, ICloneable
    {
        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <param name="bytesReceived"></param>
        void HandlePacketData(byte[] packet, int bytesReceived);
        void SuperHandle(ClientMessage message, ISession<GameClient> userSocket);
    }
}