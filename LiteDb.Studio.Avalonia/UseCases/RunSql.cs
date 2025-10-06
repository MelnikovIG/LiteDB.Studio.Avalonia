using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using LiteDB;
using LiteDb.Studio.Avalonia.Core;

namespace LiteDb.Studio.Avalonia.UseCases;

public static class RunSql
{
    public record T
    {
        public Stopwatch Stopwatch { get; init; } = new Stopwatch();
        public string Query { get; init; } = "";
        public Func<LiteDatabase> Db { get; init; } = default!;
        public CancellationToken Token { get; init; }
    }

    public static T Create(Stopwatch sw, string query, Func<LiteDatabase> db, CancellationToken token) =>
        new T { Stopwatch = sw, Query = query, Db = db, Token = token };

    public static string RemoveComments(string sql)
    {
        using var reader = new StringReader(sql);
        var sb = new StringBuilder();
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (!line.TrimStart().StartsWith("--"))
                sb.AppendLine(line);
        }
        return sb.ToString();
    }

    public static string? FindTableName(string sql)
    {
        string? GetPattern(string actualSql)
        {
            if (actualSql.Contains("INSERT")) return @"INSERT\s+INTO\s+(?<table>\w+).*";
            if (actualSql.Contains("UPDATE")) return @"UPDATE\s+(?<table>\w+).*";
            if (actualSql.Contains("DELETE")) return @"DELETE\s+(?<table>\w+).*";
            if (actualSql.Contains("SELECT")) return @"SELECT.*FROM\s+(?<table>\w+).*";
            return null;
        }

        var cleaned = RemoveComments(sql).ToUpperInvariant().Trim();
        var pattern = GetPattern(cleaned);
        if (pattern == null) return null;

        var match = Regex.Match(cleaned, pattern);
        return match.Success ? match.Groups["table"].Value : null;
    }

    public static BValType[] Run(T req)
    {
        var db = req.Db();
        req.Stopwatch.Restart();
        using var reader = DbUtils.Exec(db, RemoveComments(req.Query));
        var bsonValues = DbUtils.ReadResult(reader, req.Token).Select(BVal.Create).ToArray();
        req.Stopwatch.Stop();
        return bsonValues;
    }
}