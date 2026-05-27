# Shell & Navigation Specification

## Purpose

Defines the root window chrome, global navigation, and page-routing contract for the Arkhe UI desktop shell.

## Requirements

### Requirement: MainWindow Chrome

The system MUST render a single `MainWindow` with Mica Alt backdrop, a root `Grid` containing an `OverlayLayer`, a `NavigationView`, and a `StatusBar`.

#### Scenario: Application launch

- GIVEN the app has finished `OnLaunched`
- WHEN `MainWindow.Activate()` is called
- THEN the window appears with dark Mica Alt background
- AND the `NavigationView` is visible on the left with five menu items

#### Scenario: Window resize

- GIVEN the app is running
- WHEN the window width drops below 640 effective pixels
- THEN `NavigationView` collapses to compact mode

### Requirement: NavigationView Routing

The system MUST map each `NavigationViewItem` to a distinct `Page` via `Tag` → `Frame.Navigate()`, driven by `MainViewModel.SelectedPage`.

#### Scenario: Menu selection

- GIVEN `MainWindow` is visible
- WHEN the user selects the "Chat" `NavigationViewItem`
- THEN `MainViewModel.SelectedPage` updates to `"Chat"`
- AND the central `Frame` navigates to `ChatPage`

#### Scenario: Invalid tag handling

- GIVEN a `NavigationViewItem` has an unrecognized `Tag`
- WHEN the user selects it
- THEN the `Frame` navigates to `HomePage` as the fallback

### Requirement: StatusBar Telemetry

The system MUST display a `StatusBar` with placeholder text for VRAM usage and model status.

#### Scenario: Idle state

- GIVEN the app is on any page
- WHEN no inference is running
- THEN the `StatusBar` shows "Ready — Model: idle"

#### Scenario: Inference active

- GIVEN an inference request is in flight
- WHEN the `ChatViewModel.IsThinking` property is `true`
- THEN the `StatusBar` shows "Thinking…"
