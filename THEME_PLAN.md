# Theme Color Customization Plan

Allow users to pick a primary theme color that replaces the default Power Planner blue (`#2E366D`) across the entire app. Darker/lighter variants are auto-generated from the chosen color. The selected color syncs to the user's account so it applies on all their devices.

---

## Current State

### Color Definitions
| Token | Hex | Usage |
|---|---|---|
| `PowerPlannerBlue` / `primary` | `#2E366D` | Toolbars, chrome, buttons, FAB, accent |
| `PowerPlannerSecondaryBlue` / `primaryLight` | `#546BC7` | Hover/selected states, icon backgrounds |
| `PowerPlannerBlueHover` | `#4B60B3` | UWP hover state |
| `PowerPlannerDarkerBlue` / `primaryDark` | `#1F2656` | Status bar, nav bar, darker chrome |
| `ChromeColor` (shared) | `rgb(46,54,109)` | Set in `SharedInitialization.cs`, used by Vx toolbar/nav |
| `DefaultAccentColor` | `rgb(84,107,199)` | Accent color for links, interactive elements |
| `DefaultDarkAccentColor` | `rgb(100,127,234)` | Accent for dark theme backgrounds |
| iOS `PowerPlannerBlueChromeColor` | `rgb(93,107,162)` | Nav bar, status bar (lightened variant) |

### Where Colors Are Set
- **Shared**: `SharedInitialization.Initialize()` sets `Theme.Current.ChromeColor`, `Theme.DefaultAccentColor`, `Theme.DefaultDarkAccentColor`
- **UWP**: `App.xaml` resource dictionary defines `PowerPlannerBlueColor`, `PowerPlannerBlue`, `PowerPlannerSecondaryBlue`, `PowerPlannerBlueHover`, `PowerPlannerDarkerBlue`
- **Android**: `Resources/values/colors.xml` (and `values-night/colors.xml`) defines `primary`, `primaryLight`, `primaryDark`, `accent`, `powerPlannerBlue`
- **iOS**: `ColorResources.cs` defines `PowerPlannerBlueChromeColor`, `PowerPlannerAccentBlue`

### Existing Theme Infrastructure
- `Vx/Views/Theme.cs` — abstract `Theme` class with `ChromeColor`, `AccentColor`, `DefaultAccentColor`, `DefaultDarkAccentColor` properties
- `ThemeExtension` — platform-specific with `Relaunch()` method (used by light/dark theme switching)
- `ThemeSettingsViewModel` — existing settings page for light/dark/auto theme selection
- `ColorBytesHelper` — shared utility for `byte[]` ↔ `System.Drawing.Color` conversion
- `ColorPicker` (UWP) — existing color picker with 13 predefined colors (used for class colors)

### Settings Sync Infrastructure
- `AccountDataItem` stores synced settings with `[DataMember]` serialization
- `SyncedSettings` class in `PowerPlannerSending` carries settings to/from server
- `ChangedSetting` flags enum triggers selective sync via `Sync.SyncSettings()`
- `ApplySyncedSettings()` method applies server values locally
- Pattern: property setter → `NeedsToSyncSettings = true` → `AccountsManager.Save()` → `Sync.SyncSettings()` with flag
- `SyncedSettings` has a `byte[] NoClassColor` field (added 1/6/2026) — this is for the "no class" item color, **not** for theming
- A new `byte[] PrimaryThemeColor` field must be added to `SyncedSettings` for theme support

---

## Design

### What the User Controls
- **Primary color** — a single color pick (from a curated palette or custom hex)
- All variant colors are **auto-derived** from the primary color

### Auto-Generated Variants
From the user's chosen primary color, derive:
| Variant | Algorithm | Usage |
|---|---|---|
| **Primary** | As picked | Toolbars, chrome, buttons, FAB |
| **PrimaryLight** | Increase lightness by ~20% (HSL) | Hover/selected states, icon backgrounds |
| **PrimaryHover** | Increase lightness by ~12% (HSL) | UWP button hover |
| **PrimaryDark** | Decrease lightness by ~15% (HSL) | Status bar, navigation bar |
| **AccentColor** | Increase saturation + lightness for visibility | Links, interactive elements on light backgrounds |
| **DarkAccentColor** | Further lighten accent for dark backgrounds | Accent on dark theme |
| **iOSChrome** | Increase lightness by ~25% from primary | iOS nav bar (since translucent is disabled) |

Implementation: a shared `ThemeColorGenerator` utility class in `PowerPlannerAppDataLibrary` that takes one `Color` input and produces all variants via HSL manipulation.

