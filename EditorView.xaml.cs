using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace LineEditor;

public partial class EditorView : UserControl
{
    public EditorView() => InitializeComponent();

    private async void RefreshClicked(object sender, RoutedEventArgs e)
    {
        if (!Form.IsEnabled)
            return;

        var view = MapView.Active;
        await RunAsync(async () =>
        {
            var lines = await QueuedTask.Run(() =>
                view?.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                    .Where(layer => layer.ShapeType == esriGeometryType.esriGeometryPolyline)
                    .Select(layer => new LayerChoice(layer, layer.Name))
                    .ToArray() ?? []);
            UpdateChoices(LineALayer, lines);
            UpdateChoices(LineBLayer, lines);
            return view is null ? "Open a map, then refresh layers." : "Choose the Line A and Line B layers.";
        });
    }

    private static void UpdateChoices(ComboBox combo, LayerChoice[] options)
    {
        var previous = (combo.SelectedItem as LayerChoice)?.Layer;
        combo.ItemsSource = options;
        combo.SelectedItem = options.FirstOrDefault(option => option.Layer == previous);
    }

    private async void MoveClicked(object sender, RoutedEventArgs e)
    {
        if (LineALayer.SelectedItem is not LayerChoice lineA ||
            LineBLayer.SelectedItem is not LayerChoice lineB || lineA.Layer == lineB.Layer)
        {
            Status.Text = "Choose separate layers for Line A and Line B.";
            return;
        }

        if (!double.TryParse(Distance.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var feet) ||
            !double.IsFinite(feet) || feet < 0)
        {
            Status.Text = "Enter a distance of zero or more feet.";
            return;
        }

        var fromEnd = Origin.SelectedIndex == 1;
        var view = MapView.Active;
        Status.Text = "Finding the measured point on Line A...";
        await RunAsync(() => QueuedTask.Run(() =>
            LinePlacement.Move(view, lineA.Layer, lineB.Layer, feet, fromEnd)));
    }

    private async Task RunAsync(Func<Task<string>> work)
    {
        Form.IsEnabled = false;
        try
        {
            Status.Text = await work();
        }
        catch (Exception ex)
        {
            Status.Text = ex.Message;
        }
        finally
        {
            Form.IsEnabled = true;
        }
    }

    private sealed record LayerChoice(FeatureLayer Layer, string Name);
}
