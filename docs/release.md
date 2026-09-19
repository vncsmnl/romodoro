# Release process

The release workflow publishes desktop packages when a semantic version tag is pushed.

1. Add the user-facing changes under `## [Unreleased]` in `CHANGELOG.md`.
2. Move those entries to a dated heading such as `## [0.1.0] - 2026-09-18`.
3. Commit the changelog and push the commit.
4. Create and push the tag:

   ```bash
   git tag v0.1.0
   git push origin v0.1.0
   ```

The tag must match `vMAJOR.MINOR.PATCH`. GitHub Actions builds framework-dependent Inno Setup installers for Windows and self-contained single-file ZIP packages for Unix platforms:

- Windows: x86, x64, and arm64. The installer checks for the matching .NET 10 runtime and downloads it from Microsoft when it is missing.
- Linux: x64 and arm64 (the .NET desktop apphost does not provide a supported `linux-x86` runtime).
- macOS: x64 and arm64 (32-bit macOS is not supported by current macOS releases).

Each package is accompanied by a SHA-256 checksum. Windows releases also include a generated WinGet manifest bundle. To install from the public WinGet source after its manifest PR is accepted, run:

```powershell
winget install --id vncsmnl.Romodoro -e
```

The release body is extracted from the matching `CHANGELOG.md` version heading; the release job fails when that heading is missing. Public WinGet submission remains a separate pull request to `microsoft/winget-pkgs` using the attached manifest bundle.
