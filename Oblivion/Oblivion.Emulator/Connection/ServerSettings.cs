namespace Oblivion.Connection;

public class ServerSettings
{
    #region Properties

    public int Backlog
    {
        get; set;
    }

    public int BufferSize
    {
        get; set;
    }

    public string IP
    {
        get; set;
    }

    public int MaxConnections
    {
        get; set;
    }

    public int MaxIOThreads
    {
        get; set;
    }

    public int MaxWorkingThreads
    {
        get; set;
    }

    public int MinIOThreads
    {
        get; set;
    }

    public int MinWorkingThreads
    {
        get; set;
    }

    public int Port
    {
        get; set;
    }

    public bool TcpNoDelay { get; set; }

    #endregion Properties

    #region Constructors

    public ServerSettings()
    {
        MinIOThreads = 0;
        MaxIOThreads = 0;
        BufferSize = 4096;
        Backlog = 100;
        MaxConnections = 1000;
    }

    #endregion Constructors
}