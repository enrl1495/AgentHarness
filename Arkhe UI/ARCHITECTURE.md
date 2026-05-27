# Architecture — Arkhe UI Design System

## Overview

Arkhe UI is a WinUI 3 desktop shell built with .NET 10, Windows App SDK, and CommunityToolkit.Mvvm. This document describes the design system, visual tokens, navigation pattern, and project structure.

## Design Tokens

All visual tokens are defined in `Resources/ModernTheme.xaml` as a single source of truth.

### Color Palette

| Token | Value | Usage |
|-------|-------|-------|
| `Color.Background.MicaAlt` | `#202020` | Main window background |
| `Color.Background.Surface` | `#2D2D2D` | Navigation pane, elevated surfaces |
| `Color.Background.Glass` | `#2A2A2A` | Cards, chips |
| `Color.Accent.Primary` | `#D4A373` | Buttons, toggles, selection (orange-bronze) |
| `Color.Text.Primary` | `#FFFFFF` | Body text, titles |
| `Color.Text.Secondary` | `#A0A0A0` | Captions, metadata |

### Brushes

Brushes follow the naming convention `Brush.{Category}.{Variant}`:

```xaml
<SolidColorBrush x:Key="Brush.Background.MicaAlt" Color="{StaticResource Color.Background.MicaAlt}" />
<SolidColorBrush x:Key="Brush.Accent.Primary" Color="{StaticResource Color.Accent.Primary}" />
```

### Typography

All text uses **Segoe UI Variable** font family:

| Style | Font | Size | Weight |
|-------|------|------|--------|
| `TextBlock.Display` | Segoe UI Variable Display | 28px | Bold |
| `TextBlock.Title` | Segoe UI Variable Display Semibold | 20px | SemiBold |
| `TextBlock.Body` | Segoe UI Variable Text | 14px | Regular |
| `TextBlock.Caption` | Segoe UI Variable Text | 12px | Regular |

### Shape & Spacing

```xaml
<CornerRadius x:Key="ControlCornerRadius">12</CornerRadius>
<CornerRadius x:Key="ChipCornerRadius">6</CornerRadius>
<CornerRadius x:Key="BubbleCornerRadius">16</CornerRadius>

<Thickness x:Key="Padding.Page">24</Thickness>
<Thickness x:Key="Padding.Card">16</Thickness>
```

### Animation

| Token | Duration | Easing |
|-------|----------|--------|
| `Duration.Fast` | 150ms | CubicEase (EaseOut) |
| `Duration.Normal` | 250ms | CubicEase (EaseOut) |
| `Duration.Slow` | 400ms | CubicEase (EaseOut) |

## Materials

| Surface | Material | Implementation |
|---------|----------|----------------|
| MainWindow | Mica Alt | `{StaticResource Brush.Background.MicaAlt}` |
| Navigation pane | Solid surface | `{StaticResource Brush.Background.Surface}` |
| Cards / chips | Glass (Acrylic 40%) | `{StaticResource Brush.Background.Glass}` |
| StatusBar | Mica Alt | `{StaticResource Brush.Background.MicaAlt}` |

## Views

All views are located in the `Views/` folder and inherit from `Page`:

| View | Purpose | Status |
|------|---------|--------|
| `HomePage.xaml` | Landing dashboard | Phase 2 |
| `ChatPage.xaml` | Chat interface with thought panel | Phase 3 |
| `LibraryPage.xaml` | Document library grid | Phase 4 |
| `SkillsPage.xaml` | Skills manager list | Phase 4 |
| `SettingsPage.xaml` | App configuration | Phase 5 |

### View Templates

Reusable `DataTemplate` definitions live in `Resources/ViewTemplates.xaml`:

- `ChatMessageTemplate` — Chat bubble with role-based alignment
- `LibraryCardTemplate` — Glass card with tags and relevance badge
- `SkillRowTemplate` — Skill row with toggle switch

## Navigation Pattern

Navigation uses `NavigationView` with a `Tag`→`Type` mapping:

```csharp
// MainWindow.xaml.cs
private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
{
    if (args.SelectedItem is NavigationViewItem item)
    {
        var tag = item.Tag?.ToString();
        var pageType = tag switch
        {
            "Home" => typeof(HomePage),
            "Chat" => typeof(ChatPage),
            "Library" => typeof(LibraryPage),
            "Skills" => typeof(SkillsPage),
            "Settings" => typeof(SettingsPage),
            _ => typeof(HomePage)
        };
        ContentFrame.Navigate(pageType);
    }
}
```

