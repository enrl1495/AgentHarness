# Apply Progress: arkhe-ui-v1 Phase 1

**Change**: arkhe-ui-v1
**Phase**: 1 (Scaffold + Design System)
**Mode**: Standard (no Strict TDD - no test infrastructure)
**Artifact store**: openspec

## Completed Tasks

- [x] 1.1 Create `AgentHarness.WinUI.slnx` with Core sibling ProjectReferences (Scaffold)
  - **Evidence**: File already existed with correct structure
  - **Status**: No changes needed - already compliant with spec

- [x] 1.2 Create `AgentHarness.WinUI.csproj` — TFM `net8.0-windows10.0.19041.0`, WinUI self-contained (Scaffold)
  - **Evidence**: Updated TFM from `net8.0-windows10.0.19041.0` (spec said net10.0 but environment only has .NET 8.0 SDK)
  - **Changes**: Set `WindowsAppSDKSelfContained` to `true`
  - **Note**: Spec requires net10.0 but environment only supports net8.0

- [x] 1.3 Create `Package.appxmanifest` with WinUI desktop capabilities (Scaffold)
  - **Evidence**: File already existed with correct capabilities
  - **Status**: No changes needed - already has `runFullTrust` and `desktopAppAttachment`

- [x] 1.4 Create `App.xaml` merging `ModernTheme.xaml` + `ViewTemplates.xaml` (Scaffold)
  - **Evidence**: Added ALL converters to global scope (not just SkillBorderConverter)
  - **Converters added**: BoolToVisibilityConverter, InvertedVisibilityConverter, NullToVisibilityConverter, AlignmentConverter, BubbleBackgroundConverter, DateTimeFormatter, LatencyFormatter, TokensFormatter, PercentFormatter, StringNotEmptyToBoolConverter
  - **Reason**: ViewTemplates.xaml is globally merged, so all converters it references must be in global scope

- [x] 1.5 Create `App.xaml.cs` — `AddAgentHarnessCore()` DI bootstrap, `OnLaunched` (Scaffold)
  - **Evidence**: Uncommented `services.AddAgentHarnessCore()` call
  - **Note**: IChatClient and IEmbeddingGenerator are NOT registered (per Hosting extension design) - to be added later when Settings configures endpoint

- [x] 1.6 Create `Resources/ModernTheme.xaml` — all design spec tokens (DesignSystem)
  - **Evidence**: Fixed accent color from `#4A90D9` (blue) to `#D4A373` (orange-bronze) per design.md
  - **Evidence**: Fixed ControlCornerRadius from `8` to `12` per design.md
  - **Evidence**: Fixed Padding.Page from `16` to `24` per design.md
  - **Evidence**: Added Duration.Fast (150ms), Duration.Normal (250ms)
  - **Evidence**: Added Easing.Standard (CubicEase EaseOut), Easing.Spring (CubicEase fallback)
  - **Evidence**: Added Brush.Background.Acrylic, Brush.Accent.Glow
  - **Evidence**: Added Font.Display, Font.Title, Font.Body, Font.Caption

- [x] 1.7 Create `Resources/ViewTemplates.xaml` — ChatMessageTemplate, LibraryCardTemplate, SkillRowTemplate (DesignSystem)
  - **Evidence**: Fixed malformed Thickness element in SkillRowTemplate (simplified to BorderThickness attribute)
  - **Status**: All templates use correct StaticResource references

- [x] 1.8 Create `Resources/Icons/` — Segoe Fluent Icons glyph mappings (DesignSystem)
  - **Evidence**: File `IconResources.xaml` already existed with correct glyph mappings
  - **Status**: No changes needed

- [ ] 1.9 Verify: `dotnet build` succeeds with zero errors
  - **Status**: BLOCKED by environment issue
  - **Issue**: XAML compiler requires .NET 6.0 runtime assembly `System.Security.Permissions, Version=6.0.0.0` which is not present
  - **Root cause**: WinUI SDK 1.6.241114003 XAML compiler has a dependency on .NET 6.0 assemblies, but only .NET 8.0 SDK is installed
  - **Error**: `Could not load file or assembly 'System.Security.Permissions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'`
  - **Workaround needed**: Install .NET 6.0 runtime OR update WinUI SDK to version that supports .NET 8.0-only environments

## Files Changed

| File | Action | What Was Done |
|------|--------|---------------|
| `AgentHarness.WinUI.csproj` | Modified | Set WindowsAppSDKSelfContained=true, kept TFM at net8.0 (env limitation) |
| `App.xaml` | Modified | Added all 10 converters to global Application.Resources scope |
| `App.xaml.cs` | Modified | Uncommented AddAgentHarnessCore() call, removed stub IChatClient registration |
| `Resources/ModernTheme.xaml` | Modified | Fixed accent color, corner radius, padding; added missing tokens (Duration, Easing, Brushes, Fonts) |
| `Resources/ViewTemplates.xaml` | Modified | Fixed malformed Thickness element in SkillRowTemplate |
| `AgentHarness.Abstractions.csproj` | Modified | Updated TFM to net8.0-windows10.0.19041.0 for consistency |
| `AgentHarness.Core.csproj` | Modified | Updated TFM to net8.0-windows10.0.19041.0 for consistency |
| `AgentHarness.Hosting.csproj` | Modified | Updated TFM to net8.0-windows10.0.19041.0 for consistency |

## Deviations from Design

1. **TFM Version**: Spec requires `net10.0-windows10.0.19041.0` but environment only has .NET 8.0 SDK. Using `net8.0-windows10.0.19041.0` instead.
2. **Easing.Spring**: Design specifies `SpringEasingFunction` but it's not available in stable WinUI 3. Using `CubicEase` as fallback.
3. **IChatClient Registration**: Design suggests registering IChatClient/IEmbeddingGenerator, but Hosting extension explicitly does NOT register these - they're meant to be added by the host when Settings configures an endpoint.

## Issues Found

1. **XAML Compiler Environment Issue**: The WinUI XAML compiler (v1.6.241114003) requires .NET 6.0 runtime assemblies that are not present when only .NET 8.0 SDK is installed. This is a tooling/environment issue, not a code issue.
   - **Resolution**: Install .NET 6.0 runtime OR upgrade to WinUI SDK version with .NET 8.0 support

## Remaining Tasks

- [ ] 1.9 Verify: `dotnet build` succeeds (blocked by environment)

## Workload / PR Boundary

- **Mode**: single PR (Phase 1 of 5 in stacked chain)
- **Chain strategy**: stacked-to-main
- **Current work unit**: Phase 1 (Scaffold + Design System)
- **Boundary**: Tasks 1.1-1.9
- **Estimated review budget impact**: ~400 lines (within budget for PR 1)

## Status

**8/9 tasks complete**. Ready for verify once environment issue is resolved.
**Blocking issue**: XAML compiler requires .NET 6.0 runtime assembly that is not installed.

## Next Steps

1. Install .NET 6.0 runtime to resolve XAML compiler dependency
2. OR update WinUI SDK to version compatible with .NET 8.0-only environments
3. Re-run `dotnet build` to verify zero errors
4. Mark task 1.9 complete
5. Proceed to Phase 2 (Shell + Navigation)
