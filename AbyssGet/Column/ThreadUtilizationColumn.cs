using System.Collections.Concurrent;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace AbyssGet.Column;

public sealed class ThreadUtilizationColumn(ConcurrentDictionary<string, int> threadCounts, int maxThreads) : ProgressColumn
{
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        threadCounts.TryGetValue(task.Description, out var threads);
        return new Text($"{threads}T", Style.Plain);
    }
    
    public override int? GetColumnWidth(RenderOptions options)
    {
        return maxThreads.ToString().Length + 1;
    }
}