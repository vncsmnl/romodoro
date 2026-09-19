$ErrorActionPreference = 'Stop'

function Assert-Equal([object] $Expected, [object] $Actual, [string] $Message) {
    if ($Expected -ne $Actual) { throw "${Message}: expected '$Expected', got '$Actual'" }
}

function Assert-True([bool] $Condition, [string] $Message) {
    if (-not $Condition) { throw $Message }
}

function Assert-Throws([scriptblock] $Action, [string] $Message) {
    try { & $Action } catch { return }
    throw "Expected exception: $Message"
}

$scriptPath = Join-Path (Join-Path $PSScriptRoot '..') 'Resolve-DotNetRuntime.ps1'
$fixtureRoot = Join-Path $PSScriptRoot 'fixtures'
$metadataPath = Join-Path $fixtureRoot 'releases.json'
$fixtureUri = ([Uri] $fixtureRoot).AbsoluteUri.TrimEnd('/')
$metadata = (Get-Content -Raw -LiteralPath $metadataPath).Replace('file:///__FIXTURE_ROOT__', $fixtureUri)
$tempMetadataPath = Join-Path ([IO.Path]::GetTempPath()) "romodoro-releases-$PID.json"
$missingMetadata = $null
Set-Content -LiteralPath $tempMetadataPath -Value $metadata -Encoding utf8

try {
    . $scriptPath

    $x64 = Resolve-DotNetRuntimePackage -Architecture x64 -ReleaseMetadataPath $tempMetadataPath
    Assert-Equal '10.0.12' $x64.Version 'x64 version'
    Assert-Equal 'dotnet-runtime-10.0.12-win-x64.exe' $x64.FileName 'x64 file'
    Assert-Equal 64 $x64.Sha256.Length 'x64 hash length'
    Assert-True ($x64.Url -match 'win-x64\.exe$') 'x64 URL must select x64 payload'

    foreach ($architecture in @('x86', 'arm64')) {
        $result = Resolve-DotNetRuntimePackage -Architecture $architecture -ReleaseMetadataPath $tempMetadataPath
        Assert-True ($result.FileName -match "win-$architecture\.exe$") "$architecture file must match architecture"
        Assert-Equal 64 $result.Sha256.Length "$architecture hash length"
    }

    Assert-Throws { Resolve-DotNetRuntimePackage -Architecture sparc -ReleaseMetadataPath $tempMetadataPath } 'unsupported architecture'

    $missingMetadata = Join-Path ([IO.Path]::GetTempPath()) "romodoro-missing-$PID.json"
    $missing = $metadata.Replace('dotnet-runtime-10.0.12-win-x64.exe', 'dotnet-runtime-10.0.12-win-sparc.exe')
    Set-Content -LiteralPath $missingMetadata -Value $missing -Encoding utf8
    Assert-Throws { Resolve-DotNetRuntimePackage -Architecture x64 -ReleaseMetadataPath $missingMetadata } 'missing runtime package'

    Write-Output 'PASS runtime resolver architecture, version, hash, and failure tests'
}
finally {
    Remove-Item -LiteralPath $tempMetadataPath -Force -ErrorAction SilentlyContinue
    if ($missingMetadata) { Remove-Item -LiteralPath $missingMetadata -Force -ErrorAction SilentlyContinue }
}
