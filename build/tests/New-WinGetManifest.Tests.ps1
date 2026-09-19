$ErrorActionPreference = 'Stop'

function Assert-True([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function Assert-Contains([string] $Text, [string] $Needle, [string] $Message) {
    Assert-True ($Text.IndexOf($Needle, [StringComparison]::Ordinal) -ge 0) "${Message}: '$Needle'"
}

$scriptPath = Join-Path (Join-Path $PSScriptRoot '..') 'New-WinGetManifest.ps1'
$fixtureRoot = Join-Path $PSScriptRoot 'winget-fixtures'
$output = Join-Path ([IO.Path]::GetTempPath()) "romodoro-winget-$PID"
New-Item -ItemType Directory -Force -Path $output | Out-Null

try {
    & $scriptPath `
        -Version '0.2.0' `
        -Repository 'vncsmnl/romodoro' `
        -InstallerX86Path (Join-Path $fixtureRoot 'Romodoro-win-x86-setup.exe') `
        -InstallerX64Path (Join-Path $fixtureRoot 'Romodoro-win-x64-setup.exe') `
        -InstallerArm64Path (Join-Path $fixtureRoot 'Romodoro-win-arm64-setup.exe') `
        -OutputDirectory $output

    $version = Get-Content -Raw -LiteralPath (Join-Path $output 'vncsmnl.Romodoro.yaml')
    $installer = Get-Content -Raw -LiteralPath (Join-Path $output 'vncsmnl.Romodoro.installer.yaml')
    $locale = Get-Content -Raw -LiteralPath (Join-Path $output 'vncsmnl.Romodoro.locale.en-US.yaml')

    Assert-Contains $version 'PackageVersion: 0.2.0' 'version manifest version'
    Assert-Contains $locale 'PackageLocale: en-US' 'locale manifest locale'
    Assert-Contains $installer 'Architecture: x86' 'x86 installer'
    Assert-Contains $installer 'Architecture: x64' 'x64 installer'
    Assert-Contains $installer 'Architecture: arm64' 'arm64 installer'
    Assert-Contains $installer 'PackageIdentifier: Microsoft.DotNet.Runtime.10' 'runtime dependency'
    Assert-Contains $installer 'Silent: /VERYSILENT /SUPPRESSMSGBOXES /NORESTART' 'silent switches'
    Assert-Contains $installer 'InstallerUrl: https://github.com/vncsmnl/romodoro/releases/download/v0.2.0/Romodoro-win-x64-setup.exe' 'release URL'
    Assert-True ($installer -notmatch '\{\{[^}]+\}\}') 'manifest contains unresolved tokens'
    Assert-True ((Get-ChildItem -LiteralPath $output -File).Count -eq 3) 'manifest output count'

    Write-Output 'PASS WinGet manifest generation and hash/dependency tests'
}
finally {
    Remove-Item -LiteralPath $output -Recurse -Force -ErrorAction SilentlyContinue
}
