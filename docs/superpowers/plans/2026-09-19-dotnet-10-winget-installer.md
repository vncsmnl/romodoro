# .NET 10 and WinGet Installer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate Romodoro to .NET 10 and ship small Windows installers that bootstrap the matching .NET runtime and can be submitted to WinGet.

**Architecture:** The application and tests target `net10.0`. Windows release jobs publish framework-dependent, architecture-specific files and compile them with Inno Setup; the installer checks the official .NET installation registry and securely downloads a pinned runtime only when needed. Release automation renders multi-file WinGet manifests from the final installer URLs and hashes, while Linux and macOS remain self-contained.

**Tech Stack:** .NET 10 SDK, Avalonia 12, GitHub Actions, PowerShell 7, Inno Setup 6, WinGet manifest schema 1.10.

**Spec:** `docs/superpowers/specs/2026-09-19-dotnet-10-winget-installer-design.md`

## Global Constraints

- The application and test projects target exactly `net10.0`.
- Windows artifacts cover `win-x86`, `win-x64`, and `win-arm64` and are framework-dependent.
- Linux x64/ARM64 and macOS x64/ARM64 remain self-contained single-file ZIPs.
- The prerequisite is `Microsoft.NETCore.App` 10.x, represented in WinGet by `Microsoft.DotNet.Runtime.10`.
- Runtime downloads use Microsoft's official release metadata and are verified with a build-calculated SHA-256 digest.
- Direct installers and WinGet installations must both succeed on a machine without .NET 10.
- The WinGet package identifier is `vncsmnl.Romodoro`.
- Public submission to `microsoft/winget-pkgs` remains outside repository automation.

## Review Focus

- A machine with only .NET 9 must be treated as missing the prerequisite and receive .NET 10.
- A machine with any serviced 10.0 patch for the correct architecture must skip the runtime download.
- An x64 runtime must not satisfy an ARM64 or x86 installer check.
- A failed or hash-mismatched runtime download must abort before application files are installed.
- Silent WinGet installation must neither display the post-install launch prompt nor start Romodoro.

---

### Task 1: Migrate the solution and CI to .NET 10

**Files:**
- Modify: `src/Romodoro/Romodoro.csproj`
- Modify: `tests/Romodoro.Tests/Romodoro.Tests.csproj`
- Modify: `.github/workflows/ci.yml`
- Modify: `README.md`
- Modify: `docs/getting-started.md`

**Interfaces:**
- Consumes: .NET 10 SDK installed locally and through `actions/setup-dotnet@v4`.
- Produces: application and test assemblies targeting `net10.0`; CI contract `dotnet-version: 10.0.x`.

- [ ] **Step 1: Establish the .NET 9 baseline**

Run:

```powershell
rtk dotnet test Romodoro.sln --configuration Release
```

Expected: PASS before changing the target framework.

- [ ] **Step 2: Change both project targets to .NET 10**

Replace `<TargetFramework>net9.0</TargetFramework>` with:

```xml
<TargetFramework>net10.0</TargetFramework>
```

in both project files. Do not change the Avalonia or test package versions unless restore proves a package incompatible with .NET 10.

- [ ] **Step 3: Verify the migrated target restores and tests**

Run:

```powershell
rtk dotnet restore Romodoro.sln
rtk dotnet test Romodoro.sln --configuration Release --no-restore
```

Expected: restore succeeds and all tests pass under `net10.0`.

- [ ] **Step 4: Update the CI SDK and user-facing prerequisites**

Change every `dotnet-version` value in `.github/workflows/ci.yml` from `9.0.x` to `10.0.x`. Update README and getting-started commands/text so development requires the .NET 10 SDK; do not describe the runtime installer yet because Task 4 owns release documentation.

- [ ] **Step 5: Verify formatting, build, and tests as CI does**

Run:

```powershell
rtk dotnet format Romodoro.sln --verify-no-changes
rtk dotnet build Romodoro.sln --configuration Release --no-restore
rtk dotnet test Romodoro.sln --configuration Release --no-build
```

Expected: all three commands exit 0.

- [ ] **Step 6: Commit the migration**

```powershell
rtk git add src/Romodoro/Romodoro.csproj tests/Romodoro.Tests/Romodoro.Tests.csproj .github/workflows/ci.yml README.md docs/getting-started.md
rtk git commit -m "build: migrate Romodoro to .NET 10"
```

