namespace Oblivion.Connection.Connection
{
    /// <summary>
    /// Class GameSocketManagerStatics.
    /// </summary>
    public class GameSocketManagerStatics
    {
        /// <summary>
        /// The buffer size
        /// </summary>
        public static readonly int BufferSize = 8192; // habbo buffer size (JSON support - camera)

        /// <summary>
        /// The maximum packet size
        /// </summary>
        public static readonly int MaxPacketSize = (BufferSize - 4);
    }
}