using System.Globalization;
using LiteDB;

namespace LiteDb.Studio.Avalonia.Core;

public record BObjectId(string Type, ObjectId Value, BsonValue Raw);
public record BDoc(string Type, int Count, BsonDocument Value, BsonValue Raw);
public record BArray(string Type, int Count, BsonArray Value, BsonValue Raw);
public record BNull(string Type, BsonValue Value, BsonValue Raw);
public record BBytes(string Type, double SizeKB, byte[] Value, BsonValue Raw);
public record BBool(string Type, bool Value, BsonValue Raw);
public record BDecimal(string Type, decimal Value, BsonValue Raw);
public record BDouble(string Type, double Value, BsonValue Raw);
public record BGuid(string Type, Guid Value, BsonValue Raw);
public record BInt(string Type, int Value, BsonValue Raw);
public record BLong(string Type, long Value, BsonValue Raw);
public record BString(string Type, string Value, BsonValue Raw);
public record BDateTime(string Type, DateTime Value, BsonValue Raw);

public abstract record BValType
{
    public BsonValue Raw => this switch
    {
        Document d => d.Raw,
        Array a => a.Raw,
        Bytes b => b.Raw,
        Bool b => b.Raw,
        Decimal d => d.Raw,
        Double d => d.Raw,
        Guid g => g.Raw,
        Int i => i.Raw,
        Long l => l.Raw,
        String s => s.Raw,
        DateTime d => d.Raw,
        Nil n => n.Raw,
        ObjectId o => o.Raw,
        _ => throw new InvalidOperationException()
    };

    public sealed record Document(BDoc Value) : BValType;
    public sealed record Array(BArray Value) : BValType;
    public sealed record Bytes(BBytes Value) : BValType;
    public sealed record Bool(BBool Value) : BValType;
    public sealed record Decimal(BDecimal Value) : BValType;
    public sealed record Double(BDouble Value) : BValType;
    public sealed record Guid(BGuid Value) : BValType;
    public sealed record Int(BInt Value) : BValType;
    public sealed record Long(BLong Value) : BValType;
    public sealed record String(BString Value) : BValType;
    public sealed record DateTime(BDateTime Value) : BValType;
    public sealed record Nil(BNull Value) : BValType;
    public sealed record ObjectId(BObjectId Value) : BValType;
}

public static class BVal
{
    public static BsonValue GetRawValue(BValType bval) => bval.Raw;
    public static bool IsObjectId(BValType bval) => bval.Raw.IsObjectId;

    public static BsonValue? FindObjectId(BValType bval)
    {
        if (bval is BValType.Document doc)
        {
            var match = doc.Value.Value.FirstOrDefault(kv => kv.Key == "_id");
            return match.Equals(default(System.Collections.Generic.KeyValuePair<string, BsonValue>)) ? null : match.Value;
        }
        return null;
    }

    public static BDoc CreateBDoc(BsonValue v) => new("document", v.AsDocument.Keys.Count, v.AsDocument, v);
    public static BArray CreateBArray(BsonValue v) => new("array", v.AsArray.Count, v.AsArray, v);
    public static BBytes CreateBBytes(BsonValue v) => new("bytes", v.AsBinary.LongLength / 1024.0, v.AsBinary, v);
    public static BObjectId CreateBObjectId(BsonValue v) => new("objectId", v.AsObjectId, v);
    public static BBool CreateBBool(BsonValue v) => new("bool", v.AsBoolean, v);
    public static BDecimal CreateBDecimal(BsonValue v) => new("decimal", v.AsDecimal, v);
    public static BDouble CreateBDouble(BsonValue v) => new("double", v.AsDouble, v);
    public static BGuid CreateBGuid(BsonValue v) => new("guid", v.AsGuid, v);
    public static BInt CreateBInt(BsonValue v) => new("int", v.AsInt32, v);
    public static BLong CreateBLong(BsonValue v) => new("long", v.AsInt64, v);
    public static BString CreateBString(BsonValue v) => new("string", v.AsString, v);
    public static BDateTime CreateBDateTime(BsonValue v) => new("dateTime", v.AsDateTime, v);
    public static BNull CreateBNull(BsonValue v) => new("", v, v);

    public static BsonValue CreateBsonValue(BValType typ, string target)
    {
        return typ switch
        {
            BValType.String => new BsonValue(target),
            BValType.DateTime => new BsonValue(System.DateTime.Parse(target, CultureInfo.InvariantCulture)),
            BValType.Bool => new BsonValue(Convert.ToBoolean(target)),
            BValType.Decimal => new BsonValue(Convert.ToDecimal(target, CultureInfo.InvariantCulture)),
            BValType.Double => new BsonValue(Convert.ToDouble(target, CultureInfo.InvariantCulture)),
            BValType.Long => new BsonValue(Convert.ToInt64(target, CultureInfo.InvariantCulture)),
            BValType.Int => new BsonValue(Convert.ToInt32(target, CultureInfo.InvariantCulture)),
            BValType.Guid => new BsonValue(System.Guid.Parse(target)),
            _ => JsonSerializer.Deserialize(target)
        };
    }

    public static BValType Create(BsonValue v)
    {
        if (v.IsDocument) return new BValType.Document(CreateBDoc(v));
        if (v.IsArray) return new BValType.Array(CreateBArray(v));
        if (v.IsNull) return new BValType.Nil(CreateBNull(v));
        if (v.IsBinary) return new BValType.Bytes(CreateBBytes(v));
        if (v.IsBoolean) return new BValType.Bool(CreateBBool(v));
        if (v.IsDecimal) return new BValType.Decimal(CreateBDecimal(v));
        if (v.IsDouble) return new BValType.Double(CreateBDouble(v));
        if (v.IsGuid) return new BValType.Guid(CreateBGuid(v));
        if (v.IsInt32) return new BValType.Int(CreateBInt(v));
        if (v.IsInt64) return new BValType.Long(CreateBLong(v));
        if (v.IsString) return new BValType.String(CreateBString(v));
        if (v.IsDateTime) return new BValType.DateTime(CreateBDateTime(v));
        if (v.IsObjectId) return new BValType.ObjectId(CreateBObjectId(v));
        throw new InvalidOperationException("Unsupported BsonValue type.");
    }
}