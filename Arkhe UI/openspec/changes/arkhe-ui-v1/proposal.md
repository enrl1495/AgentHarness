# Proposal: arkhe-ui-v1 — Initial WinUI 3 Desktop Shell

## Intent

Deliver the first runnable WinUI 3 desktop shell for AgentHarness. Provide a dark, high-density UI (Pro-App/Dark Studio) with five functional views, a shared design system, and clean MVVM wiring via CommunityToolkit.Mvvm. This is the presentation-layer foundation; without it, the Core has no visual host.

## Scope

### In Scope
- Project scaffold: `.csproj`, `.slnx`, `App.xaml`, `Package.appxmanifest`
- Global theme system: `Resources/ModernTheme.xaml`, `Resources/ViewTemplates.xaml`, `Resources/Icons/`
- MainWindow shell with `NavigationView` + `Frame` routing
- Five views: Home, Chat, Library (RAG), Skills, Settings
- Five ViewModels (`Main`, `Chat`, `Library`, `Skills`, `Settings`) using `[ObservableProperty]` / `[RelayCommand]`
- `ARCHITECTURE.md` documenting design tokens, materials, navigation, and Core DI integration

### Out of Scope
- Real-time VRAM telemetry (placeholder string in StatusBar)
- Actual chat backend integration (UI-only; wires to `IChatClient` stubs)
- E2E or unit tests (no test infrastructure in this project yet)

## Capabilities

### New Capabilities
- `shell-navigation`: `NavigationView` + `Frame` routing bound to `MainViewModel.SelectedPage`
- `chat-ui`: Message bubbles, internal-thought collapsible panel, input bar, context micro-bar
- `library-grid`: Responsive `GridView` with glass cards, acrylic chips, relevance badges
- `skills-manager`: Vertical card list reading `.md` from `%APPDATA%/AgentHarness/skills/`, toggle state
- `design-system`: `ModernTheme.xaml` tokens (colors, typography, corners, easing), `ViewTemplates.xaml`

### Modified Capabilities
- None (clean slate)

## Approach

MVVM with compiled bindings (`x:Bind`, `Mode=OneWay`). All shared visual state lives in `ModernTheme.xaml`; data templates live in `ViewTemplates.xaml`. Navigation uses `NavigationViewItem.Tag` → `Frame.Navigate()` driven by `MainViewModel.SelectedPage`. DI bootstraps via `AddAgentHarnessCore()` from Core sibling projects.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `AgentHarness.WinUI.csproj` | New | Project file, WinUI self-contained, .NET 10 TFM |
| `AgentHarness.WinUI.slnx` | New | Solution with Core `ProjectReference` |
| `App.xaml` / `App.xaml.cs` | New | App bootstrap, resource dictionaries |
| `MainWindow.xaml` | New | Root grid, OverlayLayer, NavigationView, StatusBar |
| `Views/HomePage.xaml` | New | Landing/dashboard view |
| `Views/ChatPage.xaml` | New | Chat bubbles, thought panel, input |
| `Views/LibraryPage.xaml` | New | RAG grid, search, cards |
| `Views/SkillsPage.xaml` | New | Skill list, toggles |
| `Views/SettingsPage.xaml` | New | Theme, model, endpoint settings |
| `ViewModels/` | New | `Main`, `Chat`, `Library`, `Skills`, `Settings` |
| `Resources/` | New | `ModernTheme.xaml`, `ViewTemplates.xaml`, `Icons/` |
| `ARCHITECTURE.md` | New | Design system and project structure docs |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| WinUI 3 Mica/Acrylic render issues on different Windows builds | Med | Test on 19041+; fallback to solid brushes in theme |
| `x:Bind` path complexity with nested VM types | Med | Keep VM shapes flat; avoid nested `x:Bind` in templates |
| Solution-relative Core references break on other machines | Low | Use `$(SolutionDir)` or document expected repo layout |

## Rollback Plan

1. Revert/delete all created files under `Arkhe UI/`.
2. Remove `.slnx` and `.csproj`.
3. Confirm no Core projects are modified (this change is UI-only).
4. Restore `README.md` if overwritten.

## Dependencies

- `AgentHarness.Core`, `AgentHarness.Hosting`, `AgentHarness.Abstractions` (sibling projects)
- Windows App SDK 1.6+ and .NET 10 SDK
- Segoe Fluent Icons font (Windows 11 inbox)

## Success Criteria

- [ ] `dotnet build` succeeds with zero errors
- [ ] App launches and shows `MainWindow` with dark Mica Alt background
- [ ] All five `NavigationView` items route to their respective pages
- [ ] `ModernTheme.xaml` is the single source of truth for colors, type, corners, and easing
- [ ] `ARCHITECTURE.md` documents the 5 views, navigation pattern, and Core DI connection

## Review Workload Forecast

**Estimated changed lines**: ~1,800–2,400 (XAML + C# + csproj + slnx + docs)
**400-line budget risk**: **High**
**Chained PRs recommended**: **Yes** (stacked-to-main)
**Decision needed before apply**: No — session preference is `auto-chain`

### Proposed Chained PRs
1. **Scaffold + Design System**: `.csproj`, `.slnx`, `App.xaml`, `Resources/` (~400 lines)
2. **Shell + Navigation**: `MainWindow`, `MainViewModel`, `HomePage` (~450 lines)
3. **Chat View**: `ChatPage`, `ChatViewModel`, thought panel, input bar (~500 lines)
4. **Library + Skills**: `LibraryPage`, `SkillsPage`, `GridView`, cards (~450 lines)
5. **Settings + Docs**: `SettingsPage`, `ARCHITECTURE.md`, polish (~300 lines)