---

### Task 2: Add deterministic .NET runtime metadata resolution

**Files:**
- Create: `build/Resolve-DotNetRuntime.ps1`
- Create: `build/tests/Resolve-DotNetRuntime.Tests.ps1`
- Create: `build/tests/fixtures/releases.json`

**Interfaces:**
- Consumes: `-Architecture` (`x86`, `x64`, or `arm64`), optional `-ReleaseMetadataPath`, and optional GitHub Actions output path through `$env:GITHUB_OUTPUT`.
- Produces: a PowerShell object with `Version`, `Url`, `Sha256`, and `FileName`; GitHub outputs `runtime_version`, `runtime_url`, `runtime_sha256`, and `runtime_file_name`.

- [ ] **Step 1: Add a minimal deterministic metadata fixture**

Create `build/tests/fixtures/releases.json` with one .NET 10 release containing three `runtime` files named `dotnet-runtime-10.0.12-win-x86.exe`, `dotnet-runtime-10.0.12-win-x64.exe`, and `dotnet-runtime-10.0.12-win-arm64.exe`. Point their URLs at three small `file:///` fixture payloads created by the test script and include a second, irrelevant Linux file to prove filtering. The resolver may accept `file:///` only when `-ReleaseMetadataPath` is supplied; live metadata must resolve only HTTPS package URLs.

- [ ] **Step 2: Write the failing resolver tests**

Create a dependency-free PowerShell test harness that dot-sources `Resolve-DotNetRuntime.ps1`, invokes `Resolve-DotNetRuntimePackage`, and throws unless all assertions hold:

```powershell
$result = Resolve-DotNetRuntimePackage -Architecture x64 -ReleaseMetadataPath $fixture
Assert-Equal '10.0.12' $result.Version
Assert-Equal 'dotnet-runtime-10.0.12-win-x64.exe' $result.FileName
Assert-Equal 64 $result.Sha256.Length
Assert-Throws { Resolve-DotNetRuntimePackage -Architecture sparc -ReleaseMetadataPath $fixture }
Assert-Throws { Resolve-DotNetRuntimePackage -Architecture x64 -ReleaseMetadataPath $missingRuntimeFixture }
```

The harness must also assert that x86 and ARM64 select their own files and never the x64 URL.

- [ ] **Step 3: Run the harness and confirm the resolver is absent**

Run:

```powershell
rtk proxy pwsh -NoProfile -File build/tests/Resolve-DotNetRuntime.Tests.ps1
```

Expected: FAIL because `Resolve-DotNetRuntimePackage` is not defined.

- [ ] **Step 4: Implement the resolver**

Implement this public function contract in `build/Resolve-DotNetRuntime.ps1`:

```powershell
function Resolve-DotNetRuntimePackage {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('x86', 'x64', 'arm64')]
        [string] $Architecture,
        [string] $ReleaseMetadataPath
    )
    # Read the supplied fixture or https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/10.0/releases.json.
    # Select the newest stable release and its runtime file matching dotnet-runtime-*-win-$Architecture.exe.
    # Download it to a temporary file, calculate SHA-256, delete the temporary file, and return the object.
}
```

The script entry point calls the function, writes the four GitHub outputs when `$env:GITHUB_OUTPUT` is set, and otherwise emits the object. Use `try/finally` so temporary downloads are deleted on success and failure. Reject an empty release set, preview releases, unsupported architectures, missing files, non-HTTPS remote URLs, and non-200 downloads with terminating errors.

- [ ] **Step 5: Run resolver tests**

Run:

```powershell
rtk proxy pwsh -NoProfile -File build/tests/Resolve-DotNetRuntime.Tests.ps1
```

Expected: PASS messages for x86, x64, ARM64, unsupported architecture, missing package, correct SHA-256, and architecture isolation.

- [ ] **Step 6: Smoke-test official metadata without retaining the runtime**

Run:

```powershell
rtk proxy pwsh -NoProfile -File build/Resolve-DotNetRuntime.ps1 -Architecture x64
```

Expected: an object for a stable 10.0.x runtime with an HTTPS Microsoft URL and a 64-character SHA-256 digest; the downloaded prerequisite is removed afterward.

- [ ] **Step 7: Commit the resolver**

