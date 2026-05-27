# ViewModels Specification

## Purpose

Defines the observable state and command surface for all five MVVM ViewModels, using CommunityToolkit.Mvvm source generators.

## Requirements

### Requirement: MainViewModel

The system MUST provide a `MainViewModel` with an `ObservableProperty` for `SelectedPage` and a `RelayCommand` for `Navigate`.

#### Scenario: Page navigation

- GIVEN `MainViewModel.SelectedPage` is `"Home"`
- WHEN `Navigate("Chat")` is executed
- THEN `SelectedPage` becomes `"Chat"`
- AND `INotifyPropertyChanged.PropertyChanged` fires for `SelectedPage`

### Requirement: ChatViewModel

The system MUST provide a `ChatViewModel` with observable collections for `Messages` and `ThoughtLog`, plus `IsThinking` and `ContextPercent` properties.

#### Scenario: Send message command

- GIVEN the input text is non-empty
- WHEN `SendMessageCommand` executes
- THEN the text is added to `Messages` as a user message
- AND `IsThinking` becomes `true`

#### Scenario: Context overflow

- GIVEN the conversation exceeds the token threshold
- WHEN `ContextPercent` updates to 95
- THEN `PropertyChanged` fires for `ContextPercent`
- AND the UI context bar turns red

### Requirement: LibraryViewModel

The system MUST provide a `LibraryViewModel` with an `ObservableCollection<LibraryItem>` and a `SelectedItem` property.

#### Scenario: Item selection

- GIVEN `LibraryViewModel.Items` contains three items
- WHEN `SelectedItem` is set to the second item
- THEN `PropertyChanged` fires for `SelectedItem`

### Requirement: SkillsViewModel

The system MUST provide a `SkillsViewModel` that loads `.md` files and exposes an `ObservableCollection<SkillItem>` with `IsEnabled` toggles.

#### Scenario: Load skills

- GIVEN the skills folder has two `.md` files
- WHEN `LoadSkillsCommand` executes
- THEN `Skills` contains two `SkillItem` instances
- AND each `SkillItem.IsEnabled` defaults to `false`

### Requirement: SettingsViewModel

The system MUST provide a `SettingsViewModel` with properties for theme, model endpoint, and temperature.

#### Scenario: Theme toggle

- GIVEN the current theme is "Dark"
- WHEN the user toggles theme to "Light"
- THEN `Theme` property updates to "Light"
- AND `PropertyChanged` fires for `Theme`
