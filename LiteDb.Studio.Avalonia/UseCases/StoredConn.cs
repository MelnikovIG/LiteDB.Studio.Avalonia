using LiteDB;
using LiteDb.Studio.Avalonia.Core;

namespace LiteDb.Studio.Avalonia.UseCases;

public class StoredConnUseCase
{
    public record T(Func<LiteDatabase> Db);

    public static T Create(Func<LiteDatabase> db) => new T(db);

    public static ConnParamType? LoadById(int id, T req)
    {
        var c = req.Db().GetCollection<ConnParamType>();
        c.EnsureIndex(x => x.Id);
        return c.FindById(id);
    }

    public static IEnumerable<ConnParamType> LoadAll(T req)
    {
        var c = req.Db().GetCollection<ConnParamType>();
        c.EnsureIndex(x => x.Id);
        return c.FindAll();
    }

    public static void Save(ConnParamType settings, T req)
    {
        var c = req.Db().GetCollection<ConnParamType>();
        c.EnsureIndex(x => x.Id);
        c.Upsert(settings);
    }

    public static void DeleteById(int id, T req)
    {
        var c = req.Db().GetCollection<ConnParamType>();
        c.EnsureIndex(x => x.Id);
        c.Delete(id);
    }
}