### Navigation Items

| Tag | Content | Icon Glyph |
|-----|---------|------------|
| `Home` | Home | `&#xE189;` |
| `Chat` | Chat | `&#xE154;` |
| `Library` | Library | `&#xE1DD;` |
| `Skills` | Skills | `&#xE711;` |
| `Settings` | Settings | `&#xE713;` |

## Dependency Injection

DI is bootstrapped in `App.xaml.cs` using `Microsoft.Extensions.DependencyInjection`:

```csharp
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    var services = new ServiceCollection();
    services.AddAgentHarnessCore();
    _services = services.BuildServiceProvider();

    var mainWindow = new MainWindow();
    mainWindow.Activate();
}
```

### Core Integration

The UI references three sibling Core projects:

- `AgentHarness.Abstractions` — Interfaces and DTOs
- `AgentHarness.Core` — Domain logic
- `AgentHarness.Hosting` — DI extensions (`AddAgentHarnessCore()`)

Project references use relative paths:
```xml
<ProjectReference Include="..\Arkhe Core\AgentHarness.Abstractions\AgentHarness.Abstractions.csproj" />
```

## File Structure

```
Arkhe UI/
├── AgentHarness.WinUI.slnx          # Solution file
├── AgentHarness.WinUI.csproj        # Project file
├── Package.appxmanifest             # App manifest
├── app.manifest                     # Windows compatibility manifest
├── App.xaml                         # Application resources
├── App.xaml.cs                      # DI bootstrap, OnLaunched
├── MainWindow.xaml                  # Shell with NavigationView
├── MainWindow.xaml.cs               # Navigation handler
├── ARCHITECTURE.md                  # This file
├── Resources/
│   ├── ModernTheme.xaml             # Design tokens (colors, fonts, etc.)
│   ├── ViewTemplates.xaml           # DataTemplates for views
│   └── Icons/
│       └── IconResources.xaml       # Segoe Fluent Icons mappings
├── Views/
│   ├── HomePage.xaml(.cs)           # Landing page
│   ├── ChatPage.xaml(.cs)           # Chat interface
│   ├── LibraryPage.xaml(.cs)        # Library grid
│   ├── SkillsPage.xaml(.cs)         # Skills manager
│   └── SettingsPage.xaml(.cs)       # Settings form
├── ViewModels/                      # ViewModels (Phase 2+)
│   └── *.cs
└── Controls/                        # Custom controls (future)
    └── *.xaml
```

## Binding Strategy

- Use `x:Bind` (compiled binding) by default for performance and type safety
- Use `StaticResource` for theme references (brushes, styles, templates)
- ViewModels use `ObservableObject` and `RelayCommand` from CommunityToolkit.Mvvm

## Next Phases

- **Phase 2**: Shell + Navigation — Implement `MainViewModel`, wire up navigation ✅
- **Phase 3**: Chat View — Full chat interface with thought panel ✅
- **Phase 4**: Library + Skills — Document grid and skills manager ✅
- **Phase 5**: Settings + Documentation — Configuration and final docs ✅

## Complete View Inventory

All five views are implemented and wired via navigation:

| View | Purpose | ViewModel | Key Features |
|------|---------|-----------|--------------|
| `HomePage.xaml` | Landing dashboard | — | Static welcome screen |
| `ChatPage.xaml` | Chat interface | `ChatViewModel` | Messages, thought panel, input bar, context micro-bar |
| `LibraryPage.xaml` | Document library | `LibraryViewModel` | Search, responsive GridView, glass cards |
| `SkillsPage.xaml` | Skills manager | `SkillsViewModel` | Skill list from `%APPDATA%`, toggle switches |
| `SettingsPage.xaml` | App configuration | `SettingsViewModel` | Theme, Mica, model, endpoint settings |

## Design System Summary

### All Design Tokens

Tokens live in `Resources/ModernTheme.xaml`:

