using LiteDB;
using LiteDb.Studio.Avalonia.Infra;

namespace LiteDb.Studio.Avalonia.Repo;

public static class Repo
{
    private static readonly LiteDatabase db = new LiteDatabase(Path.Combine(Utils.GetAppDataPath(), $"{Utils.AppName}.db"));

    public static LiteDatabase GetDb() => db;

    public static void Disconnect() => db.Dispose();
}