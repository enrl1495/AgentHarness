# Design: arkhe-ui-v1 — Initial WinUI 3 Desktop Shell

## Technical Approach

Build a dark, high-density WinUI 3 shell using MVVM with compiled bindings (`x:Bind`). All visual tokens live in `ModernTheme.xaml`; data templates in `ViewTemplates.xaml`. Navigation uses `NavigationViewItem.Tag` mapped to page types, driven by `MainViewModel.SelectedPage`. DI bootstraps via `AddAgentHarnessCore()` from the Core sibling projects, plus WinUI-specific services (`IChatClient`, window handle, dispatcher).

## Architecture Decisions

| Decision | Choice | Alternatives | Rationale |
|----------|--------|-------------|-----------|
| Navigation pattern | `NavigationView` + `Frame` with `Tag`→`Type` map | TabView, custom router | `NavigationView` is Fluent-native, supports Mica, collapsible, and integrates with `PaneFooter`. |
| Binding strategy | `x:Bind` `OneWay` default, `StaticResource` for theme | Classic `{Binding}` | Compiled bindings reduce overhead and catch errors at build time. |
| MVVM toolkit | `CommunityToolkit.Mvvm` source generators | Prism, MVVM Light | Zero-boilerplate `[ObservableProperty]`/`[RelayCommand]`; aligns with modern WinUI docs. |
| Theme architecture | Two dictionaries: `ModernTheme.xaml` (tokens), `ViewTemplates.xaml` (templates) | Single merged dictionary | Separation keeps tokens stable while templates evolve per view. |
| Project references | Relative `..\..\Arkhe Core\*` paths | NuGet packages | Core is sibling source; relative paths keep inner-loop fast. |
| DI container | `Microsoft.Extensions.DependencyInjection` | `HostBuilder`, custom | `AddAgentHarnessCore()` already uses M.E.DI; WinUI app calls `BuildServiceProvider()` in `App.xaml.cs`. |

## Data Flow

```
User Action ──→ View (XAML) ──→ x:Bind ──→ ViewModel
                                              │
                                              ▼
                                    Command / PropertyChanged
                                              │
                                              ▼
                                    Core Service (DI)
                                              │
                                              ▼
                                    ViewModel updates collection
                                              │
                                              ▼
                                    UI rebinds via ObservableCollection
```

- `MainViewModel.SelectedPage` → `NavigationView` selection + `Frame.Navigate()`
- `ChatViewModel.Messages` → `ListView` via `ObservableCollection<ChatMessageViewModel>`
- `LibraryViewModel.Results` → `GridView` via `ObservableCollection<LibraryItemViewModel>`
- `SkillsViewModel.Skills` → `ListView` via `ObservableCollection<SkillViewModel>`

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `AgentHarness.WinUI.slnx` | Create | Solution referencing Core projects via relative path |
| `AgentHarness.WinUI.csproj` | Create | TFM `net10.0-windows10.0.19041.0`, WinUI self-contained |
| `App.xaml` | Create | Merged dictionaries: ModernTheme, ViewTemplates |
| `App.xaml.cs` | Create | DI bootstrap: `AddAgentHarnessCore()` + WinUI services |
| `MainWindow.xaml` | Create | Root Grid, OverlayLayer, NavigationView, StatusBar |
| `MainWindow.xaml.cs` | Create | Navigation handler, overlay management |
| `Views/HomePage.xaml` | Create | Landing dashboard |
| `Views/ChatPage.xaml` | Create | Messages ListView, thought panel, input bar |
| `Views/LibraryPage.xaml` | Create | Search box, responsive GridView |
| `Views/SkillsPage.xaml` | Create | Skill card list, ToggleSwitch |
| `Views/SettingsPage.xaml` | Create | Theme, model, endpoint settings |
| `ViewModels/MainViewModel.cs` | Create | `SelectedPage`, `SelectedFooterItem` |
| `ViewModels/ChatViewModel.cs` | Create | `Messages`, `SendCommand` |
| `ViewModels/LibraryViewModel.cs` | Create | `Search`, `Results`, `SelectedItem` |
| `ViewModels/SkillsViewModel.cs` | Create | `Skills`, `ToggleSkillCommand` |
| `ViewModels/SettingsViewModel.cs` | Create | `Theme`, `Model`, `Endpoint` |
| `Resources/ModernTheme.xaml` | Create | Brushes, fonts, corners, padding, easing, styles |
| `Resources/ViewTemplates.xaml` | Create | DataTemplates for messages, cards, skills |
| `Resources/Icons/` | Create | Segoe Fluent Icons references (font glyph mappings) |
| `ARCHITECTURE.md` | Create | Design system, navigation, DI, file structure docs |