| Category | Token | Value | Usage |
|----------|-------|-------|-------|
| **Background Colors** | `Color.Background.MicaAlt` | `#202020` | MainWindow |
| | `Color.Background.Surface` | `#2D2D2D` | Navigation pane |
| | `Color.Background.Glass` | `#2A2A2A` | Cards, chips |
| | `Color.Background.Elevated` | `#1E1E1E` | Elevated surfaces |
| **Accent Colors** | `Color.Accent.Primary` | `#D4A373` | Buttons, toggles |
| | `Color.Accent.Glow` | `#D4A373` (30%) | Icon glows |
| | `Color.Accent.Hover` | `#E5B88A` | Hover states |
| | `Color.Accent.Pressed` | `#B8926A` | Pressed states |
| **Text Colors** | `Color.Text.Primary` | `#FFFFFF` | Body, titles |
| | `Color.Text.Secondary` | `#A0A0A0` | Captions |
| | `Color.Text.Disabled` | `#606060` | Disabled text |
| **Border Colors** | `Color.Border.Subtle` | `#404040` | Subtle borders |
| | `Color.Border.Strong` | `#505050` | Strong borders |
| **Fonts** | `Font.Display` | Segoe UI Variable Display | Headings (28px) |
| | `Font.Title` | Segoe UI Variable Display Semibold | Titles (20px) |
| | `Font.Body` | Segoe UI Variable Text | Body (14px) |
| | `Font.Caption` | Segoe UI Variable Text | Captions (12px) |
| **Radii** | `ControlCornerRadius` | 12 | Cards, buttons |
| | `ChipCornerRadius` | 6 | Tags, badges |
| | `BubbleCornerRadius` | 16 | Chat bubbles |
| **Spacing** | `Padding.Page` | 24 | Page containers |
| | `Padding.Card` | 16 | Card internals |
| | `Spacing.Small` | 8 | Small gaps |
| | `Spacing.Medium` | 16 | Medium gaps |
| | `Spacing.Large` | 24 | Large gaps |
| **Animation** | `Duration.Fast` | 150ms | Hover, press |
| | `Duration.Normal` | 250ms | Transitions |
| | `Duration.Slow` | 400ms | Slow fades |
| **Easing** | `Easing.Standard` | CubicEase (EaseOut) | Generic |
| | `Easing.Gentle` | QuadraticEase (EaseOut) | Subtle |

### Material Strategy

| Surface | Material | Resource | Fallback |
|---------|----------|----------|----------|
| MainWindow | Mica Alt | `Brush.Background.MicaAlt` | `#1E1E1E` solid |
| Navigation pane | Solid surface | `Brush.Background.Surface` | `#2D2D2D` |
| Thought sidebar | Mica solid | `Brush.Background.Surface` | `#2D2D2D` |
| Library cards | Glass (Acrylic 40%) | `Brush.Background.Glass` | `#2A2A2A` |
| Skills rows | Glass | `Brush.Background.Glass` | `#2A2A2A` |
| Tooltips | Acrylic 60% | Custom | `#333333` (90%) |
| StatusBar | Mica Alt | `Brush.Background.MicaAlt` | `#1E1E1E` |
| Tags/chips | Glass | `Brush.Background.Glass` | `#2A2A2A` |

### Animation Catalog

| Animation | Target | Duration | Easing | Trigger |
|-----------|--------|----------|--------|---------|
| `BubbleAppear` | Chat bubble opacity + translateY | 200ms | Standard | Message added |
| `ThoughtFadeIn` | Thought panel opacity | 250ms | Standard | Panel expand |
| `ThoughtStagger` | Thought feed items | 50ms stagger | Standard | New thought |
| `CardHover` | Scale 1.02 + shadow | 150ms | Standard | PointerOver |
| `ToggleSpring` | ToggleSwitch translation | 250ms | Spring (native) | IsOn changed |
| `NavSelection` | Content crossfade | 200ms | Standard | Page change |
| `SidebarSlide` | Right sidebar translateX | 250ms | Standard | Show/hide |
| `Shimmer` | Thought placeholder | 1.5s loop | Linear | Loading |

## Navigation Pattern

NavigationView + Frame with Tag→Type mapping in `MainWindow.xaml.cs`:

```csharp
private static readonly Dictionary<string, Type> PageMap = new()
{
    ["Home"] = typeof(HomePage),
    ["Chat"] = typeof(ChatPage),
    ["Library"] = typeof(LibraryPage),
    ["Skills"] = typeof(SkillsPage),
    ["Settings"] = typeof(SettingsPage)
};
```

### Navigation Routing Table

| NavigationViewItem Tag | Page Type | ViewModel | Injection |
|------------------------|-----------|-----------|-----------|
| `Home` | `HomePage` | — | Static |
| `Chat` | `ChatPage` | `ChatViewModel` | Transient |
| `Library` | `LibraryPage` | `LibraryViewModel` | Transient |
| `Skills` | `SkillsPage` | `SkillsViewModel` | Transient |
| `Settings` | `SettingsPage` | `SettingsViewModel` | Transient |

