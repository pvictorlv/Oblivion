using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
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
            MainServerWorkers = new MultithreadEventLoopGroup(8);
            
            ChildServerWorkers = new MultithreadEventLoopGroup(8);

            ClientConnections = new ConcurrentDictionary<string, ConnectionActor>();

            try
            {
                ServerBootstrap server = new ServerBootstrap();
                
                server
                    .Group(MainServerWorkers, ChildServerWorkers)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 1000)
                    .Option(ChannelOption.SoKeepalive, true)
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.AutoRead, true)
                    .Option(ChannelOption.WriteBufferHighWaterMark, 64 * 1024)
                    .Option(ChannelOption.WriteBufferLowWaterMark, 32 * 1024)
                    .Option(ChannelOption.SoRcvbuf, GameSocketManagerStatics.BufferSize)
                    .ChildOption(ChannelOption.WriteBufferHighWaterMark, 64 * 1024)
                    .ChildOption(ChannelOption.WriteBufferLowWaterMark, 32 * 1024)
                    .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.TcpNodelay, true)
                    
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        string clientAddress = (channel.RemoteAddress as IPEndPoint)?.Address.ToString();

                        if (ConnectionSecurity.CheckAvailability(clientAddress))
                        {

                            channel.Pipeline.AddLast(new ConnectionHandler());

                            var parser = new GamePacketParser();

                            ConnectionActor connectionActor = new ConnectionActor(parser, channel);

                            if (ClientConnections.ContainsKey(connectionActor.IpAddress))
                                connectionActor.HandShakePartialCompleted = true;

                            ClientConnections.AddOrUpdate(connectionActor.IpAddress, connectionActor, (key, value) => connectionActor);
                        }
                    }));
                var task = server.BindAsync(IPAddress.Any, Convert.ToInt32(ConfigurationData.Data["game.tcp.port"]));
                task.Wait();
                ServerChannel = task.Result;
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