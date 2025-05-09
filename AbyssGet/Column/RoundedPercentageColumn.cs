using Spectre.Console;
using Spectre.Console.Rendering;

namespace AbyssGet.Column;

public sealed class RoundedPercentageColumn(int decimals) : ProgressColumn
{
    public Style Style { get; set; } = Style.Plain;

    public Style CompletedStyle { get; set; } = Color.Green;

    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var percentage = task.Percentage;
        var style = (int)percentage == 100 ? CompletedStyle : Style;
        return new Text($"{Math.Round(percentage, decimals)}%", style).RightJustified();
    }

    public override int? GetColumnWidth(RenderOptions options)
    {
        return 4 + decimals + 1;
    }
}