## Dependency Injection

Bootstrapped in `App.xaml.cs`:

```csharp
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    var services = new ServiceCollection();
    services.AddAgentHarnessCore();
    
    // ViewModels
    services.AddSingleton<MainViewModel>();
    services.AddTransient<ChatViewModel>();
    services.AddTransient<LibraryViewModel>();
    services.AddTransient<SkillsViewModel>();
    services.AddTransient<SettingsViewModel>();
    
    _services = services.BuildServiceProvider();
    var mainWindow = new MainWindow();
    mainWindow.Activate();
}
```

### Core Connection

The UI consumes three sibling Core projects via relative path references:

- `AgentHarness.Abstractions` — Interfaces and DTOs
- `AgentHarness.Core` — Domain logic
- `AgentHarness.Hosting` — DI extension `AddAgentHarnessCore()`

The `AddAgentHarnessCore()` method registers all core services (chat client, skills loader, library indexer, etc.) into the same `IServiceCollection` used by the WinUI shell.

## File Structure

```
Arkhe UI/
├── AgentHarness.WinUI.slnx          # Solution (Core ProjectReferences)
├── AgentHarness.WinUI.csproj        # TFM net10.0-windows10.0.19041.0
├── Package.appxmanifest             # WinUI desktop capabilities
├── app.manifest                     # Windows compatibility
├── App.xaml                         # Merged dictionaries
├── App.xaml.cs                      # DI bootstrap, OnLaunched
├── MainWindow.xaml                  # NavigationView shell
├── MainWindow.xaml.cs               # Tag→Type navigation
├── ARCHITECTURE.md                  # This document
├── Resources/
│   ├── ModernTheme.xaml             # All design tokens
│   ├── ViewTemplates.xaml           # DataTemplates
│   └── Icons/
│       └── IconResources.xaml       # Segoe Fluent Icons
├── Views/
│   ├── HomePage.xaml(.cs)           # Landing (Phase 2)
│   ├── ChatPage.xaml(.cs)           # Chat + thought panel (Phase 3)
│   ├── LibraryPage.xaml(.cs)        # Document grid (Phase 4)
│   ├── SkillsPage.xaml(.cs)         # Skills manager (Phase 4)
│   └── SettingsPage.xaml(.cs)       # Configuration (Phase 5)
├── ViewModels/
│   ├── MainViewModel.cs             # SelectedPage, SelectedFooterItem
│   ├── ChatViewModel.cs             # Messages, ThoughtLog, SendCommand
│   ├── ChatMessageViewModel.cs      # Role, Text, Timestamp
│   ├── LibraryViewModel.cs          # Items, SelectedItem, SearchQuery
│   ├── LibraryEntryViewModel.cs     # Title, Tags, Relevance
│   ├── SkillsViewModel.cs           # Skills, LoadSkillsCommand
│   ├── SkillViewModel.cs            # Name, Description, IsEnabled
│   └── SettingsViewModel.cs         # Theme, Model, Endpoint, Mica
└── Controls/                        # Future custom controls
```

## Binding Strategy

- **`x:Bind`** (compiled binding) for all ViewModel bindings — type-safe, build-time errors
- **`StaticResource`** for all theme references (brushes, styles, templates)
- **`ObservableObject`** and **`[ObservableProperty]`** from CommunityToolkit.Mvvm
- **`[RelayCommand]`** for all commands — no boilerplate `ICommand` implementations
- **`x:DataType`** annotations on all DataTemplates for IntelliSense

## ViewModel Patterns

All ViewModels use CommunityToolkit.Mvvm source generators:

```csharp
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _theme = "Dark";
    [ObservableProperty] private string _model = "deepseek-v4-pro";
    [ObservableProperty] private string _endpoint = "http://localhost:11434";
    [ObservableProperty] private bool _isMicaEnabled = true;
    [ObservableProperty] private double _micaOpacity = 0.7;
    [ObservableProperty] private string _accentColor = "#FF8C42";

    [RelayCommand] private void Save() { /* TODO: persistence */ }
    [RelayCommand] private void Reset() { /* reset to defaults */ }
}
```

PropertyChanged is auto-generated for all `[ObservableProperty]` fields. Commands implement `ICommand` with optional `CanExecute` predicates.
