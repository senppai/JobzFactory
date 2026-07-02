# Export Cursor IDE settings for transfer to another machine.
$ErrorActionPreference = "Stop"

$ExportRoot = Join-Path $PSScriptRoot "..\docs\Cursor_Settings_Export"
$ExportRoot = (Resolve-Path $ExportRoot -ErrorAction SilentlyContinue).Path
if (-not $ExportRoot) {
    $ExportRoot = Join-Path (Split-Path $PSScriptRoot -Parent) "docs\Cursor_Settings_Export"
}
New-Item -ItemType Directory -Force -Path $ExportRoot | Out-Null

$UserProfile = $env:USERPROFILE
$AppDataUser = Join-Path $env:APPDATA "Cursor\User"

$sources = @(
    @{ Source = Join-Path $AppDataUser "settings.json"; Dest = "settings.json" },
    @{ Source = Join-Path $AppDataUser "keybindings.json"; Dest = "keybindings.json" },
    @{ Source = Join-Path $UserProfile ".cursor\argv.json"; Dest = "argv.json" },
    @{ Source = Join-Path $UserProfile ".cursor\extensions\extensions.json"; Dest = "extensions.json" }
)

$copied = @()
$missing = @()

foreach ($item in $sources) {
    if (Test-Path $item.Source) {
        Copy-Item -Path $item.Source -Destination (Join-Path $ExportRoot $item.Dest) -Force
        $copied += $item.Dest
    } else {
        $missing += $item.Dest
    }
}

# Plain-text extension list for easy reinstall
$extFile = Join-Path $ExportRoot "extensions-list.txt"
if (Test-Path (Join-Path $ExportRoot "extensions.json")) {
    $extensions = Get-Content (Join-Path $ExportRoot "extensions.json") -Raw | ConvertFrom-Json
    $lines = foreach ($ext in $extensions) {
        $id = $ext.identifier.id
        $ver = $ext.version
        if ($ver) { "$id@$ver" } else { $id }
    }
    $lines | Set-Content -Path $extFile -Encoding UTF8
    $copied += "extensions-list.txt"
}

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
$settingsPath = Join-Path $ExportRoot "settings.json"
$keybindingsPath = Join-Path $ExportRoot "keybindings.json"

$readme = @"
# Cursor Settings Export

Exported: $timestamp  
Machine: $env:COMPUTERNAME  
User: $env:USERNAME

## Files in this folder

| File | Purpose |
|------|---------|
| settings.json | Editor + Cursor preferences |
| keybindings.json | Custom keyboard shortcuts |
| argv.json | Cursor startup arguments |
| extensions.json | Full installed extensions metadata |
| extensions-list.txt | Extension IDs for bulk reinstall |
| RESTORE.md | Step-by-step restore guide |

## Copied

$($copied | ForEach-Object { "- $_" } | Out-String)

## Missing (not found on this machine)

$(if ($missing.Count) { ($missing | ForEach-Object { "- $_" }) -join "`n" } else { "- none" })

## Quick restore (new PC)

1. Install Cursor and sign in.
2. Close Cursor completely.
3. Copy files from this folder to the paths in RESTORE.md.
4. Reinstall extensions: see extensions-list.txt
5. Restart Cursor.

## Does NOT transfer automatically

- Cursor account login (sign in again)
- Chat history (use Cursor_Chat_History_Combined.md)
- API keys / tokens in extensions
- MCP servers (mcp.json was not configured on source machine)
- Project rules (.cursor/rules — none in this project)
"@

Set-Content -Path (Join-Path $ExportRoot "README.md") -Value $readme -Encoding UTF8

$restore = @"
# Restore Cursor settings on a new PC

## 1. Copy config files

Close Cursor first, then copy:

| From (this export) | To (new PC) |
|--------------------|-------------|
| settings.json | %APPDATA%\Cursor\User\settings.json |
| keybindings.json | %APPDATA%\Cursor\User\keybindings.json |
| argv.json | %USERPROFILE%\.cursor\argv.json |

PowerShell example (adjust source path):

``````powershell
`$src = "D:\project\JobzFactory\docs\Cursor_Settings_Export"
Copy-Item "`$src\settings.json" "$env:APPDATA\Cursor\User\settings.json" -Force
Copy-Item "`$src\keybindings.json" "$env:APPDATA\Cursor\User\keybindings.json" -Force
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\.cursor" | Out-Null
Copy-Item "`$src\argv.json" "$env:USERPROFILE\.cursor\argv.json" -Force
``````

## 2. Reinstall extensions

Option A — one by one in Cursor Extensions panel using extensions-list.txt

Option B — bulk install (Cursor CLI if available):

``````powershell
Get-Content extensions-list.txt | ForEach-Object {
    `$id = `$_ -replace '@.*',''
    cursor --install-extension `$id
}
``````

## 3. Merge vs replace

If the new PC already has settings, **merge** settings.json manually instead of overwriting, or back up the existing file first.

## 4. Cursor-specific settings in this export

See settings.json. Notable Cursor keys:

- cursor.composer.usageSummaryDisplay
- claudeCode.preferredLocation

## 5. Optional: Settings Sync

In Cursor: **Settings → turn on Settings Sync** to keep settings/extensions in sync across machines going forward.
"@

Set-Content -Path (Join-Path $ExportRoot "RESTORE.md") -Value $restore -Encoding UTF8

Write-Host "Export complete: $ExportRoot"
Write-Host "Files: $($copied -join ', ')"
