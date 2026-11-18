using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;

namespace LineEditor;

internal static class LinePlacement
{
    public static string Move(MapView? view, FeatureLayer lineALayer, FeatureLayer lineBLayer,
        double feet, bool fromEnd)
    {
        var layers = view?.Map.GetLayersAsFlattenedList();
        if (layers is null || !layers.Contains(lineALayer) || !layers.Contains(lineBLayer))
            throw new InvalidOperationException("Refresh layers for the active map.");

        if (lineALayer == lineBLayer)
            throw new InvalidOperationException("Choose separate layers for Line A and Line B.");

        if (!lineBLayer.IsEditable)
            throw new InvalidOperationException("Enable editing for the Line B layer.");

        var lineAIds = SelectedIds(lineALayer);
        var lineBIds = SelectedIds(lineBLayer);
        if (lineAIds.Length != 1 || lineBIds.Length != 1)
            throw new InvalidOperationException("Select exactly one feature in Line A and one in Line B.");

        var lineA = ReadLine(lineALayer, lineAIds[0], "Line A");
        var originalB = ReadLine(lineBLayer, lineBIds[0], "Line B");
        var referenceA = lineA.SpatialReference;
        var referenceB = originalB.SpatialReference;
        if (referenceA is null || !referenceA.IsProjected)
            throw new InvalidOperationException("Line A needs a projected coordinate system to measure distance.");
        if (referenceB is null || referenceB.IsUnknown)
            throw new InvalidOperationException("Line B needs a defined coordinate system.");

        var unitsPerFoot = 0.3048 / referenceA.Unit.ConversionFactor;
        var distance = feet * unitsPerFoot;
        if (!double.IsFinite(distance) || distance < 0 || distance > lineA.Length + referenceA.XYTolerance)
            throw new InvalidOperationException($"Enter a distance from zero to {lineA.Length / unitsPerFoot:N2} feet.");

        var engine = GeometryEngine.Instance;
        distance = Math.Min(distance, lineA.Length);
        var targetA = engine.QueryPoint(lineA, SegmentExtensionType.NoExtension,
            fromEnd ? lineA.Length - distance : distance, AsRatioOrLength.AsLength);
        var targetB = (MapPoint)engine.Project(targetA, referenceB);

        // Choose the endpoint nearest Line A, then translate the original geometry in its own coordinates.
        var first = (MapPoint)engine.Project(originalB.Points[0], referenceA);
        var last = (MapPoint)engine.Project(originalB.Points[^1], referenceA);
        var attachment = engine.Distance(lineA, last) < engine.Distance(lineA, first)
            ? originalB.Points[^1] : originalB.Points[0];
        var moved = engine.Move(originalB, targetB.X - attachment.X, targetB.Y - attachment.Y);
        var edit = new EditOperation { Name = "Move Line B along Line A" };
        edit.Modify(lineBLayer, lineBIds[0], moved);
        if (!edit.Execute())
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(edit.ErrorMessage)
                ? "ArcGIS Pro could not apply the edit." : edit.ErrorMessage);

        return $"Moved Line B feature {lineBIds[0]} to {distance / unitsPerFoot:G8} ft from Line A's {(fromEnd ? "End Node" : "Start Node")}. Use Undo or Save Edits in ArcGIS Pro.";
    }

    private static long[] SelectedIds(FeatureLayer layer)
    {
        using var selection = layer.GetSelection();
        return selection.GetObjectIDs().ToArray();
    }

    private static Polyline ReadLine(FeatureLayer layer, long oid, string label)
    {
        using var cursor = layer.Search(new QueryFilter { ObjectIDs = [oid] });
        if (!cursor.MoveNext())
            throw new InvalidOperationException($"The selected {label} could not be read.");

        using var row = cursor.Current;
        if (row is not Feature feature || feature.GetShape() is not Polyline line ||
            line.IsEmpty || line.PartCount != 1 || line.Length <= 0)
            throw new InvalidOperationException($"The selected {label} must be a single continuous line with positive length.");

        return line;
    }
}
