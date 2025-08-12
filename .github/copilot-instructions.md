# Power Planner Apps - Cross Platform Student Academic Planner

**ALWAYS** reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

Power Planner is a cross-platform academic app for students with UWP (Windows), Android (Xamarin), and iOS (Xamarin) platform apps sharing a common C# data library for syncing, storage, and view model logic.

## Working Effectively

### Initial Repository Setup - REQUIRED FIRST STEPS
**NEVER skip these steps for any development work:**

1. **Clone with submodules**: `git clone --recurse-submodules https://github.com/powerplanner/powerplannerapps.git` -- takes 2-3 minutes. NEVER CANCEL.
   - If already cloned: `git submodule update --init --recursive` -- takes 1-2 minutes
2. **Generate secrets**: `pwsh -File ApplySecrets.ps1` -- takes 15-30 seconds. Creates `secrets.json` and generates required secret files. Ignore errors about missing properties - this is expected with blank secrets.
3. **Fix path case sensitivity** (Linux/Mac only): `sed -i 's|PortableLibraries|portablelibraries|g' Vx/Vx.csproj`

### Build Prerequisites and Limitations

**CRITICAL BUILD REQUIREMENTS:**
- **Full Development**: Requires Windows with Visual Studio 2019+ with Xamarin and UWP SDKs
- **Limited Development**: Linux/Mac can build some components but NOT mobile apps or UWP
- **.NET SDK**: Requires .NET 8 with MAUI workloads for mobile development (Windows only)

### What CAN Be Built (All Platforms)
- **Shared libraries**: `dotnet build portablelibraries/ToolsPortable/ToolsPortable/ToolsPortable.csproj` -- takes 3-5 seconds. NEVER CANCEL.
- **UI framework**: `dotnet build Vx/Vx.csproj` -- takes 10-15 seconds after ToolsPortable is built
- **Submodule libraries**: Individual components in `portablelibraries/` and `shared/` directories

### What CANNOT Be Built (Linux/Mac)
- **Main data library**: `PowerPlannerAppDataLibrary/PowerPlanner.csproj` - requires Android workload due to multi-targeting
- **Android app**: `PowerPlannerAndroid/PowerPlannerAndroid.csproj` - requires MAUI Android workload
- **iOS app**: `PowerPlanneriOS/PowerPlanneriOS.csproj` - requires MAUI iOS workload and Mac
- **UWP app**: `PowerPlannerUWP/PowerPlannerUWP.csproj` - Windows-only platform
- **Full solution**: `PowerPlannerApps.sln` - contains platform-specific projects

## Testing

### Available Tests  
- **Legacy .NET Framework tests**: `PowerPlannerAppDataLibraryUnitTests/` - requires Windows with .NET Framework 4.6.1
- **Modern tests**: `Tests/Vx.Test/` - requires .NET Core 3.1 (not available in current environment)
- **UWP tests**: `UnitTestProject/` - Windows Visual Studio only

### Testing Limitations
**CRITICAL**: The test suite cannot be run in Linux/non-Windows environments due to:
- Legacy tests require .NET Framework (Windows only)
- Modern tests require .NET Core 3.1 (not installed)
- Main data library tests require Android workloads

### Manual Validation - ALWAYS REQUIRED
When making changes to the codebase:
1. **Build shared components** to verify no compilation errors
2. **Test on Windows environment** with full Visual Studio setup for complete validation
3. **Review code changes** carefully since automated testing is limited in non-Windows environments

## Architecture Overview

### Project Structure
- **Apps** (`PowerPlannerUWP`, `PowerPlannerAndroid`, `PowerPlanneriOS`): Platform-specific apps
- **Shared Data** (`PowerPlannerAppDataLibrary`): Common data layer, syncing, storage, view models  
- **UI Framework** (`Vx*`): Cross-platform UI framework and components
- **Dependencies** (`portablelibraries/`, `shared/`): External dependencies via submodules

