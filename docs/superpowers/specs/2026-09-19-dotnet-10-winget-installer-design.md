# .NET 10 and WinGet installer design

## Goal

Reduce the size of the Windows downloads by removing the bundled .NET runtime while preserving a straightforward installation experience. A user installing from GitHub or through WinGet must receive a working Romodoro installation even when the required .NET runtime is not already present.

## Scope

- Move the application and test projects from .NET 9 to .NET 10.
- Publish framework-dependent Windows builds for x86, x64, and ARM64.
- Package each Windows build in an Inno Setup executable installer.
- Detect and install the matching .NET 10 runtime when a direct installer is used.
- Produce WinGet manifest templates that declare the .NET 10 runtime dependency.
- Keep the existing self-contained ZIP distribution for Linux and macOS.
- Update release documentation for the new artifacts and submission process.

This change does not automate submission to the public `microsoft/winget-pkgs` repository. Publishing a new public WinGet version remains a separate pull request after the GitHub Release exists and its final installer hashes are known.

## Runtime choice

Romodoro's generated runtime configuration references `Microsoft.NETCore.App`; Avalonia does not require `Microsoft.WindowsDesktop.App`. Windows installations therefore depend on `Microsoft.DotNet.Runtime.10`, not the larger Desktop Runtime package.

The application targets `net10.0` and accepts the latest installed .NET 10 patch through the standard .NET patch roll-forward behavior.

## Windows publication

The release matrix continues to build `win-x86`, `win-x64`, and `win-arm64`. Each Windows publish uses:

- `--self-contained false`
- the matching runtime identifier
- release configuration
- no debug symbols

The published directory contains the platform-specific Avalonia native files and the managed application, but not the .NET runtime. It is passed to Inno Setup to create one installer for each architecture.

The installer:

- installs for all users under `{autopf}\Romodoro` and therefore requests administrative privileges;
- creates Start Menu and optional desktop shortcuts;
- registers Romodoro in Windows Apps & Features;
- supports silent installation and uninstallation for WinGet;
- replaces an older Romodoro installation during an upgrade while retaining a stable application identity;
- launches Romodoro after an interactive installation when requested, but never during a silent installation.

## Runtime bootstrap

Before copying the application, the installer checks for a compatible `Microsoft.NETCore.App` 10.x runtime for its own architecture. Detection uses the official .NET host installation registry layout rather than parsing localized Apps & Features display names.

If a compatible runtime is absent, the installer downloads a pinned, architecture-matching .NET 10 runtime installer from Microsoft's official distribution endpoint, verifies its expected SHA-256 digest using Inno Setup's built-in downloader, and runs it silently with restart suppression. The Romodoro installation stops with an actionable error if downloading, verification, or runtime installation fails.

The runtime version and download URL are build inputs instead of handwritten constants spread through the installer source. The release workflow resolves them from Microsoft's official .NET release metadata, downloads the prerequisite once in the packaging job to calculate its SHA-256 digest, and passes that digest to the Inno compiler. Each Romodoro release therefore uses a pinned, supported .NET 10 patch while the resulting Romodoro installer remains small because the runtime payload is external.

## WinGet manifests

The repository contains a multi-file WinGet manifest template for package identifier `vncsmnl.Romodoro`. Release automation renders a version-specific manifest bundle containing:

- package version and metadata;
- GitHub Release URLs for the three Windows installers;
- SHA-256 hashes for each installer;
- architecture declarations;
- silent installer switches;
- Apps & Features matching data;
- `Microsoft.DotNet.Runtime.10` as a package dependency.

The installer retains its own runtime bootstrap even though WinGet normally installs the declared dependency first. This keeps direct GitHub downloads reliable and protects against installation paths that skip dependency processing.

Generated manifests are attached to the GitHub Release as an artifact. They are validated locally in CI when the required WinGet tooling is available. Public catalog submission occurs afterward because the final download URLs and hashes must reference the published GitHub Release.

## Other platforms

Linux x64/ARM64 and macOS x64/ARM64 remain self-contained single-file ZIPs. Their installation behavior and runtime requirements do not change. This avoids imposing a system-wide .NET prerequisite on platforms for which this work does not introduce a package-manager installation flow.

## Build and release flow

1. CI restores, builds, and tests the solution with the .NET 10 SDK.
2. A version tag starts the release workflow.
3. Linux and macOS jobs create the existing self-contained ZIP artifacts.
4. Windows jobs create framework-dependent publish directories and compile architecture-specific Inno Setup installers.
5. Every distributable receives a SHA-256 checksum.
6. The release job publishes all installers, ZIPs, checksums, and the rendered WinGet manifest bundle.
7. A maintainer tests the manifest and submits it to `microsoft/winget-pkgs`.

## Failure handling

- A missing .NET 10 SDK fails CI during setup rather than producing mixed-target artifacts.
- Failure to resolve or verify official runtime metadata fails the Windows packaging job.
- A failed prerequisite download or installation prevents Romodoro from being installed partially.
- Inno Setup compilation warnings and errors fail the release job.
- Manifest validation errors prevent the WinGet bundle from being attached as ready for submission.

## Verification

- Restore, build, and run all automated tests on `net10.0`.
- Confirm the generated runtime configuration requests `Microsoft.NETCore.App` 10.0.
- Compare Windows artifact sizes with the previous self-contained packages.
- Install silently on a clean Windows Sandbox without .NET 10 and confirm runtime plus application installation.
- Install silently when .NET 10 is already present and confirm the prerequisite is skipped.
- Upgrade an existing Romodoro installation and uninstall it cleanly.
- Validate the generated manifests with `winget validate` and test them with `winget install --manifest` in Windows Sandbox.

## Documentation

The release guide will distinguish direct installer usage, WinGet installation, and portable ZIP artifacts. It will also document that the public WinGet entry becomes available only after the corresponding manifest pull request is accepted upstream.
