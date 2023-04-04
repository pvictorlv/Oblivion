using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using System.Net;
using System;
using System.Threading.Tasks;
using Oblivion.Configuration;
using Oblivion.Messages;

namespace Oblivion.Connection.Netty;

public class MessageHandler<T> : ChannelHandlerAdapter, ISession<T>
{
    #region Fields
    

    public IChannel Channel { get; set; }
    private ConnectionClosed<T> OnConnectionClosed;
    private ConnectionOpened<T> OnConnectionOpened;
    private MessageReceived<T> OnMessage;

    #endregion Fields

    #region Properties

    public ARC4 clientRC4 { get; set; }

    public IPAddress RemoteAddress
    {
        get { return ((IPEndPoint)Channel.RemoteAddress).Address; }
    }

    public ARC4 serverRC4 { get; set; }

    public T UserData { get; set; }

    #endregion Properties

    #region Constructors

    public MessageHandler(IChannel channel, MessageReceived<T> onMessage, ConnectionClosed<T> onConnectionClosed,
        ConnectionOpened<T> onConnectionOpened)
    {
        this.Channel = channel;
        this.OnMessage = onMessage;
        this.OnConnectionClosed = onConnectionClosed;
        this.OnConnectionOpened = onConnectionOpened;
    }

    #endregion Constructors

    #region Methods

    public override void ChannelActive(IChannelHandlerContext context)
    {
        OnConnectionOpened(this);
        base.ChannelActive(context);
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        OnConnectionClosed(this);
        base.ChannelInactive(context);
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        IByteBuffer dataBuffer = message as IByteBuffer;

        byte[] data = new byte[dataBuffer.ReadableBytes];

        dataBuffer.ReadBytes(data);

        OnMessage(this, data);

        dataBuffer.Release();
    }

    public void Disconnect()
    {
        Channel.DisconnectAsync();
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Logging.HandleException(exception, context.Name);
    //    context.CloseAsync();
    }

    public Task Send(ServerMessage data)
    {
        try
        {
            return this.Channel.WriteAndFlushAsync(data);
        }
        catch (Exception ex)
        {
            Logging.HandleException(ex, "MessageHandler");
        }

        return Task.CompletedTask;
    }

    public Task Send(IByteBuffer data)
    {
        try
        {
            return this.Channel.WriteAndFlushAsync(data);
        }
        catch (Exception ex)
        {
            Logging.HandleException(ex, "MessageHandler");
        }

        return Task.CompletedTask;
    }

    public async Task Send(byte[] data)
    {
        try
        {
            await this.Channel.WriteAndFlushAsync(data);
        }
        catch (Exception ex)
        {
            Logging.HandleException(ex, "MessageHandler");
        }

    }

    public async Task Send(ArraySegment<byte> data)
    {
        try
        {
            byte[] buffer = new byte[data.Count];
            Array.Copy(data.Array, data.Offset, buffer, 0, data.Count);
            await Send(buffer);
        }
        catch (Exception ex)
        {
            Logging.HandleException(ex, "MessageHandler");
        }
    }

    #endregion Methods

    public void Dispose()
    {
        UserData = default;
    }
}