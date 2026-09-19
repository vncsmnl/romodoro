[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+$')]
    [string] $Version,
    [string] $Repository = 'vncsmnl/romodoro',
    [Parameter(Mandatory)][string] $InstallerX86Path,
    [Parameter(Mandatory)][string] $InstallerX64Path,
    [Parameter(Mandatory)][string] $InstallerArm64Path,
    [Parameter(Mandatory)][string] $OutputDirectory
)

$ErrorActionPreference = 'Stop'

function Get-Sha256Hex {
    param([Parameter(Mandatory)][string] $Path)
    $sha256 = [Security.Cryptography.SHA256]::Create()
    $stream = [IO.File]::OpenRead($Path)
    try {
        return ([BitConverter]::ToString($sha256.ComputeHash($stream)) -replace '-', '').ToLowerInvariant()
    }
    finally {
        $stream.Dispose()
        $sha256.Dispose()
    }
}

function Get-InstallerData([string] $Architecture, [string] $Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Installer not found: $Path" }
    $file = Get-Item -LiteralPath $Path
    if ($file.Extension -ne '.exe') { throw "Installer must be an EXE: $Path" }
    [pscustomobject]@{
        Architecture = $Architecture
        FileName = $file.Name
        Url = "https://github.com/$Repository/releases/download/v$Version/$($file.Name)"
        Sha256 = Get-Sha256Hex -Path $file.FullName
    }
}

$installers = @(
    (Get-InstallerData 'x86' $InstallerX86Path)
    (Get-InstallerData 'x64' $InstallerX64Path)
    (Get-InstallerData 'arm64' $InstallerArm64Path)
)

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$templateRoot = Join-Path (Split-Path -Parent $PSScriptRoot) 'packaging\winget'
$templateNames = @(
    'vncsmnl.Romodoro.yaml.template',
    'vncsmnl.Romodoro.installer.yaml.template',
    'vncsmnl.Romodoro.locale.en-US.yaml.template'
)
$tokens = @{
    '{{VERSION}}' = $Version
    '{{URL_X86}}' = $installers[0].Url
    '{{SHA256_X86}}' = $installers[0].Sha256
    '{{URL_X64}}' = $installers[1].Url
    '{{SHA256_X64}}' = $installers[1].Sha256
    '{{URL_ARM64}}' = $installers[2].Url
    '{{SHA256_ARM64}}' = $installers[2].Sha256
}

foreach ($templateName in $templateNames) {
    $templatePath = Join-Path $templateRoot $templateName
    if (-not (Test-Path -LiteralPath $templatePath -PathType Leaf)) { throw "Manifest template not found: $templatePath" }
    $content = Get-Content -Raw -LiteralPath $templatePath
    foreach ($token in $tokens.Keys) { $content = $content.Replace($token, [string] $tokens[$token]) }
    if ($content -match '\{\{[^}]+\}\}') { throw "Unresolved manifest token in $templateName" }
    $outputName = $templateName -replace '\.template$', ''
    Set-Content -LiteralPath (Join-Path $OutputDirectory $outputName) -Value $content -Encoding utf8
}