```powershell
rtk git add build/Resolve-DotNetRuntime.ps1 build/tests
rtk git commit -m "build: resolve pinned .NET runtime prerequisites"
```

---

### Task 3: Create the framework-dependent Windows installer

**Files:**
- Create: `installer/Romodoro.iss`
- Create: `installer/tests/Verify-Installer.ps1`
- Modify: `.github/workflows/release.yml`

**Interfaces:**
- Consumes: Inno preprocessor values `AppVersion`, `RuntimeArchitecture`, `RuntimeVersion`, `RuntimeUrl`, `RuntimeSha256`, `PublishDir`, and `OutputDir`.
- Produces: `Romodoro-win-<architecture>-setup.exe`, an Apps & Features entry named `Romodoro`, and silent switches `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART`.

- [ ] **Step 1: Write static installer contract tests**

Create `installer/tests/Verify-Installer.ps1` that loads `installer/Romodoro.iss` as raw text and fails unless it finds:

```text
PrivilegesRequired=admin
ArchitecturesAllowed=
ArchitecturesInstallIn64BitMode=
Microsoft.NETCore.App
DownloadTemporaryFile
/install /quiet /norestart
skipifsilent
```

The test must also invoke the compiler for x64 against a supplied publish directory and assert the expected setup executable exists. Accept parameters `-IsccPath`, `-PublishDir`, and `-OutputDir` so the same script runs locally and in Actions.

- [ ] **Step 2: Run the installer test and verify it fails**

Run after locating `ISCC.exe`:

```powershell
rtk proxy pwsh -NoProfile -File installer/tests/Verify-Installer.ps1 -IsccPath "$env:ProgramFiles(x86)\Inno Setup 6\ISCC.exe" -PublishDir src/Romodoro/bin/Release/net10.0/win-x64/publish -OutputDir artifacts/installer-test
```

Expected: FAIL because `installer/Romodoro.iss` does not exist.

- [ ] **Step 3: Implement the installer definition**

Create `installer/Romodoro.iss` with a stable `AppId`, `DefaultDirName={autopf}\Romodoro`, `UninstallDisplayName=Romodoro`, Portuguese and English wizard languages, LZMA2 compression, and architecture directives selected from `RuntimeArchitecture`.

Implement these Pascal helpers:

```pascal
function IsRequiredRuntimeInstalled(): Boolean;
function DownloadAndInstallRuntime(): String;
function PrepareToInstall(var NeedsRestart: Boolean): String;
```

`IsRequiredRuntimeInstalled` enumerates version value names below `HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\{#RuntimeArchitecture}\sharedfx\Microsoft.NETCore.App` and returns true only for a semantic version whose major component is `10`. `PrepareToInstall` skips downloading when detection succeeds; otherwise it calls `DownloadTemporaryFile` with `{#RuntimeUrl}` and `{#RuntimeSha256}`, executes the prerequisite with `/install /quiet /norestart`, and returns a localized blocking error for a download exception or non-zero process exit code. Keep the application `[Files]` copy after prerequisite preparation so failure cannot leave a new partial install.

Configure `[Run]` with `Flags: nowait postinstall skipifsilent` so WinGet never launches the app. Configure Start Menu and optional desktop icons through `[Icons]` and `[Tasks]`.

- [ ] **Step 4: Split release publication by platform behavior**

Refactor `.github/workflows/release.yml` so every job uses `dotnet-version: 10.0.x`. Preserve the seven-RID matrix, but use conditional steps:

```yaml
- name: Publish Windows framework-dependent build
  if: runner.os == 'Windows'
  run: dotnet publish ... --self-contained false --output publish/${{ matrix.rid }}

- name: Publish self-contained Unix build
  if: runner.os != 'Windows'
  run: dotnet publish ... --self-contained true -p:PublishSingleFile=true ...
```

For Windows, install Inno Setup 6, call `build/Resolve-DotNetRuntime.ps1` with the matrix architecture, compile `installer/Romodoro.iss` with all seven required `/D` values, run `Verify-Installer.ps1`, and hash the resulting setup EXE. For Linux/macOS, preserve ZIP packaging and its checksum.

- [ ] **Step 5: Build and compile one local x64 installer**

Run:

