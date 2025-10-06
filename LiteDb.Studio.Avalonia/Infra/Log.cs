using System.Diagnostics;

namespace LiteDb.Studio.Avalonia.Infra;

public enum LogType
{
    Info,
    Warn,
    Debug,
    Error
}

public static class Log
{
    private static void Write(string msg) => Console.WriteLine(msg);

    private static void DoLog(LogType logType, string msg)
    {
        switch (logType)
        {
            case LogType.Info:
                Write($"INFO: {msg}");
                break;
            case LogType.Warn:
                Write($"WARN: {msg}");
                break;
            case LogType.Error:
                Write($"ERROR: {msg}");
                break;
            case LogType.Debug:
                Debug.WriteLine(msg);
                break;
        }
    }

    public static void LogInfo(string msg) => DoLog(LogType.Info, msg);
    public static void LogError(string msg) => DoLog(LogType.Error, msg);
    public static void LogWarn(string msg) => DoLog(LogType.Warn, msg);
    public static void LogDebug(string msg) => DoLog(LogType.Debug, msg);

    public static void LogExc(Exception exc) =>
        LogError(exc.ToString());
}