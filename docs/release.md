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

The tag must match `vMAJOR.MINOR.PATCH`. GitHub Actions builds self-contained single-file ZIP packages for:

- Windows: x86, x64, and arm64.
- Linux: x64 and arm64 (the .NET desktop apphost does not provide a supported `linux-x86` runtime).
- macOS: x64 and arm64 (32-bit macOS is not supported by current macOS releases).

Each package is accompanied by a SHA-256 checksum. The release body is extracted from the matching `CHANGELOG.md` version heading; the release job fails when that heading is missing.
