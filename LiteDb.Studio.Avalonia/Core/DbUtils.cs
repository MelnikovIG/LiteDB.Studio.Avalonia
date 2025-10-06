using System.Text;
using LiteDB;

namespace LiteDb.Studio.Avalonia.Core;

public static class DbUtils
{
    public static IBsonDataReader ExecWithArg(LiteDatabase db, string sql, BsonDocument arg)
    {
        using var reader = new StringReader(sql);
        return db.Execute(reader, arg);
    }

    public static IBsonDataReader Exec(LiteDatabase db, string sql)
    {
        return ExecWithArg(db, sql, new BsonDocument());
    }

    public static async Task Checkpoint(LiteDatabase db)
    {
        await Task.Run(() => db.Checkpoint());
    }

    public static async Task Shrink(LiteDatabase db)
    {
        await Task.Run(() => db.Rebuild());
    }

    public static IEnumerable<BsonValue> ReadResult(IBsonDataReader reader, CancellationToken token)
    {
        while (!token.IsCancellationRequested && reader.Read())
            yield return reader.Current;
    }

    public static string ToJson(List<BsonValue> result)
    {
        int index = 0;
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var json = new JsonWriter(writer)
        {
            Pretty = true,
            Indent = 2
        };

        foreach (var value in result)
        {
            if (result.Count > 1)
            {
                index++;
                sb.AppendLine($"// {index}");
            }
            json.Serialize(value);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static LiteDatabase GetDb(ConnectionString conStr) => new(conStr);

    public static IEnumerable<BsonDocument> GetSystemTables(LiteDatabase db)
    {
        return db
            .GetCollection("$cols")
            .Query()
            .Where("type = 'system'")
            .OrderBy("name")
            .ToDocuments();
    }

    public static IEnumerable<string> GetCollectionNames(LiteDatabase db)
    {
        return db.GetCollectionNames().OrderBy(x => x);
    }

    public static void DisposeHack(LiteDatabase liteDb)
    {
        try
        {
            liteDb.Dispose();
            // Reflection cleanup commented out (as in F# source)
        }
        catch
        {
            // ignore
        }
    }
}