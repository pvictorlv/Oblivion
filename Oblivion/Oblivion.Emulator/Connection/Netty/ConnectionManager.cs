using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Xsl;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets.Extensions.Compression;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Oblivion.Connection.Netty.WS;
using Oblivion.Util;

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

            X509Certificate2 tlsCertificate = null;

            if (!Debugger.IsAttached)
            {
                tlsCertificate = BuildSelfSignedServerCertificate();
                Out.WriteLine("Setting up Websocket SSL Certificate", "Server.AsyncSocketListener");
            }




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
                    if (tlsCertificate != null)
                    {
                        channel.Pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                    }
                    channel.Pipeline.AddLast(new HttpServerCodec());
                    channel.Pipeline.AddLast(new HttpObjectAggregator(65536));
                    channel.Pipeline.AddLast(new WebSocketServerCompressionHandler());
                    channel.Pipeline.AddLast(new WebSocketMessageEncoder());
                    channel.Pipeline.AddLast(new WebSocketChannelHandler());

                    //channel.Pipeline.AddLast(headerDecoder, messageHandler);
                }));

            WSServerChannel = await bootstrapWebSocket.BindAsync(new IPEndPoint(IPAddress.Any, 2087));
            

            return true;
        }
        catch (Exception ex)
        {
            throw ex;
            return false;
        }
    }
    private X509Certificate2 BuildSelfSignedServerCertificate()
    {
        SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
        sanBuilder.AddDnsName("localhost");
        sanBuilder.AddDnsName("hretro.net");
        sanBuilder.AddDnsName(Environment.MachineName);

        X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN=hretro.net");

        using (RSA rsa = RSA.Create(2048))
        {
            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));


            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

            request.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
            certificate.FriendlyName = "hretro.net";

            return new X509Certificate2(certificate.Export(X509ContentType.Pfx, "WeNeedASaf3rPassword"), "WeNeedASaf3rPassword", X509KeyStorageFlags.MachineKeySet);
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