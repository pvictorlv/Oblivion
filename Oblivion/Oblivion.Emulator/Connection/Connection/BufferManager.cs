using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

public class BufferManager
{
    public byte[] Buffer;
    private readonly int BufferSize;
    private readonly Stack<int> FreeIndexPool;
    private int Index;
    private readonly int TotalBytes;

    public BufferManager(int TotalBytes, int BufferSize)
    {
        this.TotalBytes = TotalBytes;
        Buffer = new byte[TotalBytes];
        FreeIndexPool = new Stack<int>();
        this.BufferSize = BufferSize;
        Index = 0;
    }

    public bool SetBuffer(SocketAsyncEventArgs Args)
    {
        if (FreeIndexPool.Count > 0)
        {
            Args.SetBuffer(Buffer, FreeIndexPool.Pop(), BufferSize);
        }
        else
        {
            if (TotalBytes - BufferSize < Index)
                return false;

            Args.SetBuffer(Buffer, Index, BufferSize);


            Interlocked.Add(ref Index, BufferSize);
        }

        return true;
    }

    public void FreeBuffer(SocketAsyncEventArgs Args)
    {
        FreeIndexPool.Push(Args.Offset);

        Args.SetBuffer(null, 0, 0);
    }
}