### Foreground Color
Not user-configurable. White foreground on chrome is kept. If a future need arises for contrast-aware foreground (e.g., user picks a very light primary), we can add auto white/black foreground selection later (P2).

### Extensibility (P2)
The data model stores a `byte[]` for the primary color. Future expansion could add:
- A `ThemePreset` enum (synced separately) for named themes
- Additional color slots (secondary, surface, etc.) stored as additional `byte[]` fields in `SyncedSettings`
- Custom foreground override

For now, only the single primary color byte array is stored and synced.

---

## Implementation Phases

### Phase 1: Shared Data Layer & Color Generation

**Goal**: Store, sync, and derive theme colors from a user-picked primary color.

#### 1A. `ThemeColorGenerator` utility
Create `PowerPlannerAppDataLibrary/Helpers/ThemeColorGenerator.cs`:
```csharp
public static class ThemeColorGenerator
{
    // Default Power Planner primary color
    public static readonly Color DefaultPrimary = Color.FromArgb(46, 54, 109); // #2E366D

    public static ThemeColors Generate(Color primary) { ... }
}

public class ThemeColors
{
    public Color Primary { get; set; }       // User-picked
    public Color PrimaryLight { get; set; }  // +20% lightness
    public Color PrimaryHover { get; set; }  // +12% lightness
    public Color PrimaryDark { get; set; }   // -15% lightness
    public Color Accent { get; set; }        // For interactive elements on light bg
    public Color DarkAccent { get; set; }    // For interactive elements on dark bg
    public Color IOSChrome { get; set; }     // Lightened for iOS nav bar
}
```
- Use HSL color math (convert RGB → HSL, adjust L, convert back)
- Clamp values to valid ranges
- Unit-testable: given a known input, verify all derived colors

#### 1B. `AccountDataItem` property
Add to `AccountDataItem`:
```csharp
private byte[] _primaryThemeColor;
[DataMember]
public byte[] PrimaryThemeColor
{
    get => _primaryThemeColor;
    set => SetProperty(ref _primaryThemeColor, value, nameof(PrimaryThemeColor));
}
```

Add a setter method following existing patterns (e.g., `SetCurrentSemesterAsync`):
```csharp
public async Task SetPrimaryThemeColorAsync(byte[] color, bool uploadSettings = true)
{
    if (ColorArrayEquals(PrimaryThemeColor, color)) return;
    PrimaryThemeColor = color;
    NeedsToSyncSettings = true;
    await AccountsManager.Save(this);
    if (uploadSettings && IsOnlineAccount)
    {
        _ = Sync.SyncSettings(this, Sync.ChangedSetting.PrimaryThemeColor);
    }
}
```

#### 1C. Sync integration
- Add `PrimaryThemeColor` to `ChangedSetting` flags enum in `Sync.cs`
- Add a new `byte[] PrimaryThemeColor` field to `SyncedSettings` in `Requests.cs` (`NoClassColor` is a separate field for the "no class" item color and must not be reused)
- In the sync settings worker, when `ChangedSetting.PrimaryThemeColor` is flagged, populate `SyncedSettings.PrimaryThemeColor` from `account.PrimaryThemeColor`
- In `ApplySyncedSettings()`, apply the incoming `PrimaryThemeColor` value

#### 1D. Cached theme color for instant startup

**Problem**: `SharedInitialization.Initialize()` sets chrome/accent colors *before* the account is loaded from the database. Without a cache, the user would see the default blue flash before their chosen color kicks in.

**Startup timeline** (approximate):
| Event | Time | Account ready? | UI visible? |
|---|---|---|---|
| `SharedInitialization.Initialize()` | ~10ms | No | No |
| Splash screen (compiled resources) | ~50ms | No | Yes |
| `HandleNormalLaunchActivation()` | ~150ms | Local metadata only | Yes |
| `SetCurrentAccount()` | ~200ms | Yes (local) | Yes |
| Initial sync (if needed) | ~2-5s | After sync completes | Yes |

**Solution**: Cache the last-applied primary theme color in `Settings.AppSettings` (`CrossSettings.Current`), which is available **immediately** at process start with no async I/O.

Add to `PowerPlannerAppDataLibrary/Helpers/Settings.cs`:
```csharp
private const string CACHED_PRIMARY_THEME_COLOR = "CachedPrimaryThemeColor";

/// <summary>
/// Hex string of the user's primary theme color, cached for instant startup.
/// Updated whenever the theme color is applied from account data.
/// </summary>
public static string CachedPrimaryThemeColor
{
    get => AppSettings.GetValueOrDefault(CACHED_PRIMARY_THEME_COLOR, null);
    set => AppSettings.AddOrUpdateValue(CACHED_PRIMARY_THEME_COLOR, value);
}
```

