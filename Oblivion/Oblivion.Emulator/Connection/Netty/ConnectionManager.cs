using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets.Extensions.Compression;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Oblivion.Connection.Netty.WS;

namespace Oblivion.Connection.Netty;

public class ConnectionManager<T> : IServer<T>
{
    public event MessageReceived<T> OnMessageReceived;

    public event ConnectionOpened<T> OnConnectionOpened;

    public event ConnectionClosed<T> OnConnectionClosed;

    public long AcceptedClients { get; set; }

    /// <summary>
    ///     Server Channel
    /// </summary>
    private IChannel ServerChannel;

    private IChannel WSServerChannel;

    /// <summary>
    ///     Main Server Worker
    /// </summary>
    private IEventLoopGroup MainServerWorkers;

    /// <summary>
    ///     Child Server Workers
    /// </summary>
    private IEventLoopGroup ChildServerWorkers;

    private ServerSettings Settings;

    private CrossDomainSettings FlashPolicy;

    public ConnectionManager(ServerSettings settings, CrossDomainSettings flashPolicy)
    {
        OnConnectionClosed = delegate { };
        OnConnectionOpened = delegate { };
        OnMessageReceived = delegate { };

        this.Settings = settings;
        this.FlashPolicy = flashPolicy;
    }

    public async Task<bool> Start()
    {
        MainServerWorkers = this.Settings.MaxIOThreads == 0
            ? new MultithreadEventLoopGroup()
            : new MultithreadEventLoopGroup(this.Settings.MaxIOThreads);

        ChildServerWorkers = this.Settings.MaxWorkingThreads == 0
            ? new MultithreadEventLoopGroup()
            : new MultithreadEventLoopGroup(this.Settings.MaxWorkingThreads);

        try
        {
            ServerBootstrap server = new ServerBootstrap();

            HeaderDecoder headerDecoder = new HeaderDecoder();
            FlashPolicyHandler flashHandler = new FlashPolicyHandler(FlashPolicy);

            server
                .Group(MainServerWorkers, ChildServerWorkers)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.AutoRead, true)
                .Option(ChannelOption.SoBacklog, Settings.Backlog)
                .Option(ChannelOption.SoKeepalive, true)
                .Option(ChannelOption.ConnectTimeout, TimeSpan.MaxValue)
                .Option(ChannelOption.TcpNodelay, Settings.TcpNoDelay)
                .Option(ChannelOption.SoRcvbuf, this.Settings.BufferSize)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    /*
                     * Note: we have to create a new MessageHandler for each 
                     * session because it has stateful properties.
                     */
                    MessageHandler<T> messageHandler = new MessageHandler<T>(channel, OnMessageReceived,
                        OnConnectionClosed, OnConnectionOpened);
                    channel.Pipeline.AddFirst(flashHandler);
                    channel.Pipeline.AddLast(headerDecoder, messageHandler);
                }));

            ServerChannel = await server.BindAsync(new IPEndPoint(IPAddress.Any, Settings.Port));

            var bootstrapWebSocket = new ServerBootstrap()
                .Group(new MultithreadEventLoopGroup(), new MultithreadEventLoopGroup())
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.AutoRead, true)
                .Option(ChannelOption.SoBacklog, Settings.Backlog)
                .Option(ChannelOption.SoKeepalive, true)
                .Option(ChannelOption.ConnectTimeout, TimeSpan.MaxValue)
                .Option(ChannelOption.TcpNodelay, Settings.TcpNoDelay)
                .Option(ChannelOption.SoRcvbuf, this.Settings.BufferSize)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    /*
                     * Note: we have to create a new MessageHandler for each 
                     * session because it has stateful properties.
                     */
                    channel.Pipeline.AddLast(new HttpServerCodec());
                    channel.Pipeline.AddLast(new HttpObjectAggregator(65536));
                    channel.Pipeline.AddLast(new WebSocketServerCompressionHandler());
                    channel.Pipeline.AddLast(new WebSocketMessageEncoder());
                    channel.Pipeline.AddLast(new WebSocketChannelHandler());

                    //channel.Pipeline.AddLast(headerDecoder, messageHandler);
                }));

            WSServerChannel = await bootstrapWebSocket.BindAsync(new IPEndPoint(IPAddress.Any, 2096));
            

            return true;
        }
        catch (Exception ex)
        {
            throw ex;
            return false;
        }
    }

    public void Stop()
    {
        DoStop();
    }

    private async Task DoStop()
    {
        await ServerChannel.CloseAsync();
        
        await WSServerChannel.CloseAsync();

        await MainServerWorkers.ShutdownGracefullyAsync();

        await ChildServerWorkers.ShutdownGracefullyAsync();
    }
}