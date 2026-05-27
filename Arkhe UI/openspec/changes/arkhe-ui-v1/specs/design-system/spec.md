# Design System Specification

## Purpose

Defines the shared visual language: theme tokens, data templates, and icon resources used across all views.

## Requirements

### Requirement: ModernTheme.xaml Tokens

The system MUST provide a `ResourceDictionary` named `ModernTheme.xaml` containing color brushes, typography, corner radius, padding, and easing functions.

#### Scenario: Dark theme loaded

- GIVEN `App.xaml` merges `ModernTheme.xaml` in its `ResourceDictionary.MergedDictionaries`
- WHEN the app launches
- THEN all pages use the dark palette by default
- AND `TextBlock` styles reference `Segoe UI Variable` font family

#### Scenario: Corner radius applied

- GIVEN a `Button` uses `{StaticResource ButtonCornerRadius}`
- WHEN the button renders
- THEN its corner radius is 12 device-independent pixels

### Requirement: ViewTemplates.xaml

The system MUST provide a `ResourceDictionary` named `ViewTemplates.xaml` containing reusable `DataTemplate` definitions for chat bubbles, library cards, and skill cards.

#### Scenario: Chat bubble template

- GIVEN `ChatPage` sets `ItemTemplate` to `{StaticResource ChatMessageTemplate}`
- WHEN a message is added to `ChatViewModel.Messages`
- THEN the bubble renders with the correct alignment and background brush

#### Scenario: Library card template

- GIVEN `LibraryPage` sets `ItemTemplate` to `{StaticResource LibraryCardTemplate}`
- WHEN an item is added to `LibraryViewModel.Items`
- THEN the card renders with glass styling and tag chips

### Requirement: Segoe Fluent Icons

The system MUST reference the Segoe Fluent Icons font for all iconography in `NavigationView`, buttons, and badges.

#### Scenario: Navigation icons

- GIVEN `NavigationView` items specify `Icon` properties
- WHEN the menu renders
- THEN each item displays the correct glyph from Segoe Fluent Icons

#### Scenario: Icon fallback

- GIVEN an icon glyph is not found
- WHEN the element renders
- THEN a default placeholder glyph ("?") is shown
