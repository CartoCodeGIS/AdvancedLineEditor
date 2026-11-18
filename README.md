# Advanced Line Editor for ArcGIS Pro

Advanced Line Editor is a free ArcGIS Pro 3.5 add-in for quickly repositioning an attached lateral at a measured location along a main line. It moves the lateral by translation instead of rebuilding it, preserving the lateral's length, shape, angle, vertices, curves, Z and M values, and attributes. The tool is useful for sewer laterals, water services, stormwater leads, conduit connections, and similar line-editing workflows.

[Read the complete guide](https://cartocodegis.com/plugins/advanced-line-editor)

## What it does

- Places one selected lateral at a measured distance along one selected reference line.
- Measures from either the reference line's start node or end node.
- Automatically uses the lateral endpoint closest to the reference line as the attachment point.
- Preserves the lateral's original geometry and orientation by translating the complete feature.
- Creates a normal ArcGIS Pro edit operation so the result can be reviewed, undone, or saved.

## Requirements

- ArcGIS Pro 3.5 on Windows.
- Two separate polyline feature layers: a reference layer and an editable lateral layer.
- A projected coordinate system on the reference layer.
- A defined coordinate system on the lateral layer.
- One selected, single-part, continuous line feature in each layer.

## Installation

1. Download `AdvancedLineEditor-v1.0.0.esriAddinX` from the latest GitHub release.
2. Close ArcGIS Pro.
3. Double-click the downloaded file and choose **Install Add-In**.
4. Restart ArcGIS Pro.
5. Open **Advanced Line Editor** from the **Line Editing** group on the **Add-In** tab.

## Quick start

1. Open a map containing the main-line layer and lateral layer.
2. Open **Advanced Line Editor** and click **Refresh layers**.
3. Choose the main-line layer as **Line A** and the editable lateral layer as **Line B**.
4. Select exactly one feature in each layer.
5. Enter the distance along Line A in feet.
6. Choose **Start Node** or **End Node** as the measurement origin.
7. Click **Move Line B**.
8. Inspect the result, then use ArcGIS Pro's **Save Edits** or **Undo** command.

## How placement works

**Start Node** is the first vertex of Line A, and **End Node** is its last vertex. The tool measures the requested planar distance along Line A from the selected origin. It then identifies the endpoint of Line B nearest to Line A and translates the complete Line B geometry so that endpoint lands on the measured point.

If both lateral endpoints are equally close to Line A, the first endpoint is used. Distances are entered in feet and converted to the coordinate units used by Line A.

## Example uses

- Move a sanitary sewer lateral to a surveyed station along a sewer main.
- Reposition a water service connection while preserving its existing angle and shape.
- Align a stormwater lead or conduit connection to a measured point on a trunk line.
- Correct schematic or planning linework without redrawing the attached feature.

## Important limitations

- The requested distance must fall between zero and the full length of Line A.
- Line A and Line B must be different layers.
- Each selected feature must be a single continuous polyline with positive length.
- The tool translates Line B; it does not rotate, reshape, trim, extend, split, or snap other connected features.
- Moving a lateral can separate its opposite endpoint from another feature. Review nearby geometry and topology after every move.
- Review all output before using it for engineering, maintenance, regulatory, or emergency decisions.

## Build

```powershell
dotnet build .\LineEditor.csproj -c Release
```

## Package

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\package-addin.ps1 -Configuration Release
```

The installable add-in is created at:

```text
.\dist\AdvancedLineEditor-v1.0.0.esriAddinX
```
