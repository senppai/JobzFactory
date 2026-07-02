# One-time / repeat setup: LocalDB + schema + build + start all sites.
# Run from repo root: .\scripts\Setup-LocalMachine.ps1

$ErrorActionPreference = "Stop"
$solutionRoot = Split-Path -Parent $PSScriptRoot
$dbFolder = Join-Path $solutionRoot "Database"
$server = "(localdb)\MSSQLLocalDB"

Write-Host "=== JobzFactory local setup ===" -ForegroundColor Cyan

# 1. LocalDB
Write-Host "`n[1/4] Starting SQL LocalDB..."
sqllocaldb start MSSQLLocalDB | Out-Null

$dbExists = sqlcmd -S $server -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name=N'jobzFactory'" -h -1 -W
if ([int]$dbExists -eq 0) {
    Write-Host "Deploying database scripts..."
    $scripts = @(
        "jobzFactory_CreateDatabase.sql",
        "jobzFactory_SeedData.sql",
        "jobzFactory_SampleData.sql",
        "jobzFactory_EnglishJobs.sql",
        "jobzFactory_ActionOffre_Update.sql"
    )
    foreach ($s in $scripts) {
        $path = Join-Path $dbFolder $s
        Write-Host "  -> $s"
        sqlcmd -S $server -b -i $path | Out-Null
    }
} else {
    Write-Host "Database jobzFactory already exists — skipping scripts."
}

# 2. Build
Write-Host "`n[2/4] Building solution..."
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if (-not $msbuild) {
    $msbuild = "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
}
& $msbuild (Join-Path $solutionRoot "JobzFactory.sln") /p:Configuration=Debug /v:q /m

# 3. Stop old IIS Express
Write-Host "`n[3/4] Restarting IIS Express sites..."
Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

# 4. Start sites
Write-Host "`n[4/4] Starting web portals..."
& (Join-Path $PSScriptRoot "Start-AllWebSites.ps1")

Write-Host "`n=== Ready ===" -ForegroundColor Green
Write-Host "Open SSMS and connect to: (localdb)\MSSQLLocalDB  (database: jobzFactory)"
Write-Host "Test login: recruteur.demo / 123"
