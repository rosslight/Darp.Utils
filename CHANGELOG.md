# Changelog

## [1.20.0](https://github.com/rosslight/Darp.Utils/compare/v1.19.2...v1.20.0) (2025-11-21)


### Features

* Use release-please for releases ([e0b4e82](https://github.com/rosslight/Darp.Utils/commit/e0b4e82c8eeeeec2c293d8daa522258c86f96afa))


### Bug Fixes

* Update versions automatically ([58b5575](https://github.com/rosslight/Darp.Utils/commit/58b5575ca212aa663724fde179f2ce0baa3289ba))

## [1.19.2](https://github.com/rosslight/Darp.Utils/compare/v1.19.1...v1.19.2) (2025-10-24)

### Bug Fixes
- Adjust generator to generate ImmutableArrays ([6f7178c](https://github.com/rosslight/Darp.Utils/commit/6f7178c))
- Message subjects could skip subscribers ([c65cc15](https://github.com/rosslight/Darp.Utils/commit/c65cc15))

## [1.19.1](https://github.com/rosslight/Darp.Utils/compare/v1.19.0...v1.19.1) (2025-07-28)

### Bug Fixes
- Escape key recognition ([13a5a7e](https://github.com/rosslight/Darp.Utils/commit/13a5a7e))

## [1.19.0](https://github.com/rosslight/Darp.Utils/compare/v1.18.2...v1.19.0) (2025-07-27)

### Features
- Implement a way to avoid closing the dialog on pressing escape ([7ea7c14](https://github.com/rosslight/Darp.Utils/commit/7ea7c14))
- Introduce a dialogAwaitable to simplify cancellation of a dialog ([2fa7c85](https://github.com/rosslight/Darp.Utils/commit/2fa7c85))

## [1.18.2](https://github.com/rosslight/Darp.Utils/compare/v1.18.1...v1.18.2) (2025-07-10)

### Bug Fixes
- Properly handle cancellationToken when showing dialogs ([11f43aa](https://github.com/rosslight/Darp.Utils/commit/11f43aa))

## [1.18.1](https://github.com/rosslight/Darp.Utils/compare/v1.18.0...v1.18.1) (2025-07-07)

### Bug Fixes
- Naming of GetAssets method, named overload ([1c239d1](https://github.com/rosslight/Darp.Utils/commit/1c239d1))

## [1.18.0](https://github.com/rosslight/Darp.Utils/compare/v1.17.4...v1.18.0) (2025-06-29)

### Features
- Add sync overloads for reading/writing to assets ([1486624](https://github.com/rosslight/Darp.Utils/commit/1486624))

## [1.17.4](https://github.com/rosslight/Darp.Utils/compare/v1.17.3...v1.17.4) (2025-06-28)

### Bug Fixes
- Naming of ShowInputDialogAsync ([d7520a5](https://github.com/rosslight/Darp.Utils/commit/d7520a5))
- Simplify asset service interfaces and add factory ([f5aa0d2](https://github.com/rosslight/Darp.Utils/commit/f5aa0d2))

## [1.17.3](https://github.com/rosslight/Darp.Utils/compare/v1.17.2...v1.17.3) (2025-06-13)

### Bug Fixes
- Ensure to write texts properly ([8c6b5b7](https://github.com/rosslight/Darp.Utils/commit/8c6b5b7))

## [1.17.2](https://github.com/rosslight/Darp.Utils/compare/v1.17.1...v1.17.2) (2025-06-12)

### Bug Fixes
- Send initial editor property state to JS ([11aecda](https://github.com/rosslight/Darp.Utils/commit/11aecda))

## [1.17.1](https://github.com/rosslight/Darp.Utils/compare/v1.17.0...v1.17.1) (2025-06-12)

### Bug Fixes
- Ensure C#-JS communication can handle fast inputs ([d867983](https://github.com/rosslight/Darp.Utils/commit/d867983))

## [1.17.0](https://github.com/rosslight/Darp.Utils/compare/v1.16.1...v1.17.0) (2025-06-08)

### Bug Fixes
- AutoCompletions not counting towards user changes, clean up logging ([1e9c321](https://github.com/rosslight/Darp.Utils/commit/1e9c321))
- Correct scrollbar styles and cleanup ([c69f5c3](https://github.com/rosslight/Darp.Utils/commit/c69f5c3))
- Simplify editor code and fix run conditions ([137151b](https://github.com/rosslight/Darp.Utils/commit/137151b))

### Features
- Add isReadOnly support to the editor ([9665cfd](https://github.com/rosslight/Darp.Utils/commit/9665cfd))
- Add remaining extensions, allow text binding ([4e6a602](https://github.com/rosslight/Darp.Utils/commit/4e6a602))
- Add working theme support for editor ([181cc0d](https://github.com/rosslight/Darp.Utils/commit/181cc0d))
- editorsample: Introduce a small sample project with an editor ([ae1695d](https://github.com/rosslight/Darp.Utils/commit/ae1695d))
- Extend CodeMirror functionality with advanced options ([7397397](https://github.com/rosslight/Darp.Utils/commit/7397397))

## [1.16.1](https://github.com/rosslight/Darp.Utils/compare/v1.16.0...v1.16.1) (2025-06-03)

### Bug Fixes
- Only match known view models in Dialog ViewLocator ([42fffc6](https://github.com/rosslight/Darp.Utils/commit/42fffc6))

## [1.16.0](https://github.com/rosslight/Darp.Utils/compare/v1.15.0...v1.16.0) (2025-05-24)

### Bug Fixes
- Remove sealed from MemoryAssetsService ([86d1e55](https://github.com/rosslight/Darp.Utils/commit/86d1e55))

### Features
- Add memory assets service ([7206e84](https://github.com/rosslight/Darp.Utils/commit/7206e84))
- Add services optionally only ([8b5096b](https://github.com/rosslight/Darp.Utils/commit/8b5096b))
- Rename GetReadOnlySteam to GetReadOnlyStream ([774da18](https://github.com/rosslight/Darp.Utils/commit/774da18))

## [1.15.0](https://github.com/rosslight/Darp.Utils/compare/v1.14.3...v1.15.0) (2025-05-24)

### Features
- Add Darp.Utils.Avalonia ([aec9fb5](https://github.com/rosslight/Darp.Utils/commit/aec9fb5))

## [1.14.3](https://github.com/rosslight/Darp.Utils/compare/v1.14.2...v1.14.3) (2025-05-22)

### Bug Fixes
- Force focus to Input TextBox ([865cb09](https://github.com/rosslight/Darp.Utils/commit/865cb09))

## [1.14.2](https://github.com/rosslight/Darp.Utils/compare/v1.14.1...v1.14.2) (2025-05-22)

### Bug Fixes
- Use more verbose property syntax to suppress trim warnings properly ([534746c](https://github.com/rosslight/Darp.Utils/commit/534746c))

## [1.14.1](https://github.com/rosslight/Darp.Utils/compare/v1.14.0...v1.14.1) (2025-05-20)

### Bug Fixes
- Inverse equality condition when observing configuration ([100f29a](https://github.com/rosslight/Darp.Utils/commit/100f29a))

## [1.14.0](https://github.com/rosslight/Darp.Utils/compare/v1.13.2...v1.14.0) (2025-05-20)

### Features
- Add Observe extension to configuration service ([d3786ec](https://github.com/rosslight/Darp.Utils/commit/d3786ec))

## [1.13.2](https://github.com/rosslight/Darp.Utils/compare/v1.13.1...v1.13.2) (2025-05-19)

### Bug Fixes
- Correctly preserve properties ([6ac715f](https://github.com/rosslight/Darp.Utils/commit/6ac715f))

## [1.13.1](https://github.com/rosslight/Darp.Utils/compare/v1.13.0...v1.13.1) (2025-05-19)

### Bug Fixes
- Add dynamic dependency to validation viewmodels ([be946cb](https://github.com/rosslight/Darp.Utils/commit/be946cb))
- Remove generated AggressiveInlining attribute ([db8b6f9](https://github.com/rosslight/Darp.Utils/commit/db8b6f9))

## [1.13.0](https://github.com/rosslight/Darp.Utils/compare/v1.12.3...v1.13.0) (2025-03-05)

### Bug Fixes
- Actually implement happy path for source generator ([4dcab43](https://github.com/rosslight/Darp.Utils/commit/4dcab43))
- Add None flags to each ID ([f41e0cf](https://github.com/rosslight/Darp.Utils/commit/f41e0cf))
- Include attributes and handle multiple generic methods ([d64fe08](https://github.com/rosslight/Darp.Utils/commit/d64fe08))
- Rename files to avoid duplicates ([7ce7e60](https://github.com/rosslight/Darp.Utils/commit/7ce7e60))
- Reverse order in subject to allow for unsubscribing while iterating ([d0baeca](https://github.com/rosslight/Darp.Utils/commit/d0baeca))
- Support generic classes for sink generation ([e825595](https://github.com/rosslight/Darp.Utils/commit/e825595))
- Use JsonElements for requests ([56c470a](https://github.com/rosslight/Darp.Utils/commit/56c470a))

### Features
- Add messaging source generator ([15b7e9c](https://github.com/rosslight/Darp.Utils/commit/15b7e9c))
- Include MessageSource source generation ([68978e2](https://github.com/rosslight/Darp.Utils/commit/68978e2))

## [1.12.3](https://github.com/rosslight/Darp.Utils/compare/v1.12.2...v1.12.3) (2024-12-17)

### Bug Fixes
- Include source symbols ([322859d](https://github.com/rosslight/Darp.Utils/commit/322859d))

## [1.12.2](https://github.com/rosslight/Darp.Utils/compare/v1.12.1...v1.12.2) (2024-12-12)

### Bug Fixes
- Invalid types in TestRail object ([19a37e8](https://github.com/rosslight/Darp.Utils/commit/19a37e8))

## [1.12.1](https://github.com/rosslight/Darp.Utils/compare/v1.12.0...v1.12.1) (2024-12-11)

### Bug Fixes
- Missing overloads for dynamic json resolving ([97761e8](https://github.com/rosslight/Darp.Utils/commit/97761e8))

## [1.12.0](https://github.com/rosslight/Darp.Utils/compare/v1.11.4...v1.12.0) (2024-12-11)

### Features
- Initial test rail implementation ([0a68e4b](https://github.com/rosslight/Darp.Utils/commit/0a68e4b))

## [1.11.4](https://github.com/rosslight/Darp.Utils/compare/v1.11.3...v1.11.4) (2024-12-10)

### Bug Fixes
- Bump dependencies ([87c34c1](https://github.com/rosslight/Darp.Utils/commit/87c34c1))

## [1.11.3](https://github.com/rosslight/Darp.Utils/compare/v1.11.2...v1.11.3) (2024-12-10)

### Bug Fixes
- Allow for handling ReadOnly files properly ([cd1b125](https://github.com/rosslight/Darp.Utils/commit/cd1b125))

## [1.11.2](https://github.com/rosslight/Darp.Utils/compare/v1.11.1...v1.11.2) (2024-12-10)

### Bug Fixes
- Trim folder base paths ([4f4d825](https://github.com/rosslight/Darp.Utils/commit/4f4d825))

## [1.11.1](https://github.com/rosslight/Darp.Utils/compare/v1.11.0...v1.11.1) (2024-12-09)

### Bug Fixes
- Add a BaseDirectory assets service ([61ae814](https://github.com/rosslight/Darp.Utils/commit/61ae814))

## [1.11.0](https://github.com/rosslight/Darp.Utils/compare/v1.10.0...v1.11.0) (2024-12-09)

### Features
- Add additional SerializeTextAsync overload ([8a5f560](https://github.com/rosslight/Darp.Utils/commit/8a5f560))
- Add deserialize text method ([05cab7f](https://github.com/rosslight/Darp.Utils/commit/05cab7f))
- Add EmbeddedResource service ([a4d0cac](https://github.com/rosslight/Darp.Utils/commit/a4d0cac))
- Add EnumerateFiles support for ReadOnlyAssets Services ([66aedfe](https://github.com/rosslight/Darp.Utils/commit/66aedfe))

## [1.10.0](https://github.com/rosslight/Darp.Utils/compare/v1.9.2...v1.10.0) (2024-10-21)

### Bug Fixes
- Enhance UsernamePassword dialog and add resultdata ([cbf3159](https://github.com/rosslight/Darp.Utils/commit/cbf3159))
- Implement additional validation functions for username or password ([55af016](https://github.com/rosslight/Darp.Utils/commit/55af016))

### Features
- Add a username password dialog ([2a4a2d6](https://github.com/rosslight/Darp.Utils/commit/2a4a2d6))

## [1.9.2](https://github.com/rosslight/Darp.Utils/compare/v1.9.1...v1.9.2) (2024-10-19)

### Bug Fixes
- Restrict dialog root to a type ([761c6cf](https://github.com/rosslight/Darp.Utils/commit/761c6cf))

## [1.9.1](https://github.com/rosslight/Darp.Utils/compare/v1.9.0...v1.9.1) (2024-10-19)

### Bug Fixes
- Add a way to resolve the TopLevel ([6f22670](https://github.com/rosslight/Darp.Utils/commit/6f22670))

## [1.9.0](https://github.com/rosslight/Darp.Utils/compare/v1.8.0...v1.9.0) (2024-09-30)

### Bug Fixes
- Raise appropriate warning if resource entry has an empty name ([8526738](https://github.com/rosslight/Darp.Utils/commit/8526738))

### Features
- Add diagnostics for duplicate keys ([cdd80ec](https://github.com/rosslight/Darp.Utils/commit/cdd80ec))
- Add EmptyResxFile Warning ([00123f7](https://github.com/rosslight/Darp.Utils/commit/00123f7))
- Add missing translation diagnostic ([a26bead](https://github.com/rosslight/Darp.Utils/commit/a26bead))
- Add missing value diagnostic ([03bf2cc](https://github.com/rosslight/Darp.Utils/commit/03bf2cc))

## [1.8.0](https://github.com/rosslight/Darp.Utils/compare/v1.7.4...v1.8.0) (2024-09-26)

### Bug Fixes
- ResxSourceGenerator: Rename CultureUpdated event to CultureChanged ([d60f1b0](https://github.com/rosslight/Darp.Utils/commit/d60f1b0))
- Use EmbeddedResources to have multiple language files in compilation ([64d01e0](https://github.com/rosslight/Darp.Utils/commit/64d01e0))

### Features
- Generate values of other languages as well ([524f990](https://github.com/rosslight/Darp.Utils/commit/524f990))
- Introduce proper recognition of child files ([0201b45](https://github.com/rosslight/Darp.Utils/commit/0201b45))
- Support multiline strings in doc comments ([8482554](https://github.com/rosslight/Darp.Utils/commit/8482554))

## [1.7.4](https://github.com/rosslight/Darp.Utils/compare/v1.7.3...v1.7.4) (2024-09-23)

### Bug Fixes
- naming of .props file and add hirarchy target ([cb02a2b](https://github.com/rosslight/Darp.Utils/commit/cb02a2b))

## [1.7.3](https://github.com/rosslight/Darp.Utils/compare/v1.7.2...v1.7.3) (2024-09-22)

### Bug Fixes
- Package .props transitive build folder ([78d81b7](https://github.com/rosslight/Darp.Utils/commit/78d81b7))

## [1.7.2](https://github.com/rosslight/Darp.Utils/compare/v1.7.1...v1.7.2) (2024-09-22)

### Bug Fixes
- Actually include .props file in build ([994a8f9](https://github.com/rosslight/Darp.Utils/commit/994a8f9))

## [1.7.1](https://github.com/rosslight/Darp.Utils/compare/v1.7.0...v1.7.1) (2024-09-22)

### Bug Fixes
- Mark as generator and add .props file for applying build properties ([a13ad97](https://github.com/rosslight/Darp.Utils/commit/a13ad97))

## [1.7.0](https://github.com/rosslight/Darp.Utils/compare/v1.6.3...v1.7.0) (2024-09-22)

### Bug Fixes
- Ensure SimpleSubject handles removal of observers correctly ([47cc114](https://github.com/rosslight/Darp.Utils/commit/47cc114))

### Features
- Add a source generator generating a singleton resource class for .resx files based on the Microsoft.CodeAnalysis.ResxSourceGenerator ([0dbdea9](https://github.com/rosslight/Darp.Utils/commit/0dbdea9))
- Use resx source generator from roslyn-analyzers ([a0c1a7e](https://github.com/rosslight/Darp.Utils/commit/a0c1a7e))

## [1.6.3](https://github.com/rosslight/Darp.Utils/compare/v1.6.2...v1.6.3) (2024-09-10)

### Bug Fixes
- Remove dynamic data dependency from Darp.Utils.Dialog ([b5c9461](https://github.com/rosslight/Darp.Utils/commit/b5c9461))

## [1.6.2](https://github.com/rosslight/Darp.Utils/compare/v1.6.1...v1.6.2) (2024-09-09)

### Bug Fixes
- Add the content to the result of the ShowAsync method ([877ea47](https://github.com/rosslight/Darp.Utils/commit/877ea47))

## [1.6.1](https://github.com/rosslight/Darp.Utils/compare/v1.6.0...v1.6.1) (2024-09-09)

### Bug Fixes
- failing InputDialog validation ([ce692b5](https://github.com/rosslight/Darp.Utils/commit/ce692b5))

## [1.6.0](https://github.com/rosslight/Darp.Utils/compare/v1.5.1...v1.6.0) (2024-09-09)

### Features
- Add InputDialog overload and dependency on CommunityToolkit.Mvvm ([a2245f7](https://github.com/rosslight/Darp.Utils/commit/a2245f7))
- Use multiple projects dependent on dialog implementation ([4481a7d](https://github.com/rosslight/Darp.Utils/commit/4481a7d))

## [1.5.1](https://github.com/rosslight/Darp.Utils/compare/v1.5.0...v1.5.1) (2024-09-07)

### Bug Fixes
- Create dialogBuilder from UI thread ([ba521a2](https://github.com/rosslight/Darp.Utils/commit/ba521a2))
- Fix not possible isEnabled on SetCloseButton ([ee4c1a7](https://github.com/rosslight/Darp.Utils/commit/ee4c1a7))

## [1.5.0](https://github.com/rosslight/Darp.Utils/compare/v1.4.0...v1.5.0) (2024-09-05)

### Features
- Add dialog service project ([24bc27b](https://github.com/rosslight/Darp.Utils/commit/24bc27b))
- Add secondary button support and message boxes ([a0cca19](https://github.com/rosslight/Darp.Utils/commit/a0cca19))

## [1.4.0](https://github.com/rosslight/Darp.Utils/compare/v1.3.0...v1.4.0) (2024-08-11)

### Features
- Enable overloads which are trimable ([267ea2a](https://github.com/rosslight/Darp.Utils/commit/267ea2a))

## [1.3.0](https://github.com/rosslight/Darp.Utils/compare/v1.2.1...v1.3.0) (2024-08-09)

### Features
- Add ProgramDataService ([809b64f](https://github.com/rosslight/Darp.Utils/commit/809b64f))

## [1.2.1](https://github.com/rosslight/Darp.Utils/compare/v1.2.0...v1.2.1) (2024-08-09)

### Bug Fixes
- Analysis level and configure await warnings ([a0991d2](https://github.com/rosslight/Darp.Utils/commit/a0991d2))

## [1.2.0](https://github.com/rosslight/Darp.Utils/compare/v1.1.0...v1.2.0) (2024-08-09)

### Features
- Add FolderAssetsService ([d94f0bc](https://github.com/rosslight/Darp.Utils/commit/d94f0bc))

## [1.1.0](https://github.com/rosslight/Darp.Utils/compare/v1.0.1...v1.1.0) (2024-08-08)

### Features
- Add DI support ([58a5776](https://github.com/rosslight/Darp.Utils/commit/58a5776))

## [1.0.1](https://github.com/rosslight/Darp.Utils/compare/v1.0.0...v1.0.1) (2024-08-08)

### Bug Fixes
- Make IsDisposed observable ([e62310a](https://github.com/rosslight/Darp.Utils/commit/e62310a))
