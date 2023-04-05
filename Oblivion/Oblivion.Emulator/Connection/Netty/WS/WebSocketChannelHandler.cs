using System;
using System.Diagnostics;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Channels;
using System.Linq;
using DotNetty.Common.Utilities;
using DotNetty.Codecs.Http.WebSockets;
using Oblivion.Connection.Connection;

namespace Oblivion.Connection.Netty.WS;

public class WebSocketChannelHandler : ChannelHandlerAdapter
{
    public override void ChannelRead(IChannelHandlerContext ctx, object msg)
    {
        if (msg is IFullHttpRequest httpRequest)
        {
            HttpHeaders headers = httpRequest.Headers;
            if (headers.IsEmpty)
            {
                return;
            }
            Oblivion.GetGame().GetClientManager().CreateAndStartClient(ctx);

            
            ctx.Channel.Pipeline.Remove(this);
            ctx.Channel.Pipeline.AddLast(new WebSocketMessageDecoder());
            ctx.Channel.Pipeline.AddLast(new WebSocketMessageHandler());
            HandleHandshake(ctx, httpRequest);

            if (Debugger.IsAttached)
                Console.WriteLine("WebSocketChannelHandler :: fez o handshake");
        }
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        SocketConnectionCheck.FreeConnection(context.Channel.RemoteAddress.ToString());
        Oblivion.GetGame().GetClientManager().DisposeConnection(context.Channel.Id);
        base.ChannelInactive(context);
    }
    
    private void HandleHandshake(IChannelHandlerContext ctx, IFullHttpRequest req)
    {
        try
        {
            WebSocketServerHandshaker serverHandshake;
            WebSocketServerHandshakerFactory wsFactory = new WebSocketServerHandshakerFactory(null, null, true);
            serverHandshake = wsFactory.NewHandshaker(req);
            if (serverHandshake == null)
            {
                WebSocketServerHandshakerFactory.SendUnsupportedVersionResponse(ctx.Channel);
            }
            else
            {
                serverHandshake.HandshakeAsync(ctx.Channel, req);
            }
        } catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}