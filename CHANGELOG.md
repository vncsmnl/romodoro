# Changelog

All notable changes to Romodoro are documented here. Release notes are generated from the version heading that matches the Git tag.

## [Unreleased]

## [0.3.0] - 2026-09-19

### Changed

- Migrated the application and Windows packaging to .NET 10 with a framework-dependent installer and WinGet manifest generation.

## [0.2.0] - 2026-09-19

### Added

- Custom title bar with accessible minimize and close controls.
- Mouse resizing from every window edge and corner.
- New Romodoro application icon and title-bar branding.

### Changed

- Responsive layouts for the clock, stopwatch, and Pomodoro modes, including a compact `320x420` minimum window size.
- Scrollable stopwatch laps and proportionally scaling timer controls for smaller windows.
- Built-in vector icons replace the external Material Icons dependency.

### Fixed

- Removed the shadow artifact around the rounded lower corners.
- Corrected mode-icon alignment and scaling.
- Made clock formatting independent of the operating system time zone.
- Normalized C# whitespace so formatting checks pass consistently on Linux CI runners.

## [0.1.0] - 2026-09-18

### Added

- Avalonia desktop application for Windows, macOS, and Linux.
- Clock mode with localized Portuguese date and time.
- Stopwatch mode with pause, resume, reset, and laps.
- Standard Pomodoro cycle with 25-minute focus, 5-minute short break, and 15-minute long break after four focuses.
- Cross-platform completion notification and sound fallback.
- Figma-based dark floating interface with always-on-top control.
- Automated tests for clock, stopwatch, Pomodoro transitions, and notification fallback.
- CI/CD workflows for static analysis, tests, multi-runtime publishing, checksums, and GitHub Releases.
