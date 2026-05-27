# Chat View Specification

## Purpose

Defines the chat interface: message bubbles, internal-thought sidebar, input bar, and context-progress micro-bar.

## Requirements

### Requirement: Message Bubbles

The system MUST render chat messages as distinct bubbles aligned left (assistant) or right (user), using `x:Bind` to `ChatViewModel.Messages`.

#### Scenario: User sends a message

- GIVEN the chat input box is focused
- WHEN the user types "Hello" and presses Enter
- THEN a new bubble appears on the right with "Hello"
- AND the input box clears

#### Scenario: Assistant replies

- GIVEN a user message exists
- WHEN the assistant response stream completes
- THEN a new bubble appears on the left with the response text
- AND the thought log panel updates with reasoning steps

#### Scenario: Empty message rejection

- GIVEN the input box is empty or whitespace-only
- WHEN the user presses Enter
- THEN no bubble is created
- AND the input box remains focused

### Requirement: Thought Log Panel

The system MUST display a 320px right sidebar showing sequential reasoning steps with staggered fade-in animations.

#### Scenario: Thought steps arrive

- GIVEN the assistant is processing a request
- WHEN a new thought step is appended to `ChatViewModel.ThoughtLog`
- THEN the step appears in the sidebar with a 150ms fade-in
- AND earlier steps remain visible above it

#### Scenario: Panel collapse

- GIVEN the thought log sidebar is visible
- WHEN the user clicks the collapse chevron
- THEN the sidebar width animates to 0
- AND the chat bubbles expand to fill the space

### Requirement: Context Progress Bar

The system MUST show a micro-bar above the input box indicating context-window utilization.

#### Scenario: Low context usage

- GIVEN the conversation has few tokens
- WHEN `ChatViewModel.ContextPercent` is below 50%
- THEN the micro-bar is green and shows the percentage

#### Scenario: High context usage

- GIVEN the conversation is near the token limit
- WHEN `ChatViewModel.ContextPercent` exceeds 85%
- THEN the micro-bar turns amber
- AND a "Clear context" button becomes visible