**Startup flow**:

1. **`SharedInitialization.Initialize()`** — read `Settings.CachedPrimaryThemeColor`. If non-null, parse it and generate theme colors immediately. Otherwise fall back to default blue. This means the very first Vx-rendered frame already uses the user's color:
   ```csharp
   Color primary = ThemeColorGenerator.DefaultPrimary;
   var cached = Settings.CachedPrimaryThemeColor;
   if (cached != null)
       primary = ColorTranslator.FromHtml(cached);
   var colors = ThemeColorGenerator.Generate(primary);
   Theme.Current.ChromeColor = colors.Primary;
   Theme.DefaultAccentColor = colors.Accent;
   Theme.DefaultDarkAccentColor = colors.DarkAccent;
   ```

2. **`ThemeColorApplier.Apply(AccountDataItem account)`** — called after account load / account switch. Reads `account.PrimaryThemeColor`, generates variants, updates `Theme.Current.*`, and writes back to `Settings.CachedPrimaryThemeColor` so the next launch is instant:
   ```csharp
   public static void Apply(AccountDataItem account)
   {
       var primary = account?.PrimaryThemeColor?.ToColor()
           ?? ThemeColorGenerator.DefaultPrimary;
       var colors = ThemeColorGenerator.Generate(primary);

       Theme.Current.ChromeColor = colors.Primary;
       Theme.DefaultAccentColor = colors.Accent;
       Theme.DefaultDarkAccentColor = colors.DarkAccent;

       // Cache for next startup
       Settings.CachedPrimaryThemeColor = ColorTranslator.ToHtml(primary);

       // Platform-specific updates (UWP brushes, Android status bar, etc.)
       PlatformThemeApplier?.Invoke(colors);
   }
   ```

3. **On account switch / logout** — call `ThemeColorApplier.Apply(newAccount)`. If logging out (null account), the cache is cleared and the default blue is restored.

**What stays default blue regardless**:
- **Android compiled splash** (`SplashScreen.xml` uses `@color/powerPlannerBlue`) — shown for <1 second, can't be dynamic. Acceptable.
- **iOS `LaunchScreen.storyboard`** — system-managed launch image, can't be dynamic.
- **UWP manifest splash** — compiled at build time.

**What DOES pick up the cached color on startup** (before account loads):
- All Vx-rendered toolbars/chrome (read `Theme.Current.ChromeColor`)
- Initial sync loading screen background (if we change it to read from `Theme.Current` instead of hardcoded `#1F2656`)
- iOS `ColorResources.PowerPlannerBlueChromeColor` (once refactored to dynamic property in Phase 5)
- FAB, accent links, nav bars

---

### Phase 2: Theme Color Settings UI (Cross-Platform via Vx)

**Goal**: Let users pick a primary color from the settings page.

#### 2A. `ThemeColorSettingsViewModel`
Create `PowerPlannerAppDataLibrary/ViewModels/MainWindow/Settings/ThemeColorSettingsViewModel.cs`:
- Extends `PopupComponentViewModel`
- Shows a grid of curated primary color swatches (reuse/extend the palette from `ColorPicker`)
- Shows a live preview strip (small toolbar mockup with the selected color)
- Save button applies the color instantly

Curated palette (suggested — these are popular Material/Fluent primary colors):
| Name | Hex |
|---|---|
| Default Blue | `#2E366D` |
| Ocean Blue | `#1565C0` |
| Teal | `#00796B` |
| Green | `#2E7D32` |
| Purple | `#6A1B9A` |
| Deep Purple | `#4527A0` |
| Indigo | `#283593` |
| Red | `#C62828` |
| Pink | `#AD1457` |
| Orange | `#E65100` |
| Brown | `#4E342E` |
| Blue Grey | `#37474F` |
| Dark Grey | `#424242` |

These are intentionally darker/saturated so white foreground text always has sufficient contrast.

#### 2B. Wire into settings list
In `SettingsListViewModel.Render()`, add a new option (near existing theme option):
```csharp
if (HasAccount)
{
    RenderOption(
        layout,
        MaterialDesign.MaterialDesignIcons.Palette,
        "Theme Color", // Localize: PowerPlannerResources.GetString("Settings_ThemeColor_Title")
        currentColorName,  // e.g. "Default Blue" or colored swatch
        OpenThemeColorSettings);
}
```

