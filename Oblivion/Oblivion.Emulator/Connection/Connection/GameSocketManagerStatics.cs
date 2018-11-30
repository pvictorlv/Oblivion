namespace Oblivion.Connection.Connection
{
    /// <summary>
    /// Class GameSocketManagerStatics.
    /// </summary>
    public class GameSocketManagerStatics
    {
        public static readonly int BufferSize = 8192; // habbo buffer size (JSON support - camera)

        public static readonly int OpsToPreAllocate = 2;
        public static int MaxConnections = 10000;
        public static readonly int MaxAcceptOps = 500;
        public static readonly int NumberOfSaeaForRecSend = MaxConnections + 1;
        
        public static readonly int MaxPacketSize = (BufferSize - 4);
    }
}