### Key Development Patterns
- **View Models**: Extend `BaseViewModel` with bindable properties using `SetProperty(ref field, value, nameof(Property))`
- **Computed Properties**: Use `CachedComputation` for dependent properties
- **Localization**: Strings in `PowerPlannerAppDataLibrary/Strings/Resources.resx`
- **A/B Testing**: Configure in `PowerPlannerAppDataLibrary/Helpers/AbTestHelper.cs`

## Build Times and Expectations

**NEVER CANCEL these operations - they may appear hung but are working:**
- **Repository clone with submodules**: 2-3 minutes
- **Submodule initialization**: 1-2 minutes  
- **Secrets generation**: 15-30 seconds
- **ToolsPortable build**: 3-5 seconds
- **Vx framework build**: 10-15 seconds
- **Full Windows solution restore**: 5-10 minutes (Windows only)
- **Full Windows solution build**: 10-30 minutes (Windows only)

## Development Workflows

### Making Code Changes in Limited Environment (Linux/Mac)
1. **ALWAYS** run initial setup steps if starting fresh
2. **Focus on shared libraries**: Most changes will be in `Vx/` and `portablelibraries/` components
3. **Build incrementally**: Test shared components that can build
4. **Use IDE analysis**: Leverage language servers and IDE error detection for validation
5. **Plan for Windows testing**: Prepare changes for testing in full Windows environment

### Making Code Changes in Full Environment (Windows)
1. **Complete all limited environment steps** first
2. **Build complete solution**: `PowerPlannerApps.sln` in Visual Studio
3. **Run full test suite**: All test projects in Visual Studio
4. **Deploy to devices**: Test on actual Android/iOS/UWP devices
5. **Validate end-to-end scenarios**: Complete user workflows

### Common Issues and Solutions
- **"PortableLibraries path not found"**: Fix with case-sensitive path correction: `sed -i 's|PortableLibraries|portablelibraries|g' Vx/Vx.csproj`
- **"Workload not installed"**: Mobile development requires Windows + Visual Studio with Xamarin
- **"Secrets errors during ApplySecrets"**: Normal with blank secrets - errors about missing properties are expected
- **"Tests won't run"**: Test execution requires Windows environment with proper .NET Framework/.NET Core versions

## Validation Scenarios

### Limited Environment (Linux/Mac)
After making changes, ALWAYS verify:
1. **Shared libraries build**: Can ToolsPortable and Vx components build successfully?
2. **Code analysis**: Do language servers report any compilation errors?
3. **File structure**: Are new files properly included in project files?

### Full Environment (Windows)  
After making changes, ALWAYS test:
1. **Complete build**: Does entire solution build without errors?
2. **Test execution**: Do all test projects pass?
3. **Platform deployment**: Can apps deploy to target devices?
4. **User scenarios**: Do key user workflows function correctly?

## File Locations - Quick Reference

### Core Development Files
- **Main solution**: `PowerPlannerApps.sln` (Windows Visual Studio only)
- **Shared data**: `PowerPlannerAppDataLibrary/` (Windows build only)
- **UI framework**: `Vx/`, `Vx.Droid/`, `Vx.iOS/`, `Vx.Uwp/`
- **Shared utilities**: `portablelibraries/ToolsPortable/`

### Configuration Files  
- **Secrets**: `secrets.json` (generated), `secrets.template.json` (template)
- **Submodules**: `portablelibraries/`, `shared/`
- **Build files**: Individual `.csproj` files per project

## Environment-Specific Guidance

### Linux/Mac Development
- **Focus on**: Shared utilities, UI framework components, code analysis
- **Cannot do**: Full builds, testing, mobile app development
- **Workflow**: Make changes → validate syntax → test on Windows

### Windows Development  
- **Can do**: Everything - full builds, testing, deployment
- **Workflow**: Complete development cycle with device testing
- **Required tools**: Visual Studio 2019+, Xamarin, UWP SDK, device emulators

Use these instructions to maximize productivity within the constraints of your current development environment.