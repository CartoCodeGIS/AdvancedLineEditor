using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace LineEditor;

internal sealed class EditorModule : Module { }
internal sealed class EditorPane : DockPane { }

internal sealed class ShowEditorButton : Button
{
    protected override void OnClick() =>
        FrameworkApplication.DockPaneManager.Find("LineEditor_Dockpane")?.Activate();
}

