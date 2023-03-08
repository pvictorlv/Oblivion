using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using System.Net;
using System;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Oblivion.Messages;
using DotNetty.Buffers;

namespace Oblivion.Connection;

public interface ISession<T> : IDisposable
{
    #region Properties
    

    IChannel Channel { get; set; }


    ARC4 clientRC4
    {
        get; set;
    }
    

    ARC4 serverRC4
    {
        get; set;
    }

    T UserData
    {
        get; set;
    }

    #endregion Properties

    #region Methods

    void Disconnect();

    Task Send(ArraySegment<byte> data);
    Task Send(ServerMessage data);

    Task Send(IByteBuffer data);

    #endregion Methods
}