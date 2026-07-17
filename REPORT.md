# UWP NativeAOT Migration Report

Last updated: 2026-07-17

## Status

The Power Planner UWP project publishes with .NET 10 NativeAOT and trim analysis enabled. The supported x64 and x86 publishes complete with no compiler, trim, or AOT warnings.

Completed publish validations:

| Platform | Runtime identifier | Result |
|---|---|---|
| x64 | `win-x64` | NativeAOT publish succeeded with zero warnings |
| x86 | `win-x86` | NativeAOT publish succeeded with zero warnings |
| ARM64 | `win-arm64` | Managed projects build; native link requires the C++ ARM64 build tools on the validation machine |

Trim analysis remains visible:

- `PowerPlannerUWP/PowerPlannerUWP.csproj` sets `PublishAot=true` and `SuppressTrimAnalysisWarnings=false`.
- `PowerPlannerAppDataLibrary/PowerPlanner.csproj` and `Vx/Vx.csproj` set `IsTrimmable=true` and `EnableTrimAnalyzer=true`.
- No project-level `NoWarn` suppresses `IL2026`, `IL3050`, `IL2104`, or `IL3053` for the UWP publish.

Do not restore broad project-level IL suppression. It previously hid a startup regression that produced a blank window.

## Build Commands

Use Visual Studio 18 full-framework MSBuild. `dotnet msbuild` does not load the modern UWP XAML targets correctly.

```powershell
$msbuild = 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe'

& $msbuild .\PowerPlannerUWP\PowerPlannerUWP.csproj `
  /t:Restore,Publish `
  /p:Configuration=Release `
  /p:Platform=x64 `
  /p:RuntimeIdentifier=win-x64 `
  /v:minimal
```

Use `Platform=x86` with `RuntimeIdentifier=win-x86` for x86. Use `Platform=ARM64` with `RuntimeIdentifier=win-arm64` for ARM64.

Expected x64 output:

```text
PowerPlannerUWP/bin/Release/net10.0-windows10.0.26100.0/win-x64/publish/PowerPlannerUWP.exe
```

ARM64 additionally requires the Visual Studio Desktop Development for C++ workload and the C++ ARM64 build tools. Shared architecture-neutral projects map the ARM64 solution platform to `AnyCPU`; the UWP projects and final native executable remain ARM64.

## Completed Warning Fixes

### JSON and HTTP

- Removed repository Newtonsoft.Json use from UWP-reachable code.
- Added the source-generated `PowerPlannerJsonContext` closed-type catalog.
- Changed `ToolsPortable.WebHelper` to use caller-provided serialization delegates.
- Migrated sync payloads, changed-item persistence, grade/contact values, settings conversion, and image-upload responses to generated System.Text.Json metadata.
- Updated `PowerPlannerAppAuthLibrary` to the AOT-ready `1.260717.1` package.

Server JSON property names and existing null/default behavior remain explicit in the generated serialization models.

### Released Local Data

- Replaced DataContractSerializer with explicit AOT-safe XML readers and writers for account data.
- Added explicit XML readers for released class tile-setting and saved grade-scale files.
- Preserved the existing account upgrade path after legacy data is loaded.
- Removed DataContractSerializer from the UWP runtime closure.

These XML contracts are intentionally retained for backward compatibility. Do not remove them until support for all released file versions is no longer required.

### Telemetry

- Removed the reflection-heavy Application Insights SDK.
- Added direct Application Insights ingestion over `HttpClient`.
- Added generated JSON envelopes for events, exceptions, and metrics.
- Preserved connection-string endpoint parsing and existing telemetry entry points.

### Vx Runtime

- Replaced reflected view/view-model construction and assignment with typed factories.
- Replaced startup-critical `BindingHost` use with typed property subscriptions.
- Replaced reflected click-event discovery, input validation, settings activation, and data-item property discovery.
- Replaced Vx state discovery with render-time dependency tracking.
- Replaced `[VxSubscribe]` discovery with explicit `RegisterPropertySubscriptions()` overrides.
- Replaced reflected component-property reconciliation with generated `ApplyParametersFrom()` overrides covering public writable render inputs.
- Replaced test-menu `Activator.CreateInstance` use with typed factories.
- Replaced agenda collapse-property reflection with `ICollapsibleHeader` in the `portablelibraries` submodule.

The legacy `Vx/Binding` API remains reflection-based for Android/iOS compatibility and is explicitly marked `RequiresUnreferencedCode`. It is not called by the UWP NativeAOT runtime closure. Its narrow internal suppressions describe that public boundary; they are not project-wide warning suppression.

## Compatibility Notes

The following formats and contracts must remain stable:

- Server request and response JSON names.
- Multi-page sync serialization.
- `ChangedItems.json` contents.
- Existing account XML files.
- Existing class tile-setting XML files.
- Existing saved grade-scale XML files.
- Existing account upgrade behavior.

The serializer migration was designed to preserve these contracts, but packaged runtime tests with representative released data are still required before release.

## Local Package Validation

A locally registered NativeAOT `.Dev` package has previously been launched and exercised on Windows. Verified behavior includes:

- App startup and main-screen creation.
- Responsive process and expected window size.
- Existing account data loading.
- Correct `MainScreenViewModel` data context.
- Material icon rendering when the generated AppX resource layout is preserved.

For loose deployment, start from the generated AppX layout and overlay the NativeAOT publish payload. Copying only the flattened publish directory omits referenced-project resources such as `Vx.Uwp/Resources/Fonts`.

The development package identity is:

```text
61442BareBonesDev.PowerPlanner.Dev_5ga7fac6nanaa
```

Launch it with:

```powershell
Start-Process explorer.exe `
  'shell:AppsFolder\61442BareBonesDev.PowerPlanner.Dev_5ga7fac6nanaa!App'
```

Do not remove or overwrite the production package while testing the `.Dev` identity.

## Remaining Release Validation

Build-time warning cleanup is complete. The remaining work is runtime compatibility validation:

1. Install the C++ ARM64 build tools and complete the ARM64 native link.
2. Test account XML write/read and upgrade behavior.
3. Load representative legacy account, class tile-setting, and saved grade-scale XML files.
4. Round-trip `ChangedItems.json` and a multi-page sync payload.
5. Test create account, login, token refresh, password reset, identity confirmation, and account deletion.
6. Test normal sync, multi-page sync, image upload, and offline restart.
7. Exercise validation errors, popups, Vx state updates, list recycling, notifications, and background activation.
8. Send an event, exception, and metric and verify Application Insights ingestion.

## Repository Detail

`portablelibraries` is a submodule. Its changes to `WebHelper`, `MyHeaderedObservableList`, and the ARM64 project mapping must be committed in the submodule repository first, followed by a parent-repository submodule pointer update.

## Definition Of Done

The code and warning migration is complete when the ARM64 tool prerequisite is installed and all three architectures publish with zero warnings. Release validation is complete when the packaged smoke tests above pass against representative released data and live server flows.