#### 2C. Instant apply
When the user picks a color and taps Save:
1. Call `Account.SetPrimaryThemeColorAsync(newColorBytes)`
2. Call `ThemeColorApplier.Apply(Account)` to update `Theme.Current.*` properties
3. Each platform reacts to `Theme` property changes (see platform phases below)
4. No relaunch required — the Vx framework re-renders from `Theme.Current` values

---

### Phase 3: UWP Platform Integration

**Goal**: UWP-specific color resources update when theme color changes.

#### 3A. Dynamic resource update
UWP uses `StaticResource` for `PowerPlannerBlue` and related brushes in `App.xaml`. Static resources don't update at runtime. Options:

Change key references from `StaticResource` to `ThemeResource` where possible and update the resource values at runtime:

```csharp
// In UWP theme applier
Application.Current.Resources["PowerPlannerBlueColor"] = themeColors.Primary.ToUwpColor();
((SolidColorBrush)Application.Current.Resources["PowerPlannerBlue"]).Color = themeColors.Primary.ToUwpColor();
((SolidColorBrush)Application.Current.Resources["PowerPlannerSecondaryBlue"]).Color = themeColors.PrimaryLight.ToUwpColor();
((SolidColorBrush)Application.Current.Resources["PowerPlannerBlueHover"]).Color = themeColors.PrimaryHover.ToUwpColor();
((SolidColorBrush)Application.Current.Resources["PowerPlannerDarkerBlue"]).Color = themeColors.PrimaryDark.ToUwpColor();
```
Since `SolidColorBrush` supports property change notification in UWP, updating `.Color` on the existing brush instance propagates to all bound UI. No need to change `StaticResource` to `ThemeResource` as long as we mutate the brush rather than replace it.

#### 3B. Title bar / status bar
Update the UWP title bar color:
```csharp
var titleBar = ApplicationView.GetForCurrentView().TitleBar;
titleBar.BackgroundColor = themeColors.PrimaryDark.ToUwpColor();
titleBar.ButtonBackgroundColor = themeColors.PrimaryDark.ToUwpColor();
```

---

### Phase 4: Android Platform Integration

**Goal**: Android toolbar, status bar, FAB, and system colors update dynamically.

#### 4A. Dynamic color application
Android `colors.xml` values are compiled at build time and can't change at runtime. Override at runtime:

```csharp
// In Android theme applier (called after account load)
var activity = Platform.CurrentActivity;
var window = activity.Window;

// Status bar
window.SetStatusBarColor(themeColors.PrimaryDark.ToDroid());

// Toolbar (if using Vx toolbar, already uses Theme.Current.ChromeColor — no extra work)
// For any remaining XML-based toolbars, find and set background programmatically

// FAB — already uses Theme.Current.ChromeColor via DroidFloatingActionButton.cs — no extra work

// System accent (checkboxes, radio buttons) — override via AppCompat theme attributes
// or wrap in a ContextThemeWrapper with a dynamic ColorStateList
```

#### 4B. Widgets
Android widgets use `RemoteViews` which can set colors. Update widget rendering to read the stored theme color from `AccountDataItem` rather than hardcoded values.

#### 4C. Splash screen
The splash screen uses compiled resources and can't be dynamically themed. Keep it as the default blue. This is acceptable — it's shown for <1 second.

---

### Phase 5: iOS Platform Integration

**Goal**: iOS nav bar, status bar, and accent colors update dynamically.

#### 5A. Dynamic color application
```csharp
// In iOS theme applier (called after account load)
var chromeColor = themeColors.IOSChrome.ToUIColor();

// Update ColorResources (make them non-readonly, or introduce a ThemeColorProvider)
// Better: change ColorResources to read from a settable source
UINavigationBar.Appearance.BarTintColor = chromeColor;
UINavigationBar.Appearance.TintColor = UIColor.White;

// Tab bar
UITabBar.Appearance.TintColor = themeColors.Accent.ToUIColor();
```

#### 5B. `ColorResources.cs` refactor
Change `ColorResources.PowerPlannerBlueChromeColor` and `PowerPlannerAccentBlue` from `readonly` fields to properties that read from the current theme:
```csharp
public static UIColor PowerPlannerBlueChromeColor =>
    Theme.Current.ChromeColor.ToUIColor();
```
This way all existing references automatically get the themed color.

#### 5C. `ConfigureNavBar` update
The existing `ConfigureNavBar` method already references `PowerPlannerBlueChromeColor`. If that becomes a dynamic property (5B), no changes needed. Any new navigation controllers will pick up the themed color automatically.

---

### Phase 6: Polish & Edge Cases

