# Skills View Specification

## Purpose

Defines the skills manager: vertical card list populated from AppData `.md` files, with toggle switches and spring animations.

## Requirements

### Requirement: Skill Card List

The system MUST read `.md` files from `%APPDATA%\AgentHarness\skills\` and display each as a vertical card in a `ListView` bound to `SkillsViewModel.Skills`.

#### Scenario: Skills folder exists with files

- GIVEN `%APPDATA%\AgentHarness\skills\` contains `coding.md` and `review.md`
- WHEN the Skills page loads
- THEN two cards appear with titles "Coding" and "Review"
- AND each card shows the first line of the file as description

#### Scenario: Skills folder empty

- GIVEN the skills folder exists but has no `.md` files
- WHEN the Skills page loads
- THEN an empty-state message appears: "No skills found"
- AND a "Open folder" button is visible

#### Scenario: Skills folder missing

- GIVEN the skills folder does not exist
- WHEN the Skills page loads
- THEN the folder is created automatically
- AND the empty state is shown

### Requirement: Toggle Switches

Each skill card MUST include a toggle switch bound to `SkillItem.IsEnabled`, animated with a spring easing function.

#### Scenario: Toggle on

- GIVEN a skill toggle is off
- WHEN the user flips the switch
- THEN `SkillItem.IsEnabled` becomes `true`
- AND the switch thumb animates with spring physics

#### Scenario: Toggle off

- GIVEN a skill toggle is on
- WHEN the user flips the switch
- THEN `SkillItem.IsEnabled` becomes `false`
- AND the card opacity reduces to 60%
