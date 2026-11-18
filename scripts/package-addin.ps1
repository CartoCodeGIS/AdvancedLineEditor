param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$buildDir = Join-Path $projectRoot "bin/$Configuration/net8.0-windows"
$dist = Join-Path $projectRoot "dist"
$package = Join-Path $dist "AdvancedLineEditor-v1.0.0.esriAddinX"
$pending = Join-Path $dist "LineEditor.pending"

dotnet build (Join-Path $projectRoot "LineEditor.csproj") -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

New-Item -ItemType Directory -Force $dist | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$stream = [IO.File]::Open($pending, [IO.FileMode]::Create)
try {
    $zip = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create, $true)
    try {
        $entries = @{
            "Config.daml" = Join-Path $projectRoot "Config.daml"
            "Images/AddInDesktop32.png" = Join-Path $projectRoot "Images/AddInDesktop32.png"
            "Install/LineEditor.dll" = Join-Path $buildDir "LineEditor.dll"
            "Install/LineEditor.deps.json" = Join-Path $buildDir "LineEditor.deps.json"
        }
        foreach ($entry in $entries.GetEnumerator()) {
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $entry.Value, $entry.Key) | Out-Null
        }
    }
    finally {
        $zip.Dispose()
    }
}
finally {
    $stream.Dispose()
}
Move-Item -LiteralPath $pending -Destination $package -Force
Write-Host "Created $package"

