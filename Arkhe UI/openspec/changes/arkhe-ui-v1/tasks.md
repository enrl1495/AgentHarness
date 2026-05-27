# Tasks: arkhe-ui-v1 — Initial WinUI 3 Desktop Shell

## Review Workload Forecast

| Field | Value |
|-------|-------|
| Estimated changed lines | ~1,700–2,000 |
| 400-line budget risk | High |
| Chained PRs recommended | Yes |
| Suggested split | PR 1 → PR 2 → PR 3 → PR 4 → PR 5 |
| Delivery strategy | auto-chain |
| Chain strategy | stacked-to-main |

Decision needed before apply: No
Chained PRs recommended: Yes
Chain strategy: stacked-to-main
400-line budget risk: High

### Suggested Work Units

| Unit | Goal | Likely PR | Notes |
|------|------|-----------|-------|
| 1 | Scaffold + Design System | PR 1 | main; ~380 lines |
| 2 | Shell + Navigation | PR 2 | main; ~360 lines |
| 3 | Chat View | PR 3 | main; ~300 lines |
| 4 | Library + Skills | PR 4 | main; ~430 lines |
| 5 | Settings + Docs | PR 5 | main; ~265 lines |

## Phase 1: Scaffold + Design System (PR 1)

- [x] 1.1 Create `AgentHarness.WinUI.slnx` with Core sibling ProjectReferences (Scaffold)
- [x] 1.2 Create `AgentHarness.WinUI.csproj` — TFM `net8.0-windows10.0.19041.0`, WinUI self-contained (Scaffold)
- [x] 1.3 Create `Package.appxmanifest` with WinUI desktop capabilities (Scaffold)
- [x] 1.4 Create `App.xaml` merging `ModernTheme.xaml` + `ViewTemplates.xaml` (Scaffold)
- [x] 1.5 Create `App.xaml.cs` — `AddAgentHarnessCore()` DI bootstrap, `OnLaunched` (Scaffold)
- [x] 1.6 Create `Resources/ModernTheme.xaml` — all design spec tokens (DesignSystem)
- [x] 1.7 Create `Resources/ViewTemplates.xaml` — ChatMessageTemplate, LibraryCardTemplate, SkillRowTemplate (DesignSystem)
- [x] 1.8 Create `Resources/Icons/` — Segoe Fluent Icons glyph mappings (DesignSystem)
- [ ] 1.9 Verify: `dotnet build` succeeds with zero errors

## Phase 2: Shell + Navigation (PR 2)

- [ ] 2.1 Create `ViewModels/PageKey.cs` enum — Home, Chat, Library, Skills, Settings (ShellNav)
- [ ] 2.2 Create `ViewModels/MainViewModel.cs` — `[ObservableProperty] SelectedPage`, `SelectedFooterItem` (ShellNav)
- [ ] 2.3 Create `MainWindow.xaml` — Grid, OverlayLayer Canvas, NavigationView 5 items with Tag, StatusBar (ShellNav)
- [ ] 2.4 Create `MainWindow.xaml.cs` — SelectionChanged → Tag→Type map → `Frame.Navigate()` (ShellNav)
- [ ] 2.5 Create `Views/HomePage.xaml` landing dashboard (ShellNav)
- [ ] 2.6 Verify: App launches Mica Alt, 5 nav items visible, Home selected by default

## Phase 3: Chat View (PR 3)

- [ ] 3.1 Create `ViewModels/ChatMessageViewModel.cs` — Role, Text, Timestamp (ChatView)
- [ ] 3.2 Create `ViewModels/ChatViewModel.cs` — Messages, ThoughtLog, IsThinking, ContextPercent, `[RelayCommand] Send` (ChatView)
- [ ] 3.3 Create `Views/ChatPage.xaml` — ListView (Messages), 320px ThoughtPanel, InputBar, ContextMicroBar (ChatView)
- [ ] 3.4 Create `Views/ChatPage.xaml.cs` — thought panel collapse animation (ChatView)
- [ ] 3.5 Verify: Send→bubble appears, thought toggle works, context bar reflects ContextPercent

## Phase 4: Library + Skills (PR 4)

- [ ] 4.1 Create `ViewModels/LibraryEntryViewModel.cs` — Title, Tags, Relevance (LibraryView)
- [ ] 4.2 Create `ViewModels/LibraryViewModel.cs` — Items, SelectedItem, SearchQuery (LibraryView)
- [ ] 4.3 Create `Views/LibraryPage.xaml` — SearchBox, responsive GridView, glass cards (LibraryView)
- [ ] 4.4 Create `ViewModels/SkillViewModel.cs` — Name, Description, IsEnabled (SkillsView)
- [ ] 4.5 Create `ViewModels/SkillsViewModel.cs` — Skills, LoadSkillsCommand, ToggleSkillCommand (SkillsView)
- [ ] 4.6 Create `Views/SkillsPage.xaml` — ListView, ToggleSwitch, empty-state message (SkillsView)
- [ ] 4.7 Verify: Library hover/selection; Skills load from `%APPDATA%`, toggles animate

## Phase 5: Settings + Docs (PR 5)

- [ ] 5.1 Create `ViewModels/SettingsViewModel.cs` — Theme, ModelEndpoint, Temperature (SettingsView)
- [ ] 5.2 Create `Views/SettingsPage.xaml` — theme combo, model/endpoint TextBox, temp slider (SettingsView)
- [ ] 5.3 Create `ARCHITECTURE.md` — design tokens, navigation, DI, file structure
- [ ] 5.4 Verify: All 5 pages route correctly, `dotnet build` clean, docs complete
