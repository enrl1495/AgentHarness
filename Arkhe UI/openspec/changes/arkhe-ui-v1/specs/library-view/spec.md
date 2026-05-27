# Library View Specification

## Purpose

Defines the RAG library grid: responsive layout, glass cards, tag chips, and vector-relevance badges.

## Requirements

### Requirement: Responsive GridView

The system MUST display library items in a `GridView` that reflows based on window width, using `x:Bind` to `LibraryViewModel.Items`.

#### Scenario: Wide window

- GIVEN the window width is greater than 1200px
- WHEN the Library page loads
- THEN the `GridView` shows four columns of cards

#### Scenario: Narrow window

- GIVEN the window width is less than 800px
- WHEN the Library page loads
- THEN the `GridView` shows two columns of cards

### Requirement: Glass Cards

Each library item MUST render as a glass-styled card with acrylic background, rounded corners, and hover lift animation.

#### Scenario: Card hover

- GIVEN the mouse is over a library card
- WHEN the hover state triggers
- THEN the card elevates by 4px
- AND its acrylic background intensifies

#### Scenario: Card selection

- GIVEN a card is not selected
- WHEN the user clicks it
- THEN the card shows a selection border
- AND `LibraryViewModel.SelectedItem` updates

### Requirement: Tag Chips & Relevance Badges

Each card MUST display tag chips and a vector-relevance percentage badge.

#### Scenario: Card with multiple tags

- GIVEN a library item has tags `["docs", "api"]`
- WHEN the card renders
- THEN two tag chips appear below the title
- AND the relevance badge shows "92%"

#### Scenario: Zero relevance

- GIVEN a library item has relevance `0`
- WHEN the card renders
- THEN the relevance badge shows "0%" in muted color
