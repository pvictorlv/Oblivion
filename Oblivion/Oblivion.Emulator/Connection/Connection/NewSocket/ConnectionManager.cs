using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Oblivion.Configuration;
using Oblivion.Connection.Connection.Handler;
using Oblivion.Connection.Net;

namespace Oblivion.Connection.Connection
{
    public class ConnectionManager
    {
        /// <summary>
        ///     Data Parser
        /// </summary>
        public static GamePacketParser DataParser;

        /// <summary>
        ///     Server Channel
        /// </summary>
        public static IChannel ServerChannel;

        /// <summary>
        ///     Main Server Worker
        /// </summary>
        public static IEventLoopGroup MainServerWorkers;

        /// <summary>
        ///     Child Server Workers
        /// </summary>
        public static IEventLoopGroup ChildServerWorkers;

        /// <summary>
        ///     Client Connection Actors
        /// </summary>
        public static ConcurrentDictionary<string, ConnectionActor> ClientConnections;

        public static async Task RunServer()
        {
            MainServerWorkers = new MultithreadEventLoopGroup();

            ChildServerWorkers = new MultithreadEventLoopGroup();

            ClientConnections = new ConcurrentDictionary<string, ConnectionActor>();

            try
            {
                ServerBootstrap server = new ServerBootstrap();

                server
                    .Group(MainServerWorkers, ChildServerWorkers)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.AutoRead, true)
                    .Option(ChannelOption.SoBacklog, 100)
                    .Option(ChannelOption.SoKeepalive, true)
                    .Option(ChannelOption.ConnectTimeout, TimeSpan.MaxValue)
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.SoRcvbuf, GameSocketManagerStatics.BufferSize)
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        string clientAddress = (channel.RemoteAddress as IPEndPoint)?.Address.ToString();

                        if (ConnectionSecurity.CheckAvailability(clientAddress))
                        {
                            channel.Pipeline.AddLast(new ConnectionHandler());

                            ConnectionActor connectionActor = new ConnectionActor(DataParser.Clone() as GamePacketParser, channel);

                            if (ClientConnections.ContainsKey(connectionActor.IpAddress))
                                connectionActor.HandShakePartialCompleted = true;

                            ClientConnections.AddOrUpdate(connectionActor.IpAddress, connectionActor, (key, value) => connectionActor);
                        }
                    }));

                ServerChannel = await server.BindAsync(IPAddress.Any, Convert.ToInt32(ConfigurationData.Data["game.tcp.port"]));
            }
            catch
            {
                // ignored
            }
        }

        public static async void Start() => await RunServer();

        public static async void Stop()
        {
            await ServerChannel.CloseAsync();

            await MainServerWorkers.ShutdownGracefullyAsync();

            await ChildServerWorkers.ShutdownGracefullyAsync();
        }
    }

}