## Interfaces / Contracts

```csharp
// Navigation contract
public enum PageKey { Home, Chat, Library, Skills, Settings }

// ViewModels use CommunityToolkit.Mvvm source generators
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private PageKey _selectedPage;
    [ObservableProperty] private string? _selectedFooterItem;
}

public partial class ChatViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ChatMessageViewModel> _messages = new();
    [RelayCommand] private void Send() { }
}
```

- `ModernTheme.xaml` exposes `SolidColorBrush`, `FontFamily`, `CornerRadius`, `Thickness`, `Duration`, `EasingFunctionBase` resources keyed by convention: `Brush.Background.MicaAlt`, `Font.Display`, `Radius.Container`, `Padding.Page`, etc.
- `ViewTemplates.xaml` exposes `DataTemplate` resources keyed by view-model type: `ChatMessageTemplate`, `LibraryCardTemplate`, `SkillRowTemplate`.

## Component Tree

```
MainWindow (Grid)
├── OverlayLayer (Canvas, Z=100)
├── NavigationView
│   ├── NavigationView.MenuItems
│   │   ├── Home (Tag="Home")
│   │   ├── Chat (Tag="Chat")
│   │   ├── Library (Tag="Library")
│   │   ├── Skills (Tag="Skills")
│   │   └── Settings (Tag="Settings")
│   ├── NavigationView.PaneFooter
│   │   └── SelectorBar (ItemsSource→FooterItems, SelectedItem↔SelectedFooterItem)
│   └── Frame (Content)
│       ├── HomePage
│       ├── ChatPage
│       │   ├── Grid
│       │   │   ├── ListView (ItemsSource→Messages, ItemTemplate→ChatMessageTemplate)
│       │   │   ├── ThoughtPanel (Border, collapsed by default)
│       │   │   ├── InputBar (Grid: TextBox, AttachBtn, MicBtn, SendBtn)
│       │   │   └── ContextMicroBar (ProgressBar 2px, Tooltip)
│       ├── LibraryPage
│       │   ├── SearchBox
│       │   └── GridView (ItemsSource→Results, ItemTemplate→LibraryCardTemplate)
│       ├── SkillsPage
│       │   └── ListView (ItemsSource→Skills, ItemTemplate→SkillRowTemplate)
│       └── SettingsPage
│           └── StackPanel (ThemeCombo, ModelTextBox, EndpointTextBox)
└── StatusBar (Grid)
    ├── TextBlock (ActiveModel)
    ├── TextBlock (ToolsLoaded)
    ├── TextBlock (TokensPerSec)
    └── TextBlock (VRAM / ContextSize placeholder)
```

## Design Token Specification (ModernTheme.xaml)

| Token Category | Key | Value | Usage |
|----------------|-----|-------|-------|
| Background | `Brush.Background.MicaAlt` | `MicaAlt` backdrop | MainWindow, NavigationView |
| Background | `Brush.Background.Acrylic` | `AcrylicBrush` (60%) | Sidebar, tooltips |
| Background | `Brush.Background.Glass` | `AcrylicBrush` (40%) + tint | Cards, chips |
| Background | `Brush.Background.Solid` | `#1E1E1E` | Fallback, elevated surfaces |
| Accent | `Brush.Accent.Primary` | `#D4A373` (bronze/orange) | Buttons, toggles, selection |
| Accent | `Brush.Accent.Glow` | `#D4A373` @ 30% opacity | Icon glows, hover states |
| Text | `Brush.Text.Primary` | `#FFFFFF` | Body, titles |
| Text | `Brush.Text.Secondary` | `#A0A0A0` | Captions, metadata |
| Font | `Font.Display` | `Segoe UI Variable Display` | Headings |
| Font | `Font.Title` | `Segoe UI Variable Display Semibold` | Page titles |
| Font | `Font.Body` | `Segoe UI Variable Text` | Body text |
| Font | `Font.Caption` | `Segoe UI Variable Text` 12px | Metadata, status |
| Shape | `Radius.Container` | `12` | Cards, panels, bubbles |
| Shape | `Radius.Chip` | `6` | Tags, badges |
| Spacing | `Padding.Page` | `24` | Main containers |
| Spacing | `Padding.Card` | `16` | Card internals |
| Animation | `Duration.Fast` | `150ms` | Hover, press |
| Animation | `Duration.Normal` | `250ms` | Transitions, expand |
| Easing | `Easing.Spring` | `SpringEasingFunction DampingRatio="1" Springiness="0.8"` | Toggles, expand |
| Easing | `Easing.Standard` | `CubicEase EasingMode="EaseOut"` | Generic transitions |

