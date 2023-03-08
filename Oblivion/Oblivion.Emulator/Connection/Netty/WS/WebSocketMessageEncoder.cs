using System.Collections;
using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Oblivion.Messages;

namespace Oblivion.Connection.Netty.WS;

public class WebSocketMessageEncoder : MessageToMessageEncoder<ServerMessage>
{
    protected override void Encode(IChannelHandlerContext ctx, ServerMessage message, List<object> output)
    {
        //convert Byte[] to IByteBuffer
//        var buf = ctx.Allocator.Buffer();

        IByteBuffer buffer = Unpooled.WrappedBuffer(message.GetReversedBytes());

        output.Add(new BinaryWebSocketFrame(buffer));

    }
}