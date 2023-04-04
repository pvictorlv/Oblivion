using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;
using Oblivion.Configuration;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using Oblivion.HabboHotel.GameClients.Interfaces;
using Oblivion.Messages;

namespace Oblivion.Connection;

public class Session : ISession<GameClient>
{
    public void Dispose()
    {
        UserData = null;
    }
    

    public ARC4 clientRC4 { get; set; }
    
    public ARC4 serverRC4 { get; set; }

    public IChannel Channel { get; set; }

    public Session(IChannel channel, GameClient userData)
    {
        this.Channel = channel;
        this.UserData = userData;
    }

    
    public GameClient UserData { get; set; }
    public void Disconnect()
    {
        Channel.DisconnectAsync();
    }


    public Task Send(ServerMessage data)
    {
        return this.Channel.WriteAndFlushAsync(data);
    }

    public async Task Send(IByteBuffer data)
    {
        try
        {
            await this.Channel.WriteAndFlushAsync(new BinaryWebSocketFrame(data));
        }
        catch (Exception ex)
        {
            Logging.LogException(ex.ToString());
        }
    }

    public async Task Send(ArraySegment<byte> data)
    {
        byte[] buffer = new byte[data.Count];
        Array.Copy(data.Array, data.Offset, buffer, 0, data.Count);
        await Send(buffer);
    }
}