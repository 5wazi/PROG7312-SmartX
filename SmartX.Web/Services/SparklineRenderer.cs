using System.Globalization;

namespace SmartX.Web.Services;

/// <summary>
/// Renders the same small inline SVG sparkline used on the initial server-rendered
/// paint of the dashboard. site.js re-implements this exact drawing logic in
/// JavaScript for the AJAX-polled live updates — see renderSparkline() in site.js.
/// </summary>
public static class SparklineRenderer
{
    private const int Width = 140;
    private const int Height = 44;

    public static string Render(List<double> values, double safeMin, double safeMax)
    {
        if (values.Count < 2)
        {
            return $"<svg width='{Width}' height='{Height}'></svg>";
        }

        var min = Math.Min(values.Min(), safeMin);
        var max = Math.Max(values.Max(), safeMax);
        if (Math.Abs(max - min) < 0.0001)
        {
            max += 1;
        }

        var step = (double)Width / (values.Count - 1);
        var points = string.Join(" ", values.Select((v, i) =>
        {
            var x = i * step;
            var y = Height - ((v - min) / (max - min)) * Height;
            var xs = x.ToString("0.#", CultureInfo.InvariantCulture);
            var ys = y.ToString("0.#", CultureInfo.InvariantCulture);
            return $"{xs},{ys}";
        }));

        var lastInRange = values[^1] >= safeMin && values[^1] <= safeMax;
        var stroke = lastInRange ? "#2e7d5b" : "#b3432b";

        return $"<svg width='{Width}' height='{Height}' viewBox='0 0 {Width} {Height}'>" +
               $"<polyline fill='none' stroke='{stroke}' stroke-width='2' points='{points}' /></svg>";
    }
}
