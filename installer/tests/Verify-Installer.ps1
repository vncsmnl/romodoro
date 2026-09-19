[CmdletBinding()]
param(
    [string] $IsccPath,
    [string] $PublishDir,
    [string] $OutputDir,
    [ValidateSet('x86', 'x64', 'arm64')]
    [string] $Architecture = 'x64'
)

$ErrorActionPreference = 'Stop'
$scriptPath = Join-Path (Join-Path $PSScriptRoot '..') 'Romodoro.iss'
if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
    throw "Installer script was not found: $scriptPath"
}

$installer = Get-Content -Raw -LiteralPath $scriptPath
foreach ($required in @(
    'PrivilegesRequired=admin',
    'ArchitecturesAllowed=',
    'ArchitecturesInstallIn64BitMode=',
    'Microsoft.NETCore.App',
    'DownloadTemporaryFile',
    '/install /quiet /norestart',
    'skipifsilent'
)) {
    if ($installer.IndexOf($required, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Installer is missing required contract text: $required"
    }
}

if ($IsccPath) {
    if (-not (Test-Path -LiteralPath $IsccPath -PathType Leaf)) {
        throw "Inno Setup compiler was not found: $IsccPath"
    }
    if (-not $PublishDir -or -not (Test-Path -LiteralPath $PublishDir -PathType Container)) {
        throw 'PublishDir is required when compiling the installer.'
    }
    if (-not $OutputDir) { throw 'OutputDir is required when compiling the installer.' }
    $PublishDir = (Resolve-Path -LiteralPath $PublishDir).Path
    $OutputDir = [IO.Path]::GetFullPath($OutputDir)
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
    & $IsccPath "/DAppVersion=0.0.0" "/DRuntimeArchitecture=$Architecture" '/DRuntimeVersion=10.0.12' '/DRuntimeUrl=https://example.invalid/runtime.exe' '/DRuntimeSha256=0000000000000000000000000000000000000000000000000000000000000000' "/DPublishDir=$PublishDir" "/DOutputDir=$OutputDir" $scriptPath
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup compiler failed with exit code $LASTEXITCODE" }
    $output = Join-Path $OutputDir "Romodoro-win-$Architecture-setup.exe"
    if (-not (Test-Path -LiteralPath $output -PathType Leaf)) { throw "Expected installer was not created: $output" }
}

Write-Output 'PASS installer contract checks'
