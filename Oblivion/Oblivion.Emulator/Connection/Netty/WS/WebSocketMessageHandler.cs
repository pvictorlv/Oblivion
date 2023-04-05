using System;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Oblivion.Configuration;
using Oblivion.Connection.Connection;
using Oblivion.Messages;

namespace Oblivion.Connection.Netty.WS;

public class WebSocketMessageHandler : SimpleChannelInboundHandler<ClientMessage>
{

    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        try
        {
            context.Flush();
        }
        catch (Exception ex)
        {
            Logging.HandleException(ex, "ChannelReadComplete");
        }
    }
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        try
        {
            SocketConnectionCheck.FreeConnection(context.Channel.RemoteAddress.ToString());
            Oblivion.GetGame().GetClientManager().DisposeConnection(context.Channel.Id);
            base.ChannelInactive(context);
        }
        catch (Exception ex)
        {
            Logging.HandleException(ex, "ChannelInactive");
        }
    }


    protected override void ChannelRead0(IChannelHandlerContext ctx, ClientMessage clientMessage)
    {
        try
        {
            var session = Oblivion.GetGame().GetClientManager().GetClient(ctx.Channel.Id);
            if (session == null)
            {
            }
            else
            {
                if (session?.PacketParser == null) return;

                //using (var clientMessage = new ClientMessage(msg))
                {
                    session.PacketParser.SuperHandle(clientMessage, session.GetConnection());
                }
                clientMessage.Dispose();
            }
        }
        catch (Exception ex)
        {
            Logging.HandleException(ex, nameof(WebSocketMessageHandler));
        }
    }
}