#### 6A. Account switching
When switching accounts, call `ThemeColorApplier.Apply(newAccount)`. This updates `Theme.Current.*` live and overwrites the cached color in `Settings.CachedPrimaryThemeColor` so the next cold start uses the new account's color. Hook into `MainWindowViewModel.SetCurrentAccount()`.

#### 6B. First launch / no account
On first launch, `Settings.CachedPrimaryThemeColor` is null → default blue is used. Once the user creates or logs into an account and picks a theme color, the cache is written. On logout, `ThemeColorApplier.Apply(null)` clears the cache and restores default blue.

#### 6C. Offline accounts
Offline (local-only) accounts still store `PrimaryThemeColor` locally in `AccountDataItem`. It just won't sync. This works with no special handling since `SetPrimaryThemeColorAsync` already checks `IsOnlineAccount` before syncing.

#### 6D. Color contrast validation
Ensure the curated palette only includes colors with sufficient contrast against white foreground text (WCAG AA: ≥4.5:1 contrast ratio). Since we control the palette, validate at design time, not runtime.

#### 6E. Incoming sync
When a sync from another device brings a new `PrimaryThemeColor`, apply it live if the user is currently in the app:
- In `ApplySyncedSettings`, after updating the property, call `ThemeColorApplier.Apply(this)`
- The Vx framework will re-render chrome/toolbars on next frame

#### 6F. Localization
Add string resources:
- `Settings_ThemeColor_Title` = "Theme Color"
- `Settings_ThemeColor_Subtitle` = "Customize your app's primary color"
- `Settings_ThemeColor_Default` = "Default"

---

## File Change Summary

### New Files
| File | Description |
|---|---|
| `PowerPlannerAppDataLibrary/Helpers/ThemeColorGenerator.cs` | HSL math + variant generation |
| `PowerPlannerAppDataLibrary/Helpers/ThemeColorApplier.cs` | Applies theme colors to `Theme.Current` + writes cache to `Settings` for instant next startup |
| `PowerPlannerAppDataLibrary/ViewModels/MainWindow/Settings/ThemeColorSettingsViewModel.cs` | Color picker settings page |

### Modified Files
| File | Change |
|---|---|
| `PowerPlannerAppDataLibrary/DataLayer/AccountDataItem.cs` | Add `PrimaryThemeColor` property + `SetPrimaryThemeColorAsync()` + update `ApplySyncedSettings()` |
| `PowerPlannerAppDataLibrary/SyncLayer/Sync.cs` | Add `PrimaryThemeColor` to `ChangedSetting` enum + sync worker logic |
| `PowerPlannerAppDataLibrary/ViewModels/MainWindow/Settings/SettingsListViewModel.cs` | Add "Theme Color" option to render |
| `PowerPlannerAppDataLibrary/App/SharedInitialization.cs` | Read cached theme color from `Settings` on init for instant startup; fall back to default blue |
| `shared/PowerPlannerSending/PowerPlannerSending/Requests.cs` | Add `[DataMember] byte[] PrimaryThemeColor` field to `SyncedSettings` |
| `PowerPlannerUWP/App.xaml.cs` or `InitializeUWP.cs` | Hook theme color apply after account load |
| `PowerPlannerAndroid/App/PowerPlannerDroidApp.cs` | Hook theme color apply after account load |
| `PowerPlanneriOS/Helpers/ColorResources.cs` | Make chrome colors dynamic |
| `PowerPlanneriOS/AppDelegate.cs` | Hook theme color apply after account load |
| `PowerPlannerAppDataLibrary/Helpers/Settings.cs` | Add `CachedPrimaryThemeColor` property for instant startup |
| `PowerPlannerAppDataLibrary/Strings/Resources.resx` | New localized strings |

### Server-Side (Out of Scope, but Required)
- Add `PrimaryThemeColor` (byte[3] RGB) to the server-side `SyncedSettings` contract so it roundtrips correctly
- `NoClassColor` is a separate field for the "no class" item color and is unrelated to theming

---

## Priority & Ordering

| Phase | Priority | Effort | Dependencies |
|---|---|---|---|
| Phase 1: Data Layer | P0 | Medium | None |
| Phase 2: Settings UI | P0 | Medium | Phase 1 |
| Phase 3: UWP | P1 | Small-Medium | Phase 1 |
| Phase 4: Android | P1 | Medium | Phase 1 |
| Phase 5: iOS | P1 | Medium | Phase 1 |
| Phase 6: Polish | P1 | Small | Phases 1-5 |

Phases 3/4/5 can proceed in parallel once Phase 1 is complete.