```powershell
rtk dotnet publish src/Romodoro/Romodoro.csproj --configuration Release --runtime win-x64 --self-contained false --output artifacts/win-x64
rtk proxy pwsh -NoProfile -File build/Resolve-DotNetRuntime.ps1 -Architecture x64
rtk proxy pwsh -NoProfile -File installer/tests/Verify-Installer.ps1 -IsccPath "$env:ProgramFiles(x86)\Inno Setup 6\ISCC.exe" -PublishDir artifacts/win-x64 -OutputDir artifacts/installer-test
```

Expected: publish succeeds, runtime metadata resolves, and `Romodoro-win-x64-setup.exe` is created. If Inno Setup is not installed locally, run the equivalent release job through GitHub Actions before marking this step complete.

- [ ] **Step 6: Verify the installer runtime configuration and size**

Run:

```powershell
rtk proxy pwsh -NoProfile -Command "(Get-Content artifacts/win-x64/Romodoro.runtimeconfig.json -Raw) -match 'Microsoft.NETCore.App' -and (Get-Content artifacts/win-x64/Romodoro.runtimeconfig.json -Raw) -match '10.0.0'"
rtk proxy pwsh -NoProfile -Command "Get-Item artifacts/installer-test/Romodoro-win-x64-setup.exe | Select-Object Name,Length"
```

Expected: first command returns `True`; installer exists and is materially smaller than the prior self-contained Windows ZIP.

- [ ] **Step 7: Commit Windows packaging**

```powershell
rtk git add installer .github/workflows/release.yml
rtk git commit -m "build: add .NET 10 Windows bootstrapper"
```

---

### Task 4: Generate and validate WinGet manifests

**Files:**
- Create: `packaging/winget/vncsmnl.Romodoro.yaml.template`
- Create: `packaging/winget/vncsmnl.Romodoro.installer.yaml.template`
- Create: `packaging/winget/vncsmnl.Romodoro.locale.en-US.yaml.template`
- Create: `build/New-WinGetManifest.ps1`
- Create: `build/tests/New-WinGetManifest.Tests.ps1`
- Modify: `.github/workflows/release.yml`
- Modify: `docs/release.md`
- Modify: `CHANGELOG.md`

**Interfaces:**
- Consumes: `-Version`, `-Repository vncsmnl/romodoro`, three installer paths, and `-OutputDirectory`.
- Produces: a schema-1.10 multi-file manifest set for `vncsmnl.Romodoro`, plus a ZIP suitable for attaching to the GitHub Release.

- [ ] **Step 1: Write failing manifest generator tests**

Create three dummy installer files with distinct contents. Invoke the missing generator and assert that:

- exactly three YAML files are produced;
- all use `PackageIdentifier: vncsmnl.Romodoro` and the requested version;
- installer URLs use `https://github.com/vncsmnl/romodoro/releases/download/v<version>/`;
- x86, x64, and ARM64 entries contain the SHA-256 of their matching dummy files;
- every installer entry declares `Microsoft.DotNet.Runtime.10` under `Dependencies.PackageDependencies`;
- silent switches are `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART`;
- no unresolved `{{...}}` tokens remain.

- [ ] **Step 2: Run the manifest test and confirm failure**

Run:

```powershell
rtk proxy pwsh -NoProfile -File build/tests/New-WinGetManifest.Tests.ps1
```

Expected: FAIL because `New-WinGetManifest.ps1` and templates do not exist.

- [ ] **Step 3: Add complete schema-1.10 templates**

The version template references installer and default-locale manifests. The locale template uses publisher `vncsmnl`, package name `Romodoro`, locale `en-US`, homepage and release notes URLs under the GitHub repository, license value `Proprietary` until the repository gains an explicit license, and a concise timer/Pomodoro description. The installer template defines `InstallerType: inno`, machine scope, the three architectures, Apps & Features display name/publisher, silent switches, and this dependency for each entry:

```yaml
Dependencies:
  PackageDependencies:
    - PackageIdentifier: Microsoft.DotNet.Runtime.10
      MinimumVersion: 10.0.0
```

- [ ] **Step 4: Implement manifest rendering**

Implement `build/New-WinGetManifest.ps1` with validated parameters, `Get-FileHash -Algorithm SHA256`, exact token replacement, unresolved-token rejection, and UTF-8 without BOM output. Reject versions not matching `^[0-9]+\.[0-9]+\.[0-9]+$`, missing installer files, duplicate paths, and empty output directories.

