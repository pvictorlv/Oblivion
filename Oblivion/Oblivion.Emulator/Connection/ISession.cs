using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using System.Net;
using System;
using System.Threading.Tasks;

namespace Oblivion.Connection;

public interface ISession<T> : IDisposable
{
    #region Properties

    ARC4 clientRC4
    {
        get; set;
    }

    IPAddress RemoteAddress
    {
        get;
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

    Task Send(byte[] data);

    #endregion Methods
}