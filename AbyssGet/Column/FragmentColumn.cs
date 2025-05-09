using System.Collections.Concurrent;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace AbyssGet.Column;

public class FragmentColumn(ConcurrentDictionary<string, (int, int)> fragmentCounts) : ProgressColumn
{
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        fragmentCounts.TryGetValue(task.Description, out var fragments);
        return new Text($"{fragments.Item1}/{fragments.Item2}", Style.Plain);
    }
}