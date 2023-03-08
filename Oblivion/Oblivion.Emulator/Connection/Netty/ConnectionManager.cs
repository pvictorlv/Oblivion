using System;
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

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

    public bool Start()
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

            Task<IChannel> task = server.BindAsync(Settings.IP, Settings.Port);
            task.Wait();
            ServerChannel = task.Result;
            return true;
        }
        catch
        {
            // TODO Store/print error
            return false;
        }
    }

    public void Stop()
    {
        DoStop().Wait();
    }

    private async Task DoStop()
    {
        await ServerChannel.CloseAsync();

        await MainServerWorkers.ShutdownGracefullyAsync();

        await ChildServerWorkers.ShutdownGracefullyAsync();
    }
}