## Material Strategy

| Surface | Material | Fallback |
|---------|----------|----------|
| MainWindow background | Mica Alt | `SolidColorBrush #1E1E1E` |
| Thought sidebar (320px, right) | Mica solid | `SolidColorBrush #2D2D2D` |
| Tooltips / context bar | Acrylic (60%) | `SolidColorBrush #333333` @ 90% |
| Library cards / skill rows | Glass (Acrylic 40% + tint) | `SolidColorBrush #2A2A2A` |
| Tags / chips | Acrylic 60% | `SolidColorBrush #3A3A3A` |
| StatusBar | Mica Alt | `SolidColorBrush #1E1E1E` |

## Animation Specification

| Animation | Target | Duration | Easing | Trigger |
|-----------|--------|----------|--------|---------|
| BubbleAppear | Chat bubble opacity + translateY | 200ms | `Easing.Standard` | Item added to `Messages` |
| ThoughtFadeIn | Thought panel opacity | 250ms | `Easing.Standard` | Panel expanded |
| ThoughtStagger | Thought feed items | 50ms stagger | `Easing.Standard` | New thought item added |
| CardHover | Scale 1.02 + shadow 4→8px | 150ms | `Easing.Standard` | `PointerEntered` |
| ToggleSpring | ToggleSwitch translation | 250ms | `Easing.Spring` | `IsOn` changed |
| NavSelection | Content crossfade | 200ms | `Easing.Standard` | `SelectedPage` changed |
| SidebarSlide | Right sidebar translateX | 250ms | `Easing.Standard` | Show/hide toggle |
| Shimmer | Thought block placeholder | 1.5s loop | Linear | Loading state |

## Navigation Routing Table

| NavigationViewItem Tag | Page Type | ViewModel |
|------------------------|-----------|-----------|
| `Home` | `HomePage` | — (static) |
| `Chat` | `ChatPage` | `ChatViewModel` |
| `Library` | `LibraryPage` | `LibraryViewModel` |
| `Skills` | `SkillsPage` | `SkillsViewModel` |
| `Settings` | `SettingsPage` | `SettingsViewModel` |

- `MainWindow.xaml.cs` subscribes to `NavigationView.SelectionChanged`, reads `Tag`, maps to `Type`, calls `Frame.Navigate(type, null, vm)`.
- `MainViewModel.SelectedPage` is two-way bound so programmatic changes update the UI.

## XAML Resource Dependency Graph

```
App.xaml
├── ModernTheme.xaml
│   ├── Colors (no deps)
│   ├── Brushes (depends on Colors)
│   ├── Fonts (no deps)
│   ├── Sizes/Radii (no deps)
│   ├── Animations (no deps)
│   └── ControlStyles (depends on Brushes, Fonts, Sizes, Animations)
└── ViewTemplates.xaml
    └── DataTemplates (depends on ModernTheme brushes/fonts; used by views)
```

- Views (`*Page.xaml`) merge `ModernTheme.xaml` and `ViewTemplates.xaml` via `App.xaml` `MergedDictionaries`.
- No view directly references another view’s resources.

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | ViewModel property change notifications, command execution | Manual validation (no test infra yet) |
| Integration | DI container builds without missing registrations | `dotnet build` + app launch |
| E2E | Navigation routes to all 5 pages, theme renders | Manual smoke test |

## Migration / Rollout

No migration required. This is a clean-slate UI project. Rollback is deletion of `Arkhe UI/` files.

## Open Questions

- [ ] Should `IChatClient` be registered in `App.xaml.cs` or deferred until Settings configures an endpoint?
- [ ] Is the Skills `.md` directory format stable, or should the UI parse frontmatter?
- [ ] Should the right thought sidebar be a `SplitView` or a custom `Border` with manual animation?

## Cross-Reference to Proposal Deliverables

| Proposal Deliverable | Design Section | File(s) |
|----------------------|----------------|---------|
| `shell-navigation` | Navigation Routing Table, Component Tree | `MainWindow.xaml`, `MainViewModel.cs` |
| `chat-ui` | Component Tree, Data Flow | `ChatPage.xaml`, `ChatViewModel.cs`, `ViewTemplates.xaml` |
| `library-grid` | Component Tree, Design Tokens | `LibraryPage.xaml`, `LibraryViewModel.cs` |
| `skills-manager` | Component Tree, Data Flow | `SkillsPage.xaml`, `SkillsViewModel.cs` |
| `design-system` | Design Token Specification, Material Strategy, Animation Specification | `ModernTheme.xaml`, `ViewTemplates.xaml` |
| `ARCHITECTURE.md` | Testing Strategy, Migration | `ARCHITECTURE.md` |
