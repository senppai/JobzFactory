# Starts JobzFactory, Recruteur, and Administration under IIS Express (no Visual Studio required).
# Usage: .\scripts\Start-AllWebSites.ps1

$ErrorActionPreference = "Stop"

$solutionRoot = Split-Path -Parent $PSScriptRoot
$iisExpress = Join-Path ${env:ProgramFiles} "IIS Express\iisexpress.exe"

if (-not (Test-Path $iisExpress)) {
    $iisExpress = Join-Path ${env:ProgramFiles(x86)} "IIS Express\iisexpress.exe"
}

if (-not (Test-Path $iisExpress)) {
    throw "IIS Express was not found. Install it with Visual Studio (ASP.NET workload)."
}

function Start-WebSite {
    param(
        [string]$Name,
        [string]$Path,
        [int]$Port,
        [int]$SslPort = 0
    )

    if (-not (Test-Path $Path)) {
        throw "Project path not found for ${Name}: ${Path}"
    }

    $args = @(
        "/path:`"$Path`"",
        "/port:$Port",
        "/clr:v4.0",
        "/systray:true"
    )

    if ($SslPort -gt 0) {
        $args += "/sslport:$SslPort"
    }

    Write-Host "Starting $Name on port $Port$(if ($SslPort -gt 0) { " (HTTPS $SslPort)" })..."
    Start-Process -FilePath $iisExpress -ArgumentList $args -WindowStyle Hidden
}

Start-WebSite -Name "JobzFactory" -Path (Join-Path $solutionRoot "JobzFactory") -Port 59579
Start-Sleep -Milliseconds 500
Start-WebSite -Name "Recruteur" -Path (Join-Path $solutionRoot "Recruteur") -Port 60658
Start-Sleep -Milliseconds 500
Start-WebSite -Name "Administration" -Path (Join-Path $solutionRoot "Administration") -Port 44303 -SslPort 44303

Write-Host ""
Write-Host "All sites started:"
Write-Host "  JobzFactory    -> http://localhost:59579/"
Write-Host "  Recruteur      -> http://localhost:60658/Login"
Write-Host "  Administration -> https://localhost:44303/Account/Login"
Write-Host ""
Write-Host "Stop them from the IIS Express tray icon, or close the iisexpress processes."
