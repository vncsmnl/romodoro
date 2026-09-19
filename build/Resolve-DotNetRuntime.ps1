[CmdletBinding()]
param(
    [ValidateSet('x86', 'x64', 'arm64')]
    [string] $Architecture,
    [string] $ReleaseMetadataPath
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

function Resolve-DotNetRuntimePackage {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('x86', 'x64', 'arm64')]
        [string] $Architecture,
        [string] $ReleaseMetadataPath
    )

    $fixtureMode = -not [string]::IsNullOrWhiteSpace($ReleaseMetadataPath)
    if ($fixtureMode) {
        if (-not (Test-Path -LiteralPath $ReleaseMetadataPath -PathType Leaf)) {
            throw "Release metadata file was not found: $ReleaseMetadataPath"
        }
        $metadata = Get-Content -Raw -LiteralPath $ReleaseMetadataPath | ConvertFrom-Json
    }
    else {
        $metadata = Invoke-RestMethod -Uri 'https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/10.0/releases.json'
    }

    $release = $metadata.releases |
        Where-Object { $_.'release-version' -match '^10\.0\.\d+$' } |
        Sort-Object { [version] $_.'release-version' } -Descending |
        Select-Object -First 1

    if (-not $release) {
        throw 'No .NET 10 release was found in the metadata.'
    }

    $file = $release.runtime.files |
        Where-Object { $_.name -match "^dotnet-runtime(?:-[\d.]+)?-win-$Architecture\.exe$" } |
        Select-Object -First 1

    if (-not $file) {
        throw "No .NET runtime installer was found for architecture '$Architecture'."
    }
    if ([string]::IsNullOrWhiteSpace($file.url)) {
        throw "Runtime installer URL is empty for architecture '$Architecture'."
    }

    $uri = [Uri] $file.url
    if (-not $fixtureMode -and $uri.Scheme -ne 'https') {
        throw "Runtime installer URL must use HTTPS: $($file.url)"
    }
    if ($fixtureMode -and $uri.Scheme -notin @('https', 'file')) {
        throw "Fixture runtime installer URL must use HTTPS or file URI: $($file.url)"
    }

    $downloadDirectory = Join-Path ([IO.Path]::GetTempPath()) "romodoro-runtime-$PID-$([guid]::NewGuid().ToString('N'))"
    $downloadPath = Join-Path $downloadDirectory $file.name
    New-Item -ItemType Directory -Path $downloadDirectory -Force | Out-Null
    try {
        if ($uri.Scheme -eq 'file') {
            Copy-Item -LiteralPath $uri.LocalPath -Destination $downloadPath
        }
        else {
            Invoke-WebRequest -Uri $file.url -OutFile $downloadPath -UseBasicParsing
        }

        if (-not (Test-Path -LiteralPath $downloadPath -PathType Leaf)) {
            throw "Runtime installer download did not create a file: $($file.url)"
        }

        $hash = Get-Sha256Hex -Path $downloadPath
        [pscustomobject]@{
            Version  = [string] $release.'release-version'
            Url      = [string] $file.url
            Sha256   = $hash
            FileName = [string] $file.name
        }
    }
    finally {
        Remove-Item -LiteralPath $downloadDirectory -Recurse -Force -ErrorAction SilentlyContinue
    }
}

if ($Architecture) {
    $result = Resolve-DotNetRuntimePackage -Architecture $Architecture -ReleaseMetadataPath $ReleaseMetadataPath
    if ($env:GITHUB_OUTPUT) {
        @(
            "runtime_version=$($result.Version)"
            "runtime_url=$($result.Url)"
            "runtime_sha256=$($result.Sha256)"
            "runtime_file_name=$($result.FileName)"
        ) | Add-Content -LiteralPath $env:GITHUB_OUTPUT
    }
    else {
        $result
    }
}
