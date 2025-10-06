using LiteDB;
using LiteDb.Studio.Avalonia.Core;

namespace LiteDb.Studio.Avalonia.UseCases;

public static class UpdateField
{
    public class T
    {
        public BsonValue? BsonId { get; init; }
        public string Field { get; init; } = "";
        public string TableName { get; init; } = "";
        public BsonValue NewValue { get; init; } = default!;
        public BsonValue OldValue { get; init; } = default!;
        public Func<LiteDatabase> Db { get; init; } = default!;
    }

    public static T Create(Func<LiteDatabase> db, string table, string field, BsonValue? id, BsonValue oldValue, BsonValue newValue)
    {
        return new T
        {
            Db = db,
            Field = field,
            TableName = table,
            OldValue = oldValue,
            NewValue = newValue,
            BsonId = id
        };
    }

    public static bool Run(T req)
    {
        if (req.BsonId == null)
            return false;

        using var cs = new CancellationTokenSource();
        var updateSql = $"UPDATE {req.TableName} SET {req.Field} = @0 WHERE _id = @1 AND {req.Field} = @2";

        var arg = new BsonDocument
        {
            ["0"] = req.NewValue,
            ["1"] = req.BsonId,
            ["2"] = req.OldValue
        };

        var db = req.Db();
        var result = DbUtils.ReadResult(DbUtils.ExecWithArg(db, updateSql, arg), cs.Token).First();
        
        return result.RawValue.Equals(1);
    }
}