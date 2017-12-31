using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Oblivion.Configuration;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.GameClients.Interfaces;

namespace Oblivion.Connection.Connection.Handler
{
    public class ConnectionHandler : ChannelHandlerAdapter
    {
        internal static GameClientManager GetClient() => Oblivion.GetGame().GetClientManager();

        public override Task CloseAsync(IChannelHandlerContext context)
        {
            
            string clientAddress = (context.Channel.RemoteAddress as IPEndPoint)?.Address.ToString();
            if (clientAddress != null)
            {
                GetClient().UnregisterClient(clientAddress);

                ConnectionManager.ClientConnections.TryRemove(clientAddress, out _);
            }

            return context.CloseAsync();
            
        }

        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            string clientAddress = (context.Channel.RemoteAddress as IPEndPoint)?.Address.ToString();
            if (clientAddress != null)
            {
                GameClient client = GetClient().GetClient(clientAddress);

                if (client?.GetConnection()?.HandShakeCompleted == true && client.GetConnection()?.ConnectionId == context.Channel?.Id?.ToString())
                    client.Disconnect("Left Game", true);

                ConnectionSecurity.RemoveClientCount(clientAddress);
            }
        }

        public void ChannelInitialRead(IChannelHandlerContext context, GameClient client, byte[] dataBytes)
        {
            if (dataBytes[0] == 60)
                WriteAsync(context, CrossDomainSettings.XmlPolicyBytes);
            else if (dataBytes[0] != 67)
                client?.InitHandler();
        }


        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            // ignored.
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer dataBuffer)
            {
                string clientAddress = (context.Channel.RemoteAddress as IPEndPoint)?.Address.ToString();
                if (clientAddress != null)
                {
                    GameClient client = GetClient().GetClient(clientAddress);

                    if (client?.GetConnection() != null)
                    {
                        byte[] dataBytes = new byte[dataBuffer.ReadableBytes];
                        dataBuffer.ReadBytes(dataBytes);
//                        Array.Copy(dataBuffer.Array, dataBytes, dataBuffer.ReadableBytes);


                        if (!client.GetConnection().HandShakeCompleted ||
                            !client.GetConnection().HandShakePartialCompleted)
                            ChannelInitialRead(context, client, dataBytes);

                        if (!client.GetConnection().HandShakePartialCompleted)
                            client.GetConnection().Close();

                        if (client.GetConnection().HandShakeCompleted)
                        {
                            //                            Arc4ServerSide?.Parse(ref packet);
                            client.GetConnection().DataParser.HandlePacketData(dataBytes, dataBytes.Length);

                        }

                        return;
                    }
                }
            }

            context.WriteAndFlushAsync(message);
        }


        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer)
                return context.WriteAndFlushAsync(message);

            IByteBuffer buffer = context.Allocator.Buffer().WriteBytes(message as byte[]);

            return context.WriteAndFlushAsync(buffer);
        }

        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            string clientAddress = (context.Channel.RemoteAddress as IPEndPoint)?.Address.ToString();

            if (clientAddress != null)
            {
                ConnectionActor connectionActor;

                ConnectionManager.ClientConnections.TryGetValue(clientAddress, out connectionActor);

                if (connectionActor != null)
                {
                    connectionActor.SameHandledCount++;

                    Oblivion.GetGame().GetClientManager()?.CreateAndStartClient(clientAddress, connectionActor);

                    Oblivion.GetGame().GetClientManager()?.LogClonesOut(connectionActor.ConnectionId);
                }
            }
        }
    }
}