- [ ] **Step 5: Run generator tests and optional WinGet validation**

Run:

```powershell
rtk proxy pwsh -NoProfile -File build/tests/New-WinGetManifest.Tests.ps1
rtk winget validate --manifest artifacts/winget-test
```

Expected: harness passes and WinGet reports a valid manifest. If local manifest validation is unavailable, install/run Microsoft WinGetCreate in the Windows release job and retain its validation output as the required evidence.

- [ ] **Step 6: Wire manifest generation into the release job**

After all publish artifacts are downloaded, derive the semantic version from the tag, call `New-WinGetManifest.ps1`, validate the result, compress the three YAML files as `Romodoro-<version>-winget.zip`, calculate its SHA-256, and include both in `softprops/action-gh-release` assets. Ensure the generator receives the already-built installer paths so their hashes match the uploaded files byte-for-byte.

- [ ] **Step 7: Update release documentation and changelog**

Document:

```powershell
winget install --id vncsmnl.Romodoro -e
```

Explain direct Windows setup behavior, automatic .NET 10 runtime installation, unchanged self-contained Unix ZIPs, local `winget validate`/Sandbox testing, and the separate upstream manifest PR. Add an Unreleased changelog entry for .NET 10 and the Windows installer.

- [ ] **Step 8: Commit WinGet packaging**

```powershell
rtk git add packaging/winget build/New-WinGetManifest.ps1 build/tests/New-WinGetManifest.Tests.ps1 .github/workflows/release.yml docs/release.md CHANGELOG.md
rtk git commit -m "build: generate WinGet release manifests"
```

---

### Task 5: End-to-end verification

**Files:**
- Modify only if verification exposes a defect in files owned by Tasks 1–4.

**Interfaces:**
- Consumes: all prior task outputs.
- Produces: verified .NET 10 builds, Windows setup artifacts, WinGet manifests, and release documentation.

- [ ] **Step 1: Run repository checks**

```powershell
rtk dotnet restore Romodoro.sln
rtk dotnet format Romodoro.sln --verify-no-changes
rtk dotnet build Romodoro.sln --configuration Release --no-restore
rtk dotnet test Romodoro.sln --configuration Release --no-build
rtk proxy pwsh -NoProfile -File build/tests/Resolve-DotNetRuntime.Tests.ps1
rtk proxy pwsh -NoProfile -File build/tests/New-WinGetManifest.Tests.ps1
```

Expected: all commands exit 0.

- [ ] **Step 2: Exercise the Windows release path**

Publish all three Windows RIDs framework-dependent, resolve each matching runtime, compile all three installers, and generate manifests for a test version. Confirm each manifest hash equals `Get-FileHash` for its installer.

- [ ] **Step 3: Test clean and pre-provisioned Windows Sandbox cases**

In a clean Windows Sandbox, install the x64 setup silently and confirm both `dotnet --list-runtimes` and Romodoro installation. In a second run with .NET 10 already installed, retain the Inno log and confirm it reports the prerequisite as satisfied without downloading. Repeat architecture checks in CI or matching hardware for x86 and ARM64.

- [ ] **Step 4: Test upgrade and uninstall behavior**

Install the previous Romodoro release, install the new setup over it, launch the app, then uninstall. Confirm only one Apps & Features entry exists during upgrade and the application directory and shortcuts are removed after uninstall.

- [ ] **Step 5: Review the final diff and workflow syntax**

```powershell
rtk git diff --check HEAD~4
rtk git status --short
rtk proxy pwsh -NoProfile -Command "Get-Content .github/workflows/ci.yml -Raw | ConvertFrom-Yaml | Out-Null; Get-Content .github/workflows/release.yml -Raw | ConvertFrom-Yaml | Out-Null"
```

Expected: no whitespace errors, no unintended files, and valid YAML. If `ConvertFrom-Yaml` is unavailable, validate both workflows with the repository's existing YAML tooling or GitHub Actions.

- [ ] **Step 6: Record any verification-only fixes**

If verification required changes, rerun the affected check and stage only files changed for that correction, for example:

```powershell
rtk git add .github/workflows/release.yml installer build packaging docs/release.md CHANGELOG.md
rtk git commit -m "fix: correct .NET 10 release packaging"
```

If no fixes were required, do not create an empty commit.
