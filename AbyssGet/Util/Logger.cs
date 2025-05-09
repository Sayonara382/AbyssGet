using Spectre.Console;

namespace AbyssGet.Util;

public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error
}

public class Logger(LogLevel logLevel)
{
    private static string GetTime() => DateTime.Now.ToString("HH:mm:ss");

    private static string GetVideoString(Video video) => $"[{video.Slug!} | {video.Label, 5}]";

    private static void Log(string message, string color, string mode, Video? video = null)
    {
        if (video == null)
            AnsiConsole.MarkupLineInterpolated($"{GetTime()} [white on {color}]{mode}[/]: {message}");
        else
            AnsiConsole.MarkupLineInterpolated($"{GetTime()} [white on {color}]{mode}[/]: {GetVideoString(video)} {message}");
    }
    
    public void LogDebug(string message, Video? video = null)
    {
        if (logLevel > LogLevel.Debug) return;
        Log(message, "grey", "Debg", video);
    }

    public void LogInfo(string message, Video? video = null)
    {
        if (logLevel > LogLevel.Information) return;
        Log(message, "darkgreen", "Info", video);
    }
    
    public void LogWarn(string message, Video? video = null)
    {
        if (logLevel > LogLevel.Warning) return;
        Log(message, "darkorange3", "Warn", video);
    }

    public void LogError(string message, Video? video = null)
    {
        if (logLevel > LogLevel.Error) return;
        Log(message, "darkred_1", "Eror", video);
    }
}