using Conectify.Services.UI.Models;
using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Values;

namespace Conectify.Services.UI.Services;

/// <summary>
/// Turns raw metadata + history values into the pre-computed shapes the cubes render, so the
/// browser never has to interpret metadata itself.
/// </summary>
public static class UiCubeMapper
{
    private const string VisibleMeta = "Visible";
    private const string NameMeta = "Name";
    private const string ThresholdMeta = "Threshold";
    private const string IoTypeMeta = "IOType";

    // Metadata we consume here - excluded from the icon list since it never maps to an icon.
    private static readonly HashSet<string> ControlMeta = new(StringComparer.Ordinal)
    {
        VisibleMeta, NameMeta, ThresholdMeta, IoTypeMeta,
    };

    public static bool IsVisible(IReadOnlyCollection<ApiMetadata> metadata)
    {
        var visible = metadata.FirstOrDefault(m => m.Name == VisibleMeta);
        return visible is null || (visible.NumericValue ?? 0) > 0;
    }

    public static string? ShowName(IReadOnlyCollection<ApiMetadata> metadata) =>
        metadata.FirstOrDefault(m => m.Name == NameMeta)?.StringValue;

    public static List<UiThreshold> Thresholds(IReadOnlyCollection<ApiMetadata> metadata) =>
        metadata
            .Where(m => m.Name == ThresholdMeta)
            .Select(m => new UiThreshold { Min = m.MinVal ?? 0, Max = m.MaxVal ?? 0, Color = m.StringValue })
            .ToList();

    public static List<UiIcon> Icons(IReadOnlyCollection<ApiMetadata> metadata) =>
        metadata
            .Where(m => !ControlMeta.Contains(m.Name))
            .Select(m => new UiIcon { Name = m.Name, StringValue = m.StringValue })
            .ToList();

    public static (int IoType, float Min, float Max, float Value) IoType(IReadOnlyCollection<ApiMetadata> metadata)
    {
        var io = metadata.FirstOrDefault(m => m.Name == IoTypeMeta);
        if (io is null)
        {
            return (-1, 0, 0, 0);
        }

        var type = io.StringValue switch
        {
            "Linear" => 0,
            "Binary" => 1,
            "Trigger" => 2,
            "Color" => 3,
            "CCT" => 4,
            _ => -1,
        };

        return (type, io.MinVal ?? 0, io.MaxVal ?? 0, io.NumericValue ?? 0);
    }

    public static UiValue? ToUiValue(ApiEvent? e) => e is null ? null : new UiValue
    {
        Name = e.Name,
        Unit = e.Unit,
        StringValue = e.StringValue,
        NumericValue = e.NumericValue,
        TimeCreated = e.TimeCreated,
    };

    public static List<double?[]> ToPoints(IEnumerable<ApiEvent> values) =>
        values.Select(e => new double?[] { e.TimeCreated, e.NumericValue }).ToList();
}
