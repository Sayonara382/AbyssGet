using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace AbyssGet.Util;

public class CommandLine
{
    private static readonly Argument<IEnumerable<string>> VideoIdArguments = new("videoIds", "Video ID `K8R6OOjS7` or Player URL `https://abysscdn.com/?v=K8R6OOjS7`")
    {
        Arity = ArgumentArity.OneOrMore
    };
    
    private static readonly Option FirstUrlOnlyOption = new Option<bool>(["--first-url-only", "-f"], () => false, "Only use the first CDN url");
    private static readonly Option BestUrlPoolSizeOption = new Option<int>(["--best-url-pool-size", "-ps"], () => 3, "Size of the URL pool containing the best performing URLs");
    private static readonly Option MaxThreadsOption = new Option<int>(["--max-threads", "-t"], () => 16, "Maximum number of threads");
    private static readonly Option LogLevelOption = new Option<LogLevel>(["--log-level", "-l"], () => LogLevel.Information, "Log level");
    private static readonly Option RequestTimeoutOption = new Option<int>(["--request-timeout", "-rt"], () => 120, "Request timeout in seconds");
    private static readonly Option BlockTimeoutOption = new Option<int>(["--block-timeout", "-bt"], () => 60, "Block timeout in seconds");
    private static readonly Option RequestRetriesOption = new Option<int>(["--request-retries", "-rr"], () => 3, "Number of request retries");
    private static readonly Option DownloadInParallelOption = new Option<bool>(["--download-in-parallel", "-p"], () => false, "Download videos in parallel");
    private static readonly Option SaveDirOption = new Option<string>(["--save-dir", "-s"], () => ".", "Directory to save downloaded files");
    private static readonly Option SaveNameOption = new Option<string>(["--save-name", "-n"], () => "", "Custom name for the downloaded file (without extension)");
    private static readonly Option AutoSelectOption = new Option<bool>(["--auto-select", "-a"], () => false, "Automatically select the best quality to download");
    
    public static Command GetRootCommand()
    {
        var rootCommand = new RootCommand("Download one or more videos from abyss.to by either their Video ID or Player URL.")
        {
            Handler = CommandHandler.Create<IEnumerable<string>, bool, int, int, LogLevel, int, int, int, bool, string, string, bool>(HandleCommandAsync)
        };

        rootCommand.AddArgument(VideoIdArguments);
        
        rootCommand.AddOption(FirstUrlOnlyOption);
        rootCommand.AddOption(BestUrlPoolSizeOption);
        rootCommand.AddOption(MaxThreadsOption);
        rootCommand.AddOption(LogLevelOption);
        rootCommand.AddOption(RequestTimeoutOption);
        rootCommand.AddOption(BlockTimeoutOption);
        rootCommand.AddOption(RequestRetriesOption);
        rootCommand.AddOption(DownloadInParallelOption);
        rootCommand.AddOption(SaveDirOption);
        rootCommand.AddOption(SaveNameOption);
        rootCommand.AddOption(AutoSelectOption);
        
        return rootCommand;
    }
    
    private static async Task HandleCommandAsync(IEnumerable<string> videoIds, bool firstUrlOnly, int bestUrlPoolSize, int maxThreads, LogLevel logLevel, int requestTimeout, int blockTimeout, int requestRetries, bool downloadInParallel, string saveDir, string saveName, bool autoSelect) 
    {
        var settings = new Settings
        {
            FirstUrlOnly = firstUrlOnly,
            BestUrlPoolSize = bestUrlPoolSize,
            MaxThreads = maxThreads,
            LogLevel = logLevel,
            RequestTimeout = TimeSpan.FromSeconds(requestTimeout),
            BlockTimeout = TimeSpan.FromSeconds(blockTimeout),
            RequestRetries = requestRetries,
            DownloadInParallel = downloadInParallel,
            OutputDirectory = saveDir,
            SaveName = saveName,
            AutoSelect = autoSelect
        };

        var abyss = new Abyss(settings);
        await abyss.DownloadVideosWithPrompt(videoIds);
    }
}