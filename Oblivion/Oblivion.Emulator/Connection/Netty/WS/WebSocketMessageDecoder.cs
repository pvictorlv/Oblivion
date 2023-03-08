using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;
using Oblivion.Messages;
using Oblivion.Messages.Factorys;

namespace Oblivion.Connection.Netty.WS;

public class WebSocketMessageDecoder : MessageToMessageDecoder<WebSocketFrame>
{
    protected override void Decode(IChannelHandlerContext ctx, WebSocketFrame message, List<object> output)
    {
        
        try
        {
            var bf = message.Content;

            if (bf.ReadableBytes < 4)
            {
                //System.out.println("bf.readableBytes() < 4");
                return;
            }
            bf.MarkReaderIndex();
            int length = bf.ReadInt();

            if (bf.ReadableBytes < length)
            {
                //System.out.println("bf.readableBytes() < length");
                bf.ResetReaderIndex();
                return;
            }

            if (length < 0)
            {
                //System.out.println("length < 0");
                return;
            }

            output.Add(new ClientMessage(length, bf.ReadBytes(length)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        

        /*
        IByteBuffer dataBuffer = message.Content;

        byte[] data = new byte[dataBuffer.ReadableBytes];

        dataBuffer.ReadBytes(data);

        
        var session = Oblivion.GetGame().GetClientManager().GetClient(ctx.Channel.Id);
        if (session == null)
        {
        }
        else
        {
            if (session?.PacketParser == null) return;

            using (var clientMessage = new ClientMessage(data))
            {
                session.PacketParser.SuperHandle(clientMessage, session.GetConnection());
            }
        }

        dataBuffer.Release();
        */
    }
}