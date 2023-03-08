﻿using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Oblivion.Encryption.Encryption.Hurlant.Crypto.Prng;
using System.Net;
using System;
using System.Threading.Tasks;
using Oblivion.Messages;

namespace Oblivion.Connection.Netty;

public class MessageHandler<T> : ChannelHandlerAdapter, ISession<T>
{
    #region Fields

    private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        Logger.Warn("A networking error occured", exception);
        context.CloseAsync();
    }

    public Task Send(ServerMessage data)
    {
        return this.Channel.WriteAndFlushAsync(data);
    }

    public Task Send(IByteBuffer data)
    {
        return this.Channel.WriteAndFlushAsync(data);
    }

    public async Task Send(byte[] data)
    {
        await this.Channel.WriteAndFlushAsync(data);
    }

    public async Task Send(ArraySegment<byte> data)
    {
        byte[] buffer = new byte[data.Count];
        Array.Copy(data.Array, data.Offset, buffer, 0, data.Count);
        await Send(buffer);
    }

    #endregion Methods

    public void Dispose()
    {
        UserData = default;
    }
}