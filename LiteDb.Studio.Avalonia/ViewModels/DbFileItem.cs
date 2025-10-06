using LiteDB;
using LiteDb.Studio.Avalonia.Core;
using ReactiveUI;

namespace LiteDb.Studio.Avalonia.ViewModels;

public class DbFileItem : DbItem
{
    private LiteDatabase? liteDb;
    public ConnectionString ConnectionString { get; }

    public DbFileItem(LiteDatabase db, ConnectionString conString)
    {
        liteDb = db;
        ConnectionString = conString;

        var connectAction = new DbAction(Connect) { Header = "connect" };
        var disconnectAction = new DbAction(DisconnectInternal) { Header = "disconnect" };

        ContextMenu.Add(connectAction);
        ContextMenu.Add(disconnectAction);
    }

    public LiteDatabase? LiteDb => liteDb;

    public override bool IsConnected => liteDb != null;

    public override void Disconnect()
    {
        DisconnectInternal();
    }

    private void DisconnectInternal()
    {
        if (liteDb != null)
        {
            liteDb.Checkpoint();
            DbUtils.DisposeHack(liteDb);
            liteDb = null;
            this.RaisePropertyChanged(nameof(IsConnected));
        }
    }

    private void Connect()
    {
        if (liteDb == null)
        {
            liteDb = new LiteDatabase(ConnectionString);
            this.RaisePropertyChanged(nameof(IsConnected));
        }
    }
}