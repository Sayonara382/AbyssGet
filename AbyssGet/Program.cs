using System.CommandLine;
using AbyssGet.Util;

namespace AbyssGet;

class Program
{
    private static async Task Main(string[] args)
    {
        var rootCommand = CommandLine.GetRootCommand();
        await rootCommand.InvokeAsync(args);
    }
}