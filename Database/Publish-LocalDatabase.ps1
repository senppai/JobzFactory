# Publish jobzFactory dacpac to local SQL Server (default instance: data source=.)
param(
    [string]$Server = '.',
    [string]$Database = 'jobzFactory',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$dacpac = Join-Path $scriptRoot "jobzFactory.Database\bin\$Configuration\jobzFactory.Database.dacpac"

if (-not (Test-Path $dacpac)) {
    & (Join-Path $scriptRoot 'Build-Dacpac.ps1') -Configuration $Configuration
}

if (-not (Test-Path $dacpac)) {
    throw "DACPAC missing. Build the SQL project first."
}

$sqlpackage = Get-ChildItem -Path "${env:ProgramFiles}\Microsoft SQL Server", "${env:ProgramFiles(x86)}\Microsoft SQL Server" `
    -Recurse -Filter 'SqlPackage.exe' -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending |
    Select-Object -First 1 -ExpandProperty FullName

if (-not $sqlpackage) {
    throw 'SqlPackage.exe not found. Install SQL Server Data Tools or SSMS.'
}

Write-Host "Publishing to $Server / $Database ..."
& $sqlpackage `
    /Action:Publish `
    /SourceFile:$dacpac `
    /TargetConnectionString:"Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True"

Write-Host 'Publish complete.'
