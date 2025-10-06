using System.Globalization;
using System.Runtime.InteropServices;

namespace LiteDb.Studio.Avalonia.Infra;

public static class Utils
{
    public const string AppName = "LiteDb.Studio.Avalonia";

    public static string[] GetCultures()
    {
        return CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Select(x => x.LCID)
            .Distinct()
            .Where(x => x != 4096)
            .Select(x => CultureInfo.GetCultureInfo(x).Name)
            .ToArray();
    }

    public static string[] GetCompareOptions()
    {
        var names = Enum.GetNames(typeof(CompareOptions));
        return new[] { "" }.Concat(names).ToArray();
    }

    public static Dictionary<int, (int Start, int End)> GetPages(int pageSize, int total)
    {
        var pages = new Dictionary<int, (int, int)>();

        if (pageSize > total)
        {
            pages[0] = (0, total - 1);
        }
        else
        {
            int maxPageCount = (total / pageSize) + 1;

            for (int i = 0; i < maxPageCount; i++)
            {
                int pageStart = i * pageSize;

                if (pageStart < total)
                {
                    int pageEnd = pageStart + pageSize - 1;
                    if (pageEnd >= total) pageEnd = total - 1;

                    pages[i] = (pageStart, pageEnd);
                }
            }
        }

        return pages;
    }

    public static string GetDefaultSql(string tableName) => 
        $@"SELECT * FROM {tableName} LIMIT 100

--  UPDATE {tableName}
--  SET <key0> = <exprValue0> [,<keyN> = <exprValueN>] | <newDoc>
--  [ WHERE <filterExpr> ]

--  INSERT INTO {tableName}[: {{autoIdType}}]
--  VALUES {{doc0}} [, {{docN}}]

-- DELETE {tableName} WHERE <filterExpr>
";

    public static string GetScriptName(IEnumerable<string> names)
    {
        if (!names.Any())
            return "Script 1";

        var maxNum = names
            .Select(c => c.Split(' ').Last())
            .Select(c => Convert.ToInt32(c))
            .Max();

        return $"Script {maxNum + 1}";
    }

    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsMac() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static string GetAppDataPath()
    {
        string path;

        if (IsWindows())
        {
            path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                AppName);
        }
        else if (IsMac())
        {
            path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AppName);
        }
        else // Linux и всё остальное
        {
            path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AppName);
        }

        return Directory.CreateDirectory(path).FullName;
    }

    public static string GetTempPath()
    {
        var tempPath = Path.Combine(GetAppDataPath(), "Temp");
        return Directory.CreateDirectory(tempPath).FullName;
    }
}