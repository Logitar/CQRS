# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

Nothing yet.

## [10.1.0] - 2026-01-24

### Added

- `OnException` callback to command and query buses.

## [10.0.1] - 2026-01-23

### Fixed

- NuGet upgrade.

## [10.0.0] - 2025-12-08

### Added

- Implemented `RetrySettings`.
- Implemented Command Query Responsibility Segregation pattern, with `CommandBus` and `QueryBus`.
- Added `DependencyInjectionExtensions`.

[unreleased]: https://github.com/Logitar/EventSourcing/compare/v10.1.0...HEAD
[10.1.0]: https://github.com/Logitar/EventSourcing/compare/v10.0.1...v10.1.0
[10.0.1]: https://github.com/Logitar/EventSourcing/compare/v10.0.0...v10.0.1
[10.0.0]: https://github.com/Logitar/EventSourcing/releases/tag/v10.0.0
