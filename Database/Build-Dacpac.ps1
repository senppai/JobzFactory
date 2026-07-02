# Build jobzFactory.Database.dacpac (requires SSDT / Visual Studio SQL Data Tools)
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sqlproj = Join-Path $projectRoot 'jobzFactory.Database\jobzFactory.Database.sqlproj'

if (-not (Test-Path $sqlproj)) {
    throw "SQL project not found: $sqlproj"
}

$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
    | Select-Object -First 1

if (-not $msbuild) {
    $msbuild = 'msbuild'
}

Write-Host "Building $sqlproj with $msbuild ..."
& $msbuild $sqlproj /p:Configuration=$Configuration /restore /v:m

$dacpac = Join-Path $projectRoot "jobzFactory.Database\bin\$Configuration\jobzFactory.Database.dacpac"
if (Test-Path $dacpac) {
    Write-Host "DACPAC: $dacpac"
} else {
    Write-Warning "Build finished but DACPAC not found. Install SSDT (SQL Server Data Tools) in Visual Studio."
}
