using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Oblivion.Connection.Connection;
using Oblivion.Messages;

namespace Oblivion.Connection.Netty.WS;

public class WebSocketMessageHandler : SimpleChannelInboundHandler<ClientMessage>
{

    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        context.Flush();
        base.ChannelReadComplete(context);
    }
    public override void ChannelInactive(IChannelHandlerContext context)
    {
        SocketConnectionCheck.FreeConnection(context.Channel.RemoteAddress.ToString());
        Oblivion.GetGame().GetClientManager().DisposeConnection(context.Channel.Id);
        base.ChannelInactive(context);
    }


    protected override void ChannelRead0(IChannelHandlerContext ctx, ClientMessage clientMessage)
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
}