using LiteDB;

namespace LiteDb.Studio.Avalonia.Core;

public class ConnParamType
{
    public int Id { get; set; } = 0;
    public string DbFile { get; set; } = "";
    public string Password { get; set; } = "";
    public bool IsDirect { get; set; } = false;
    public bool IsShared { get; set; } = true;
    public long InitSizeInMB { get; set; } = 0L;
    public bool IsReadOnly { get; set; } = true;
    public bool IsUpgradingFromV4 { get; set; } = false;
    public string Collation { get; set; } = "";
}

public static class ConnectionParameters
{
    public static ConnectionString BuildConString(ConnParamType p)
    {
        var cs = new ConnectionString
        {
            Connection = p.IsDirect ? ConnectionType.Direct : ConnectionType.Shared,
            Filename = p.DbFile,
            ReadOnly = p.IsReadOnly,
            Upgrade = p.IsUpgradingFromV4,
            InitialSize = p.InitSizeInMB * 1024L * 1024L,
            Password = string.IsNullOrWhiteSpace(p.Password) ? null : p.Password.Trim(),
            Collation = string.IsNullOrWhiteSpace(p.Collation) ? null : new Collation(p.Collation)
        };

        return cs;
    }
}